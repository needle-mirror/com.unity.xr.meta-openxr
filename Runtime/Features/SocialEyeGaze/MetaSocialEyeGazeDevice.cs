using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.Scripting;

#if USE_INPUT_SYSTEM_POSE_CONTROL
using PoseControl = UnityEngine.InputSystem.XR.PoseControl;
#else
using PoseControl = UnityEngine.XR.OpenXR.Input.PoseControl;
#endif

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// An Input System device that exposes per-eye gaze data from <c>XR_FB_eye_tracking_social</c>.
    /// Enabled through <see cref="MetaSocialEyeGazeFeature"/>.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    [Preserve, InputControlLayout(displayName = "Meta Social Eye Gaze")]
    public class MetaSocialEyeGazeDevice : TrackedDevice
    {
        /// <summary>
        /// The active <see cref="MetaSocialEyeGazeDevice"/> instance, or <see langword="null"/> if no device is connected.
        /// </summary>
        public static MetaSocialEyeGazeDevice current { get; private set; }

        /// <summary>
        /// A [PoseControl](xref:UnityEngine.InputSystem.XR.PoseControl) representing the gaze pose of the left eye in head-relative (view) space.
        /// </summary>
        [InputControl(displayName = "Left Eye Pose")]
        public PoseControl leftEyePose { get; private set; }

        /// <summary>
        /// A [PoseControl](xref:UnityEngine.InputSystem.XR.PoseControl) representing the gaze pose of the right eye in head-relative (view) space.
        /// </summary>
        [InputControl(displayName = "Right Eye Pose")]
        public PoseControl rightEyePose { get; private set; }

        /// <summary>
        /// An [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) representing the gaze confidence of the left eye, in the range <c>[0, 1]</c>.
        /// </summary>
        [InputControl(displayName = "Left Eye Confidence")]
        public AxisControl leftEyeConfidence { get; private set; }

        /// <summary>
        /// An [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) representing the gaze confidence of the right eye, in the range <c>[0, 1]</c>.
        /// </summary>
        [InputControl(displayName = "Right Eye Confidence")]
        public AxisControl rightEyeConfidence { get; private set; }

        /// <summary>
        /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that is true when the left eye gaze data is valid.
        /// Corresponds to <c>XrEyeGazeFB.isValid</c> for the left eye.
        /// </summary>
        [InputControl(displayName = "Left Eye Is Valid")]
        public ButtonControl leftEyeIsValid { get; private set; }

        /// <summary>
        /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that is true when the right eye gaze data is valid.
        /// Corresponds to <c>XrEyeGazeFB.isValid</c> for the right eye.
        /// </summary>
        [InputControl(displayName = "Right Eye Is Valid")]
        public ButtonControl rightEyeIsValid { get; private set; }

        // CONTRACT: must exactly match OpenXR Plug-in's device name
        internal const string k_DeviceLocalizedName = "Social Eye Gaze OpenXR";

        /// <summary>
        /// The Input System calls this method when it adds the device to <see cref="UnityEngine.InputSystem.InputSystem.devices"/>.
        /// </summary>
        /// <remarks>
        /// This method runs after the Input System adds the device to <see cref="UnityEngine.InputSystem.InputSystem.devices"/>.
        /// </remarks>
        protected override void OnAdded()
        {
            base.OnAdded();
            current = this;
        }

        /// <summary>
        /// The Input System calls this method when it removes the device from <see cref="UnityEngine.InputSystem.InputSystem.devices"/>.
        /// </summary>
        /// <remarks>
        /// This method runs after the Input System removes the device from <see cref="UnityEngine.InputSystem.InputSystem.devices"/>.
        /// </remarks>
        protected override void OnRemoved()
        {
            base.OnRemoved();
            if (current == this)
                current = null;
        }

        /// <summary>
        /// Performs final initialization after the Input System builds the control hierarchy.
        /// </summary>
        protected override void FinishSetup()
        {
            base.FinishSetup();
            leftEyePose = GetChildControl<PoseControl>("leftEyePose");
            rightEyePose = GetChildControl<PoseControl>("rightEyePose");
            leftEyeConfidence = GetChildControl<AxisControl>("leftEyeConfidence");
            rightEyeConfidence = GetChildControl<AxisControl>("rightEyeConfidence");
            leftEyeIsValid = GetChildControl<ButtonControl>("leftEyeIsValid");
            rightEyeIsValid = GetChildControl<ButtonControl>("rightEyeIsValid");
        }

#if UNITY_EDITOR
        // Allows device to be searchable in the input actions menu
        static MetaSocialEyeGazeDevice() => RegisterLayout();
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterLayout()
        {
            // Reset static not cleared between play sessions when domain reload is disabled.
            current = null;

            InputSystem.InputSystem.RegisterLayout<MetaSocialEyeGazeDevice>(
                matches: new InputDeviceMatcher()
                    .WithProduct(k_DeviceLocalizedName));
        }
    }
}
