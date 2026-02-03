using UnityEngine.XR.OpenXR.NativeTypes;

using XrSession = System.UInt64;
using XrFutureEXT = System.UInt64;
using XrSpace = System.UInt64;
using XrTime = System.Int64;
using XrEnvironmentRaycasterMETA = System.UInt64;
using XrInstance = System.UInt64;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    unsafe delegate XrResult xrCreateEnvironmentRaycasterAsyncMETA_delegate(
        XrSession session,
        XrEnvironmentRaycasterCreateInfoMETA* createInfo,
        XrFutureEXT* future
        );

    unsafe delegate XrResult xrCreateEnvironmentRaycasterCompleteMETA_delegate(
        XrSession session,
        XrFutureEXT future,
        XrEnvironmentRaycasterCreateCompletionMETA* completion
        );

    unsafe delegate XrResult xrDestroyEnvironmentRaycasterMETA_delegate(XrEnvironmentRaycasterMETA environmentRaycaster);

    unsafe delegate XrResult xrPerformEnvironmentRaycastMETA_delegate(
        XrEnvironmentRaycasterMETA raycaster,
        XrEnvironmentRaycastHitGetInfoMETA* info,
        XrEnvironmentRaycastHitMETA* hit
        );

    unsafe delegate XrResult xrPollFutureEXT_delegate(
        XrInstance instance,
        XrFuturePollInfoEXT* pollInfo,
        XrFuturePollResultEXT* pollResult
        );

    unsafe delegate XrResult xrCancelFutureEXT_delegate(
        XrInstance instance,
        XrFutureCancelInfoEXT* cancelInfo
        );

    unsafe struct XrEnvironmentRaycasterCreateInfoMETA
    {
        internal XrStructureType type;
        internal void* next;
    };

    unsafe struct XrEnvironmentRaycastHitGetInfoMETA
    {
        internal XrStructureType type;
        internal void* next;
        internal XrSpace baseSpace;
        internal XrTime time;
        internal XrVector3f origin;
        internal XrVector3f direction;
        internal System.UInt32 filterCount;
        internal XrEnvironmentRaycastFilterBaseHeaderMETA** filters;
    }

    unsafe struct XrEnvironmentRaycastFilterBaseHeaderMETA
    {
        internal XrStructureType type;
        internal void* next;
    }

    unsafe struct XrEnvironmentRaycastFilterDistanceMETA
    {
        internal XrStructureType type;
        internal void* next;
        internal float maxDistance;
    }

    unsafe struct XrEnvironmentRaycastHitMETA
    {
        internal XrStructureType type;
        internal void* next;
        internal EnvironmentRaycastHitStatus status;
        internal XrPosef pose;
    }
    unsafe struct XrEnvironmentRaycasterCreateCompletionMETA
    {
        internal XrStructureType type;
        internal void* next;
        internal XrResult futureResult;
        internal XrEnvironmentRaycasterMETA environmentRaycaster;
    }

    unsafe struct XrSystemEnvironmentRaycastPropertiesMETA
    {
        internal XrStructureType type;
        internal void* next;
        internal XrBool32 supportsEnvironmentRaycast;
    };
}
