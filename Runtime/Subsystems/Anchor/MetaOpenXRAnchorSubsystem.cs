using System;
using System.Runtime.InteropServices;
using System.Threading;
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
            protected override bool TryInitialize()
            {
                NativeApi.Create(
                    AddAnchor.addAsyncCallback,
                    SingleSaveAnchor.saveAsyncCallback,
                    SingleLoadAnchor.loadAsyncCallback,
                    SingleEraseAnchor.eraseAsyncCallback);
                return true;
            }

            public override void Start() { }
            public override void Stop() { }

            public override void Destroy()
            {
                AddAnchor.CancelAllRequests();
                SingleSaveAnchor.CancelAllRequests();
                SingleLoadAnchor.CancelAllRequests();
                SingleEraseAnchor.CancelAllRequests();
                NativeApi.Destroy();
            }

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

            public override Awaitable<Result<XRAnchor>> TryAddAnchorAsync(Pose pose)
                => AddAnchor.TryAddAnchorAsync(pose);

            public override Awaitable<Result<SerializableGuid>> TrySaveAnchorAsync(
                TrackableId anchorId, CancellationToken _ = default)
                => SingleSaveAnchor.TrySaveAnchorAsync(anchorId);

            public override bool TryRemoveAnchor(TrackableId anchorId)
                => NativeApi.TryRemoveAnchor(anchorId);

            public override Awaitable<Result<XRAnchor>> TryLoadAnchorAsync(
                SerializableGuid savedAnchorGuid, CancellationToken cancellationToken = default)
                => SingleLoadAnchor.TryLoadAnchorAsync(savedAnchorGuid);

            public override Awaitable<XRResultStatus> TryEraseAnchorAsync(
                SerializableGuid savedAnchorGuid, CancellationToken cancellationToken = default)
                => SingleEraseAnchor.TryEraseAnchorAsync(savedAnchorGuid);

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void RegisterDescriptor()
            {
                var anchorSubsystemCinfo = new XRAnchorSubsystemDescriptor.Cinfo
                {
                    id = k_SubsystemId,
                    providerType = typeof(MetaOpenXRAnchorProvider),
                    subsystemTypeOverride = typeof(MetaOpenXRAnchorSubsystem),
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

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_TryRemoveAnchor")]
                public static extern bool TryRemoveAnchor(TrackableId anchorId);
            }
        }
    }
}
