using NUnit.Framework;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    class EnvironmentRaycastSubsystemTests : MockRuntimeTestFixture
    {
        [SetUp]
        public void Setup()
        {
            mockRuntimeEnvironment.Settings.EnableFeature<ARRaycastFeature>(true);

            mockRuntimeEnvironment.AddSupportedExtension(TestConstants.k_XrFutureExtName, 1);
            mockRuntimeEnvironment.AddSupportedExtension(TestConstants.k_XrEnvironmentRaycastExtName, 1);

            mockRuntimeEnvironment.SetSysPropertiesFunctionForXrStructureType(
                    (uint)XrStructureType.XR_TYPE_SYSTEM_ENVIRONMENT_RAYCAST_PROPERTIES_META,
                    MockRuntimeRaycastSubsystem.envrionmentRaycastPropertiesSupported_MockCallback);
        }

        [Test]
        public void RaycastWithDistance_HitBeyondMaxDistanceReturnsNoHits()
        {
            mockRuntimeEnvironment.Start();

            SetFunctionInterceptors();

            var loader = XRGeneralSettings.Instance.Manager.activeLoader;
            Assert.IsNotNull(loader);

            var raycastSubsystem = loader?.GetLoadedSubsystem<XRRaycastSubsystem>();
            Assert.IsNotNull(raycastSubsystem);
            Assert.IsInstanceOf<MetaOpenXRRaycastSubsystem>(raycastSubsystem);

            raycastSubsystem.Start();

            var hits = raycastSubsystem.Raycast(new Ray(Vector3.zero, Vector3.forward), TrackableType.Depth, Unity.Collections.Allocator.Temp, 0.1f);

            Assert.Zero(hits.Length);

            raycastSubsystem.Stop();
        }

        [Test]
        public void RaycastWithDistance_HitWithinMaxDistanceReturnsOne()
        {
            mockRuntimeEnvironment.Start();

            SetFunctionInterceptors();

            var loader = XRGeneralSettings.Instance.Manager.activeLoader;
            Assert.IsNotNull(loader);

            var raycastSubsystem = loader?.GetLoadedSubsystem<XRRaycastSubsystem>();
            Assert.IsNotNull(raycastSubsystem);
            Assert.IsInstanceOf<MetaOpenXRRaycastSubsystem>(raycastSubsystem);

            raycastSubsystem.Start();

            var hits = raycastSubsystem.Raycast(new Ray(Vector3.zero, Vector3.forward), TrackableType.Depth, Unity.Collections.Allocator.Temp, 1.1f);

            Assert.IsTrue(hits.Length == 1);
            Assert.IsTrue(hits[0].pose.position == Vector3.forward);
            Assert.IsTrue(hits[0].hitType == TrackableType.Depth);

            raycastSubsystem.Stop();
        }

        [Test]
        public void Raycast_ReturnProviderSpecificData()
        {
            mockRuntimeEnvironment.Start();

            SetFunctionInterceptors();

            var loader = XRGeneralSettings.Instance.Manager.activeLoader;
            Assert.IsNotNull(loader);

            var raycastSubsystem = loader?.GetLoadedSubsystem<XRRaycastSubsystem>();
            Assert.IsNotNull(raycastSubsystem);
            Assert.IsInstanceOf<MetaOpenXRRaycastSubsystem>(raycastSubsystem);

            raycastSubsystem.Start();

            var hit = (raycastSubsystem as MetaOpenXRRaycastSubsystem).RaycastEnvironment(new Ray(Vector3.zero, Vector3.forward), 1.1f);

            Assert.IsTrue(hit.status == XRResultStatus.unqualifiedSuccess);
            var value = hit.value;
            Assert.IsTrue(value.hitStatus == EnvironmentRaycastHitStatus.Hit);
            Assert.IsTrue(value.hit.hitType == TrackableType.Depth);
            Assert.IsTrue(value.hit.pose.position == Vector3.forward);

            raycastSubsystem.Stop();
        }

        void SetFunctionInterceptors()
        {
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrMetaCreateRaycastSyncFuncName, MockRuntimeRaycastSubsystem.xrCreateEnvironmentRaycasterAsyncMETA_ptr);
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrMetaCreateRaycastCompleteFuncName, MockRuntimeRaycastSubsystem.xrCreateEnvironmentRaycasterCompleteMETA_ptr);
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrMetaDestroyRaycasterFuncName, MockRuntimeRaycastSubsystem.xrDestroyEnvironmentRaycasterMETA_ptr);
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrMetaPerformRaycastFuncName, MockRuntimeRaycastSubsystem.xrPerformEnvironmentRaycastMETA_ptr_returnForwardHit);
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrPollFutureFuncName, MockRuntimeRaycastSubsystem.xrPollFutureEXT_ptr);
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrCancelFutureFuncName, MockRuntimeRaycastSubsystem.xrCancelFutureEXT_ptr);
        }
    }
}
