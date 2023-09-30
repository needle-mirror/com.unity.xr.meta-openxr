namespace UnityEngine.XR.OpenXR.Features.Meta
{
    static class Constants
    {
        /// <summary>
        /// Company name for OpenXR Feature implementations.
        /// </summary>
        internal const string k_CompanyName = "Unity Technologies";
        internal const string k_PackageName = "com.unity.xr.meta-openxr";
        internal const string k_MajorMinorVersion = "0.2";

        internal const string k_ARFoundationLibrary = "libUnityARFoundationMeta";

        /// <summary>
        /// Key used to store and retrieve custom configuration settings from EditorBuildSettings.
        /// </summary>
        internal const string k_SettingsKey = k_PackageName + ".settings";

        internal static class DocsUrls
        {
            const string k_DocumentationBaseUrl = "https://docs.unity3d.com/Packages/" + k_PackageName + "@" + k_MajorMinorVersion + "/manual/";
            internal const string k_IndexUrl = k_DocumentationBaseUrl + "index.html";
            internal const string k_DisplayUtilitiesUrl = k_DocumentationBaseUrl + "display-utilities.html";
            internal const string k_AnchorsUrl = k_DocumentationBaseUrl + "anchors.html";
            internal const string k_CameraUrl = k_DocumentationBaseUrl + "camera.html";
            internal const string k_PlanesUrl = k_DocumentationBaseUrl + "plane-detection.html";
            internal const string k_RaycastsUrl = k_DocumentationBaseUrl + "raycasts.html";
            internal const string k_SessionUrl = k_DocumentationBaseUrl + "session.html";
        }

        internal static class OpenXRExtensions
        {
            internal const string k_XR_FB_composition_layer_alpha_blend = "XR_FB_composition_layer_alpha_blend";
            internal const string k_XR_FB_display_refresh_rate = "XR_FB_display_refresh_rate";
            internal const string k_XR_FB_passthrough = "XR_FB_passthrough";
            internal const string k_XR_FB_spatial_entity = "XR_FB_spatial_entity";
            internal const string k_XR_FB_spatial_entity_query = "XR_FB_spatial_entity_query";
            internal const string k_XR_FB_spatial_entity_storage = "XR_FB_spatial_entity_storage";
            internal const string k_XR_FB_scene = "XR_FB_scene";
            internal const string k_XR_FB_scene_capture = "XR_FB_scene_capture";
        }
    }
}
