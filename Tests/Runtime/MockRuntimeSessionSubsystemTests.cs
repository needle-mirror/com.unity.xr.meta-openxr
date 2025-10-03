#if ENABLE_MOCK_RUNTIME_TESTS
#if OPENXR_PLUGIN_1_16_0_PRE_2_OR_NEWER
using System;
using System.Runtime.InteropServices;
using AOT;
using NUnit.Framework;
using UnityEngine.SubsystemsImplementation.Extensions;
using UnityEngine.XR.OpenXR.Features.Mock;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;
using XrAsyncRequestIdFB = System.UInt64;
using XrSession = System.UInt64;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    [TestFixture]
    class MockRuntimeSessionSubsystemTests : MockRuntimeTestFixture
    {
        const int TestRequestId = 99;

        static unsafe IntPtr s_SpatialEntityPropertiesSupported_MockCallback =
            Marshal.GetFunctionPointerForDelegate(
                (GetSystemProperties_Delegate)SpatialEntityPropertiesSupported_MockCallback);

        static unsafe IntPtr s_SceneCaptureRequestSuccess_MockCallback =
            Marshal.GetFunctionPointerForDelegate(
                (RequestSceneCaptureFB_Delegate)RequestSceneCaptureFBSuccess_MockCallback);

        [MonoPInvokeCallback(typeof(GetSystemProperties_Delegate))]
        static unsafe void SpatialEntityPropertiesSupported_MockCallback(XrSystemPropertiesBaseHeader* nativeSystemPropertiesStruct)
        {
            if (nativeSystemPropertiesStruct != null)
            {
                var spatialEntityProperties = (XrSystemSpatialEntityPropertiesFB*)nativeSystemPropertiesStruct;
                Assert.IsTrue(spatialEntityProperties->type == XrStructureType.XR_TYPE_SYSTEM_SPATIAL_ENTITY_PROPERTIES_FB);
                spatialEntityProperties->supportsSpatialEntity = true;
            }
        }

        [MonoPInvokeCallback(typeof(RequestSceneCaptureFB_Delegate))]
        static unsafe int RequestSceneCaptureFBSuccess_MockCallback(XrSession session, XrSceneCaptureRequestInfoFB* info, XrAsyncRequestIdFB* requestId)
        {
            if (info != null && requestId != null)
            {
                *requestId = TestRequestId;
            }

            return 0;
        }

        [Test]
        public void Test_AnchorSubsystemStarts_Success()
        {
            mockRuntimeEnvironment.Settings.EnableFeature<MockRuntime>(true);
            mockRuntimeEnvironment.Settings.EnableFeature<ARSessionFeature>(true);
            mockRuntimeEnvironment.Settings.EnableFeature<ARAnchorFeature>(true);

            mockRuntimeEnvironment.AddSupportedExtension(TestConstants.k_XrFbSpatialEntityExtName, Constants.XR_FB_spatial_entity_SPEC_VERSION);
            mockRuntimeEnvironment.AddSupportedExtension(TestConstants.k_XrMetaSpatialEntityDiscoveryExtName, Constants.XR_META_spatial_entity_discovery_SPEC_VERSION);
            mockRuntimeEnvironment.AddSupportedExtension(TestConstants.k_XrMetaSpatialEntityPersistenceExtName, Constants.XR_META_spatial_entity_persistence_SPEC_VERSION);
            mockRuntimeEnvironment.AddSupportedExtension(TestConstants.k_XrMetaSpatialEntitySharingExtName, Constants.XR_META_spatial_entity_sharing_SPEC_VERSION);
            mockRuntimeEnvironment.AddSupportedExtension(TestConstants.k_XrMetaSpatialEntityGroupSharingExtName, Constants.XR_META_spatial_entity_group_sharing_SPEC_VERSION);

            mockRuntimeEnvironment.SetSysPropertiesFunctionForXrStructureType(
                (uint)XrStructureType.XR_TYPE_SYSTEM_SPATIAL_ENTITY_PROPERTIES_FB,
                s_SpatialEntityPropertiesSupported_MockCallback);

            mockRuntimeEnvironment.Start();

            var loader = XRGeneralSettings.Instance.Manager.activeLoader;
            Assert.IsNotNull(loader);

            var anchorSubsystem = loader != null ? loader.GetLoadedSubsystem<XRAnchorSubsystem>() : null;
            Assert.IsNotNull(anchorSubsystem);
            Assert.IsTrue(anchorSubsystem is MetaOpenXRAnchorSubsystem);
            Assert.IsTrue(anchorSubsystem.GetProvider().GetType().Name == "MetaOpenXRAnchorProvider");

            Assert.IsFalse(anchorSubsystem.running);
            anchorSubsystem.Start();
            Assert.IsTrue(anchorSubsystem.running);
            anchorSubsystem.Stop();
            Assert.IsFalse(anchorSubsystem.running);
        }

        [Test]
        public void Test_SessionRequestSceneCapture_Success()
        {
            mockRuntimeEnvironment.Settings.EnableFeature<MockRuntime>(true);
            mockRuntimeEnvironment.Settings.EnableFeature<ARSessionFeature>(true);

            mockRuntimeEnvironment.AddSupportedExtension(TestConstants.k_XrFbSceneCaptureExtName, Constants.XR_FB_scene_capture_SPEC_VERSION);

            mockRuntimeEnvironment.Start();

            var loader = XRGeneralSettings.Instance.Manager.activeLoader;
            Assert.IsNotNull(loader);

            var sessionSubsystem = loader != null ? loader.GetLoadedSubsystem<XRSessionSubsystem>() : null;
            Assert.IsNotNull(sessionSubsystem);
            Assert.IsTrue(sessionSubsystem is MetaOpenXRSessionSubsystem);

            sessionSubsystem.Start();

            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrRequestSceneCaptureFbFuncName,
                s_SceneCaptureRequestSuccess_MockCallback);
            var metaSessionSubsystem = sessionSubsystem as MetaOpenXRSessionSubsystem;
            var requestSceneCaptureWasSuccessful = metaSessionSubsystem.TryRequestSceneCapture();
            Assert.IsTrue(requestSceneCaptureWasSuccessful);

            var locaLEventData = new XrEventDataSceneCaptureCompleteFB()
            {
                type = XrStructureType.XR_TYPE_SCENE_CAPTURE_REQUEST_COMPLETE_FB,
                next = null,
                requestId = TestRequestId,
                result = 0
            };
            EnqueueMockEvent(locaLEventData);
            mockRuntimeEnvironment.ProcessEventQueue();

            sessionSubsystem.Stop();
        }
    }
}
#endif // OPENXR_PLUGIN_1_16_0_PRE_2_OR_NEWER
#endif // ENABLE_MOCK_RUNTIME_TESTS
