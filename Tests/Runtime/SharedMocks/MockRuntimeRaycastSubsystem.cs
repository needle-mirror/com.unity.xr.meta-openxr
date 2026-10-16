using AOT;
using System;
#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif
using UnityEngine.XR.OpenXR.NativeTypes;

using XrSession = System.UInt64;
using XrFutureEXT = System.UInt64;
using XrEnvironmentRaycasterMETA = System.UInt64;
using XrInstance = System.UInt64;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    class MockRuntimeRaycastSubsystem
    {
        const XrFutureEXT k_SucceedingFuture = 1;
        const XrFutureEXT k_CancelledFuture = 2;

        // Pointers to mock callbacks that RootedFunctionPointer keeps alive for the domain's lifetime; they never change.
#if UNITY_6000_5_OR_NEWER
        [NoAutoStaticsCleanup]
#endif
        internal static unsafe IntPtr xrCreateEnvironmentRaycasterAsyncMETA_ptr =
            RootedFunctionPointer.For((xrCreateEnvironmentRaycasterAsyncMETA_delegate)xrCreateEnvironmentRaycasterAsyncMETA);

#if UNITY_6000_5_OR_NEWER
        [NoAutoStaticsCleanup]
#endif
        internal static unsafe IntPtr xrCreateEnvironmentRaycasterCompleteMETA_ptr =
            RootedFunctionPointer.For((xrCreateEnvironmentRaycasterCompleteMETA_delegate)xrCreateEnvironmentRaycasterCompleteMETA);

#if UNITY_6000_5_OR_NEWER
        [NoAutoStaticsCleanup]
#endif
        internal static unsafe IntPtr xrDestroyEnvironmentRaycasterMETA_ptr =
            RootedFunctionPointer.For((xrDestroyEnvironmentRaycasterMETA_delegate)xrDestroyEnvironmentRaycasterMETA);

#if UNITY_6000_5_OR_NEWER
        [NoAutoStaticsCleanup]
#endif
        internal static unsafe IntPtr xrPerformEnvironmentRaycastMETA_ptr_returnForwardHit =
            RootedFunctionPointer.For((xrPerformEnvironmentRaycastMETA_delegate)xrPerformEnvironmentRaycastMETA_returnForwardHit);

#if UNITY_6000_5_OR_NEWER
        [NoAutoStaticsCleanup]
#endif
        internal static unsafe IntPtr envrionmentRaycastPropertiesSupported_MockCallback =
            RootedFunctionPointer.For(
                (GetSystemProperties_delegate)EnvironmentRaycastPropertiesSupported_MockCallback);

#if UNITY_6000_5_OR_NEWER
        [NoAutoStaticsCleanup]
#endif
        internal static unsafe IntPtr xrPollFutureEXT_ptr =
            RootedFunctionPointer.For((xrPollFutureEXT_delegate)xrPollFutureEXT);

#if UNITY_6000_5_OR_NEWER
        [NoAutoStaticsCleanup]
#endif
        internal static unsafe IntPtr xrCancelFutureEXT_ptr =
            RootedFunctionPointer.For((xrCancelFutureEXT_delegate)xrCancelFutureEXT);

        [MonoPInvokeCallback(typeof(xrCreateEnvironmentRaycasterAsyncMETA_delegate))]
        internal static unsafe XrResult xrCreateEnvironmentRaycasterAsyncMETA(
            XrSession session,
            XrEnvironmentRaycasterCreateInfoMETA* info,
            XrFutureEXT* future)
        {
            if (future != null)
            {
                *future = k_SucceedingFuture;
                return XrResult.Success;
            }
            else
            {
                return XrResult.FutureInvalidEXT;
            }
        }

        [MonoPInvokeCallback(typeof(xrCreateEnvironmentRaycasterCompleteMETA_delegate))]
        internal static unsafe XrResult xrCreateEnvironmentRaycasterCompleteMETA(
            XrSession session,
            XrFutureEXT future,
            XrEnvironmentRaycasterCreateCompletionMETA* completion)
        {
            return XrResult.Success;
        }

        [MonoPInvokeCallback(typeof(xrDestroyEnvironmentRaycasterMETA_delegate))]
        internal static unsafe XrResult xrDestroyEnvironmentRaycasterMETA(XrEnvironmentRaycasterMETA environmentRaycaster)
        {
            return XrResult.Success;
        }

        [MonoPInvokeCallback(typeof(xrPerformEnvironmentRaycastMETA_delegate))]
        internal static unsafe XrResult xrPerformEnvironmentRaycastMETA_returnForwardHit(
            XrEnvironmentRaycasterMETA raycaster,
            XrEnvironmentRaycastHitGetInfoMETA* info,
            XrEnvironmentRaycastHitMETA* hit)
        {
            if (info->filterCount > 0 &&
                info->filters[0]->type == XrStructureType.XR_TYPE_ENVIRONMENT_RAYCAST_FILTER_DISTANCE_META)
            {
                var maxDistance = ((XrEnvironmentRaycastFilterDistanceMETA*)info->filters[0])->maxDistance;
                Vector3 rayOrigin = new Vector3(info->origin.X,info->origin.Y,info->origin.Z);
                if (Vector3.Distance(rayOrigin, Vector3.forward) <= maxDistance)
                {
                    hit->status = EnvironmentRaycastHitStatus.Hit;
                    hit->pose = new XrPosef(Vector3.forward, Quaternion.identity);
                    return XrResult.Success;
                }
                else
                {
                    hit->status = EnvironmentRaycastHitStatus.NoHit;
                    return XrResult.Success;
                }
            }
            else
            {
                hit->status = EnvironmentRaycastHitStatus.Hit;
                hit->pose = new XrPosef(Vector3.forward, Quaternion.identity);
                return XrResult.Success;
            }
        }

        [MonoPInvokeCallback(typeof(xrPollFutureEXT_delegate))]
        internal static unsafe XrResult xrPollFutureEXT(
            XrInstance instance,
            XrFuturePollInfoEXT* info,
            XrFuturePollResultEXT* result)
        {
            if (info->future == k_SucceedingFuture)
                result->state = XrFutureStateEXT.Ready;
            return XrResult.Success;
        }

        [MonoPInvokeCallback(typeof(xrCancelFutureEXT_delegate))]
        internal static unsafe XrResult xrCancelFutureEXT(
            XrInstance instance,
            XrFutureCancelInfoEXT* cancelInfo)
        {
            var cancelledInfo = new XrFutureCancelInfoEXT(k_CancelledFuture);
            cancelInfo = &cancelledInfo;
            return XrResult.Success;
        }

        [MonoPInvokeCallback(typeof(GetSystemProperties_delegate))]
        static unsafe void EnvironmentRaycastPropertiesSupported_MockCallback(XrSystemPropertiesBaseHeader* nativeSystemPropertiesStruct)
        {
            if (nativeSystemPropertiesStruct != null)
            {
                var environmentRaycastProperties = (XrSystemEnvironmentRaycastPropertiesMETA*)nativeSystemPropertiesStruct;
                environmentRaycastProperties->supportsEnvironmentRaycast = true;
            }
        }
    }
}
