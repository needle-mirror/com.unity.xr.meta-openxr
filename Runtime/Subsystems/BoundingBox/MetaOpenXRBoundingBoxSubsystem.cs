using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// The Meta-OpenXR implementation of <see cref="XRBoundingBoxSubsystem"/>, built with the Meta OpenXR Mobile SDK.
    /// Bounding boxes are provided based on bounded3d components present in your
    /// [Scene Model](https://developer.oculus.com/documentation/native/android/openxr-scene-overview#scene-model).
    /// </summary>
    [Preserve]
    public sealed class MetaOpenXRBoundingBoxSubsystem : XRBoundingBoxSubsystem
    {
        internal const string k_SubsystemId = "MetaOpenXR-BoundingBox";

        class MetaOpenXRBoundingBoxProvider : Provider
        {
            /// <inheritdoc/>
            protected override bool TryInitialize()
            {
                if (OpenXRRuntime.IsExtensionEnabled(Constants.OpenXRExtensions.k_XR_FB_spatial_entity) &&
                    OpenXRRuntime.IsExtensionEnabled(Constants.OpenXRExtensions.k_XR_FB_spatial_entity_query) &&
                    OpenXRRuntime.IsExtensionEnabled(Constants.OpenXRExtensions.k_XR_FB_spatial_entity_storage) &&
                    OpenXRRuntime.IsExtensionEnabled(Constants.OpenXRExtensions.k_XR_FB_scene))
                {
                    NativeApi.Create();
                    return true;
                }

                return false;
            }

            /// <inheritdoc/>
            public override void Start()
            {
                PermissionsUtility.RequestPlatformPermissions(k_SubsystemId);
                NativeApi.Start();
            }

            /// <inheritdoc/>
            public override void Stop() { }

            /// <inheritdoc/>
            public override void Destroy() => NativeApi.Destroy();

            /// <inheritdoc/>
            public override unsafe TrackableChanges<XRBoundingBox> GetChanges(XRBoundingBox defaultXRBoundingBox, Allocator allocator)
            {
                NativeApi.GetChanges(
                    out var addedPtr, out var addedCount,
                    out var updatedPtr, out var updatedCount,
                    out var removedPtr, out var removedCount,
                    out var elementSize);

                try
                {
                    return new TrackableChanges<XRBoundingBox>(
                        addedPtr, addedCount,
                        updatedPtr, updatedCount,
                        removedPtr, removedCount,
                        defaultXRBoundingBox, elementSize,
                        allocator);
                }
                finally
                {
                    NativeApi.ReleaseChanges();
                }
            }

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void RegisterDescriptor()
            {
                XRBoundingBoxSubsystemDescriptor.Register(new XRBoundingBoxSubsystemDescriptor.Cinfo
                {
                    id = k_SubsystemId,
                    providerType = typeof(MetaOpenXRBoundingBoxProvider),
                    subsystemTypeOverride = typeof(MetaOpenXRBoundingBoxSubsystem),
                    supportsClassification = true,
                });
            }
            static class NativeApi
            {
                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRMeta_BoundingBox_Create")]
                public static extern void Create();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRMeta_BoundingBox_Start")]
                public static extern void Start();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRMeta_BoundingBox_Destroy")]
                public static extern void Destroy();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRMeta_BoundingBox_GetChanges")]
                public static extern unsafe void GetChanges(
                    out void* addedPtr, out int addedCount,
                    out void* updatedPtr, out int updatedCount,
                    out void* removedPtr, out int removedCount,
                    out int elementSize);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRMeta_BoundingBox_ReleaseChanges")]
                public static extern void ReleaseChanges();
            }
        }
    }
}
