using System;
using System.Runtime.InteropServices;
using AOT;
#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif
using UnityEngine.Assertions;
using UnityEngine.XR.ARSubsystems;
using static UnityEngine.XR.ARSubsystems.XRResultStatus;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    static partial class ColocationAdvertisement
    {
        // Subscribers are added via ColocationDiscoveryFeature and must survive an XR restart within a play session,
        // so they are only cleared when entering or exiting play mode.
#if UNITY_6000_5_OR_NEWER
        [AutoStaticsCleanup]
#endif
        internal static event EventHandler<Result<ColocationState>> stateChanged;

        // Reset in CancelRequestsAndDestroyNative.
#if UNITY_6000_5_OR_NEWER
        [NoAutoStaticsCleanup]
#endif
        internal static ColocationState advertisementState { get; private set; }

        // Reset in CancelRequestsAndDestroyNative.
#if UNITY_6000_5_OR_NEWER
        [NoAutoStaticsCleanup]
#endif
        internal static SerializableGuid advertisementId { get; private set; }

        // Completion sources are reset after every use and canceled in CancelRequestsAndDestroyNative.
#if UNITY_6000_5_OR_NEWER
        [NoAutoStaticsCleanup]
#endif
        static AwaitableCompletionSource<Result<SerializableGuid>> s_StartCompletionSource = new();

#if UNITY_6000_5_OR_NEWER
        [NoAutoStaticsCleanup]
#endif
        static AwaitableCompletionSource<XRResultStatus> s_StopCompletionSource = new();

        delegate void StartAsyncDelegate(XRResultStatus resultStatus, SerializableGuid advertisingId);
        delegate void StopDelegate(XRResultStatus resultStatus);

        static readonly IntPtr s_StartAsyncCallback =
            Marshal.GetFunctionPointerForDelegate((StartAsyncDelegate)OnStartAsyncComplete);

        static readonly IntPtr s_StopAsyncCallback =
            Marshal.GetFunctionPointerForDelegate((StopDelegate)OnStopAsyncComplete);

        static readonly IntPtr s_AdvertisementStoppedCallback =
            Marshal.GetFunctionPointerForDelegate((StopDelegate)OnAdvertisementStopped);

        internal static void CreateNative()
        {
            NativeApi.Create(s_StartAsyncCallback, s_StopAsyncCallback, s_AdvertisementStoppedCallback);
        }

        internal static void CancelRequestsAndDestroyNative()
        {
            // Destroy the native provider first so that code running during cancellation (awaiters or stateChanged
            // handlers) can't start a new request that Destroy would then orphan. Such retries instead fail with
            // ProviderUninitialized and leave the state Inactive.
            NativeApi.Destroy();

            // Reset state before canceling so that awaiters resumed by the cancellation observe Inactive.
            var wasInactive = advertisementState == ColocationState.Inactive;
            advertisementState = ColocationState.Inactive;
            advertisementId = SerializableGuid.empty;

            s_StartCompletionSource.SetCanceled();
            s_StartCompletionSource.Reset();

            s_StopCompletionSource.SetCanceled();
            s_StopCompletionSource.Reset();

            if (wasInactive)
                return;

            // Raise the event last so that a throwing handler can't skip any of the cleanup above.
            var stateChangedResult = new Result<ColocationState>(
                new XRResultStatus(StatusCode.UnqualifiedSuccess), advertisementState);
            stateChanged?.Invoke(typeof(ColocationAdvertisement), stateChangedResult);
        }

        internal static unsafe Awaitable<Result<SerializableGuid>> TryStartAsync(Span<byte> message)
        {
            if (advertisementState != ColocationState.Inactive)
            {
                var validationFailure = new XRResultStatus(StatusCode.ValidationFailure);
                var result = new Result<SerializableGuid>(validationFailure, SerializableGuid.empty);
                return AwaitableUtils<Result<SerializableGuid>>.FromResult(s_StartCompletionSource, result);
            }

            XRResultStatus requestResultStatus;

            // This is safe because OpenXR makes a deep copy of this buffer so there is
            // no requirement for this buffer to persist after the call.
            fixed (byte* ptr = message)
            {
                requestResultStatus = NativeApi.TryRequestStart(ptr, (uint)message.Length);
            }

            if (requestResultStatus.IsError())
            {
                var result = new Result<SerializableGuid>(requestResultStatus, advertisementId);
                return AwaitableUtils<Result<SerializableGuid>>.FromResult(s_StartCompletionSource, result);
            }

            advertisementState = ColocationState.Starting;
            var stateChangedResult = new Result<ColocationState>(requestResultStatus, advertisementState);
            stateChanged?.Invoke(typeof(ColocationAdvertisement), stateChangedResult);

            return s_StartCompletionSource.Awaitable;
        }

        internal static Awaitable<XRResultStatus> TryStopAsync()
        {
            if (advertisementState != ColocationState.Active)
            {
                var validationFailure = new XRResultStatus(StatusCode.ValidationFailure);
                return AwaitableUtils<XRResultStatus>.FromResult(s_StopCompletionSource, validationFailure);
            }

            var requestResultStatus = NativeApi.TryRequestStop();

            if (requestResultStatus.IsError())
                return AwaitableUtils<XRResultStatus>.FromResult(s_StopCompletionSource, requestResultStatus);

            advertisementState = ColocationState.Stopping;
            var stateChangedResult = new Result<ColocationState>(requestResultStatus, advertisementState);
            stateChanged?.Invoke(typeof(ColocationAdvertisement), stateChangedResult);

            return s_StopCompletionSource.Awaitable;
        }

        [MonoPInvokeCallback(typeof(StartAsyncDelegate))]
        static void OnStartAsyncComplete(XRResultStatus resultStatus, SerializableGuid nativeAdvertisementId)
        {
            Assert.IsTrue(advertisementState == ColocationState.Starting);

            advertisementId = nativeAdvertisementId;
            advertisementState = resultStatus.IsSuccess() ? ColocationState.Active : ColocationState.Inactive;
            var stateChangedResult = new Result<ColocationState>(resultStatus, advertisementState);
            stateChanged?.Invoke(typeof(ColocationAdvertisement), stateChangedResult);

            var result = new Result<SerializableGuid>(resultStatus, advertisementId);
            s_StartCompletionSource.SetResult(result);
            s_StartCompletionSource.Reset();
        }

        [MonoPInvokeCallback(typeof(StopDelegate))]
        static void OnStopAsyncComplete(XRResultStatus resultStatus)
        {
            s_StopCompletionSource.SetResult(resultStatus);
            s_StopCompletionSource.Reset();
        }

        [MonoPInvokeCallback(typeof(StopDelegate))]
        static void OnAdvertisementStopped(XRResultStatus resultStatus)
        {
            if (advertisementState == ColocationState.Inactive)
                return;

            advertisementId = SerializableGuid.empty;
            advertisementState = ColocationState.Inactive;
            var stateChangedResult = new Result<ColocationState>(resultStatus, advertisementState);
            stateChanged?.Invoke(typeof(ColocationAdvertisement), stateChangedResult);
        }

        static class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_ColocationAdvertisement_Create")]
            internal static extern void Create(
                IntPtr startCompleteCallback, IntPtr stopCompleteCallback, IntPtr advertisementStoppedCallback);

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_ColocationAdvertisement_Destroy")]
            internal static extern void Destroy();

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_ColocationAdvertisement_TryRequestStart")]
            internal static extern unsafe XRResultStatus TryRequestStart(void* data, uint dataSize);

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_ColocationAdvertisement_TryRequestStop")]
            internal static extern XRResultStatus TryRequestStop();
        }
    }
}
