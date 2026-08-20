using UnityEngine.XR.OpenXR.NativeTypes;
using XrSpace = System.UInt64;
using XrTime = System.Int64;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    struct XrEyeGazeFB
    {
        internal XrBool32 isValid;
        internal XrPosef gazePose;
        internal float gazeConfidence;

        internal XrEyeGazeFB(bool isValid, XrPosef gazePose, float gazeConfidence)
        {
            this.isValid = isValid;
            this.gazePose = gazePose;
            this.gazeConfidence = gazeConfidence;
        }
    }

    unsafe struct XrEyeGazesFB
    {
        internal static XrEyeGazesFB defaultValue => new(default, default, default);

        internal XrStructureType type;
        internal void* next;
        internal XrEyeGazeFB gazeLeft;
        internal XrEyeGazeFB gazeRight;
        internal XrTime time;

        internal XrEyeGazesFB(XrEyeGazeFB left, XrEyeGazeFB right, XrTime time)
        {
            type = default;
            next = null;
            gazeLeft = left;
            gazeRight = right;
            this.time = time;
        }
    }

    unsafe struct XrEyeGazesInfoFB
    {
        internal XrStructureType type;
        internal void* next;
        internal XrSpace baseSpace;
        internal XrTime time;
    }

    unsafe struct XrEyeTrackerCreateInfoFB
    {
        internal XrStructureType type;
        internal void* next;
    }

    unsafe struct XrSystemEyeTrackingPropertiesFB
    {
        internal XrStructureType type;
        internal void* next;
        internal XrBool32 supportsEyeTracking;
    }
}
