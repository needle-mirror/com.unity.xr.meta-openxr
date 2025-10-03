using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using Unity.Collections;
using UnityEngine.Assertions;
using UnityEngine.Pool;
using UnityEngine.XR.ARSubsystems;
using static UnityEngine.XR.ARSubsystems.XRResultStatus;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    static class LoadAllSharedAnchors
    {
        struct LoadOperation
        {
            internal AwaitableCompletionSource<Result<NativeArray<XRAnchor>>> completionSource;
            internal Allocator allocator;
            internal Action<NativeArray<XRAnchor>> incrementalCallback;
        }

        static readonly Dictionary<SerializableGuid, LoadOperation> s_PendingOpsByRequestId = new();

        static readonly ObjectPool<AwaitableCompletionSource<Result<NativeArray<XRAnchor>>>> s_CompletionSourcePool =
            ObjectPoolCreateUtil.Create<AwaitableCompletionSource<Result<NativeArray<XRAnchor>>>>(defaultCapacity: 2);

        unsafe delegate void LoadCompletedDelegate(
            SerializableGuid requestId,
            XRResultStatus resultStatus,
            void* resultsPtr,
            int sizeOfResult,
            int numResults);

        static readonly unsafe IntPtr s_CompletedCallback =
            Marshal.GetFunctionPointerForDelegate((LoadCompletedDelegate)OnBatchLoadAsyncComplete);

        unsafe delegate void IncrementalResultsDelegate(
            SerializableGuid requestId, XRAnchor* resultsPtr, int sizeOfResult, int numResults);

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

        internal static Awaitable<Result<NativeArray<XRAnchor>>> TryLoadAllSharedAnchorsAsync(
            SerializableGuid groupId, Allocator allocator, Action<NativeArray<XRAnchor>> incrementalCallback)
        {
            var completionSource = s_CompletionSourcePool.Get();
            var awaitable = completionSource.Awaitable;

            var operation = new LoadOperation
            {
                completionSource = completionSource,
                allocator = allocator,
                incrementalCallback = incrementalCallback,
            };

            var requestId = new SerializableGuid(Guid.NewGuid());
            s_PendingOpsByRequestId.Add(requestId, operation);

            // only fails if provider isn't initialized
            if (!NativeApi.TryLoadAllSharedAnchorsAsync(requestId, groupId, s_IncrementalCallback, s_CompletedCallback))
            {
                s_PendingOpsByRequestId.Remove(requestId);
                var emptyResults = new NativeArray<XRAnchor>(0, allocator);
                var result = new Result<NativeArray<XRAnchor>>(
                    new XRResultStatus(StatusCode.ProviderUninitialized), emptyResults);

                awaitable = AwaitableUtils<Result<NativeArray<XRAnchor>>>.FromResult(completionSource, result);
                s_CompletionSourcePool.Release(completionSource);
            }

            return awaitable;
        }

        [MonoPInvokeCallback(typeof(IncrementalResultsDelegate))]
        static unsafe void OnIncrementalLoadResultsAvailable(
            SerializableGuid requestId, XRAnchor* resultsPtr, int sizeOfResult, int numResults)
        {
            Assert.IsTrue(s_PendingOpsByRequestId.ContainsKey(requestId));
            Assert.IsTrue(resultsPtr != null);
            Assert.IsTrue(sizeOfResult > 0);
            Assert.IsTrue(numResults > 0);

            var operation = s_PendingOpsByRequestId[requestId];
            if (operation.incrementalCallback == null)
                return;

            var results = NativeCopyUtility.PtrToNativeArrayWithDefault(
                XRAnchor.defaultValue, resultsPtr, sizeOfResult, numResults, Allocator.Temp);

            operation.incrementalCallback.Invoke(results);
        }

        [MonoPInvokeCallback(typeof(LoadCompletedDelegate))]
        static unsafe void OnBatchLoadAsyncComplete(
            SerializableGuid requestId, XRResultStatus resultStatus, void* resultsPtr, int sizeOfResult, int numResults)
        {
            Assert.IsTrue(s_PendingOpsByRequestId.ContainsKey(requestId));
            Assert.IsTrue(sizeOfResult > 0);

            s_PendingOpsByRequestId.Remove(requestId, out var operation);

            Result<NativeArray<XRAnchor>> result = default;
            if (resultsPtr == null)
            {
                Assert.IsTrue(numResults == 0);
            }
            else
            {
                Assert.IsTrue(numResults > 0);
                var results = NativeCopyUtility.PtrToNativeArrayWithDefault(
                    XRAnchor.defaultValue, resultsPtr, sizeOfResult, numResults, operation.allocator);

                result = new Result<NativeArray<XRAnchor>>(resultStatus, results);
            }

            operation.completionSource.SetResult(result);
            operation.completionSource.Reset();
            s_CompletionSourcePool.Release(operation.completionSource);
        }

        static class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_TryLoadAllSharedAnchorsAsync")]
            [return: MarshalAs(UnmanagedType.U1)]
            internal static extern bool TryLoadAllSharedAnchorsAsync(
                SerializableGuid requestId,
                SerializableGuid groupId,
                IntPtr incrementalCallback,
                IntPtr completedCallback);
        }
    }
}
