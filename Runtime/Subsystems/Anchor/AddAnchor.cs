using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine.Pool;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Assertions;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    static class AddAnchor
    {
        static readonly Dictionary<Guid, AwaitableCompletionSource<Result<XRAnchor>>> s_AddAsyncPendingRequests = new();

        static readonly ObjectPool<AwaitableCompletionSource<Result<XRAnchor>>> s_CompletionSourcePool =
            ObjectPoolCreateUtil.Create<AwaitableCompletionSource<Result<XRAnchor>>>(defaultCapacity: 4);

        delegate void AddAsyncDelegate(Guid requestId, XRResultStatus resultStatus, XRAnchor anchor);

        internal static readonly IntPtr addAsyncCallback =
            Marshal.GetFunctionPointerForDelegate((AddAsyncDelegate)OnAddAsyncComplete);

        internal static void CancelAllRequests()
        {
            foreach (var completionSource in s_AddAsyncPendingRequests.Values)
            {
                completionSource.SetCanceled();
                completionSource.Reset();
                s_CompletionSourcePool.Release(completionSource);
            }
            s_AddAsyncPendingRequests.Clear();
        }

        internal static Awaitable<Result<XRAnchor>> TryAddAnchorAsync(Pose pose)
        {
            var requestId = Guid.NewGuid();
            var completionSource = s_CompletionSourcePool.Get();
            var awaitable = completionSource.Awaitable;
            s_AddAsyncPendingRequests.Add(requestId, completionSource);

            var synchronousResultStatus = new XRResultStatus();
            var xrResult = NativeApi.TryAddAnchorAsync(requestId, pose);

            if (xrResult.IsError())
            {
                var result = new Result<XRAnchor>(synchronousResultStatus, XRAnchor.defaultValue);
                awaitable = AwaitableUtils<Result<XRAnchor>>.FromResult(completionSource, result);
                s_CompletionSourcePool.Release(completionSource);
            }

            return awaitable;
        }

        [MonoPInvokeCallback(typeof(AddAsyncDelegate))]
        static void OnAddAsyncComplete(Guid requestId, XRResultStatus resultStatus, XRAnchor anchor)
        {
            Assert.IsTrue(s_AddAsyncPendingRequests.ContainsKey(requestId));
            s_AddAsyncPendingRequests.Remove(requestId, out var completionSource);

            completionSource.SetResult(new Result<XRAnchor>(resultStatus, anchor));
            completionSource.Reset();
            s_CompletionSourcePool.Release(completionSource);
        }

        static class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_TryAddAnchorAsync")]
            public static extern XRResultStatus TryAddAnchorAsync(Guid requestId, Pose pose);
        }
    }
}
