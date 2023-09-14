using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// Enables AR Foundation passthrough support via OpenXR for Meta Quest devices.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "AR Foundation: Meta Quest Camera",
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = Constants.k_CompanyName,
        Desc = "AR Foundation camera support on Meta Quest devices",
        DocumentationLink = Constants.k_DocumentationURL,
        OpenxrExtensionStrings = k_OpenXRRequestedExtensions,
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "0.1.0")]
#endif
    public class ARCameraFeature : OpenXRFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.arfoundation-meta-camera";

        /// <summary>
        /// The set of OpenXR spec extension strings to enable, separated by spaces.
        /// For more information, refer to
        /// <see href="https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.6/manual/features.html#enabling-openxr-spec-extension-strings"/>.
        /// </summary>
        const string k_OpenXRRequestedExtensions = "XR_FB_passthrough XR_FB_composition_layer_alpha_blend";

        static List<XRCameraSubsystemDescriptor> s_CameraDescriptors = new();

        /// <summary>
        /// Instantiates Meta OpenXR Camera subsystem instances, but does not start it.
        /// (Start/Stop is typically handled by AR Foundation managers.)
        /// </summary>
        protected override void OnSubsystemCreate()
        {
            if (OpenXRRuntime.IsExtensionEnabled("XR_FB_passthrough"))
            {
                CreateSubsystem<XRCameraSubsystemDescriptor, XRCameraSubsystem>(
                    s_CameraDescriptors,
                    MetaOpenXRCameraSubsystem.k_SubsystemId);
            }
        }

        /// <summary>
        /// Destroys the camera subsystem.
        /// </summary>
        protected override void OnSubsystemDestroy()
        {
            DestroySubsystem<XRCameraSubsystem>();
        }
    }
}
