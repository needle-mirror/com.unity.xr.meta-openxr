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
    static class BatchShareAnchors
    {
        struct ShareOperation
        {
            internal AwaitableCompletionSource<NativeArray<XRShareAnchorResult>> completionSource;
            internal Allocator allocator;
        }

        static readonly Dictionary<SerializableGuid, ShareOperation> s_PendingOpsByRequestId = new();

        static readonly ObjectPool<AwaitableCompletionSource<NativeArray<XRShareAnchorResult>>> s_CompletionSourcePool =
            ObjectPoolCreateUtil.Create<AwaitableCompletionSource<NativeArray<XRShareAnchorResult>>>();

        unsafe delegate void BatchShareCompletedDelegate(
            SerializableGuid requestId, XRShareAnchorResult* resultsPtr, int sizeOfResult, int numResults);

        static readonly unsafe IntPtr s_CompletedCallback =
            Marshal.GetFunctionPointerForDelegate((BatchShareCompletedDelegate)OnBatchShareAsyncComplete);

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

        internal static unsafe Awaitable<NativeArray<XRShareAnchorResult>> TryShareAnchorsAsync(
            NativeArray<TrackableId> anchorIds, SerializableGuid groupId, Allocator allocator)
        {
            var completionSource = s_CompletionSourcePool.Get();
            var awaitable = completionSource.Awaitable;

            if (anchorIds.Length == 0)
            {
                awaitable = AwaitableUtils<NativeArray<XRShareAnchorResult>>.FromResult(
                    completionSource, new NativeArray<XRShareAnchorResult>(0, allocator));

                s_CompletionSourcePool.Release(completionSource);
                return awaitable;
            }

            var operation = new ShareOperation
            {
                completionSource = completionSource,
                allocator = allocator,
            };

            var requestId = new SerializableGuid(Guid.NewGuid());
            s_PendingOpsByRequestId.Add(requestId, operation);

            var success = NativeApi.TryShareAnchorsAsync(
                requestId,
                anchorIds.GetUnsafePtr(),
                (uint)anchorIds.Length,
                groupId,
                s_CompletedCallback);

            // only fails if provider isn't initialized
            if (!success)
            {
                s_PendingOpsByRequestId.Remove(requestId);
                var results = new NativeArray<XRShareAnchorResult>(anchorIds.Length, allocator);
                for (var i = 0; i < anchorIds.Length; ++i)
                {
                    results[i] = new XRShareAnchorResult(new XRResultStatus(StatusCode.ProviderUninitialized), anchorIds[i]);
                }

                awaitable = AwaitableUtils<NativeArray<XRShareAnchorResult>>.FromResult(completionSource, results);
                s_CompletionSourcePool.Release(completionSource);
            }

            return awaitable;
        }

        [MonoPInvokeCallback(typeof(BatchShareCompletedDelegate))]
        static unsafe void OnBatchShareAsyncComplete(
            SerializableGuid requestId, XRShareAnchorResult* resultsPtr, int sizeOfResult, int numResults)
        {
            Assert.IsTrue(s_PendingOpsByRequestId.ContainsKey(requestId));
            s_PendingOpsByRequestId.Remove(requestId, out var operation);

            Assert.IsTrue(resultsPtr != null);
            Assert.IsTrue(sizeOfResult > 0);

            var results = NativeCopyUtility.PtrToNativeArrayWithDefault(
                XRShareAnchorResult.defaultValue, resultsPtr, sizeOfResult, numResults, operation.allocator);

            operation.completionSource.SetResult(results);
            operation.completionSource.Reset();
            s_CompletionSourcePool.Release(operation.completionSource);
        }
    }
}
