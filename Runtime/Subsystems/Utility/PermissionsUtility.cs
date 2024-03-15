using UnityEngine;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    static class PermissionsUtility
    {
        const string k_AndroidScenePermission = "com.oculus.permission.USE_SCENE";

        internal static void RequestPlatformPermissions(string subsystemNameRequestingPermission)
        {
#if UNITY_ANDROID
            // Meta requires that we ask for scene permission beginning with OpenXR 1.0.31
            if (OpenXRUtility.IsOpenXRVersionGreaterOrEqual(1, 0, 31) &&
                !Permission.HasUserAuthorizedPermission(k_AndroidScenePermission))
            {
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionDenied += _ => LogAndroidPermissionFailure(subsystemNameRequestingPermission);
#if UNITY_2023_1_OR_NEWER
                callbacks.PermissionRequestDismissed += _ => LogAndroidPermissionFailure(subsystemNameRequestingPermission);
#else
                callbacks.PermissionDeniedAndDontAskAgain += _ => LogAndroidPermissionFailure();
#endif // UNITY_2023_1_OR_NEWER
                Permission.RequestUserPermission(k_AndroidScenePermission, callbacks);
            }
#endif // UNITY_ANDROID
        }

#if UNITY_ANDROID
        static void LogAndroidPermissionFailure(string subsystemNameRequestingPermission) =>
            Debug.LogError($"{subsystemNameRequestingPermission} requires system permission {k_AndroidScenePermission}, but permission was not granted.");
#endif
    }
}
