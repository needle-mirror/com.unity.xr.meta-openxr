using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using static UnityEngine.XR.OpenXR.Features.Meta.Constants.OpenXRExtensions;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// Enable this feature to expose per-eye gaze data from <c>XR_FB_eye_tracking_social</c>
    /// as a <see cref="MetaSocialEyeGazeDevice"/> in the Unity Input System.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(
        UiName = displayName,
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.Android },
        Company = Constants.k_CompanyName,
        Desc = "Exposes per-eye gaze data from XR_FB_eye_tracking_social as a Unity Input Device.",
        DocumentationLink = Constants.DocsUrls.k_SocialEyeGazeUrl,
        OpenxrExtensionStrings = k_XR_FB_eye_tracking_social,
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "0.1.0")]
#endif
    public class MetaSocialEyeGazeFeature : OpenXRFeature
    {
        internal const string displayName = "Meta Quest: Social Eye Gaze";

        /// <summary>
        /// The unique ID that identifies this feature.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.meta-social-eye-gaze";

        /// <summary>
        /// When <see langword="true"/>, sets <c>android:required="true"</c> on the
        /// <c>oculus.software.eye_tracking</c> <c>uses-feature</c> manifest entry,
        /// restricting installation to devices with eye tracking hardware.
        /// </summary>
        [SerializeField, Tooltip("Sets android: required =\"true\" on the oculus.software.eye_tracking uses-feature manifest entry. When disabled, the app can be installed on devices without eye tracking hardware.")]
        bool m_EyeTrackingRequired;

        /// <summary>
        /// Whether eye tracking hardware is required for this app to install and run.
        /// Controls <c>android:required</c> on the <c>oculus.software.eye_tracking</c> manifest entry.
        /// </summary>
        public bool eyeTrackingRequired => m_EyeTrackingRequired;

        /// <summary>
        /// Called after `xrCreateInstance`. Override this method to validate that any necessary OpenXR extensions were
        /// successfully enabled (<see cref="OpenXRRuntime.IsExtensionEnabled">OpenXRRuntime.IsExtensionEnabled</see>)
        /// and that any required system properties are supported. If this method returns <see langword="false"/>,
        /// the feature's <see cref="OpenXRFeature.enabled"/> property is set to <see langword="false"/>.
        /// </summary>
        /// <param name="xrInstance">Handle of the native `xrInstance`.</param>
        /// <returns><see langword="true"/> if this feature successfully initialized. Otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// If this feature is a required feature of an enabled feature set, returning <see langword="false"/> here
        /// causes the `OpenXRLoader` to fail, and XR Plug-in Management will fall back to another loader if enabled.
        /// </remarks>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!OpenXRUtils.IsExtensionEnabled(k_XR_FB_eye_tracking_social, displayName))
                return false;

            InputSystem.InputSystem.RegisterLayout<MetaSocialEyeGazeDevice>(
                matches: new InputDeviceMatcher()
                    .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                    .WithProduct(MetaSocialEyeGazeDevice.k_DeviceLocalizedName));

            return true;
        }
    }
}
