#if OPENXR_PLUGIN_1_16_0_PRE_2_OR_NEWER
using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using UnityEngine.XR.OpenXR.TestTooling;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    abstract class MockRuntimeTestFixture
    {
        /// <summary>
        /// Accessor to the MockOpenXREnvironment for use by test classes such as customizing settings.
        /// This should *only* be accessed _after_ the call to Startup(), and _before_ the call to Shutdown().
        /// </summary>
        internal MockOpenXREnvironment mockRuntimeEnvironment { get; private set; }

        /// <summary>
        /// This method should be called within the test class "Setup" method.
        /// </summary>
        [SetUp]
        public void Startup()
        {
            mockRuntimeEnvironment = MockOpenXREnvironment.CreateEnvironment();
            DisableAllFeatures();
        }

        /// <summary>
        /// This method should be called within the test class "TearDown" method.
        /// </summary>
        [TearDown]
        public void Shutdown()
        {
            mockRuntimeEnvironment.Dispose();
        }

        protected void EnqueueMockEvent<MockedEventType>(MockedEventType mockedEvent)
        {
            IntPtr unmanagedEventData = Marshal.AllocHGlobal(Constants.EventDataBufferSize);
            Marshal.StructureToPtr(mockedEvent, unmanagedEventData, false);
            mockRuntimeEnvironment.EnqueueMockEventData(unmanagedEventData);
            Marshal.FreeHGlobal(unmanagedEventData);
        }

        protected void DisableAllFeatures()
        {
            MockOpenXREnvironmentSettings.DisableAllFeaturesForBuildTarget();
        }
    }
}
#endif
