using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR.NativeTypes;
using Unity.Collections;
using System.Threading;
using System.Runtime.InteropServices;
using static UnityEngine.XR.ARSubsystems.XRResultStatus;
using System;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// OpenXR Meta implementation of <see cref="XRRaycastSubsystem"/>. This implementation performs
    /// provider-based ray casts, and allows the [ARRaycastManager](xref:UnityEngine.XR.ARFoundation.ARRaycastManager)
    /// to execute the fallback ray casts.
    /// </summary>
    public sealed class MetaOpenXRRaycastSubsystem : XRRaycastSubsystem
    {
        internal const string k_SubsystemId = "Meta-Raycast";

        const int XR_ERROR_PERMISSION_INSUFFICIENT = -1000710000;
        const string k_ScenePermissionError = "To use environmental ray casts, your app must first be granted the permission com.oculus.permission.USE_SCENE."
            + " Don't enable the AR Raycast Manager component until permission is granted."
            + " Refer to the Unity OpenXR: Meta package documentation for more information.";

        class MetaOpenXRRaycastProvider : Provider
        {
            CancellationTokenSource m_TokenSource;
            bool m_PermissionDenied;

            protected override bool TryInitialize()
            {
                NativeApi.Create();
                return true;
            }

            public override void Start()
            {
                var result = TryStart();

                if (result.IsError())
                    return;

                m_TokenSource = new CancellationTokenSource();
                WaitForRaycasterReady(m_TokenSource.Token);
            }

            XRResultStatus TryStart()
            {
                var result = NativeApi.Start();

                if (result.IsSuccess())
                {
                    m_PermissionDenied = false;
                    return result;
                }
                if (result.nativeStatusCode == XR_ERROR_PERMISSION_INSUFFICIENT)
                {
                    Debug.LogError(k_ScenePermissionError);
                    m_PermissionDenied = true;
                }
                else if (result.IsError() && result.statusCode == StatusCode.PlatformError)
                {
                    Debug.LogError($"Raycast subsystem failed to start with error: {(XrResult)result.nativeStatusCode}");
                }
                else if (result.IsError())
                {
                    Debug.LogError("Raycast subsystem failed to start. Check logs for more information.");
                }

                return result;
            }

            public override void Stop()
            {
                if (m_PermissionDenied)
                    return;

                m_TokenSource?.Cancel();
                NativeApi.CancelOrDestroyRaycaster();
            }

            public override void Destroy()
            {
                NativeApi.Destroy();
            }

            public override NativeArray<XRRaycastHit> Raycast(XRRaycastHit defaultRaycastHit, Ray ray, TrackableType trackableTypeMask, Allocator allocator)
            {
                if (m_PermissionDenied)
                {
                    Debug.LogError(k_ScenePermissionError);
                    return new NativeArray<XRRaycastHit>(0, allocator);
                }

                var hit = XRRaycastHit.defaultValue;
                XRResultStatus resultStatus = NativeApi.Raycast(ray.origin, ray.direction, ref hit);
                if (resultStatus.IsError() || hit.hitType == TrackableType.None)
                    return new NativeArray<XRRaycastHit>(0, allocator);

                var toReturn = new NativeArray<XRRaycastHit>(1, allocator);
                toReturn[0] = hit;
                return toReturn;
            }

            async void WaitForRaycasterReady(CancellationToken cancelToken)
            {
                if (m_PermissionDenied)
                    return;

                while (!NativeApi.IsRaycasterAsyncReady() && !cancelToken.IsCancellationRequested)
                {
                    try
                    {
                        await Awaitable.NextFrameAsync(cancelToken);
                    }
                    catch (OperationCanceledException) {}
                }
                if (!cancelToken.IsCancellationRequested)
                    NativeApi.CompleteRaycaster();
            }

            static unsafe class NativeApi
            {
                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_Raycast_Create")]
                internal static extern void Create();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_Raycast_Start")]
                internal static extern XRResultStatus Start();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_Raycast_Destroy")]
                internal static extern XRResultStatus Destroy();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_Raycast_Raycast")]
                internal static extern XRResultStatus Raycast(Vector3 origin, Vector3 direction, ref XRRaycastHit raycastHit);

                [return : MarshalAs(UnmanagedType.U1)]
                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_Raycast_IsRaycasterAsyncReady")]
                internal static extern bool IsRaycasterAsyncReady();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_Raycast_CompleteRaycaster")]
                internal static extern XRResultStatus CompleteRaycaster();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_Raycast_CancelOrDestroyRaycaster")]
                internal static extern void CancelOrDestroyRaycaster();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            XRRaycastSubsystemDescriptor.Register(new XRRaycastSubsystemDescriptor.Cinfo
            {
                id = k_SubsystemId,
                providerType = typeof(MetaOpenXRRaycastProvider),
                subsystemTypeOverride = typeof(MetaOpenXRRaycastSubsystem),
                supportsViewportBasedRaycast = false,
                supportsWorldBasedRaycast = true,
                supportedTrackableTypes = TrackableType.Depth,
                supportsTrackedRaycasts = false,
            });
        }
    }
}
