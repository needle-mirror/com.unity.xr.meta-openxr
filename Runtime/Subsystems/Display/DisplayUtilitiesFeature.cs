using System;
using static UnityEngine.XR.OpenXR.Features.Meta.ARSessionFeature;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// Use this OpenXR feature to enable OpenXR extensions associated with the
    /// <see cref="MetaOpenXRDisplaySubsystemExtensions"/>. Without the necessary OpenXR extensions enabled, the
    /// display subsystem extension methods will always return <see langword="false"/>.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = displayName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android, BuildTargetGroup.Standalone },
        Company = Constants.k_CompanyName,
        Desc = "Enables you to request a specific display refresh rate",
        DocumentationLink = Constants.DocsUrls.k_DisplayUtilitiesUrl,
        OpenxrExtensionStrings = Constants.OpenXRExtensions.k_XR_FB_display_refresh_rate,
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "0.1.0")]
#endif
    public class DisplayUtilitiesFeature : OpenXRFeature
    {
        /// <summary>
        /// UI display name of this feature.
        /// </summary>
        public const string displayName = "Meta Quest: Display Utilities";

        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.meta-display-utilities";

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
        /// <returns>Returns `true` if successful. Returns `false` otherwise.</returns>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            return base.OnInstanceCreate(xrInstance)
                    && NativeApi.UnityOpenXRMeta_OnInstanceCreate(
                        xrInstance,
                        xrGetInstanceProcAddr);
        }

        /// <summary>
        /// Called before xrDestroyInstance.
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
        /// Called when the OpenXR loader receives the `XR_TYPE_EVENT_DATA_SESSION_STATE_CHANGED` event
        /// from the runtime signaling that the XrSessionState has changed.
        /// </summary>
        /// <param name="oldState">Previous state</param>
        /// <param name="newState">New state</param>
        protected override void OnSessionStateChange(int oldState, int newState)
        {
            base.OnSessionStateChange(oldState, newState);
            MetaOpenXRSessionSubsystem.instance?.OnSessionStateChange(oldState, newState);
        }

        /// <summary>
        /// Called when the reference xrSpace for the app changes.
        /// </summary>
        /// <param name="xrSpace">Handle of the xrSpace</param>
        protected override void OnAppSpaceChange(ulong xrSpace)
        {
            NativeApi.UnityOpenXRMeta_OnAppSpaceChange(xrSpace);
        }
    }
}
