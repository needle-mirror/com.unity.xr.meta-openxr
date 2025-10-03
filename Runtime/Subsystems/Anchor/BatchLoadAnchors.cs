using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;
using UnityEngine.Pool;
using UnityEngine.XR.ARSubsystems;
using static UnityEngine.XR.ARSubsystems.XRResultStatus;
using static UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRAnchorSubsystem;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    static class BatchLoadAnchors
    {
        struct LoadOperation
        {
            public AwaitableCompletionSource<NativeArray<XRLoadAnchorResult>> completionSource;
            public Allocator allocator;
            public Action<NativeArray<XRLoadAnchorResult>> incrementalCallback;
        }

        static readonly Dictionary<SerializableGuid, LoadOperation> s_PendingOpsByRequestId = new();

        static readonly ObjectPool<AwaitableCompletionSource<NativeArray<XRLoadAnchorResult>>> s_CompletionSourcePool =
            ObjectPoolCreateUtil.Create<AwaitableCompletionSource<NativeArray<XRLoadAnchorResult>>>(defaultCapacity: 2);

        unsafe delegate void BatchLoadAsyncDelegate(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults);

        static readonly unsafe IntPtr s_CompletedCallback =
            Marshal.GetFunctionPointerForDelegate((BatchLoadAsyncDelegate)OnBatchLoadAsyncComplete);

        unsafe delegate void IncrementalResultsDelegate(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults);

        static readonly unsafe IntPtr s_IncrementalCallback =
            Marshal.GetFunctionPointerForDelegate((IncrementalResultsDelegate)OnIncrementalLoadResultsAvailable);

        internal static void CancelAllRequests()
        {
            foreach (var operation in s_PendingOpsByRequestId.Values)
            {
                operation.completionSource.SetCanceled();
                operation.completionSource.Reset();
                s_CompletionSourcePool.Release(operation.completionSource);
            }
            s_PendingOpsByRequestId.Clear();
        }

        internal static unsafe Awaitable<NativeArray<XRLoadAnchorResult>> TryLoadAnchorsAsync(
            NativeArray<SerializableGuid> savedAnchorGuids,
            Allocator allocator,
            Action<NativeArray<XRLoadAnchorResult>> incrementalCallback)
        {
            var completionSource = s_CompletionSourcePool.Get();
            var awaitable = completionSource.Awaitable;

            if (savedAnchorGuids.Length == 0)
            {
                awaitable = AwaitableUtils<NativeArray<XRLoadAnchorResult>>.FromResult(
                    completionSource, new NativeArray<XRLoadAnchorResult>(0, allocator));

                s_CompletionSourcePool.Release(completionSource);
                return awaitable;
            }

            var operation = new LoadOperation
            {
                completionSource = completionSource,
                allocator = allocator,
                incrementalCallback = incrementalCallback,
            };

            var requestId = new SerializableGuid(Guid.NewGuid());
            s_PendingOpsByRequestId.Add(requestId, operation);

            var success = NativeApi.TryLoadAnchorsAsync(
                requestId,
                savedAnchorGuids.GetUnsafePtr(),
                (uint)savedAnchorGuids.Length,
                s_IncrementalCallback,
                s_CompletedCallback);

            // only fails if provider isn't initialized
            if (!success)
            {
                s_PendingOpsByRequestId.Remove(requestId);
                var results = new NativeArray<XRLoadAnchorResult>(savedAnchorGuids.Length, allocator);
                for (var i = 0; i < savedAnchorGuids.Length; ++i)
                {
                    results[i] = new XRLoadAnchorResult
                    {
                        resultStatus = new XRResultStatus(StatusCode.ProviderUninitialized),
                        savedAnchorGuid = savedAnchorGuids[i],
                        xrAnchor = default,
                    };
                }

                awaitable = AwaitableUtils<NativeArray<XRLoadAnchorResult>>.FromResult(completionSource, results);
                s_CompletionSourcePool.Release(completionSource);
            }

            return awaitable;
        }

        [MonoPInvokeCallback(typeof(IncrementalResultsDelegate))]
        static unsafe void OnIncrementalLoadResultsAvailable(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults)
        {
            Assert.IsTrue(s_PendingOpsByRequestId.ContainsKey(requestId));
            Assert.IsTrue(resultsPtr != null);
            Assert.IsTrue(sizeOfResult > 0);
            Assert.IsTrue(numResults > 0);

            var operation = s_PendingOpsByRequestId[requestId];
            if (operation.incrementalCallback == null)
                return;

            var loadResults = NativeCopyUtility.PtrToNativeArrayWithDefault(
                XRLoadAnchorResult.defaultValue, resultsPtr, sizeOfResult, numResults, Allocator.Temp);

            operation.incrementalCallback.Invoke(loadResults);
        }

        [MonoPInvokeCallback(typeof(BatchLoadAsyncDelegate))]
        static unsafe void OnBatchLoadAsyncComplete(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults)
        {
            Assert.IsTrue(s_PendingOpsByRequestId.ContainsKey(requestId));
            Assert.IsTrue(sizeOfResult > 0);
            Assert.IsTrue(numResults > 0);

            s_PendingOpsByRequestId.Remove(requestId, out var operation);

            var loadResults = NativeCopyUtility.PtrToNativeArrayWithDefault(
                    XRLoadAnchorResult.defaultValue, resultsPtr, sizeOfResult, numResults, operation.allocator);

            operation.completionSource.SetResult(loadResults);
            operation.completionSource.Reset();
            s_CompletionSourcePool.Release(operation.completionSource);
        }
    }
}
