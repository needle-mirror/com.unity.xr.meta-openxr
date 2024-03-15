using System;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Meta;
using Unity.XR.Management.AndroidManifest.Editor;

namespace UnityEditor.XR.OpenXR.Features.Meta
{
    internal class ModifyAndroidManifestMeta : OpenXRFeatureBuildHooks
    {
        public override int callbackOrder => 1;

        public override Type featureType => typeof(ARSessionFeature);

        protected override void OnPreprocessBuildExt(BuildReport report)
        {
        }

        protected override void OnPostGenerateGradleAndroidProjectExt(string path)
        {
        }

        protected override void OnPostprocessBuildExt(BuildReport report)
        {
        }

        protected override ManifestRequirement ProvideManifestRequirementExt()
        {
            var androidOpenXRSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            var elementsToAdd = new List<ManifestElement>();

            var arCameraFeature = androidOpenXRSettings.GetFeature<ARCameraFeature>();
            if (arCameraFeature != null && arCameraFeature.enabled)
            {
                elementsToAdd.Add(
                    new ManifestElement()
                    {
                        ElementPath = new List<string> { "manifest", "uses-feature" },
                        Attributes = new Dictionary<string, string>
                        {
                            { "name", "com.oculus.feature.PASSTHROUGH" },
                            { "required", "true" },
                        }
                    }
                );
            }

            var arAnchorFeature = androidOpenXRSettings.GetFeature<ARAnchorFeature>();
            if (arAnchorFeature != null && arAnchorFeature.enabled)
            {
                elementsToAdd.Add(
                    new ManifestElement()
                    {
                        ElementPath = new List<string> { "manifest", "uses-permission" },
                        Attributes = new Dictionary<string, string>
                        {
                            { "name", "com.oculus.permission.USE_ANCHOR_API" },
                        }
                    }
                );
            }

            var arPlaneFeature = androidOpenXRSettings.GetFeature<ARPlaneFeature>();
            if (arPlaneFeature != null && arPlaneFeature.enabled)
            {
                elementsToAdd.Add(
                    new ManifestElement()
                    {
                        ElementPath = new List<string> { "manifest", "uses-permission" },
                        Attributes = new Dictionary<string, string>
                        {
                            { "name", "com.oculus.permission.USE_SCENE" },
                        }
                    }
                );
            }

            return new ManifestRequirement
            {
                SupportedXRLoaders = new HashSet<Type>()
                {
                    typeof(OpenXRLoader)
                },
                NewElements = elementsToAdd
            };
        }
    }
}