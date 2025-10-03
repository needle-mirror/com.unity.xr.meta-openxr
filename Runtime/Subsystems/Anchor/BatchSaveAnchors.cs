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
    static class BatchSaveAnchors
    {
        struct SaveOperation
        {
            public AwaitableCompletionSource<NativeArray<XRSaveAnchorResult>> completionSource;
            public Allocator allocator;
        }

        static readonly Dictionary<SerializableGuid, SaveOperation> s_PendingOpsByRequestId = new();

        static readonly ObjectPool<AwaitableCompletionSource<NativeArray<XRSaveAnchorResult>>> s_CompletionSourcePool =
            ObjectPoolCreateUtil.Create<AwaitableCompletionSource<NativeArray<XRSaveAnchorResult>>>(defaultCapacity: 2);

        unsafe delegate void BatchSaveAsyncDelegate(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults);

        static readonly unsafe IntPtr s_CompletedCallback =
            Marshal.GetFunctionPointerForDelegate((BatchSaveAsyncDelegate)OnBatchSaveAsyncComplete);

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

        internal static unsafe Awaitable<NativeArray<XRSaveAnchorResult>> TrySaveAnchorsAsync(
            NativeArray<TrackableId> anchorIds, Allocator allocator)
        {
            var completionSource = s_CompletionSourcePool.Get();
            var awaitable = completionSource.Awaitable;

            if (anchorIds.Length == 0)
            {
                awaitable = AwaitableUtils<NativeArray<XRSaveAnchorResult>>.FromResult(
                    completionSource, new NativeArray<XRSaveAnchorResult>(0, allocator));

                s_CompletionSourcePool.Release(completionSource);
                return awaitable;
            }

            var operation = new SaveOperation
            {
                completionSource = completionSource,
                allocator = allocator,
            };

            var requestId = new SerializableGuid(Guid.NewGuid());
            s_PendingOpsByRequestId.Add(requestId, operation);

            // only fails if provider isn't initialized
            var success = NativeApi.TrySaveAnchorsAsync(
                requestId, anchorIds.GetUnsafePtr(), (uint)anchorIds.Length, s_CompletedCallback);

            if (!success)
            {
                s_PendingOpsByRequestId.Remove(requestId);
                var results = new NativeArray<XRSaveAnchorResult>(anchorIds.Length, allocator);
                for (var i = 0; i < anchorIds.Length; ++i)
                {
                    results[i] = new XRSaveAnchorResult(
                        new XRResultStatus(StatusCode.ProviderUninitialized), anchorIds[i], default);
                }

                awaitable = AwaitableUtils<NativeArray<XRSaveAnchorResult>>.FromResult(completionSource, results);
                s_CompletionSourcePool.Release(completionSource);
            }

            return awaitable;
        }

        [MonoPInvokeCallback(typeof(BatchSaveAsyncDelegate))]
        static unsafe void OnBatchSaveAsyncComplete(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults)
        {
            Assert.IsTrue(s_PendingOpsByRequestId.ContainsKey(requestId));
            s_PendingOpsByRequestId.Remove(requestId, out var operation);

            Assert.IsTrue(resultsPtr != null);
            Assert.IsTrue(sizeOfResult > 0);

            var saveResults = NativeCopyUtility.PtrToNativeArrayWithDefault(
                XRSaveAnchorResult.defaultValue, resultsPtr, sizeOfResult, numResults, operation.allocator);

            operation.completionSource.SetResult(saveResults);
            operation.completionSource.Reset();
            s_CompletionSourcePool.Release(operation.completionSource);
        }
    }
}
