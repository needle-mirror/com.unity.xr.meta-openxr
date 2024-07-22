using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using AOT;
using Unity.Collections;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// The Meta-OpenXR implementation of the <see cref="XRAnchorSubsystem"/>.
    /// Do not create this directly. Use the <see cref="SubsystemManager"/> instead.
    /// </summary>
    [Preserve]
    public sealed class MetaOpenXRAnchorSubsystem : XRAnchorSubsystem
    {
        internal const string k_SubsystemId = "Meta-Anchor";

        class MetaOpenXRAnchorProvider : Provider
        {
            static readonly Dictionary<Guid, AwaitableCompletionSource<Result<XRAnchor>>> s_AddAsyncPendingRequests = new();

            static readonly Dictionary<TrackableId, AwaitableCompletionSource<Result<SerializableGuid>>> s_SaveAsyncPendingRequests = new();

            static readonly Dictionary<SerializableGuid, AwaitableCompletionSource<Result<XRAnchor>>> s_LoadAsyncPendingRequests = new();

            static readonly Dictionary<SerializableGuid, AwaitableCompletionSource<XRResultStatus>> s_EraseAsyncPendingRequests = new();

            static readonly Pool.ObjectPool<AwaitableCompletionSource<Result<XRAnchor>>> s_AddAsyncCompletionSources = new(
                    createFunc: () => new AwaitableCompletionSource<Result<XRAnchor>>(),
                    actionOnGet: null,
                    actionOnRelease: null,
                    actionOnDestroy: null,
                    collectionCheck: false,
                    defaultCapacity: 8,
                    maxSize: 1024);

            static readonly Pool.ObjectPool<AwaitableCompletionSource<Result<SerializableGuid>>> s_SaveAsyncCompletionSources = new(
                    createFunc: () => new AwaitableCompletionSource<Result<SerializableGuid>>(),
                    actionOnGet: null,
                    actionOnRelease: null,
                    actionOnDestroy: null,
                    collectionCheck: false,
                    defaultCapacity: 8,
                    maxSize: 1024);

            static readonly Pool.ObjectPool<AwaitableCompletionSource<Result<XRAnchor>>> s_LoadAsyncCompletionSources = new(
                createFunc: () => new AwaitableCompletionSource<Result<XRAnchor>>(),
                actionOnGet: null,
                actionOnRelease: null,
                actionOnDestroy: null,
                collectionCheck: false,
                defaultCapacity: 8,
                maxSize: 1024);

            static readonly Pool.ObjectPool<AwaitableCompletionSource<XRResultStatus>> s_EraseAsyncCompletionSources = new(
                createFunc: () => new AwaitableCompletionSource<XRResultStatus>(),
                actionOnGet: null,
                actionOnRelease: null,
                actionOnDestroy: null,
                collectionCheck: false,
                defaultCapacity: 8,
                maxSize: 1024);

            protected override bool TryInitialize()
            {
                if (OpenXRRuntime.IsExtensionEnabled(Constants.OpenXRExtensions.k_XR_FB_spatial_entity) &&
                    OpenXRRuntime.IsExtensionEnabled(Constants.OpenXRExtensions.k_XR_META_spatial_entity_discovery) &&
                    OpenXRRuntime.IsExtensionEnabled(Constants.OpenXRExtensions.k_XR_META_spatial_entity_persistence))
                {
                    NativeApi.Create(
                        s_AddAsyncCallback,
                        s_SaveAsyncCallback,
                        s_LoadAsyncCallback,
                        s_EraseAsyncCallback);
                    return true;
                }

                return false;
            }

            public override void Start() => NativeApi.Start();

            public override void Stop() { }

            public override void Destroy() => NativeApi.Destroy();

            public override unsafe TrackableChanges<XRAnchor> GetChanges(XRAnchor defaultAnchor, Allocator allocator)
            {
                int addedCount = 0, updatedCount = 0, removedCount = 0, elementSize = 0;
                void* addedPtr = null, updatedPtr = null, removedPtr = null;
                NativeApi.AcquireChanges(
                    ref addedPtr, ref addedCount,
                    ref updatedPtr, ref updatedCount,
                    ref removedPtr, ref removedCount,
                    ref elementSize);

                try
                {
                    return new TrackableChanges<XRAnchor>(
                        addedPtr, addedCount,
                        updatedPtr, updatedCount,
                        removedPtr, removedCount,
                        defaultAnchor, elementSize,
                        allocator);
                }
                finally
                {
                    NativeApi.ReleaseChanges();
                }
            }

            /// <summary>
            /// Attempts to create an anchor at the given <paramref name="pose"/>.
            /// </summary>
            /// <param name="pose">The pose, in session space, of the anchor.</param>
            /// <returns>The result of the async operation.</returns>
            public override Awaitable<Result<XRAnchor>> TryAddAnchorAsync(Pose pose)
            {
                var requestId = Guid.NewGuid();
                var completionSource = s_AddAsyncCompletionSources.Get();
                var wasAddedToMap = s_AddAsyncPendingRequests.TryAdd(requestId, completionSource);

                if (!wasAddedToMap)
                {
                    var resultStatus = new XRResultStatus(XRResultStatus.StatusCode.UnknownError);
                    var result = new Result<XRAnchor>(resultStatus, XRAnchor.defaultValue);
                    return AwaitableUtils<Result<XRAnchor>>.FromResult(completionSource, result);
                }

                var synchronousResultStatus = new XRResultStatus();
                NativeApi.TryAddAnchorAsync(requestId, pose, ref synchronousResultStatus);

                if (synchronousResultStatus.IsError())
                {
                    var result = new Result<XRAnchor>(synchronousResultStatus, XRAnchor.defaultValue);
                    return AwaitableUtils<Result<XRAnchor>>.FromResult(completionSource, result);
                }

                return completionSource.Awaitable;
            }

            public override bool TryRemoveAnchor(TrackableId anchorId)
                => NativeApi.TryRemoveAnchor(anchorId);

            /// <summary>
            /// Attempts to persistently save the given anchor so that it can be loaded in a future AR session. Use the
            /// `SerializableGuid` returned by this method as an input parameter to <see cref="TryLoadAnchorAsync"/> or
            /// <see cref="TryEraseAnchorAsync"/>.
            /// </summary>
            /// <param name="anchorId">The TrackableId of the anchor to save.</param>
            /// <param name="cancellationToken">An optional `CancellationToken` that you can use to cancel the operation
            /// in progress if the loaded provider <see cref="XRAnchorSubsystemDescriptor.supportsAsyncCancellation"/>.</param>
            /// <returns>The result of the async operation, containing a new persistent anchor GUID if the operation
            /// succeeded. You are responsible to <see langword="await"/> this result.</returns>
            /// <seealso cref="XRAnchorSubsystemDescriptor.supportsSaveAnchor"/>
            public override Awaitable<Result<SerializableGuid>> TrySaveAnchorAsync(
                TrackableId anchorId, CancellationToken cancellationToken = default)
            {
                var completionSource = s_SaveAsyncCompletionSources.Get();
                var wasAddedToMap = s_SaveAsyncPendingRequests.TryAdd(anchorId, completionSource);

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

            /// <summary>
            /// Attempts to load an anchor given its persistent anchor GUID.
            /// </summary>
            /// <param name="savedAnchorGuid">A persistent anchor GUID created by <see cref="TrySaveAnchorAsync"/>.</param>
            /// <param name="cancellationToken">An optional `CancellationToken` that you can use to cancel the operation
            /// in progress if the loaded provider <see cref="XRAnchorSubsystemDescriptor.supportsAsyncCancellation"/>.</param>
            /// <returns>The result of the async operation, containing the newly added anchor if the operation succeeded.
            /// You are responsible to <see langword="await"/> this result.</returns>
            /// <seealso cref="XRAnchorSubsystemDescriptor.supportsLoadAnchor"/>
            public override Awaitable<Result<XRAnchor>> TryLoadAnchorAsync(
                SerializableGuid savedAnchorGuid, CancellationToken cancellationToken = default)
            {
                var completionSource = s_LoadAsyncCompletionSources.Get();
                var wasAddedToMap = s_LoadAsyncPendingRequests.TryAdd(savedAnchorGuid, completionSource);

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

            /// <summary>
            /// Attempts to erase the persistent saved data associated with an anchor given its persistent anchor GUID.
            /// </summary>
            /// <param name="savedAnchorGuid">A persistent anchor GUID created by <see cref="TrySaveAnchorAsync"/>.</param>
            /// <param name="cancellationToken">An optional `CancellationToken` that you can use to cancel the operation
            /// in progress if the loaded provider <see cref="XRAnchorSubsystemDescriptor.supportsAsyncCancellation"/>.</param>
            /// <returns>The result of the async operation. You are responsible to <see langword="await"/> this result.</returns>
            /// <seealso cref="XRAnchorSubsystemDescriptor.supportsEraseAnchor"/>
            public override Awaitable<XRResultStatus> TryEraseAnchorAsync(
                SerializableGuid savedAnchorGuid, CancellationToken cancellationToken = default)
            {
                var completionSource = s_EraseAsyncCompletionSources.Get();
                var wasAddedToMap = s_EraseAsyncPendingRequests.TryAdd(savedAnchorGuid, completionSource);

                if (!wasAddedToMap)
                {
                    Debug.LogError($"Cannot erase persistent anchor GUID [{savedAnchorGuid}] while erasing for it is already in progress!");
                    var resultStatus = new XRResultStatus(XRResultStatus.StatusCode.UnknownError);
                    return AwaitableUtils<XRResultStatus>.FromResult(completionSource, resultStatus);
                }

                var synchronousResultStatus = new XRResultStatus();
                NativeApi.TryEraseAnchorAsync(savedAnchorGuid, ref synchronousResultStatus);

                if (synchronousResultStatus.IsError())
                {
                    return AwaitableUtils<XRResultStatus>.FromResult(completionSource, synchronousResultStatus);
                }

                return completionSource.Awaitable;
            }

            /// <summary>
            /// Function pointer marshalled to native API to call when <see cref="TryAddAnchorAsync"/> is complete.
            /// </summary>
            static readonly IntPtr s_AddAsyncCallback = Marshal.GetFunctionPointerForDelegate((AddAsyncDelegate)OnAddAsyncComplete);

            /// <summary>
            /// Function pointer marshalled to native API to call when <see cref="TrySaveAnchorAsync"/> is complete.
            /// </summary>
            static readonly IntPtr s_SaveAsyncCallback = Marshal.GetFunctionPointerForDelegate((SaveAsyncDelegate)OnSaveAsyncComplete);

            /// <summary>
            /// Function pointer marshalled to native API to call when <see cref="TryLoadAnchorAsync"/> is complete.
            /// </summary>
            static readonly IntPtr s_LoadAsyncCallback = Marshal.GetFunctionPointerForDelegate((LoadAsyncDelegate)OnLoadAsyncComplete);

            /// <summary>
            /// Function pointer marshalled to native API to call when <see cref="TryEraseAnchorAsync"/> is complete.
            /// </summary>
            static readonly IntPtr s_EraseAsyncCallback = Marshal.GetFunctionPointerForDelegate((EraseAsyncDelegate)OnEraseAsyncComplete);

            /// <summary>
            /// Delegate method type for <see cref="MetaOpenXRAnchorProvider.s_AddAsyncCallback"/>.
            /// </summary>
            delegate void AddAsyncDelegate(Guid requestId, XRResultStatus resultStatus, XRAnchor anchor);

            /// <summary>
            /// Delegate method type for <see cref="MetaOpenXRAnchorProvider.s_SaveAsyncCallback"/>.
            /// </summary>
            delegate void SaveAsyncDelegate(TrackableId anchorId, XRResultStatus resultStatus);

            /// <summary>
            /// Delegate method type for <see cref="MetaOpenXRAnchorProvider.s_LoadAsyncCallback"/>.
            /// </summary>
            delegate void LoadAsyncDelegate(XRAnchor anchor, XRResultStatus resultStatus);

            /// <summary>
            /// Delegate method type for <see cref="MetaOpenXRAnchorProvider.s_EraseAsyncCallback"/>.
            /// </summary>
            delegate void EraseAsyncDelegate(SerializableGuid savedAnchorGuid, XRResultStatus resultStatus);

            [MonoPInvokeCallback(typeof(AddAsyncDelegate))]
            static void OnAddAsyncComplete(Guid requestId, XRResultStatus resultStatus, XRAnchor anchor)
            {
                if (!s_AddAsyncPendingRequests.Remove(requestId, out var completionSource))
                {
                    Debug.LogError($"An unknown error occurred during a system callback for {nameof(TryAddAnchorAsync)}.");
                    return;
                }

                completionSource.SetResult(new Result<XRAnchor>(resultStatus, anchor));
                completionSource.Reset();
                s_AddAsyncCompletionSources.Release(completionSource);
            }

            [MonoPInvokeCallback(typeof(SaveAsyncDelegate))]
            static void OnSaveAsyncComplete(TrackableId anchorId, XRResultStatus resultStatus)
            {
                if (!s_SaveAsyncPendingRequests.Remove(anchorId, out var completionSource))
                {
                    Debug.LogError($"An unknown error occurred during a system callback for {nameof(TrySaveAnchorAsync)}.");
                    return;
                }

                completionSource.SetResult(new Result<SerializableGuid>(resultStatus, anchorId));
                completionSource.Reset();
                s_SaveAsyncCompletionSources.Release(completionSource);
            }

            [MonoPInvokeCallback(typeof(LoadAsyncDelegate))]
            static void OnLoadAsyncComplete(XRAnchor anchor, XRResultStatus resultStatus)
            {
                if (!s_LoadAsyncPendingRequests.Remove(anchor.trackableId, out var completionSource))
                {
                    Debug.LogError($"An unknown error occurred during a system callback for {nameof(TryLoadAnchorAsync)}.");
                    return;
                }

                completionSource.SetResult(new Result<XRAnchor>(resultStatus, anchor));
                completionSource.Reset();
                s_LoadAsyncCompletionSources.Release(completionSource);
            }

            [MonoPInvokeCallback(typeof(EraseAsyncDelegate))]
            static void OnEraseAsyncComplete(SerializableGuid savedAnchorGuid, XRResultStatus resultStatus)
            {
                if (!s_EraseAsyncPendingRequests.Remove(savedAnchorGuid, out var completionSource))
                {
                    Debug.LogError($"An unknown error occurred during a system callback for {nameof(TryEraseAnchorAsync)}.");
                    return;
                }

                completionSource.SetResult(resultStatus);
                completionSource.Reset();
                s_EraseAsyncCompletionSources.Release(completionSource);
            }

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void RegisterDescriptor()
            {
                var anchorSubsystemCinfo = new XRAnchorSubsystemDescriptor.Cinfo
                {
                    id = k_SubsystemId,
                    providerType = typeof(MetaOpenXRAnchorProvider),
                    subsystemTypeOverride = null,
                    supportsTrackableAttachments = false,
                    supportsSynchronousAdd = false,
                    supportsSaveAnchor = true,
                    supportsLoadAnchor = true,
                    supportsEraseAnchor = true,
                    supportsGetSavedAnchorIds = false,
                    supportsAsyncCancellation = false,
                };

                XRAnchorSubsystemDescriptor.Register(anchorSubsystemCinfo);
            }

            static unsafe class NativeApi
            {
                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_Create")]
                public static extern void Create(
                    IntPtr tryAddAnchorAsyncCallback,
                    IntPtr trySaveAnchorAsyncCallback,
                    IntPtr tryLoadAnchorAsyncCallback,
                    IntPtr tryEraseAnchorAsyncCallback);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_Start")]
                public static extern void Start();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_Destroy")]
                public static extern void Destroy();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchors_AcquireChanges")]
                public static extern void AcquireChanges(
                    ref void* addedPtr, ref int addedCount,
                    ref void* updatedPtr, ref int updatedCount,
                    ref void* removedPtr, ref int removedCount,
                    ref int elementSize);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_ReleaseChanges")]
                public static extern void ReleaseChanges();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_TryAddAnchorAsync")]
                public static extern void TryAddAnchorAsync(Guid requestId, Pose pose, ref XRResultStatus synchronousResultStatus);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_TryRemoveAnchor")]
                public static extern bool TryRemoveAnchor(TrackableId anchorId);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_TrySaveAnchorAsync")]
                public static extern void TrySaveAnchorAsync(TrackableId anchorId, ref XRResultStatus synchronousResultStatus);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_TryLoadAnchorAsync")]
                public static extern void TryLoadAnchorAsync(SerializableGuid anchorSaveId, ref XRResultStatus synchronousResultStatus);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_TryEraseAnchorAsync")]
                public static extern void TryEraseAnchorAsync(SerializableGuid anchorSaveId, ref XRResultStatus synchronousResultStatus);
            }
        }
    }
}
