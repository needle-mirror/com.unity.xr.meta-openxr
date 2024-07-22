using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// Enables AR Foundation meshing support via OpenXR for Meta Quest devices.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "Meta Quest: Meshing",
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = Constants.k_CompanyName,
        Desc = "AR Foundation mesh support on Meta Quest devices",
        DocumentationLink = Constants.DocsUrls.k_MeshUrl,
        OpenxrExtensionStrings = k_OpenXRRequestedExtensions,
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "0.1.0")]
#endif
    public class ARMeshFeature : OpenXRFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.arfoundation-meta-mesh";

        const string k_SubsystemId = "MetaOpenXR-Meshing";

        /// <summary>
        /// The set of OpenXR spec extension strings to enable, separated by spaces.
        /// For more information, refer to
        /// <see href="https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.6/manual/features.html#enabling-openxr-spec-extension-strings"/>.
        /// </summary>
        /// <seeaslo href="https://developer.oculus.com/documentation/native/android/mobile-scene-api-ref/"/>
        const string k_OpenXRRequestedExtensions =
            Constants.OpenXRExtensions.k_XR_FB_spatial_entity + " " +
            Constants.OpenXRExtensions.k_XR_FB_scene + " " +
            Constants.OpenXRExtensions.k_XR_META_spatial_entity_mesh + " " +
            Constants.OpenXRExtensions.k_XR_META_spatial_entity_discovery;

        static List<XRMeshSubsystemDescriptor> s_MeshDescriptors = new();

        /// <summary>
        /// Instantiates Meta OpenXR mesh subsystem instance, but does not start it.
        /// (Start/Stop is typically handled by AR Foundation managers.)
        /// </summary>
        protected override void OnSubsystemCreate()
        {
            CreateSubsystem<XRMeshSubsystemDescriptor, XRMeshSubsystem>(
                s_MeshDescriptors,
                k_SubsystemId);
        }

        /// <summary>
        /// Destroys the mesh subsystem.
        /// </summary>
        protected override void OnSubsystemDestroy()
        {
            DestroySubsystem<XRMeshSubsystem>();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Validation Rules for ARMeshFeature.
        /// </summary>
        protected override void GetValidationChecks(List<ValidationRule> rules, BuildTargetGroup targetGroup)
        {
            rules.AddRange(SharedValidationRules.EnableARSessionValidationRules(this));
        }
#endif
    }
}
