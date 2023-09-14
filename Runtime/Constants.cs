namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// Static constants
    /// </summary>
    static class Constants
    {
        internal const string k_ARFoundationLibrary = "libUnityARFoundationMeta";

        /// <summary>
        /// Key used to store and retrieve custom configuration settings from EditorBuildSettings.
        /// </summary>
        internal const string k_SettingsKey = "com.unity.xr.meta-openxr.settings";

#if UNITY_EDITOR
        /// <summary>
        /// Root URL for the Unity OpenXR: Meta documentation
        /// </summary>
        const string k_DocumentationManualURL = "https://docs.unity3d.com/Packages/com.unity.xr.meta-openxr@0.1/manual/";

        /// <summary>
        /// Main documentation URL for Unity OpenXR: Meta
        /// </summary>
        internal const string k_DocumentationURL = k_DocumentationManualURL + "index.html";

        /// <summary>
        /// Company name for OpenXR Feature implementations.
        /// </summary>
        internal const string k_CompanyName = "Unity Technologies";
#endif
    }
}
