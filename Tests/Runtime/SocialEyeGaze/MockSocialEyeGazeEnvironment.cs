using System;
using System.Threading.Tasks;
using UnityEngine.XR.OpenXR.TestTooling;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    class MockSocialEyeGazeEnvironment : IDisposable
    {
        const string k_ExtensionName = "XR_FB_eye_tracking_social";
        const uint k_ExtensionVersion = 1;

        readonly MockOpenXREnvironment m_Environment;

        public SocialEyeGazeMocks Mocks { get; } = new SocialEyeGazeMocks();

        public MockSocialEyeGazeEnvironment()
        {
            m_Environment = MockOpenXREnvironment.CreateEnvironment();
        }

        /// <summary>
        /// Configures the mock environment for social eye gaze testing.
        /// Call once after construction, before setting per-test mocks and calling <see cref="Start"/>.
        /// </summary>
        /// <param name="supportsEyeTracking">
        /// Whether the mock system reports eye tracking support via
        /// <c>XrSystemEyeTrackingPropertiesFB.supportsEyeTracking</c>.
        /// Set to <c>false</c> to test the unsupported-hardware path.
        /// </param>
        public void SetUpDefaultEnvironment(bool supportsEyeTracking = true)
        {
            m_Environment.AddSupportedExtension(k_ExtensionName, k_ExtensionVersion);
            m_Environment.Settings.EnableFeature<MetaSocialEyeGazeFeature>(true);
            m_Environment.SetSysPropertiesFunctionForXrStructureType(
                (uint)NativeTypes.XrStructureType.SystemEyeTrackingPropertiesFB,
                supportsEyeTracking ? SocialEyeGazeMocks.EyeTrackingSupportedPtr : SocialEyeGazeMocks.EyeTrackingUnsupportedPtr);
        }

        /// <summary>
        /// Registers function interceptors and starts the mock OpenXR environment.
        /// Call after setting per-test mock delegates on <see cref="Mocks"/>.
        /// </summary>
        public void Start()
        {
            SocialEyeGazeMocks.Activate(Mocks);
            m_Environment.SetFunctionForInterceptor("xrCreateEyeTrackerFB", SocialEyeGazeMocks.CreatePtr);
            m_Environment.SetFunctionForInterceptor("xrGetEyeGazesFB", SocialEyeGazeMocks.GetGazesPtr);
            m_Environment.SetFunctionForInterceptor("xrDestroyEyeTrackerFB", SocialEyeGazeMocks.DestroyPtr);
            m_Environment.Start();
        }

        /// <summary>
        /// Advances the environment by <paramref name="frameCount"/> frames, pumping the OpenXR message
        /// loop on each one.
        /// </summary>
        /// <remarks>
        /// Tests must pump the message loop themselves. <c>OpenXRLoader</c> normally drives it from
        /// <see cref="Application.onBeforeRender"/>, which never fires in a batch mode test run. Without
        /// pumping, the session never advances past <c>XR_SESSION_STATE_READY</c>: the display and input
        /// subsystems never start, <c>xrCreateEyeTrackerFB</c> is never called, and no device is registered.
        /// Each pumped frame also polls <c>xrGetEyeGazesFB</c> once, so a mock swapped in during a test
        /// takes effect on the next call to this method.
        /// </remarks>
        /// <param name="frameCount">The number of frames to advance.</param>
        /// <returns>A task that completes once the frames have elapsed.</returns>
        public async Task ProcessFramesAsync(int frameCount = 1)
        {
            for (var i = 0; i < frameCount; ++i)
            {
                m_Environment.ProcessEventQueue();
                await Awaitable.NextFrameAsync();
            }
        }

        /// <summary>
        /// Stops the mock OpenXR environment.
        /// Active mocks are kept live until after Stop so the native side can call destroy during teardown.
        /// </summary>
        public void Stop()
        {
            m_Environment?.Stop();
            SocialEyeGazeMocks.Deactivate();
        }

        public void Dispose()
        {
            Stop();
            m_Environment?.Dispose();
        }
    }
}
