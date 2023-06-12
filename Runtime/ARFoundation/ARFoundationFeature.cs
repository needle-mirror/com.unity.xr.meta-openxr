using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.XR.ARSubsystems;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// Enables AR Foundation support via OpenXR for Meta devices.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "AR Foundation: Meta Quest Features",
        BuildTargetGroups = new[] {BuildTargetGroup.Android },
        Company = "Unity Technologies",
        Desc = "AR Foundation support on Meta devices",
        DocumentationLink = Constants.k_DocumentationURL,
        OpenxrExtensionStrings = openxrRequestedExtensions,
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "0.1.0")]
#endif
    public class ARFoundationFeature : OpenXRFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.arfoundation-meta";

        /// <summary>
        /// The set of OpenXR spec extension strings to enable, separated by spaces.
        /// For more information, see
        /// <see href="https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.6/manual/features.html#enabling-openxr-spec-extension-strings"/>.
        /// </summary>
        const string openxrRequestedExtensions =
            // Passthrough
            "XR_FB_passthrough " +
            // Plane (https://developer.oculus.com/documentation/native/android/mobile-scene-api-ref/)
            "XR_FB_spatial_entity " +
            "XR_FB_spatial_entity_query " +
            "XR_FB_spatial_entity_storage " +
            "XR_FB_scene ";

        static List<XRSessionSubsystemDescriptor> s_SessionDescriptors = new();
        static List<XRCameraSubsystemDescriptor> s_CameraDescriptors = new();
        static List<XRAnchorSubsystemDescriptor> s_AnchorDescriptors = new();
        static List<XRPlaneSubsystemDescriptor> s_PlaneDescriptors = new();
        static List<XRRaycastSubsystemDescriptor> s_RaycastDescriptors = new();

        /// <summary>
        /// Called to hook xrGetInstanceProcAddr.
        /// Returning a different function pointer allows intercepting any OpenXR method.
        /// </summary>
        /// <param name="func">xrGetInstanceProcAddr native function pointer</param>
        /// <returns>Function pointer that Unity will use to look up OpenXR native functions.</returns>
        protected override IntPtr HookGetInstanceProcAddr(IntPtr func)
            => NativeApi.UnityOpenXRMeta_InterceptXrGetInstanceProcAddr(func);

        /// <summary>
        /// Called after xrGetSystem.
        /// </summary>
        /// <param name="xrSystem">Handle of the xrSystemId</param>
        protected override void OnSystemChange(ulong xrSystem)
        {
            base.OnSystemChange(xrSystem);
            NativeApi.UnityOpenXRMeta_OnSystemChange(xrSystem);
        }

        /// <summary>
        /// Called after xrCreateInstance.
        /// </summary>
        /// <param name="xrInstance">Handle of the xrInstance</param>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            return base.OnInstanceCreate(xrInstance)
                    && NativeApi.UnityOpenXRMeta_OnInstanceCreate(
                        xrInstance,
                        xrGetInstanceProcAddr);
        }

        /// <summary>
        /// Called before xrDestroyInstance
        /// </summary>
        /// <param name="xrInstance">Handle of the xrInstance</param>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            base.OnInstanceDestroy(xrInstance);
            NativeApi.UnityOpenXRMeta_OnInstanceDestroy(xrInstance);
        }

        /// <summary>
        /// Called after xrCreateSession.
        /// </summary>
        /// <param name="xrSession">Handle of the xrSession</param>
        protected override void OnSessionCreate(ulong xrSession)
        {
            base.OnSessionCreate(xrSession);
            NativeApi.UnityOpenXRMeta_OnSessionCreate(xrSession);
        }

        /// <summary>
        /// Called before xrDestroySession.
        /// </summary>
        /// <param name="xrSession">Handle of the xrSession</param>
        protected override void OnSessionDestroy(ulong xrSession)
        {
            base.OnSessionDestroy(xrSession);
            NativeApi.UnityOpenXRMeta_OnSessionDestroy(xrSession);
        }

        /// <summary>
        /// Instantiates all Meta OpenXR subsystem instances, but does not start them.
        /// (Start/Stop is not a Loader's responsibility and is typically handled by AR Foundation managers.)
        /// </summary>
        protected override void OnSubsystemCreate()
        {
            CreateSubsystem<XRSessionSubsystemDescriptor, XRSessionSubsystem>(
                s_SessionDescriptors,
                MetaOpenXRSessionSubsystem.k_SubsystemId);

            if (OpenXRRuntime.IsExtensionEnabled("XR_FB_passthrough"))
            {
                CreateSubsystem<XRCameraSubsystemDescriptor, XRCameraSubsystem>(
                    s_CameraDescriptors,
                    MetaOpenXRCameraSubsystem.k_SubsystemId);
            }

            if (OpenXRRuntime.IsExtensionEnabled("XR_FB_spatial_entity"))
            {
                CreateSubsystem<XRAnchorSubsystemDescriptor, XRAnchorSubsystem>(
                    s_AnchorDescriptors,
                    MetaOpenXRAnchorSubsystem.k_SubsystemId);

                CreateSubsystem<XRPlaneSubsystemDescriptor, XRPlaneSubsystem>(
                    s_PlaneDescriptors,
                    MetaOpenXRPlaneSubsystem.k_SubsystemId);

                CreateSubsystem<XRRaycastSubsystemDescriptor, XRRaycastSubsystem>(
                    s_RaycastDescriptors,
                    MetaOpenXRRaycastSubsystem.k_SubsystemId);
            }
        }

        /// <summary>
        /// This only destroy subsystems since the stop of the
        /// subsystems is controlled by AR Foundation managers
        /// </summary>
        protected override void OnSubsystemDestroy()
        {
            DestroySubsystem<XRRaycastSubsystem>();
            DestroySubsystem<XRPlaneSubsystem>();
            DestroySubsystem<XRAnchorSubsystem>();
            DestroySubsystem<XRCameraSubsystem>();
            DestroySubsystem<XRSessionSubsystem>();
        }

        /// <summary>
        /// Called when the reference xrSpace for the app changes.
        /// </summary>
        /// <param name="xrSpace">Handle of the xrSpace</param>
        protected override void OnAppSpaceChange(ulong xrSpace) => NativeApi.UnityOpenXRMeta_OnAppSpaceChange(xrSpace);

        static class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern IntPtr UnityOpenXRMeta_InterceptXrGetInstanceProcAddr(IntPtr func);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern void UnityOpenXRMeta_OnSystemChange(ulong xrSystem);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern bool UnityOpenXRMeta_OnInstanceCreate(
                ulong xrInstance,
                IntPtr xrGetInstanceProcAddr);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern void UnityOpenXRMeta_OnInstanceDestroy(ulong xrInstance);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern void UnityOpenXRMeta_OnSessionCreate(ulong xrSession);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern void UnityOpenXRMeta_OnSessionDestroy(ulong xrSession);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern void UnityOpenXRMeta_OnAppSpaceChange(ulong xrSpace);
        }
    }
}
