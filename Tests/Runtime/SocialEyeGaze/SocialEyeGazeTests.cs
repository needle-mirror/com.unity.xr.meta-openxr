using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.XR.OpenXR.NativeTypes;
using UnityEngine.XR.OpenXR.TestTooling;

using XrSession      = System.UInt64;
using XrEyeTrackerFB = System.UInt64;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    class SocialEyeGazeTests
    {
        MockSocialEyeGazeEnvironment m_Env;
        GameObject m_XrOrigin;

        [SetUp]
        public void SetUp()
        {
            m_Env = new MockSocialEyeGazeEnvironment();
            MockOpenXREnvironmentSettings.DisableAllFeaturesForBuildTarget();
        }

        [TearDown]
        public void TearDown()
        {
            m_Env?.Dispose();
            m_Env = null;
            Object.Destroy(m_XrOrigin);
            m_XrOrigin = null;
        }

        // Sets up mocks that let a session start successfully with a valid eye tracker.
        void SetDefaultWorkingMocks(XrEyeTrackerFB trackerHandle = 1)
        {
            m_Env.Mocks.createEyeTracker = (XrSession _, out XrEyeTrackerFB tracker) =>
            {
                tracker = trackerHandle;
                return XrResult.Success;
            };
            m_Env.Mocks.getEyeGazes = (XrEyeTrackerFB _, out XrEyeGazesFB gazes) =>
            {
                gazes = XrEyeGazesFB.defaultValue;
                return XrResult.Success;
            };
            m_Env.Mocks.destroyEyeTracker = _ => XrResult.Success;
        }

        /// <summary>
        /// Verifies that <see cref="MetaSocialEyeGazeDevice.current"/> is set after the
        /// environment starts and cleared after it stops.
        /// </summary>
        [Test]
        public async Task Lifecycle_DeviceRegisteredAndRemoved()
        {
            m_Env.SetUpDefaultEnvironment();
            SetDefaultWorkingMocks();
            m_Env.Start();

            await m_Env.ProcessFramesAsync();

            Assert.IsNotNull(MetaSocialEyeGazeDevice.current, "Device should be registered after start");

            m_Env.Stop();

            // Nothing is pumped after Stop because the loader is deinitialized. One Unity frame is
            // enough for the Input System to process the device removal.
            await Awaitable.NextFrameAsync();

            Assert.IsNull(MetaSocialEyeGazeDevice.current, "Device should be removed after stop");
        }

        /// <summary>
        /// Verifies that a failed <c>xrCreateEyeTrackerFB</c> does not crash and leaves
        /// the device reporting no gaze data.
        /// </summary>
        [Test]
        public async Task Lifecycle_FailedTrackerCreation_NoGazeData()
        {
            m_Env.SetUpDefaultEnvironment();

            bool createCalled = false;
            m_Env.Mocks.createEyeTracker = (XrSession _, out XrEyeTrackerFB tracker) =>
            {
                createCalled = true;
                tracker = 0;
                return XrResult.RuntimeFailure;
            };
            m_Env.Mocks.destroyEyeTracker = _ => XrResult.Success;
            // getEyeGazes not set — xrGetEyeGazesFB is never called when the tracker handle is null.

            m_Env.Start();
            await m_Env.ProcessFramesAsync();

            Assert.IsTrue(createCalled, "xrCreateEyeTrackerFB should have been called");

            var device = MetaSocialEyeGazeDevice.current;
            Assert.IsNotNull(device, "Device should still be registered even when tracker creation fails");
            Assert.IsFalse(device.leftEyeIsValid.isPressed,  "Left eye should not be valid when tracker creation failed");
            Assert.IsFalse(device.rightEyeIsValid.isPressed, "Right eye should not be valid when tracker creation failed");

            m_Env.Stop();
            await Awaitable.NextFrameAsync();
        }

        /// <summary>
        /// Verifies that valid gaze data sets tracking state flags and confidence correctly.
        /// Uses distinct confidence values per eye to confirm the channels are independently routed.
        /// </summary>
        [Test]
        public async Task ValidGaze_SetsTrackingStateAndConfidence()
        {
            const float k_LeftConfidence  = 0.3f;
            const float k_RightConfidence = 0.9f;
            var leftGaze  = new XrEyeGazeFB(true,  new XrPosef(Vector3.zero, Quaternion.identity), k_LeftConfidence);
            var rightGaze = new XrEyeGazeFB(true,  new XrPosef(Vector3.zero, Quaternion.identity), k_RightConfidence);

            m_Env.SetUpDefaultEnvironment();
            SetDefaultWorkingMocks();
            m_Env.Mocks.getEyeGazes = (XrEyeTrackerFB _, out XrEyeGazesFB gazes) =>
            {
                gazes = new XrEyeGazesFB(leftGaze, rightGaze, 0);
                return XrResult.Success;
            };

            m_Env.Start();
            await m_Env.ProcessFramesAsync();

            var device = MetaSocialEyeGazeDevice.current;
            Assert.IsNotNull(device);

            Assert.IsTrue(device.leftEyeIsValid.isPressed,  "Left eye should be valid");
            Assert.IsTrue(device.rightEyeIsValid.isPressed, "Right eye should be valid");

            Assert.IsTrue(device.isTracked.isPressed, "Device should be tracked when both eyes are valid");
            var deviceTracking = (InputTrackingState)device.trackingState.ReadValue();
            Assert.That(deviceTracking & InputTrackingState.Position, Is.EqualTo(InputTrackingState.Position), "Device tracking state should have Position flag");
            Assert.That(deviceTracking & InputTrackingState.Rotation, Is.EqualTo(InputTrackingState.Rotation), "Device tracking state should have Rotation flag");

            var leftTracking = (InputTrackingState)device.leftEyePose.trackingState.ReadValue();
            Assert.That(leftTracking & InputTrackingState.Position, Is.EqualTo(InputTrackingState.Position), "Left tracking state should have Position flag");
            Assert.That(leftTracking & InputTrackingState.Rotation, Is.EqualTo(InputTrackingState.Rotation), "Left tracking state should have Rotation flag");

            var rightTracking = (InputTrackingState)device.rightEyePose.trackingState.ReadValue();
            Assert.That(rightTracking & InputTrackingState.Position, Is.EqualTo(InputTrackingState.Position), "Right tracking state should have Position flag");
            Assert.That(rightTracking & InputTrackingState.Rotation, Is.EqualTo(InputTrackingState.Rotation), "Right tracking state should have Rotation flag");

            Assert.That(device.leftEyeConfidence.ReadValue(),  Is.EqualTo(k_LeftConfidence).Within(0.001f),  "Left confidence should match mock value");
            Assert.That(device.rightEyeConfidence.ReadValue(), Is.EqualTo(k_RightConfidence).Within(0.001f), "Right confidence should match mock value");

            m_Env.Stop();
            await Awaitable.NextFrameAsync();
        }

        /// <summary>
        /// Verifies that invalid gaze data clears tracking state and zeroes confidence.
        /// </summary>
        [Test]
        public async Task InvalidGaze_ClearsTrackingState()
        {
            var invalidLeft  = new XrEyeGazeFB(false, new XrPosef(Vector3.zero, Quaternion.identity), 0f);
            var invalidRight = new XrEyeGazeFB(false, new XrPosef(Vector3.zero, Quaternion.identity), 0f);

            m_Env.SetUpDefaultEnvironment();
            SetDefaultWorkingMocks();
            m_Env.Mocks.getEyeGazes = (XrEyeTrackerFB _, out XrEyeGazesFB gazes) =>
            {
                gazes = new XrEyeGazesFB(invalidLeft, invalidRight, 0);
                return XrResult.Success;
            };

            m_Env.Start();
            await m_Env.ProcessFramesAsync();

            var device = MetaSocialEyeGazeDevice.current;
            Assert.IsNotNull(device);

            Assert.IsFalse(device.leftEyeIsValid.isPressed,  "Left eye should not be valid");
            Assert.IsFalse(device.rightEyeIsValid.isPressed, "Right eye should not be valid");

            Assert.IsFalse(device.isTracked.isPressed, "Device should not be tracked when both eyes are invalid");
            var deviceTracking = (InputTrackingState)device.trackingState.ReadValue();
            Assert.That(deviceTracking, Is.EqualTo(InputTrackingState.None), "Device tracking state should be None when both eyes are invalid");

            var leftTracking = (InputTrackingState)device.leftEyePose.trackingState.ReadValue();
            Assert.That(leftTracking, Is.EqualTo(InputTrackingState.None), "Left tracking state should be None when gaze is invalid");

            var rightTracking = (InputTrackingState)device.rightEyePose.trackingState.ReadValue();
            Assert.That(rightTracking, Is.EqualTo(InputTrackingState.None), "Right tracking state should be None when gaze is invalid");

            Assert.That(device.leftEyeConfidence.ReadValue(),  Is.EqualTo(0f).Within(0.001f), "Left confidence should be 0 when gaze is invalid");
            Assert.That(device.rightEyeConfidence.ReadValue(), Is.EqualTo(0f).Within(0.001f), "Right confidence should be 0 when gaze is invalid");

            m_Env.Stop();
            await Awaitable.NextFrameAsync();
        }

        /// <summary>
        /// Verifies that a failed <c>xrGetEyeGazesFB</c> does not crash and leaves the device
        /// reporting no gaze data, regardless of what the returned struct contains.
        /// </summary>
        [Test]
        public async Task FailedGetEyeGazes_ReportsNoGazeData()
        {
            m_Env.SetUpDefaultEnvironment();
            SetDefaultWorkingMocks();
            m_Env.Mocks.getEyeGazes = (XrEyeTrackerFB _, out XrEyeGazesFB gazes) =>
            {
                // Return a struct with both eyes marked valid — the failure result should override this.
                gazes = new XrEyeGazesFB(
                    new XrEyeGazeFB(true, new XrPosef(Vector3.zero, Quaternion.identity), 1f),
                    new XrEyeGazeFB(true, new XrPosef(Vector3.zero, Quaternion.identity), 1f),
                    0);
                return XrResult.RuntimeFailure;
            };

            m_Env.Start();
            await m_Env.ProcessFramesAsync();

            var device = MetaSocialEyeGazeDevice.current;
            Assert.IsNotNull(device);

            Assert.IsFalse(device.leftEyeIsValid.isPressed, "Left eye should not be valid when xrGetEyeGazesFB fails");
            Assert.IsFalse(device.rightEyeIsValid.isPressed, "Right eye should not be valid when xrGetEyeGazesFB fails");
            Assert.IsFalse(device.isTracked.isPressed, "Device should not be tracked when xrGetEyeGazesFB fails");
            Assert.That(device.leftEyeConfidence.ReadValue(), Is.EqualTo(0f).Within(0.001f), "Left confidence should be 0 when xrGetEyeGazesFB fails");
            Assert.That(device.rightEyeConfidence.ReadValue(), Is.EqualTo(0f).Within(0.001f), "Right confidence should be 0 when xrGetEyeGazesFB fails");

            m_Env.Stop();
            await Awaitable.NextFrameAsync();
        }

        /// <summary>
        /// Verifies that no device is registered when the system reports no eye tracking support.
        /// </summary>
        [Test]
        public async Task UnsupportedHardware_NoDeviceRegistered()
        {
            // No mocks needed — supportsEyeTracking: false causes InitializeAndRegisterNumberOfDevices
            // to return 0 immediately, so none of the FB functions are ever called.
            m_Env.SetUpDefaultEnvironment(supportsEyeTracking: false);
            m_Env.Start();

            await m_Env.ProcessFramesAsync();

            Assert.IsNull(MetaSocialEyeGazeDevice.current, "Device should not be registered on unsupported hardware");

            m_Env.Stop();
            await Awaitable.NextFrameAsync();
        }

        /// <summary>
        /// Verifies that position, rotation, confidence, and validity flow end-to-end without
        /// corruption across multiple frames, and that left and right eye data are routed
        /// independently. Distinct poses and confidence values per eye catch indexing bugs that
        /// would swap or mix left/right data. Changing values on frame 2 confirms the extension
        /// re-queries <c>xrGetEyeGazesFB</c> every frame rather than caching the first result.
        /// </summary>
        [Test]
        public async Task ValidGaze_PoseDataPassedThrough()
        {
            // Frame 1 data
            var leftPos1  = new Vector3(1f, 2f, 3f);
            var leftRot1  = Quaternion.AngleAxis(90f, Vector3.up);
            var rightPos1 = new Vector3(4f, 5f, 6f);
            var rightRot1 = Quaternion.AngleAxis(45f, Vector3.right);
            const float k_LeftConfidence1  = 0.4f;
            const float k_RightConfidence1 = 0.6f;

            // Frame 2 data — all values distinct from frame 1
            var leftPos2  = new Vector3(-1f, -2f, -3f);
            var leftRot2  = Quaternion.AngleAxis(30f, Vector3.forward);
            var rightPos2 = new Vector3(-4f, -5f, -6f);
            var rightRot2 = Quaternion.AngleAxis(60f, Vector3.up);
            const float k_LeftConfidence2  = 0.8f;
            const float k_RightConfidence2 = 0.2f;

            m_Env.SetUpDefaultEnvironment();
            SetDefaultWorkingMocks();
            m_Env.Mocks.getEyeGazes = (XrEyeTrackerFB _, out XrEyeGazesFB gazes) =>
            {
                gazes = new XrEyeGazesFB(
                    new XrEyeGazeFB(true, new XrPosef(leftPos1,  leftRot1),  k_LeftConfidence1),
                    new XrEyeGazeFB(true, new XrPosef(rightPos1, rightRot1), k_RightConfidence1),
                    0);
                return XrResult.Success;
            };

            m_Env.Start();
            await m_Env.ProcessFramesAsync();

            var device = MetaSocialEyeGazeDevice.current;
            Assert.IsNotNull(device);

            // Frame 1 assertions
            Assert.IsTrue(device.leftEyeIsValid.isPressed,  "Frame 1: left eye should be valid");
            Assert.IsTrue(device.rightEyeIsValid.isPressed, "Frame 1: right eye should be valid");

            var lp1 = device.leftEyePose.position.ReadValue();
            Assert.That(lp1.x, Is.EqualTo(leftPos1.x).Within(0.001f), "Frame 1: left position X");
            Assert.That(lp1.y, Is.EqualTo(leftPos1.y).Within(0.001f), "Frame 1: left position Y");
            Assert.That(lp1.z, Is.EqualTo(leftPos1.z).Within(0.001f), "Frame 1: left position Z");
            Assert.That(Quaternion.Angle(device.leftEyePose.rotation.ReadValue(), leftRot1), Is.LessThan(0.1f), "Frame 1: left rotation");
            Assert.That(device.leftEyeConfidence.ReadValue(), Is.EqualTo(k_LeftConfidence1).Within(0.001f), "Frame 1: left confidence");

            var rp1 = device.rightEyePose.position.ReadValue();
            Assert.That(rp1.x, Is.EqualTo(rightPos1.x).Within(0.001f), "Frame 1: right position X");
            Assert.That(rp1.y, Is.EqualTo(rightPos1.y).Within(0.001f), "Frame 1: right position Y");
            Assert.That(rp1.z, Is.EqualTo(rightPos1.z).Within(0.001f), "Frame 1: right position Z");
            Assert.That(Quaternion.Angle(device.rightEyePose.rotation.ReadValue(), rightRot1), Is.LessThan(0.1f), "Frame 1: right rotation");
            Assert.That(device.rightEyeConfidence.ReadValue(), Is.EqualTo(k_RightConfidence1).Within(0.001f), "Frame 1: right confidence");

            // Switch to frame 2 data
            m_Env.Mocks.getEyeGazes = (XrEyeTrackerFB _, out XrEyeGazesFB gazes) =>
            {
                gazes = new XrEyeGazesFB(
                    new XrEyeGazeFB(true, new XrPosef(leftPos2,  leftRot2),  k_LeftConfidence2),
                    new XrEyeGazeFB(true, new XrPosef(rightPos2, rightRot2), k_RightConfidence2),
                    0);
                return XrResult.Success;
            };

            await m_Env.ProcessFramesAsync();

            // Frame 2 assertions
            Assert.IsTrue(device.leftEyeIsValid.isPressed,  "Frame 2: left eye should still be valid");
            Assert.IsTrue(device.rightEyeIsValid.isPressed, "Frame 2: right eye should still be valid");

            var lp2 = device.leftEyePose.position.ReadValue();
            Assert.That(lp2.x, Is.EqualTo(leftPos2.x).Within(0.001f), "Frame 2: left position X");
            Assert.That(lp2.y, Is.EqualTo(leftPos2.y).Within(0.001f), "Frame 2: left position Y");
            Assert.That(lp2.z, Is.EqualTo(leftPos2.z).Within(0.001f), "Frame 2: left position Z");
            Assert.That(Quaternion.Angle(device.leftEyePose.rotation.ReadValue(), leftRot2), Is.LessThan(0.1f), "Frame 2: left rotation");
            Assert.That(device.leftEyeConfidence.ReadValue(), Is.EqualTo(k_LeftConfidence2).Within(0.001f), "Frame 2: left confidence");

            var rp2 = device.rightEyePose.position.ReadValue();
            Assert.That(rp2.x, Is.EqualTo(rightPos2.x).Within(0.001f), "Frame 2: right position X");
            Assert.That(rp2.y, Is.EqualTo(rightPos2.y).Within(0.001f), "Frame 2: right position Y");
            Assert.That(rp2.z, Is.EqualTo(rightPos2.z).Within(0.001f), "Frame 2: right position Z");
            Assert.That(Quaternion.Angle(device.rightEyePose.rotation.ReadValue(), rightRot2), Is.LessThan(0.1f), "Frame 2: right rotation");
            Assert.That(device.rightEyeConfidence.ReadValue(), Is.EqualTo(k_RightConfidence2).Within(0.001f), "Frame 2: right confidence");

            m_Env.Stop();
            await Awaitable.NextFrameAsync();
        }

        /// <summary>
        /// Verifies that when one eye is valid and the other is not, the device reports as tracked
        /// and the per-eye validity controls reflect the asymmetric state.
        /// </summary>
        [Test]
        public async Task MixedEyeValidity_DeviceTrackedWhenOneEyeValid()
        {
            var validGaze   = new XrEyeGazeFB(true,  new XrPosef(Vector3.zero, Quaternion.identity), 0.8f);
            var invalidGaze = new XrEyeGazeFB(false, new XrPosef(Vector3.zero, Quaternion.identity), 0f);

            m_Env.SetUpDefaultEnvironment();
            SetDefaultWorkingMocks();
            m_Env.Mocks.getEyeGazes = (XrEyeTrackerFB _, out XrEyeGazesFB gazes) =>
            {
                gazes = new XrEyeGazesFB(validGaze, invalidGaze, 0);
                return XrResult.Success;
            };

            m_Env.Start();
            await m_Env.ProcessFramesAsync();

            var device = MetaSocialEyeGazeDevice.current;
            Assert.IsNotNull(device);

            Assert.IsTrue(device.leftEyeIsValid.isPressed,   "Left eye should be valid");
            Assert.IsFalse(device.rightEyeIsValid.isPressed, "Right eye should not be valid");
            Assert.IsTrue(device.isTracked.isPressed, "Device should report tracked when left eye is valid and right is not");

            // Flip: right valid, left invalid
            m_Env.Mocks.getEyeGazes = (XrEyeTrackerFB _, out XrEyeGazesFB gazes) =>
            {
                gazes = new XrEyeGazesFB(invalidGaze, validGaze, 0);
                return XrResult.Success;
            };

            await m_Env.ProcessFramesAsync();

            Assert.IsFalse(device.leftEyeIsValid.isPressed,  "Left eye should not be valid");
            Assert.IsTrue(device.rightEyeIsValid.isPressed,  "Right eye should be valid");
            Assert.IsTrue(device.isTracked.isPressed, "Device should report tracked when right eye is valid and left is not");

            m_Env.Stop();
            await Awaitable.NextFrameAsync();
        }

        /// <summary>
        /// Verifies that gaze pose values are head-relative (view space) and are not affected by
        /// XR Origin rotation in world space. Rotating a Camera.main stand-in must not change the
        /// device-reported pose — any accidental world-space transform would be caught here.
        /// </summary>
        [Test]
        public async Task ValidGaze_HeadRelative_UnaffectedByXROriginRotation()
        {
            var gazePosition = new Vector3(0.1f, -0.05f, -1f);
            var gazeRotation = Quaternion.AngleAxis(10f, Vector3.right);
            var gaze = new XrEyeGazeFB(true, new XrPosef(gazePosition, gazeRotation), 1f);

            m_Env.SetUpDefaultEnvironment();
            SetDefaultWorkingMocks();
            m_Env.Mocks.getEyeGazes = (XrEyeTrackerFB _, out XrEyeGazesFB gazes) =>
            {
                gazes = new XrEyeGazesFB(gaze, gaze, 0);
                return XrResult.Success;
            };

            // Simulate an XR Origin rotating in world space. Plain GameObject only — adding a
            // Camera component triggers URP's XR render-pass path on the mock display subsystem,
            // which has no render passes and throws IndexOutOfRangeException.
            m_XrOrigin = new GameObject("XR Origin");
            m_XrOrigin.transform.rotation = Quaternion.identity;

            m_Env.Start();
            await m_Env.ProcessFramesAsync();

            var device = MetaSocialEyeGazeDevice.current;
            Assert.IsNotNull(device);

            var leftPosBeforeRotation  = device.leftEyePose.position.ReadValue();
            var leftRotBeforeRotation  = device.leftEyePose.rotation.ReadValue();
            var rightPosBeforeRotation = device.rightEyePose.position.ReadValue();
            var rightRotBeforeRotation = device.rightEyePose.rotation.ReadValue();

            m_XrOrigin.transform.rotation = Quaternion.AngleAxis(90f, Vector3.up);

            await m_Env.ProcessFramesAsync();

            var leftPosAfterRotation  = device.leftEyePose.position.ReadValue();
            var leftRotAfterRotation  = device.leftEyePose.rotation.ReadValue();
            var rightPosAfterRotation = device.rightEyePose.position.ReadValue();
            var rightRotAfterRotation = device.rightEyePose.rotation.ReadValue();

            Assert.That(Vector3.Distance(leftPosAfterRotation, leftPosBeforeRotation), Is.LessThan(0.001f),
                "Head-relative gaze position must not change when XR Origin rotates (left eye)");
            Assert.That(Quaternion.Angle(leftRotAfterRotation, leftRotBeforeRotation), Is.LessThan(0.1f),
                "Head-relative gaze rotation must not change when XR Origin rotates (left eye)");

            Assert.That(Vector3.Distance(rightPosAfterRotation, rightPosBeforeRotation), Is.LessThan(0.001f),
                "Head-relative gaze position must not change when XR Origin rotates (right eye)");
            Assert.That(Quaternion.Angle(rightRotAfterRotation, rightRotBeforeRotation), Is.LessThan(0.1f),
                "Head-relative gaze rotation must not change when XR Origin rotates (right eye)");

            m_Env.Stop();
            await Awaitable.NextFrameAsync();
        }

        /// <summary>
        /// Verifies that tracking state, confidence, and per-eye validity update correctly across
        /// frames as the runtime transitions between providing and withholding gaze data. Covers
        /// the real-world scenario where Android eye tracking permission is granted or revoked
        /// mid-session without a restart.
        /// </summary>
        [Test]
        public async Task GazeValidity_TransitionsBetweenFrames()
        {
            const float k_Confidence = 0.8f;
            var validGaze   = new XrEyeGazeFB(true,  new XrPosef(Vector3.zero, Quaternion.identity), k_Confidence);
            var invalidGaze = new XrEyeGazeFB(false, new XrPosef(Vector3.zero, Quaternion.identity), 0f);

            m_Env.SetUpDefaultEnvironment();
            SetDefaultWorkingMocks();
            m_Env.Mocks.getEyeGazes = (XrEyeTrackerFB _, out XrEyeGazesFB gazes) =>
            {
                gazes = new XrEyeGazesFB(validGaze, validGaze, 0);
                return XrResult.Success;
            };

            m_Env.Start();
            await m_Env.ProcessFramesAsync();

            var device = MetaSocialEyeGazeDevice.current;
            Assert.IsNotNull(device);

            // Frame 1: valid
            Assert.IsTrue(device.leftEyeIsValid.isPressed,  "Frame 1: left eye should be valid");
            Assert.IsTrue(device.rightEyeIsValid.isPressed, "Frame 1: right eye should be valid");
            Assert.IsTrue(device.isTracked.isPressed, "Frame 1: device should be tracked");
            Assert.That(device.leftEyeConfidence.ReadValue(),  Is.EqualTo(k_Confidence).Within(0.001f), "Frame 1: left confidence");
            Assert.That(device.rightEyeConfidence.ReadValue(), Is.EqualTo(k_Confidence).Within(0.001f), "Frame 1: right confidence");

            // Transition to invalid
            m_Env.Mocks.getEyeGazes = (XrEyeTrackerFB _, out XrEyeGazesFB gazes) =>
            {
                gazes = new XrEyeGazesFB(invalidGaze, invalidGaze, 0);
                return XrResult.Success;
            };

            await m_Env.ProcessFramesAsync();

            // Frame 2: invalid
            Assert.IsFalse(device.leftEyeIsValid.isPressed,  "Frame 2: left eye should be invalid");
            Assert.IsFalse(device.rightEyeIsValid.isPressed, "Frame 2: right eye should be invalid");
            Assert.IsFalse(device.isTracked.isPressed, "Frame 2: device should not be tracked");
            Assert.That(device.leftEyeConfidence.ReadValue(),  Is.EqualTo(0f).Within(0.001f), "Frame 2: left confidence should be 0");
            Assert.That(device.rightEyeConfidence.ReadValue(), Is.EqualTo(0f).Within(0.001f), "Frame 2: right confidence should be 0");

            // Transition back to valid
            m_Env.Mocks.getEyeGazes = (XrEyeTrackerFB _, out XrEyeGazesFB gazes) =>
            {
                gazes = new XrEyeGazesFB(validGaze, validGaze, 0);
                return XrResult.Success;
            };

            await m_Env.ProcessFramesAsync();

            // Frame 3: valid again
            Assert.IsTrue(device.leftEyeIsValid.isPressed,  "Frame 3: left eye should be valid again");
            Assert.IsTrue(device.rightEyeIsValid.isPressed, "Frame 3: right eye should be valid again");
            Assert.IsTrue(device.isTracked.isPressed, "Frame 3: device should be tracked again");
            Assert.That(device.leftEyeConfidence.ReadValue(),  Is.EqualTo(k_Confidence).Within(0.001f), "Frame 3: left confidence");
            Assert.That(device.rightEyeConfidence.ReadValue(), Is.EqualTo(k_Confidence).Within(0.001f), "Frame 3: right confidence");

            m_Env.Stop();
            await Awaitable.NextFrameAsync();
        }
    }
}
