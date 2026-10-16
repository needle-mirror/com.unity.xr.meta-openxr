using System;
using System.Runtime.InteropServices;
using AOT;
using Unity.Collections;
#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif
using UnityEngine.Assertions;
using UnityEngine.XR.ARSubsystems;
using static UnityEngine.XR.ARSubsystems.XRResultStatus;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    static partial class ColocationDiscovery
    {
        // Subscribers are added via ColocationDiscoveryFeature and must survive an XR restart within a play session,
        // so they are only cleared when entering or exiting play mode.
#if UNITY_6000_5_OR_NEWER
        [AutoStaticsCleanup]
#endif
        internal static event EventHandler<Result<ColocationState>> stateChanged;

#if UNITY_6000_5_OR_NEWER
        [AutoStaticsCleanup]
#endif
        internal static event EventHandler<ColocationDiscoveryMessage> messageDiscovered;

        // Reset in CancelRequestsAndDestroyNative.
#if UNITY_6000_5_OR_NEWER
        [NoAutoStaticsCleanup]
#endif
        internal static ColocationState discoveryState;

        // Completion sources are reset after every use and canceled in CancelRequestsAndDestroyNative.
#if UNITY_6000_5_OR_NEWER
        [NoAutoStaticsCleanup]
#endif
        static AwaitableCompletionSource<XRResultStatus> s_StartCompletionSource = new();

#if UNITY_6000_5_OR_NEWER
        [NoAutoStaticsCleanup]
#endif
        static AwaitableCompletionSource<XRResultStatus> s_StopCompletionSource = new();

        delegate void StartAsyncDelegate(XRResultStatus resultStatus);
        delegate void StopDelegate(XRResultStatus resultStatus);
        unsafe delegate void MessageDiscoveredDelegate(
            void* messageBytesPtr,
            int sizeOfMessage,
            int bytesCount,
            SerializableGuid advertisementId);

        static readonly IntPtr s_StartAsyncCallback = Marshal.GetFunctionPointerForDelegate((StartAsyncDelegate)OnStartAsyncComplete);
        static readonly IntPtr s_StopAsyncCallback = Marshal.GetFunctionPointerForDelegate((StopDelegate)OnStopAsyncComplete);
        static readonly IntPtr s_DiscoveryStoppedCallback = Marshal.GetFunctionPointerForDelegate((StopDelegate)OnDiscoveryStopped);
        static readonly unsafe IntPtr s_MessageDiscoveredCallback = Marshal.GetFunctionPointerForDelegate((MessageDiscoveredDelegate)OnMessageDiscovered);

        internal static void Create()
        {
            NativeApi.Create(
                s_StartAsyncCallback, s_StopAsyncCallback, s_DiscoveryStoppedCallback, s_MessageDiscoveredCallback);
        }

        internal static void CancelRequestsAndDestroyNative()
        {
            // Destroy the native provider first so that code running during cancellation (awaiters or stateChanged
            // handlers) can't start a new request that Destroy would then orphan. Such retries instead fail with
            // ProviderUninitialized and leave the state Inactive.
            NativeApi.Destroy();

            // Reset state before canceling so that awaiters resumed by the cancellation observe Inactive.
            var wasInactive = discoveryState == ColocationState.Inactive;
            discoveryState = ColocationState.Inactive;

            s_StartCompletionSource.SetCanceled();
            s_StartCompletionSource.Reset();

            s_StopCompletionSource.SetCanceled();
            s_StopCompletionSource.Reset();

            if (wasInactive)
                return;

            // Raise the event last so that a throwing handler can't skip any of the cleanup above.
            var stateChangedResult = new Result<ColocationState>(
                new XRResultStatus(StatusCode.UnqualifiedSuccess), discoveryState);
            stateChanged?.Invoke(typeof(ColocationDiscovery), stateChangedResult);
        }

        internal static Awaitable<XRResultStatus> TryStartAsync()
        {
            if (discoveryState != ColocationState.Inactive)
            {
                var validationFailure = new XRResultStatus(StatusCode.ValidationFailure);
                return AwaitableUtils<XRResultStatus>.FromResult(s_StartCompletionSource, validationFailure);
            }

            var requestResultStatus = NativeApi.TryRequestStart();

            if (requestResultStatus.IsError())
                return AwaitableUtils<XRResultStatus>.FromResult(s_StartCompletionSource, requestResultStatus);

            discoveryState = ColocationState.Starting;
            var stateChangedResult = new Result<ColocationState>(requestResultStatus, discoveryState);
            stateChanged?.Invoke(typeof(ColocationDiscovery), stateChangedResult);

            return s_StartCompletionSource.Awaitable;
        }

        internal static Awaitable<XRResultStatus> TryStopAsync()
        {
            if (discoveryState != ColocationState.Active)
            {
                var validationFailure = new XRResultStatus(StatusCode.ValidationFailure);
                return AwaitableUtils<XRResultStatus>.FromResult(s_StopCompletionSource, validationFailure);
            }

            var requestResultStatus = NativeApi.TryRequestStop();

            if (requestResultStatus.IsError())
                return AwaitableUtils<XRResultStatus>.FromResult(s_StopCompletionSource, requestResultStatus);

            discoveryState = ColocationState.Stopping;
            var stateChangedResult = new Result<ColocationState>(
                new XRResultStatus(StatusCode.UnqualifiedSuccess), discoveryState);
            stateChanged?.Invoke(typeof(ColocationAdvertisement), stateChangedResult);

            return s_StopCompletionSource.Awaitable;
        }

        [MonoPInvokeCallback(typeof(StartAsyncDelegate))]
        static void OnStartAsyncComplete(XRResultStatus resultStatus)
        {
            Assert.IsTrue(discoveryState == ColocationState.Starting);

            discoveryState = resultStatus.IsSuccess() ? ColocationState.Active : ColocationState.Inactive;
            var stateChangedResult = new Result<ColocationState>(resultStatus, discoveryState);
            stateChanged?.Invoke(typeof(ColocationDiscovery), stateChangedResult);

            s_StartCompletionSource.SetResult(resultStatus);
            s_StartCompletionSource.Reset();
        }

        [MonoPInvokeCallback(typeof(StopDelegate))]
        static void OnStopAsyncComplete(XRResultStatus resultStatus)
        {
            s_StopCompletionSource.SetResult(resultStatus);
            s_StopCompletionSource.Reset();
        }

        [MonoPInvokeCallback(typeof(StopDelegate))]
        static void OnDiscoveryStopped(XRResultStatus resultStatus)
        {
            if (discoveryState == ColocationState.Inactive)
                return;

            discoveryState = ColocationState.Inactive;
            var stateChangedResult = new Result<ColocationState>(resultStatus, discoveryState);
            stateChanged?.Invoke(typeof(ColocationDiscovery), stateChangedResult);
        }

        [MonoPInvokeCallback(typeof(MessageDiscoveredDelegate))]
        static unsafe void OnMessageDiscovered(
            void* messageBytesPtr,
            int sizeOfMessage,
            int bytesCount,
            SerializableGuid advertisementId)
        {
            var data = NativeCopyUtility.PtrToNativeArrayWithDefault(
                (byte)0, messageBytesPtr, sizeOfMessage, bytesCount, Allocator.Temp);

            var result = new ColocationDiscoveryMessage(advertisementId, data);
            messageDiscovered?.Invoke(typeof(ColocationDiscovery), result);
        }

        static class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_ColocationDiscovery_Create")]
            internal static extern void Create(
                IntPtr startCallback, IntPtr stopCallback, IntPtr discoveryStoppedCallback, IntPtr messageDiscoveredCallback);

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_ColocationDiscovery_Destroy")]
            internal static extern void Destroy();

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_ColocationDiscovery_TryRequestStart")]
            internal static extern XRResultStatus TryRequestStart();

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_ColocationDiscovery_TryRequestStop")]
            internal static extern XRResultStatus TryRequestStop();
        }
    }
}
