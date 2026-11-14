using UnityEngine.XR.ARFoundation;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    public class MetaSceneCaptureSample
    {
        public void ExampleRequestSceneCapture()
        {
            #region meta_scene_capture
            // Getting a reference to the ARSessionManager at runtime is not optimal.
            // For better runtime performance, reuse a saved reference to the ARSessionManager instead
            var arSession = Object.FindAnyObjectByType<ARSessionManager>();

            // To access the scene capture API,
            // cast the ARSessionManager's subsystem to MetaOpenXRSessionSubsystem
            var success = (arSession.subsystem as MetaOpenXRSessionSubsystem)
                .TryRequestSceneCapture();
            #endregion
        }
    }
}
