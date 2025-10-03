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
    static class SingleSaveAnchor
    {
        static readonly Dictionary<SerializableGuid, AwaitableCompletionSource<Result<SerializableGuid>>>
            s_PendingCompletionSourcesByRequestId = new();

        static readonly ObjectPool<AwaitableCompletionSource<Result<SerializableGuid>>> s_CompletionSourcePool =
            ObjectPoolCreateUtil.Create<AwaitableCompletionSource<Result<SerializableGuid>>>();

        unsafe delegate void SingleSaveAsyncDelegate(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults);

        static readonly unsafe IntPtr s_CompletedCallback =
            Marshal.GetFunctionPointerForDelegate((SingleSaveAsyncDelegate)OnSingleSaveAsyncComplete);

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

        internal static unsafe Awaitable<Result<SerializableGuid>> TrySaveAnchorAsync(TrackableId anchorId)
        {
            var completionSource = s_CompletionSourcePool.Get();
            var awaitable = completionSource.Awaitable;

            var requestId = new SerializableGuid(Guid.NewGuid());
            s_PendingCompletionSourcesByRequestId.Add(requestId, completionSource);

            var anchorsToSave = new NativeArray<TrackableId>(1, Allocator.Temp)
            {
                [0] = anchorId
            };

            // only fails if provider isn't initialized
            var success = NativeApi.TrySaveAnchorsAsync(
                requestId, anchorsToSave.GetUnsafePtr(), 1, s_CompletedCallback);

            if (!success)
            {
                s_PendingCompletionSourcesByRequestId.Remove(requestId);
                awaitable = AwaitableUtils<Result<SerializableGuid>>.FromResult(
                    completionSource,
                    new Result<SerializableGuid>(new XRResultStatus(StatusCode.ProviderUninitialized), default));
                s_CompletionSourcePool.Release(completionSource);
            }

            return awaitable;
        }

        [MonoPInvokeCallback(typeof(SingleSaveAsyncDelegate))]
        static unsafe void OnSingleSaveAsyncComplete(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults)
        {
            Assert.IsTrue(s_PendingCompletionSourcesByRequestId.ContainsKey(requestId));
            s_PendingCompletionSourcesByRequestId.Remove(requestId, out var completionSource);

            Assert.IsTrue(resultsPtr != null);
            Assert.IsTrue(sizeOfResult > 0);
            Assert.AreEqual(numResults, 1);

            var saveResult = XRSaveAnchorResult.defaultValue;
            UnsafeUtility.MemCpyStride(
                &saveResult, sizeof(XRSaveAnchorResult), resultsPtr, sizeOfResult, sizeOfResult, numResults);

            completionSource.SetResult(new Result<SerializableGuid>(
                saveResult.resultStatus, saveResult.savedAnchorGuid));

            completionSource.Reset();
            s_CompletionSourcePool.Release(completionSource);
        }
    }
}
