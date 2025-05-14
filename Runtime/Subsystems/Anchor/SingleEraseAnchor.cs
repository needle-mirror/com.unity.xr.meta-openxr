using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Pool;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    static class SingleEraseAnchor
    {
        static readonly Dictionary<SerializableGuid, AwaitableCompletionSource<XRResultStatus>> s_PendingCompletionSourcesByAnchorId = new();

        static readonly ObjectPool<AwaitableCompletionSource<XRResultStatus>> s_CompletionSourcePool =
            ObjectPoolCreateUtil.Create<AwaitableCompletionSource<XRResultStatus>>();

        delegate void EraseAsyncDelegate(SerializableGuid savedAnchorGuid, XRResultStatus resultStatus);
        internal static readonly IntPtr eraseAsyncCallback = Marshal.GetFunctionPointerForDelegate((EraseAsyncDelegate)OnEraseAsyncComplete);

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

        internal static Awaitable<XRResultStatus> TryEraseAnchorAsync(SerializableGuid savedAnchorGuid)
        {
            var completionSource = s_CompletionSourcePool.Get();
            var wasAddedToMap = s_PendingCompletionSourcesByAnchorId.TryAdd(savedAnchorGuid, completionSource);

            if (!wasAddedToMap)
            {
                Debug.LogError($"Cannot erase persistent anchor GUID [{savedAnchorGuid}] while erasing for it is already in progress!");
                var resultStatus = new XRResultStatus(XRResultStatus.StatusCode.UnknownError);
                return AwaitableUtils<XRResultStatus>.FromResult(completionSource, resultStatus);
            }

            var synchronousResultStatus = new XRResultStatus();
            NativeApi.TryEraseAnchorAsync(savedAnchorGuid, ref synchronousResultStatus);

            if (synchronousResultStatus.IsError())
                return AwaitableUtils<XRResultStatus>.FromResult(completionSource, synchronousResultStatus);

            return completionSource.Awaitable;
        }

        [MonoPInvokeCallback(typeof(EraseAsyncDelegate))]
        static void OnEraseAsyncComplete(SerializableGuid savedAnchorGuid, XRResultStatus resultStatus)
        {
            if (!s_PendingCompletionSourcesByAnchorId.Remove(savedAnchorGuid, out var completionSource))
            {
                Debug.LogError($"An unknown error occurred during a system callback for {nameof(TryEraseAnchorAsync)}.");
                return;
            }

            completionSource.SetResult(resultStatus);
            completionSource.Reset();
            s_CompletionSourcePool.Release(completionSource);
        }

        static class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_TryEraseAnchorAsync")]
            public static extern void TryEraseAnchorAsync(SerializableGuid anchorSaveId, ref XRResultStatus synchronousResultStatus);
        }
    }
}
