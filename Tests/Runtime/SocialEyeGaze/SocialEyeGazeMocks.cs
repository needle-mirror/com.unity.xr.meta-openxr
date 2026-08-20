using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine.XR.OpenXR.NativeTypes;

using XrSession = System.UInt64;
using XrEyeTrackerFB = System.UInt64;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    /// <summary>
    /// Owns all native interceptor machinery for XR_FB_eye_tracking_social mock tests.
    /// Holds per-test mock delegates, native ABI pinning, and the static routing to the active instance.
    /// </summary>
    class SocialEyeGazeMocks
    {
        // Friendly delegate types — used by tests to configure per-test behavior
        public delegate XrResult CreateEyeTrackerDelegate(XrSession session, out XrEyeTrackerFB tracker);
        public delegate XrResult GetEyeGazesDelegate(XrEyeTrackerFB tracker, out XrEyeGazesFB gazes);
        public delegate XrResult DestroyEyeTrackerDelegate(XrEyeTrackerFB tracker);

        // Per-test mocks — assign before calling MockSocialEyeGazeEnvironment.Start()
        public CreateEyeTrackerDelegate createEyeTracker;
        public GetEyeGazesDelegate getEyeGazes;
        public DestroyEyeTrackerDelegate destroyEyeTracker;

        // Active instance — routes static interceptors to the current test's mock delegates
        static SocialEyeGazeMocks s_Current;

        internal static void Activate(SocialEyeGazeMocks mocks) => s_Current = mocks;
        internal static void Deactivate() => s_Current = null;

        // Native function pointers — passed to MockOpenXREnvironment.SetFunctionForInterceptor
        internal static IntPtr CreatePtr => s_CreatePtr;
        internal static IntPtr GetGazesPtr => s_GetGazesPtr;
        internal static IntPtr DestroyPtr => s_DestroyPtr;

        internal static IntPtr EyeTrackingSupportedPtr => k_EyeTrackingSupportedPtr;
        internal static IntPtr EyeTrackingUnsupportedPtr => k_EyeTrackingUnsupportedPtr;

        // Native ABI delegate types — must exactly match the C function signatures.
        // Stored as static readonly fields to prevent GC collection of the
        // trampoline the native runtime holds a pointer into.
        delegate XrResult CreateNative(
            XrSession session, in XrEyeTrackerCreateInfoFB createInfo, out XrEyeTrackerFB eyeTracker);
        delegate XrResult GetGazesNative(
            XrEyeTrackerFB eyeTracker, in XrEyeGazesInfoFB gazesInfo, out XrEyeGazesFB eyeGazes);
        delegate XrResult DestroyNative(XrEyeTrackerFB eyeTracker);

        static readonly CreateNative s_CreatePinned = CreateInterceptor;
        static readonly GetGazesNative s_GetGazesPinned = GetGazesInterceptor;
        static readonly DestroyNative s_DestroyPinned = DestroyInterceptor;

        static readonly IntPtr s_CreatePtr = Marshal.GetFunctionPointerForDelegate(s_CreatePinned);
        static readonly IntPtr s_GetGazesPtr = Marshal.GetFunctionPointerForDelegate(s_GetGazesPinned);
        static readonly IntPtr s_DestroyPtr = Marshal.GetFunctionPointerForDelegate(s_DestroyPinned);

        static readonly unsafe IntPtr k_EyeTrackingSupportedPtr =
            Marshal.GetFunctionPointerForDelegate(
                (GetSystemProperties_delegate)EyeTrackingSupported_MockCallback);
        static readonly unsafe IntPtr k_EyeTrackingUnsupportedPtr =
            Marshal.GetFunctionPointerForDelegate(
                (GetSystemProperties_delegate)EyeTrackingUnsupported_MockCallback);

        // Interceptors — called by the native mock runtime, routed to s_Current
        [MonoPInvokeCallback(typeof(CreateNative))]
        static XrResult CreateInterceptor(
            XrSession session, in XrEyeTrackerCreateInfoFB createInfo, out XrEyeTrackerFB eyeTracker)
        {
            eyeTracker = 0;
            if (s_Current?.createEyeTracker == null)
            {
                return XrResult.FunctionUnsupported;
            }

            try
            {
                return s_Current.createEyeTracker(session, out eyeTracker);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return XrResult.RuntimeFailure;
            }
        }

        [MonoPInvokeCallback(typeof(GetGazesNative))]
        static XrResult GetGazesInterceptor(
            XrEyeTrackerFB eyeTracker, in XrEyeGazesInfoFB gazesInfo, out XrEyeGazesFB eyeGazes)
        {
            eyeGazes = XrEyeGazesFB.defaultValue;
            if (s_Current?.getEyeGazes == null)
            {
                return XrResult.FunctionUnsupported;
            }

            try
            {
                return s_Current.getEyeGazes(eyeTracker, out eyeGazes);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return XrResult.RuntimeFailure;
            }
        }

        [MonoPInvokeCallback(typeof(DestroyNative))]
        static XrResult DestroyInterceptor(XrEyeTrackerFB eyeTracker)
        {
            if (s_Current?.destroyEyeTracker == null)
            {
                return XrResult.FunctionUnsupported;
            }

            try
            {
                return s_Current.destroyEyeTracker(eyeTracker);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return XrResult.RuntimeFailure;
            }
        }

        // System properties callbacks — report eye tracking hardware support
        [MonoPInvokeCallback(typeof(GetSystemProperties_delegate))]
        static unsafe void EyeTrackingSupported_MockCallback(XrSystemPropertiesBaseHeader* header)
        {
            if (header == null)
                return;

            var props = (XrSystemEyeTrackingPropertiesFB*)header;
            props->supportsEyeTracking = true;
        }

        [MonoPInvokeCallback(typeof(GetSystemProperties_delegate))]
        static unsafe void EyeTrackingUnsupported_MockCallback(XrSystemPropertiesBaseHeader* header)
        {
            if (header == null)
                return;

            var props = (XrSystemEyeTrackingPropertiesFB*)header;
            props->supportsEyeTracking = false;
        }
    }
}
