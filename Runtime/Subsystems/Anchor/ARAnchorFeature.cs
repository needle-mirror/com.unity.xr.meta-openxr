using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// Enables AR Foundation anchor support via OpenXR for Meta Quest devices.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "AR Foundation: Meta Quest Anchor",
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = Constants.k_CompanyName,
        Desc = "AR Foundation anchor support on Meta Quest devices",
        DocumentationLink = Constants.k_DocumentationURL,
        OpenxrExtensionStrings = k_OpenXRRequestedExtensions,
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "0.1.0")]
#endif
    public class ARAnchorFeature : OpenXRFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.arfoundation-meta-anchor";

        /// <summary>
        /// The set of OpenXR spec extension strings to enable, separated by spaces.
        /// For more information, refer to
        /// <see href="https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.6/manual/features.html#enabling-openxr-spec-extension-strings"/>.
        /// </summary>
        const string k_OpenXRRequestedExtensions =
            // (https://developer.oculus.com/documentation/native/android/mobile-scene-api-ref/)
            "XR_FB_spatial_entity " +
            "XR_FB_spatial_entity_query " +
            "XR_FB_spatial_entity_storage ";

        static List<XRAnchorSubsystemDescriptor> s_AnchorDescriptors = new();

        /// <summary>
        /// Instantiates Meta OpenXR Anchor subsystem instance, but does not start it.
        /// (Start/Stop is typically handled by AR Foundation managers.)
        /// </summary>
        protected override void OnSubsystemCreate()
        {
            if (OpenXRRuntime.IsExtensionEnabled("XR_FB_spatial_entity"))
            {
                CreateSubsystem<XRAnchorSubsystemDescriptor, XRAnchorSubsystem>(
                    s_AnchorDescriptors,
                    MetaOpenXRAnchorSubsystem.k_SubsystemId);
            }
        }

        /// <summary>
        /// Destroys the anchor subsystem.
        /// </summary>
        protected override void OnSubsystemDestroy()
        {
            DestroySubsystem<XRAnchorSubsystem>();
        }
    }
}
