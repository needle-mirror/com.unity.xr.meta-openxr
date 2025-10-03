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
    static class SingleEraseAnchor
    {
        static readonly Dictionary<SerializableGuid, AwaitableCompletionSource<XRResultStatus>>
            s_PendingCompletionSourcesByRequestId = new();

        static readonly ObjectPool<AwaitableCompletionSource<XRResultStatus>> s_CompletionSourcePool =
            ObjectPoolCreateUtil.Create<AwaitableCompletionSource<XRResultStatus>>();

        unsafe delegate void SingleEraseAsyncDelegate(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults);

        static readonly unsafe IntPtr s_SingleEraseAsyncCallback =
            Marshal.GetFunctionPointerForDelegate((SingleEraseAsyncDelegate)OnSingleEraseAsyncComplete);

        internal static void CancelAllRequests()
        {
            foreach (var completionSource in s_PendingCompletionSourcesByRequestId.Values)
            {
                completionSource.SetCanceled();
                completionSource.Reset();
                s_CompletionSourcePool.Release(completionSource);
            }
            s_PendingCompletionSourcesByRequestId.Clear();
        }

        internal static unsafe Awaitable<XRResultStatus> TryEraseAnchorAsync(SerializableGuid savedAnchorGuid)
        {
            var completionSource = s_CompletionSourcePool.Get();
            var awaitable = completionSource.Awaitable;

            var requestId = new SerializableGuid(Guid.NewGuid());
            s_PendingCompletionSourcesByRequestId.Add(requestId, completionSource);

            var anchorIdsToErase = new NativeArray<SerializableGuid>(1, Allocator.Temp)
            {
                [0] = savedAnchorGuid
            };

            // only fails if provider isn't initialized
            var success = NativeApi.TryEraseAnchorsAsync(
                requestId, anchorIdsToErase.GetUnsafePtr(), 1, s_SingleEraseAsyncCallback);

            if (!success)
            {
                s_PendingCompletionSourcesByRequestId.Remove(requestId);
                awaitable = AwaitableUtils<XRResultStatus>.FromResult(
                    completionSource, new XRResultStatus(StatusCode.ProviderUninitialized));
                s_CompletionSourcePool.Release(completionSource);
            }

            return awaitable;
        }

        [MonoPInvokeCallback(typeof(SingleEraseAsyncDelegate))]
        static unsafe void OnSingleEraseAsyncComplete(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults)
        {
            Assert.IsTrue(s_PendingCompletionSourcesByRequestId.ContainsKey(requestId));
            s_PendingCompletionSourcesByRequestId.Remove(requestId, out var completionSource);

            Assert.IsTrue(resultsPtr != null);
            Assert.IsTrue(sizeOfResult > 0);
            Assert.AreEqual(numResults, 1);

            var eraseResult = XREraseAnchorResult.defaultValue;
            UnsafeUtility.MemCpyStride(
                &eraseResult, sizeof(XREraseAnchorResult), resultsPtr, sizeOfResult, sizeOfResult, numResults);

            completionSource.SetResult(new XRResultStatus(eraseResult.resultStatus));
            completionSource.Reset();
            s_CompletionSourcePool.Release(completionSource);
        }
    }
}
