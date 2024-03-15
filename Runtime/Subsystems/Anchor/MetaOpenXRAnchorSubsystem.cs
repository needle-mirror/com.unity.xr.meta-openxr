using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

            static readonly UnityEngine.Pool.ObjectPool<AwaitableCompletionSource<Result<XRAnchor>>> s_AddAsyncCompletionSources = new(
                createFunc: () => new AwaitableCompletionSource<Result<XRAnchor>>(),
                actionOnGet: null,
                actionOnRelease: null,
                actionOnDestroy: null,
                collectionCheck: false,
                defaultCapacity: 8,
                maxSize: 1024);

            protected override bool TryInitialize()
            {
                if (!OpenXRRuntime.IsExtensionEnabled(Constants.OpenXRExtensions.k_XR_FB_spatial_entity))
                    return false;

                NativeApi.Create(s_AddAsyncCallback);
                return true;
            }

            public override void Start()
            { }

            public override void Stop()
            { }

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
                s_AddAsyncPendingRequests.Add(requestId, completionSource);
                NativeApi.TryAddAnchorAsync(requestId, pose);
                return completionSource.Awaitable;
            }

            public override bool TryRemoveAnchor(TrackableId anchorId)
                => NativeApi.TryRemoveAnchor(anchorId);

            /// <summary>
            /// Function pointer marshalled to native API to call when <see cref="TryAddAnchorAsync"/> is complete.
            /// </summary>
            static readonly IntPtr s_AddAsyncCallback = Marshal.GetFunctionPointerForDelegate((AddAsyncDelegate)OnAddAsyncComplete);

            /// <summary>
            /// Delegate method type for <see cref="MetaOpenXRAnchorProvider.s_AddAsyncCallback"/>.
            /// </summary>
            delegate void AddAsyncDelegate(Guid requestId, int xrResult, XRAnchor anchor);

            [MonoPInvokeCallback(typeof(AddAsyncDelegate))]
            static void OnAddAsyncComplete(Guid requestId, int xrResult, XRAnchor anchor)
            {
                if (!s_AddAsyncPendingRequests.Remove(requestId, out var completionSource))
                {
                    Debug.LogError($"An unknown error occurred during a system callback for {nameof(TryAddAnchorAsync)}.");
                    return;
                }

                completionSource.SetResult(new Result<XRAnchor>(new XRResultStatus(xrResult), anchor));
                completionSource.Reset();
                s_AddAsyncCompletionSources.Release(completionSource);
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
                };

                XRAnchorSubsystemDescriptor.Register(anchorSubsystemCinfo);
            }

            static unsafe class NativeApi
            {
                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_Create")]
                public static extern void Create(IntPtr tryAddAnchorAsyncCallback);

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
                public static extern void TryAddAnchorAsync(Guid requestId, Pose pose);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_TryRemoveAnchor")]
                public static extern bool TryRemoveAnchor(TrackableId anchorId);
            }
        }
    }
}
