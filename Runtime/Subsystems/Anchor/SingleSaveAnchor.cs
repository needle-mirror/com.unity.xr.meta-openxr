using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Pool;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    static class SingleSaveAnchor
    {
        static readonly Dictionary<TrackableId, AwaitableCompletionSource<Result<SerializableGuid>>>
            s_PendingCompletionSourcesByAnchorId = new();

        static readonly ObjectPool<AwaitableCompletionSource<Result<SerializableGuid>>> s_CompletionSourcePool =
            ObjectPoolCreateUtil.Create<AwaitableCompletionSource<Result<SerializableGuid>>>();

        delegate void SaveAsyncDelegate(TrackableId anchorId, XRResultStatus resultStatus);
        internal static readonly IntPtr saveAsyncCallback = Marshal.GetFunctionPointerForDelegate((SaveAsyncDelegate)OnSaveAsyncComplete);

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

        internal static Awaitable<Result<SerializableGuid>> TrySaveAnchorAsync(TrackableId anchorId)
        {
            var completionSource = s_CompletionSourcePool.Get();
            var wasAddedToMap = s_PendingCompletionSourcesByAnchorId.TryAdd(anchorId, completionSource);

            if (!wasAddedToMap)
            {
                Debug.LogError($"Cannot save anchor with trackableId [{anchorId}] while saving for it is already in progress!");
                var resultStatus = new XRResultStatus(XRResultStatus.StatusCode.UnknownError);
                var result = new Result<SerializableGuid>(resultStatus, default);
                return AwaitableUtils<Result<SerializableGuid>>.FromResult(completionSource, result);
            }

            var synchronousResultStatus = new XRResultStatus();
            NativeApi.TrySaveAnchorAsync(anchorId, ref synchronousResultStatus);

            if (synchronousResultStatus.IsError())
            {
                var result = new Result<SerializableGuid>(synchronousResultStatus, default);
                return AwaitableUtils<Result<SerializableGuid>>.FromResult(completionSource, result);
            }

            return completionSource.Awaitable;
        }

        [MonoPInvokeCallback(typeof(SaveAsyncDelegate))]
        static void OnSaveAsyncComplete(TrackableId anchorId, XRResultStatus resultStatus)
        {
            if (!s_PendingCompletionSourcesByAnchorId.Remove(anchorId, out var completionSource))
            {
                Debug.LogError($"An unknown error occurred during a system callback for {nameof(TrySaveAnchorAsync)}.");
                return;
            }

            completionSource.SetResult(new Result<SerializableGuid>(resultStatus, anchorId));
            completionSource.Reset();
            s_CompletionSourcePool.Release(completionSource);
        }

        static class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_TrySaveAnchorAsync")]
            public static extern void TrySaveAnchorAsync(TrackableId anchorId, ref XRResultStatus synchronousResultStatus);
        }
    }
}
