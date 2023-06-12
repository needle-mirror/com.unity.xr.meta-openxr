using UnityEngine.XR.OpenXR.Features.Meta;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;

namespace UnityEditor.XR.OpenXR.Features.Meta
{
    [OpenXRFeatureSet(
        FeatureIds = new[]
        {
            ARFoundationFeature.featureId,
            MetaQuestFeature.featureId,
        },
        DefaultFeatureIds = new[]
        {
            ARFoundationFeature.featureId,
            MetaQuestFeature.featureId,
        },
        RequiredFeatureIds = new[]
        {
            MetaQuestFeature.featureId,
        },
        UiName = "Meta Quest",
        FeatureSetId = featureSetId,
        SupportedBuildTargets = new[] { BuildTargetGroup.Android }
    )]
    class MetaFeatureSet {
        const string featureSetId = "com.unity.openxr.featureset.meta";
    }
}
