using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Pool;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    static class LoadAllSharedAnchors
    {
        struct LoadRequest
        {
            public AwaitableCompletionSource<Result<NativeArray<XRAnchor>>> completionSource;
            public Allocator allocator;
            public Action<NativeArray<XRAnchor>> incrementalResultsCallback;
        }

        static readonly Dictionary<SerializableGuid, LoadRequest> s_PendingRequestsByRequestId = new();

        static readonly ObjectPool<AwaitableCompletionSource<Result<NativeArray<XRAnchor>>>> s_CompletionSourcePool =
            ObjectPoolCreateUtil.Create<AwaitableCompletionSource<Result<NativeArray<XRAnchor>>>>(defaultCapacity: 2);

        unsafe delegate void LoadAsyncDelegate(SerializableGuid requestId, void* loadAnchorResultsPtr, uint resultsCount);
        static readonly unsafe IntPtr s_BatchLoadAsyncCallback = Marshal.GetFunctionPointerForDelegate((LoadAsyncDelegate)OnBatchLoadAsyncComplete);

        unsafe delegate void IncrementalResultsDelegate(SerializableGuid requestId, void* loadAnchorResultsPtr, uint resultsCount);
        static readonly unsafe IntPtr s_IncrementalLoadResultsCallback = Marshal.GetFunctionPointerForDelegate((IncrementalResultsDelegate)OnIncrementalLoadResultsAvailable);

        internal static void CancelAllRequests()
        {
            foreach (var loadRequest in s_PendingRequestsByRequestId.Values)
            {
                var completionSource = loadRequest.completionSource;
                completionSource.SetCanceled();
                completionSource.Reset();
                s_CompletionSourcePool.Release(completionSource);
            }
            s_PendingRequestsByRequestId.Clear();
        }

        internal static Awaitable<Result<NativeArray<XRAnchor>>> TryLoadAllSharedAnchorsAsync(
            SerializableGuid groupId,
            Allocator allocator,
            Action<NativeArray<XRAnchor>> incrementalResultsCallback)
        {
            var completionSource = s_CompletionSourcePool.Get();
            var loadRequest = new LoadRequest
            {
                completionSource = completionSource,
                allocator = allocator,
                incrementalResultsCallback = incrementalResultsCallback,
            };

            var requestId = new SerializableGuid(Guid.NewGuid());
            s_PendingRequestsByRequestId.Add(requestId, loadRequest);

            var synchronousResultStatus = new XRResultStatus();
            NativeApi.TryLoadAllSharedAnchorsAsync(
                requestId,
                groupId,
                s_IncrementalLoadResultsCallback,
                s_BatchLoadAsyncCallback,
                ref synchronousResultStatus);

            if (synchronousResultStatus.IsError())
            {
                var failedLoadResults = new NativeArray<XRAnchor>(
                    0,
                    allocator);

                s_PendingRequestsByRequestId.Remove(requestId);
                var result = new Result<NativeArray<XRAnchor>>(
                    new XRResultStatus(synchronousResultStatus),
                    failedLoadResults);

                return AwaitableUtils<Result<NativeArray<XRAnchor>>>.FromResult(
                    completionSource,
                    result);
            }

            return completionSource.Awaitable;
        }

        [MonoPInvokeCallback(typeof(IncrementalResultsDelegate))]
        static unsafe void OnIncrementalLoadResultsAvailable(
            SerializableGuid requestId,
            void* loadResultsPtr,
            uint resultsCount)
        {
            if (!s_PendingRequestsByRequestId.TryGetValue(requestId, out var loadRequest))
            {
                Debug.LogError("An unknown error occurred during a system callback for incremental load results.");
                return;
            }

            if (loadRequest.incrementalResultsCallback == null)
                return;

            var loadResult = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<XRAnchor>(
                loadResultsPtr,
                (int)resultsCount,
                Allocator.Temp);

            loadRequest.incrementalResultsCallback.Invoke(loadResult);
        }

        [MonoPInvokeCallback(typeof(LoadAsyncDelegate))]
        static unsafe void OnBatchLoadAsyncComplete(
            SerializableGuid requestId,
            void* loadResultsPtr,
            uint resultsCount)
        {
            if (!s_PendingRequestsByRequestId.Remove(requestId, out var loadRequest))
            {
                Debug.LogError($"An unknown error occurred during a system callback for {nameof(TryLoadAllSharedAnchorsAsync)}.");
                return;
            }

            var loadResults =
                NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<XRAnchor>(
                    loadResultsPtr,
                    (int)resultsCount,
                    Allocator.None);

            var loadResultsCopy = new NativeArray<XRAnchor>(
                (int)resultsCount, loadRequest.allocator);

            NativeArray<XRAnchor>.Copy(loadResults, loadResultsCopy);
            var result = new Result<NativeArray<XRAnchor>>(
                new XRResultStatus(XRResultStatus.StatusCode.UnqualifiedSuccess),
                loadResultsCopy);

            var completionSource = loadRequest.completionSource;
            completionSource.SetResult(result);
            completionSource.Reset();
            s_CompletionSourcePool.Release(completionSource);
        }

        static class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_TryLoadAllSharedAnchorsAsync")]
            public static extern void TryLoadAllSharedAnchorsAsync(
                SerializableGuid requestId,
                SerializableGuid groupId,
                IntPtr incrementalResultsCallback,
                IntPtr tryLoadSharedAnchorAsyncCallback,
                ref XRResultStatus synchronousResultStatus);
        }
    }
}
