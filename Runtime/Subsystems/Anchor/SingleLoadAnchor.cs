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
    static class SingleLoadAnchor
    {
        static readonly Dictionary<SerializableGuid, AwaitableCompletionSource<Result<XRAnchor>>>
            s_PendingCompletionSourcesByRequestId = new();

        static readonly ObjectPool<AwaitableCompletionSource<Result<XRAnchor>>> s_CompletionSourcePool =
            ObjectPoolCreateUtil.Create<AwaitableCompletionSource<Result<XRAnchor>>>();

        unsafe delegate void SingleLoadAsyncDelegate(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults);

        static readonly unsafe IntPtr s_SingleLoadAsyncCallback =
            Marshal.GetFunctionPointerForDelegate((SingleLoadAsyncDelegate)OnSingleLoadAsyncComplete);

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

        internal static unsafe Awaitable<Result<XRAnchor>> TryLoadAnchorAsync(SerializableGuid savedAnchorGuid)
        {
            var completionSource = s_CompletionSourcePool.Get();
            var awaitable = completionSource.Awaitable;

            var requestId = new SerializableGuid(Guid.NewGuid());
            s_PendingCompletionSourcesByRequestId.Add(requestId, completionSource);

            var anchorIdsToLoad = new NativeArray<SerializableGuid>(1, Allocator.Temp)
            {
                [0] = savedAnchorGuid
            };

            // only fails if provider isn't initialized
            var success = NativeApi.TryLoadAnchorsAsync(
                requestId,
                anchorIdsToLoad.GetUnsafePtr(),
                (uint)anchorIdsToLoad.Length,
                IntPtr.Zero,
                s_SingleLoadAsyncCallback);

            if (!success)
            {
                s_PendingCompletionSourcesByRequestId.Remove(requestId);
                awaitable = AwaitableUtils<Result<XRAnchor>>.FromResult(completionSource, new Result<XRAnchor>(
                    new XRResultStatus(StatusCode.ProviderUninitialized), XRAnchor.defaultValue));
                s_CompletionSourcePool.Release(completionSource);
            }

            return awaitable;
        }

        [MonoPInvokeCallback(typeof(SingleLoadAsyncDelegate))]
        static unsafe void OnSingleLoadAsyncComplete(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults)
        {
            Assert.IsTrue(s_PendingCompletionSourcesByRequestId.ContainsKey(requestId));
            s_PendingCompletionSourcesByRequestId.Remove(requestId, out var completionSource);

            Assert.IsTrue(resultsPtr != null);
            Assert.IsTrue(sizeOfResult > 0);
            Assert.AreEqual(numResults, 1);

            var loadResult = XRLoadAnchorResult.defaultValue;
            UnsafeUtility.MemCpyStride(
                &loadResult, sizeof(XRLoadAnchorResult), resultsPtr, sizeOfResult, sizeOfResult, numResults);

            completionSource.SetResult(new Result<XRAnchor>(loadResult.resultStatus, loadResult.xrAnchor));
            completionSource.Reset();
            s_CompletionSourcePool.Release(completionSource);
        }
    }
}
