using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Pool;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    static class SingleLoadAnchor
    {
        static readonly Dictionary<SerializableGuid, AwaitableCompletionSource<Result<XRAnchor>>>
            s_PendingCompletionSourcesByAnchorId = new();

        static readonly ObjectPool<AwaitableCompletionSource<Result<XRAnchor>>>
            s_CompletionSourcePool = ObjectPoolCreateUtil.Create<AwaitableCompletionSource<Result<XRAnchor>>>();

        delegate void LoadAsyncDelegate(XRAnchor anchor, XRResultStatus resultStatus);
        internal static readonly IntPtr loadAsyncCallback = Marshal.GetFunctionPointerForDelegate((LoadAsyncDelegate)OnLoadAsyncComplete);

        internal static void CancelAllRequests()
        {
            foreach (var completionSource in s_PendingCompletionSourcesByAnchorId.Values)
            {
                completionSource.SetCanceled();
                completionSource.Reset();
                s_CompletionSourcePool.Release(completionSource);
            }
            s_PendingCompletionSourcesByAnchorId.Clear();
        }

        internal static Awaitable<Result<XRAnchor>> TryLoadAnchorAsync(SerializableGuid savedAnchorGuid)
        {
            var completionSource = s_CompletionSourcePool.Get();
            var wasAddedToMap = s_PendingCompletionSourcesByAnchorId.TryAdd(savedAnchorGuid, completionSource);

            if (!wasAddedToMap)
            {
                Debug.LogError($"Cannot load persistent anchor GUID [{savedAnchorGuid}] while loading for it is already in progress!");
                var resultStatus = new XRResultStatus(XRResultStatus.StatusCode.UnknownError);
                var result = new Result<XRAnchor>(resultStatus, XRAnchor.defaultValue);
                return AwaitableUtils<Result<XRAnchor>>.FromResult(completionSource, result);
            }

            var synchronousResultStatus = new XRResultStatus();
            NativeApi.TryLoadAnchorAsync(savedAnchorGuid, ref synchronousResultStatus);

            if (synchronousResultStatus.IsError())
            {
                var result = new Result<XRAnchor>(synchronousResultStatus, XRAnchor.defaultValue);
                return AwaitableUtils<Result<XRAnchor>>.FromResult(completionSource, result);
            }

            return completionSource.Awaitable;
        }

        [MonoPInvokeCallback(typeof(LoadAsyncDelegate))]
        static void OnLoadAsyncComplete(XRAnchor anchor, XRResultStatus resultStatus)
        {
            if (!s_PendingCompletionSourcesByAnchorId.Remove(anchor.trackableId, out var completionSource))
            {
                Debug.LogError($"An unknown error occurred during a system callback for {nameof(TryLoadAnchorAsync)}.");
                return;
            }

            completionSource.SetResult(new Result<XRAnchor>(resultStatus, anchor));
            completionSource.Reset();
            s_CompletionSourcePool.Release(completionSource);
        }

        static class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_TryLoadAnchorAsync")]
            public static extern void TryLoadAnchorAsync(SerializableGuid anchorSaveId, ref XRResultStatus synchronousResultStatus);
        }
    }
}
