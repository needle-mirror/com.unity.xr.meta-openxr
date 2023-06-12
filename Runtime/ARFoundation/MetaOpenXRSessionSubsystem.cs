using System;
using System.Runtime.InteropServices;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// The Meta-OpenXR implementation of the <see cref="XRSessionSubsystem"/>.
    /// Do not create this directly. Use the <see cref="SubsystemManager"/> instead.
    /// </summary>
    public sealed class MetaOpenXRSessionSubsystem : XRSessionSubsystem
    {
        internal const string k_SubsystemId = "Meta-Session";

        class MetaOpenXRProvider : Provider
        {
            /// <inheritdoc/>
            public MetaOpenXRProvider() => NativeApi.UnityOpenXRMeta_Session_Construct();

            /// <inheritdoc/>
            public override void Start() => NativeApi.UnityOpenXRMeta_Session_Start();

            /// <inheritdoc/>
            public override void Stop() => NativeApi.UnityOpenXRMeta_Session_Stop();

            /// <inheritdoc/>
            public override void Destroy() => NativeApi.UnityOpenXRMeta_Session_Destruct();

            public override Promise<SessionAvailability> GetAvailabilityAsync() =>
                Promise<SessionAvailability>.CreateResolvedPromise(SessionAvailability.Supported | SessionAvailability.Installed);

        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            XRSessionSubsystemDescriptor.RegisterDescriptor(new XRSessionSubsystemDescriptor.Cinfo
            {
                id = k_SubsystemId,
                providerType = typeof(MetaOpenXRProvider),
                subsystemTypeOverride = typeof(MetaOpenXRSessionSubsystem),
                supportsInstall = false,
                supportsMatchFrameRate = false
            });
        }

        static class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern void UnityOpenXRMeta_Session_Construct();

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern void UnityOpenXRMeta_Session_Destruct();

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern void UnityOpenXRMeta_Session_Start();

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern void UnityOpenXRMeta_Session_Stop();
        }
    }
}
