#if ENABLE_MOCK_RUNTIME_TESTS
#if OPENXR_PLUGIN_1_16_0_PRE_2_OR_NEWER
using System;
using System.Runtime.InteropServices;
using AOT;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine.SubsystemsImplementation.Extensions;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR.Features.Mock;
using UnityEngine.XR.OpenXR.NativeTypes;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    using XrSession = UInt64;
    using XrSpace = UInt64;
    using XrAsyncRequestIdFB = UInt64;
    using XrTime = Int64;
    using XrUuidEXT = SerializableGuid;

    [TestFixture]
    class MockRuntimePlaneSubsystemTests : MockRuntimeTestFixture
    {
        const float k_FloatCompareEpsilon = 0.0001f;
        const XrAsyncRequestIdFB k_DiscoverSpacesTestRequestId = 99UL;
        const XrSpace k_TestSpaceId = 456UL;
        static readonly XrUuidEXT k_TestPlaneUuid = new (23UL, 86UL);

        private static readonly XrVector2f[] TestPlanePoints =
        {
            new(1.1F, 0.0F), new(0.0F, 1.1F), new(-0.9F, -0.1F)
        };

        static unsafe IntPtr s_DiscoverSpacesMETAPtr =
            Marshal.GetFunctionPointerForDelegate((XrDiscoverSpacesMETA_Delegate)DiscoverSpacesMETA_MockDelegate);

        static unsafe IntPtr s_RetrieveSpaceDiscoveryResults_OneTestPlane_Ptr =
            Marshal.GetFunctionPointerForDelegate(
                (XrRetrieveSpaceDiscoveryResultsMETA_Delegate)RetrieveSpaceDiscoveryResultsMETA_OneTestPlane_MockDelegate);

        static unsafe IntPtr s_RetrieveSpaceDiscoveryResults_NoPlanes_Ptr =
            Marshal.GetFunctionPointerForDelegate(
                (XrRetrieveSpaceDiscoveryResultsMETA_Delegate)RetrieveSpaceDiscoveryResultsMETA_NoPlanes_MockDelegate);

        static unsafe IntPtr s_SpaceComponentStatusFBPtr =
            Marshal.GetFunctionPointerForDelegate((XrGetSpaceComponentStatusFB_Delegate)SpaceComponentStatusFB_MockDelegate);

        static unsafe IntPtr s_LocateSpacePtr =
            Marshal.GetFunctionPointerForDelegate((XrLocateSpace_Delegate)LocateSpace_MockDelegate);

        static unsafe IntPtr s_SpaceBoundingBox2DFBPtr =
            Marshal.GetFunctionPointerForDelegate((XrGetSpaceBoundingBox2DFB_Delegate)SpaceBoundingBox2DFB_MockDelegate);

        static unsafe IntPtr s_LocateViewsPtr =
            Marshal.GetFunctionPointerForDelegate((XrLocateViews_Delegate)LocateViews_MockDelegate);

        static unsafe IntPtr s_SpaceBoundary2DFBSuccessPtr =
            Marshal.GetFunctionPointerForDelegate((XrGetSpaceBoundary2DFB_Delegate)SpaceBoundary2DFB_SuccessMockDelegate);

        static unsafe IntPtr s_SpaceBoundary2DFBFailurePtr =
            Marshal.GetFunctionPointerForDelegate((XrGetSpaceBoundary2DFB_Delegate)SpaceBoundary2DFB_FailureMockDelegate);

        static unsafe IntPtr s_SpatialEntityPropertiesSupported_MockCallback =
            Marshal.GetFunctionPointerForDelegate(
                (GetSystemProperties_Delegate)SpatialEntityPropertiesSupported_MockCallback);

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

        static unsafe void SpaceBoundary2DFB_MockDelegate(
            XrSession session,
            XrSpace space,
            XrBoundary2DFB* boundary2DOutput)
        {
            Assert.IsTrue(boundary2DOutput != null);

            if (boundary2DOutput->vertices == null)
            {
                boundary2DOutput->vertexCountOutput = (uint)TestPlanePoints.Length;
            }
            else
            {
                boundary2DOutput->vertexCountOutput = (uint)TestPlanePoints.Length;
                for (var i = 0; i < TestPlanePoints.Length; ++i)
                {
                    boundary2DOutput->vertices[i] = TestPlanePoints[i];
                }
            }
        }

        [MonoPInvokeCallback(typeof(XrGetSpaceBoundary2DFB_Delegate))]
        static unsafe XrResult SpaceBoundary2DFB_SuccessMockDelegate(
            XrSession session,
            XrSpace space,
            XrBoundary2DFB* boundary2DOutput)
        {
            SpaceBoundary2DFB_MockDelegate(session, space, boundary2DOutput);
            return XrResult.Success;
        }

        [MonoPInvokeCallback(typeof(XrGetSpaceBoundary2DFB_Delegate))]
        static unsafe XrResult SpaceBoundary2DFB_FailureMockDelegate(
            XrSession session,
            XrSpace space,
            XrBoundary2DFB* boundary2DOutput)
        {
            SpaceBoundary2DFB_MockDelegate(session, space, boundary2DOutput);
            return XrResult.FunctionUnsupported;
        }

        [MonoPInvokeCallback(typeof(XrLocateViews_Delegate))]
        static unsafe XrResult LocateViews_MockDelegate(
            XrSession session,
            XrViewLocateInfo* viewLocateInfo,
            XrViewState* viewState,
            uint viewCapacityInput,
            uint* viewCountOutput,
            XrView* views)
        {
            viewState->viewStateFlags = XrViewStateFlags.PositionTracked | XrViewStateFlags.OrientationTracked;
            *viewCountOutput = 0;
            return XrResult.Success;
        }

        [MonoPInvokeCallback(typeof(XrGetSpaceBoundingBox2DFB_Delegate))]
        static unsafe XrResult SpaceBoundingBox2DFB_MockDelegate(
            XrSession session,
            XrSpace space,
            XrRect2Df* boundingBox2DOutput)
        {
            return XrResult.Success;
        }

        [MonoPInvokeCallback(typeof(XrLocateSpace_Delegate))]
        static unsafe XrResult LocateSpace_MockDelegate(
            XrSpace space,
            XrSpace baseSpace,
            XrTime time,
            XrSpaceLocation* location)
        {
            location->locationFlags = XrSpaceLocationFlags.XR_SPACE_LOCATION_ORIENTATION_VALID_BIT |
                                      XrSpaceLocationFlags.XR_SPACE_LOCATION_POSITION_VALID_BIT;
            // this quaternion represents a right-handed rotation about the X-axis by 30 degrees
            // q = 0.966 + 0.259i (using an online quaternion calculator)
            location->pose.Orientation.X = 0.966F;
            location->pose.Orientation.Y = 0.259F;
            location->pose.Orientation.Z = 0.0F;
            location->pose.Orientation.W = 0.0F;
            return XrResult.Success;
        }

        [MonoPInvokeCallback(typeof(XrGetSpaceComponentStatusFB_Delegate))]
        static unsafe XrResult SpaceComponentStatusFB_MockDelegate(
            XrSpace space,
            XrSpaceComponentTypeFB componentType,
            XrSpaceComponentStatusFB* status)
        {
            Assert.IsTrue(space == k_TestSpaceId);
            Assert.IsTrue(componentType == XrSpaceComponentTypeFB.XR_SPACE_COMPONENT_TYPE_LOCATABLE_FB);

            Assert.IsTrue(status != null);
            {
                status->type = XrStructureType.XR_TYPE_SPACE_COMPONENT_STATUS_FB;
                status->next = null;
                status->enabled = new XrBool32(true);
                status->changePending = new XrBool32(false);
            }

            return XrResult.Success;
        }

        [MonoPInvokeCallback(typeof(XrRetrieveSpaceDiscoveryResultsMETA_Delegate))]
        static unsafe XrResult RetrieveSpaceDiscoveryResultsMETA_NoPlanes_MockDelegate(
            XrSession session,
            XrAsyncRequestIdFB requestId,
            XrSpaceDiscoveryResultsMETA* results)
        {
            results->resultCountOutput = 0;
            return XrResult.Success;
        }

        [MonoPInvokeCallback(typeof(XrRetrieveSpaceDiscoveryResultsMETA_Delegate))]
        static unsafe XrResult RetrieveSpaceDiscoveryResultsMETA_OneTestPlane_MockDelegate(
            XrSession session,
            XrAsyncRequestIdFB requestId,
            XrSpaceDiscoveryResultsMETA* results)
        {
            if (results->results == null)
            {
                results->resultCountOutput = 1;
            }
            else
            {
                var result = results->results;
                result->space = k_TestSpaceId;
                result->uuid = k_TestPlaneUuid;
            }

            return XrResult.Success;
        }

        [MonoPInvokeCallback(typeof(XrDiscoverSpacesMETA_Delegate))]
        static unsafe XrResult DiscoverSpacesMETA_MockDelegate(
            XrSession session,
            XrSpaceDiscoveryInfoMETA* info,
            XrAsyncRequestIdFB* requestId)
        {
            for (uint i = 0; i < info->filterCount; i++)
            {
                var filter = info->filters[i];
                switch (filter->type)
                {
                    case XrStructureType.XR_TYPE_SPACE_FILTER_COMPONENT_META:
                    {
                        break;
                    }
                    default:
                    {
                        Debug.LogError($"Unknown XR structure type: {filter->type}");
                        break;
                    }
                }
            }

            *requestId = k_DiscoverSpacesTestRequestId;

            return XrResult.Success;
        }

        unsafe void EnqueueSpaceDiscoveryCompleteEvent()
        {
            var discoveryCompleteEvent = new XrEventDataSpaceDiscoveryCompleteMETA();
            discoveryCompleteEvent.type = XrStructureType.XR_TYPE_EVENT_DATA_SPACE_DISCOVERY_COMPLETE_META;
            discoveryCompleteEvent.next = null;
            discoveryCompleteEvent.requestId = k_DiscoverSpacesTestRequestId;
            discoveryCompleteEvent.result = XrResult.Success;
            EnqueueMockEvent(discoveryCompleteEvent);
        }

        unsafe void EnqueueSpaceDiscoveryResultsAvailableEvent()
        {
            var resultsAvailableEvent = new XrEventDataSpaceDiscoveryResultsAvailableMETA();
            resultsAvailableEvent.type = XrStructureType.XR_TYPE_EVENT_DATA_SPACE_DISCOVERY_RESULTS_AVAILABLE_META;
            resultsAvailableEvent.next = null;
            resultsAvailableEvent.requestId = k_DiscoverSpacesTestRequestId;
            EnqueueMockEvent(resultsAvailableEvent);
        }

        [SetUp]
        public void Setup()
        {
            mockRuntimeEnvironment.Settings.EnableFeature<MockRuntime>(true);
            mockRuntimeEnvironment.Settings.EnableFeature<ARSessionFeature>(true);
            mockRuntimeEnvironment.Settings.EnableFeature<ARPlaneFeature>(true);

            mockRuntimeEnvironment.AddSupportedExtension(TestConstants.k_XrFbSpatialEntityExtName, Constants.XR_FB_spatial_entity_SPEC_VERSION);
            mockRuntimeEnvironment.AddSupportedExtension(TestConstants.k_XrFbSceneExtName, Constants.XR_FB_scene_SPEC_VERSION);
            mockRuntimeEnvironment.AddSupportedExtension(TestConstants.k_XrMetaSpatialEntityDiscoveryExtName, Constants.XR_META_spatial_entity_discovery_SPEC_VERSION);

            mockRuntimeEnvironment.SetSysPropertiesFunctionForXrStructureType(
                (uint)XrStructureType.XR_TYPE_SYSTEM_SPATIAL_ENTITY_PROPERTIES_FB,
                s_SpatialEntityPropertiesSupported_MockCallback);
        }

        [Test]
        public void Test_SubsystemStartAndStop_Works()
        {
            mockRuntimeEnvironment.Start();

            var loader = XRGeneralSettings.Instance.Manager.activeLoader;
            Assert.IsNotNull(loader);

            var planeSubsystem = loader != null ? loader.GetLoadedSubsystem<XRPlaneSubsystem>() : null;
            Assert.IsNotNull(planeSubsystem);
            Assert.IsTrue(planeSubsystem is MetaOpenXRPlaneSubsystem);

            var planeProvider = planeSubsystem.GetProvider();
            Assert.IsNotNull(planeProvider);
            Assert.IsTrue(planeProvider.GetType().Name == "MetaOpenXRPlaneProvider");

            Assert.IsFalse(planeSubsystem.running);

            planeSubsystem.Start();

            Assert.IsTrue(planeSubsystem.running);

            planeSubsystem.Stop();

            Assert.IsFalse(planeSubsystem.running);
        }

        [Test]
        public void Test_PlaneAlignmentThreshold_Works()
        {
            mockRuntimeEnvironment.Start();

            var loader = XRGeneralSettings.Instance.Manager.activeLoader;
            var planeSubsystem = loader != null ? loader.GetLoadedSubsystem<XRPlaneSubsystem>() : null;
            Assert.IsNotNull(planeSubsystem);

            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrLocateViewsFuncName, s_LocateViewsPtr);
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrDiscoverSpacesMetaFuncName, s_DiscoverSpacesMETAPtr);
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrGetSpaceComponentsStatusFbFuncName, s_SpaceComponentStatusFBPtr);
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrLocateSpaceFuncName, s_LocateSpacePtr);
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrGetSpaceBoundingBox2dFbFuncName, s_SpaceBoundingBox2DFBPtr);

            planeSubsystem.Start();

            // Note: this call will lazily start the asynchronous plane discovery process
            planeSubsystem.GetChanges(Allocator.Temp);

            // Next, time to mock some planes
            // ... this should invoke plane additions
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrRetrieveSpaceDiscoveryResultsMetaFuncName, s_RetrieveSpaceDiscoveryResults_OneTestPlane_Ptr);

            MetaOpenXRPlaneSubsystem.planeAlignmentThreshold = 0.5F;
            EnqueueSpaceDiscoveryResultsAvailableEvent();
            mockRuntimeEnvironment.ProcessEventQueue();
            EnqueueSpaceDiscoveryCompleteEvent();
            mockRuntimeEnvironment.ProcessEventQueue();

            var planeChanges = planeSubsystem.GetChanges(Allocator.Temp);
            Assert.IsTrue(planeChanges.added.Length == 1);
            Assert.IsTrue(planeChanges.added[0].alignment == PlaneAlignment.HorizontalUp);

            // Note: this call will lazily start the asynchronous plane discovery process
            planeSubsystem.GetChanges(Allocator.Temp);

            // Next, time to announce that the plane discovery is finished
            // ... this should invoke plane removals
            XrFuncTableUtils.ClearCachedFunc(TestConstants.k_XrRetrieveSpaceDiscoveryResultsMetaFuncName);
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrRetrieveSpaceDiscoveryResultsMetaFuncName, s_RetrieveSpaceDiscoveryResults_NoPlanes_Ptr);

            EnqueueSpaceDiscoveryResultsAvailableEvent();
            mockRuntimeEnvironment.ProcessEventQueue();
            EnqueueSpaceDiscoveryCompleteEvent();
            mockRuntimeEnvironment.ProcessEventQueue();

            planeChanges = planeSubsystem.GetChanges(Allocator.Temp);
            Assert.IsTrue(planeChanges.removed.Length == 1);

            // Next, time to mock some planes again
            // ... this should invoke plane additions
            XrFuncTableUtils.ClearCachedFunc(TestConstants.k_XrRetrieveSpaceDiscoveryResultsMetaFuncName);
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrRetrieveSpaceDiscoveryResultsMetaFuncName, s_RetrieveSpaceDiscoveryResults_OneTestPlane_Ptr);

            MetaOpenXRPlaneSubsystem.planeAlignmentThreshold = 0.0F;
            EnqueueSpaceDiscoveryResultsAvailableEvent();
            mockRuntimeEnvironment.ProcessEventQueue();
            EnqueueSpaceDiscoveryCompleteEvent();
            mockRuntimeEnvironment.ProcessEventQueue();

            planeChanges = planeSubsystem.GetChanges(Allocator.Temp);
            Assert.IsTrue(planeChanges.added.Length == 1);
            Assert.IsTrue(planeChanges.added[0].alignment == PlaneAlignment.NotAxisAligned);

            planeSubsystem.Stop();
        }

        [Test]
        public void Test_PlaneDetectionMode_Works()
        {
            mockRuntimeEnvironment.Start();

            var loader = XRGeneralSettings.Instance.Manager.activeLoader;
            var planeSubsystem = loader != null ? loader.GetLoadedSubsystem<XRPlaneSubsystem>() : null;
            Assert.IsNotNull(planeSubsystem);

            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrLocateViewsFuncName, s_LocateViewsPtr);
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrDiscoverSpacesMetaFuncName, s_DiscoverSpacesMETAPtr);
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrGetSpaceComponentsStatusFbFuncName, s_SpaceComponentStatusFBPtr);
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrLocateSpaceFuncName, s_LocateSpacePtr);
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrGetSpaceBoundingBox2dFbFuncName, s_SpaceBoundingBox2DFBPtr);

            planeSubsystem.Start();

            // Note: this call will lazily start the asynchronous plane discovery process
            planeSubsystem.GetChanges(Allocator.Temp);

            // Next, time to mock some planes
            // ... this should invoke plane additions
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrRetrieveSpaceDiscoveryResultsMetaFuncName, s_RetrieveSpaceDiscoveryResults_OneTestPlane_Ptr);

            planeSubsystem.requestedPlaneDetectionMode = PlaneDetectionMode.NotAxisAligned;
            EnqueueSpaceDiscoveryResultsAvailableEvent();
            mockRuntimeEnvironment.ProcessEventQueue();
            EnqueueSpaceDiscoveryCompleteEvent();
            mockRuntimeEnvironment.ProcessEventQueue();

            // Test that a NotAxisAligned plane WILL be included if the detection mode includes NotAxisAligned
            var planeChanges = planeSubsystem.GetChanges(Allocator.Temp);
            Assert.IsTrue(planeChanges.added.Length == 1);

            // Note: this call will lazily start the asynchronous plane discovery process
            planeSubsystem.GetChanges(Allocator.Temp);

            // Next, time to announce that the plane discovery is finished
            // ... this should invoke plane removals
            XrFuncTableUtils.ClearCachedFunc(TestConstants.k_XrRetrieveSpaceDiscoveryResultsMetaFuncName);
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrRetrieveSpaceDiscoveryResultsMetaFuncName, s_RetrieveSpaceDiscoveryResults_NoPlanes_Ptr);

            EnqueueSpaceDiscoveryResultsAvailableEvent();
            mockRuntimeEnvironment.ProcessEventQueue();
            EnqueueSpaceDiscoveryCompleteEvent();
            mockRuntimeEnvironment.ProcessEventQueue();

            planeChanges = planeSubsystem.GetChanges(Allocator.Temp);
            Assert.IsTrue(planeChanges.removed.Length == 1);

            // Note: this call will lazily start the asynchronous plane discovery process
            planeSubsystem.GetChanges(Allocator.Temp);

            // Next, time to mock some planes
            // ... this should invoke plane additions
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrRetrieveSpaceDiscoveryResultsMetaFuncName, s_RetrieveSpaceDiscoveryResults_OneTestPlane_Ptr);

            planeSubsystem.requestedPlaneDetectionMode = PlaneDetectionMode.Horizontal;
            EnqueueSpaceDiscoveryResultsAvailableEvent();
            mockRuntimeEnvironment.ProcessEventQueue();
            EnqueueSpaceDiscoveryCompleteEvent();
            mockRuntimeEnvironment.ProcessEventQueue();

            // Test that a NotAxisAligned plane will NOT be included if the detection mode does not include NotAxisAligned
            planeChanges = planeSubsystem.GetChanges(Allocator.Temp);
            Assert.IsTrue(planeChanges.added.Length == 0);

            planeSubsystem.Stop();
        }

        [Test]
        public void Test_GetPlaneBoundary_Works()
        {
            mockRuntimeEnvironment.Start();

            var loader = XRGeneralSettings.Instance.Manager.activeLoader;
            var planeSubsystem = loader != null ? loader.GetLoadedSubsystem<XRPlaneSubsystem>() : null;

            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrLocateViewsFuncName, s_LocateViewsPtr);
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrDiscoverSpacesMetaFuncName, s_DiscoverSpacesMETAPtr);
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrGetSpaceComponentsStatusFbFuncName, s_SpaceComponentStatusFBPtr);
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrLocateSpaceFuncName, s_LocateSpacePtr);
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrGetSpaceBoundingBox2dFbFuncName, s_SpaceBoundingBox2DFBPtr);

            planeSubsystem.Start();

            // Note: this call will lazily start the asynchronous plane discovery process
            var planeChanges = planeSubsystem.GetChanges(Allocator.Temp);
            Assert.IsNotNull(planeChanges);
            Assert.IsTrue(planeChanges.added.Length == 0);
            Assert.IsTrue(planeChanges.removed.Length == 0);

            // Next, time to mock some planes
            // ... this should invoke plane additions
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrRetrieveSpaceDiscoveryResultsMetaFuncName, s_RetrieveSpaceDiscoveryResults_OneTestPlane_Ptr);

            EnqueueSpaceDiscoveryResultsAvailableEvent();
            mockRuntimeEnvironment.ProcessEventQueue();
            EnqueueSpaceDiscoveryCompleteEvent();
            mockRuntimeEnvironment.ProcessEventQueue();

            planeChanges = planeSubsystem.GetChanges(Allocator.Temp);
            Assert.IsTrue(planeChanges.added.Length == 1);
            var boundedPlane = planeChanges.added[0];

            // first check for successful boundary retrieval
            NativeArray<Vector2> boundary1 = new NativeArray<Vector2>();
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrGetSpaceBoundary2dFbFuncName, s_SpaceBoundary2DFBSuccessPtr);
            planeSubsystem.GetBoundary(boundedPlane.trackableId, Allocator.TempJob, ref boundary1);
            Assert.IsTrue(boundary1.Length == TestPlanePoints.Length);

            // note: since we go from a right-handed coordinate system to a left-handed
            // coordinate system, the winding order should be reversed
            for (var i = 0; i < boundary1.Length; ++i)
            {
                var inverseIndex = boundary1.Length - i - 1;
                Assert.IsTrue(Math.Abs(boundary1[i].x - TestPlanePoints[inverseIndex].X) < k_FloatCompareEpsilon);
                Assert.IsTrue(Math.Abs(boundary1[i].y - TestPlanePoints[inverseIndex].Y) < k_FloatCompareEpsilon);
            }

            planeSubsystem.Stop();
        }

        [Test]
        public void Test_GetTrackableChanges_Works()
        {
            mockRuntimeEnvironment.Start();

            var loader = XRGeneralSettings.Instance.Manager.activeLoader;
            var planeSubsystem = loader != null ? loader.GetLoadedSubsystem<XRPlaneSubsystem>() : null;
            Assert.IsNotNull(planeSubsystem);

            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrLocateViewsFuncName, s_LocateViewsPtr);
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrDiscoverSpacesMetaFuncName, s_DiscoverSpacesMETAPtr);
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrGetSpaceComponentsStatusFbFuncName, s_SpaceComponentStatusFBPtr);
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrLocateSpaceFuncName, s_LocateSpacePtr);
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrGetSpaceBoundingBox2dFbFuncName, s_SpaceBoundingBox2DFBPtr);

            planeSubsystem.Start();

            // Note: this call will lazily start the asynchronous plane discovery process
            var planeChanges = planeSubsystem.GetChanges(Allocator.Temp);
            Assert.IsNotNull(planeChanges);
            Assert.IsTrue(planeChanges.added.Length == 0);
            Assert.IsTrue(planeChanges.removed.Length == 0);

            // Next, time to mock some planes
            // ... this should invoke plane additions
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrRetrieveSpaceDiscoveryResultsMetaFuncName, s_RetrieveSpaceDiscoveryResults_OneTestPlane_Ptr);

            EnqueueSpaceDiscoveryResultsAvailableEvent();
            mockRuntimeEnvironment.ProcessEventQueue();
            EnqueueSpaceDiscoveryCompleteEvent();
            mockRuntimeEnvironment.ProcessEventQueue();

            planeChanges = planeSubsystem.GetChanges(Allocator.Temp);
            Assert.IsTrue(planeChanges.added.Length == 1);

            // Note: this call will lazily start the asynchronous plane discovery process
            planeChanges = planeSubsystem.GetChanges(Allocator.Temp);
            Assert.IsNotNull(planeChanges);
            Assert.IsTrue(planeChanges.added.Length == 0);
            Assert.IsTrue(planeChanges.removed.Length == 0);

            // Next, time to announce that the plane discovery is finished
            // ... this should invoke plane removals
            XrFuncTableUtils.ClearCachedFunc(TestConstants.k_XrRetrieveSpaceDiscoveryResultsMetaFuncName);
            mockRuntimeEnvironment.SetFunctionForInterceptor(TestConstants.k_XrRetrieveSpaceDiscoveryResultsMetaFuncName, s_RetrieveSpaceDiscoveryResults_NoPlanes_Ptr);

            EnqueueSpaceDiscoveryResultsAvailableEvent();
            mockRuntimeEnvironment.ProcessEventQueue();
            EnqueueSpaceDiscoveryCompleteEvent();
            mockRuntimeEnvironment.ProcessEventQueue();

            planeChanges = planeSubsystem.GetChanges(Allocator.Temp);
            Assert.IsTrue(planeChanges.removed.Length == 1);

            planeSubsystem.Stop();
        }
    }
}
#endif // OPENXR_PLUGIN_1_16_0_PRE_2_OR_NEWER
#endif // ENABLE_MOCK_RUNTIME_TESTS
