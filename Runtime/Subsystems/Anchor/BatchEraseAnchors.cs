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
    static class BatchEraseAnchors
    {
        struct EraseOperation
        {
            internal AwaitableCompletionSource<NativeArray<XREraseAnchorResult>> completionSource;
            internal Allocator allocator;
        }

        static readonly Dictionary<SerializableGuid, EraseOperation> s_PendingOpsByRequestId = new();

        static readonly ObjectPool<AwaitableCompletionSource<NativeArray<XREraseAnchorResult>>> s_CompletionSourcePool =
            ObjectPoolCreateUtil.Create<AwaitableCompletionSource<NativeArray<XREraseAnchorResult>>>(defaultCapacity: 2);

        unsafe delegate void BatchEraseAsyncDelegate(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults);

        static readonly unsafe IntPtr s_CompletedCallback =
            Marshal.GetFunctionPointerForDelegate((BatchEraseAsyncDelegate)OnBatchEraseAsyncComplete);

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

        internal static unsafe Awaitable<NativeArray<XREraseAnchorResult>> TryEraseAnchorsAsync(
            NativeArray<SerializableGuid> savedAnchorGuids, Allocator allocator)
        {
            var completionSource = s_CompletionSourcePool.Get();
            var awaitable = completionSource.Awaitable;

            if (savedAnchorGuids.Length == 0)
            {
                awaitable = AwaitableUtils<NativeArray<XREraseAnchorResult>>.FromResult(
                    completionSource, new NativeArray<XREraseAnchorResult>(0, allocator));

                s_CompletionSourcePool.Release(completionSource);
                return awaitable;
            }

            var eraseRequest = new EraseOperation
            {
                completionSource = completionSource,
                allocator = allocator,
            };

            var requestId = new SerializableGuid(Guid.NewGuid());
            s_PendingOpsByRequestId.Add(requestId, eraseRequest);

            // only fails if provider isn't initialized
            var success = NativeApi.TryEraseAnchorsAsync(
                requestId, savedAnchorGuids.GetUnsafePtr(), (uint)savedAnchorGuids.Length, s_CompletedCallback);

            if (!success)
            {
                s_PendingOpsByRequestId.Remove(requestId);
                var results = new NativeArray<XREraseAnchorResult>(savedAnchorGuids.Length, allocator);
                for (var i = 0; i < savedAnchorGuids.Length; ++i)
                {
                    results[i] = new XREraseAnchorResult(
                        new XRResultStatus(StatusCode.ProviderUninitialized), savedAnchorGuids[i]);
                }

                awaitable = AwaitableUtils<NativeArray<XREraseAnchorResult>>.FromResult(completionSource, results);
                s_CompletionSourcePool.Release(completionSource);
            }

            return awaitable;
        }

        [MonoPInvokeCallback(typeof(BatchEraseAsyncDelegate))]
        static unsafe void OnBatchEraseAsyncComplete(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults)
        {
            Assert.IsTrue(s_PendingOpsByRequestId.ContainsKey(requestId));
            s_PendingOpsByRequestId.Remove(requestId, out var operation);

            Assert.IsTrue(resultsPtr != null);
            Assert.IsTrue(sizeOfResult > 0);

            var eraseResults = NativeCopyUtility.PtrToNativeArrayWithDefault(
                XREraseAnchorResult.defaultValue, resultsPtr, sizeOfResult, numResults, operation.allocator);

            operation.completionSource.SetResult(eraseResults);
            operation.completionSource.Reset();
            s_CompletionSourcePool.Release(operation.completionSource);
        }
    }
}
