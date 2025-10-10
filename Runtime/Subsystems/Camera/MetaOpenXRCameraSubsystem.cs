//#define VERBOSE_LOGGING
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.XR.CompositionLayers;
using Unity.XR.CompositionLayers.Layers;
using Unity.XR.CompositionLayers.Services;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR.CompositionLayers;
using UnityEngine.XR.OpenXR.NativeTypes.Meta;
using UnityEngine.XR.OpenXR.NativeTypes;
using Unity.XR.CoreUtils;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// The Meta-OpenXR implementation of the <see cref="XRCameraSubsystem"/>.
    /// Do not create this directly. Use the <see cref="SubsystemManager"/> instead.
    /// </summary>
    public sealed class MetaOpenXRCameraSubsystem : XRCameraSubsystem
    {
        internal const string k_SubsystemId = "Meta-Camera";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            var cameraSubsystemCinfo = new XRCameraSubsystemDescriptor.Cinfo
            {
                id = k_SubsystemId,
                providerType = typeof(MetaOpenXRProvider),
                subsystemTypeOverride = typeof(MetaOpenXRCameraSubsystem),
                supportsAverageBrightness = false,
                supportsAverageColorTemperature = false,
                supportsColorCorrection = false,
                supportsDisplayMatrix = true,
                supportsProjectionMatrix = true,
                supportsTimestamp = true,
                // NOTE: the subsystem does not support the setting of the camera configuration for now
                supportsCameraConfigurations = false,
                // NOTE: the "world camera" image is coming from the Quest's left eye camera
#if UNITY_ANDROID && !UNITY_EDITOR
                supportsCameraImage = true,
#else
                supportsCameraImage = false,
#endif
                supportsAverageIntensityInLumens = false,
                supportsFocusModes = false,
                supportsFaceTrackingAmbientIntensityLightEstimation = false,
                supportsFaceTrackingHDRLightEstimation = false,
                supportsWorldTrackingAmbientIntensityLightEstimation = false,
                supportsWorldTrackingHDRLightEstimation = false,
                supportsCameraGrain = false,
                supportsExifData = false,
                supportsCameraTorchMode = false,
            };

            XRCameraSubsystemDescriptor.Register(cameraSubsystemCinfo);
        }

        class MetaOpenXRProvider : Provider
        {
            bool m_IsInitialized;

            protected override bool TryInitialize()
            {
                OpenXRLayerProvider.Started += CreateAndRegisterLayerHandler;
                return base.TryInitialize();
            }

            public override void Destroy()
            {
                OpenXRLayerProvider.Started -= CreateAndRegisterLayerHandler;
            }

            static void CreateAndRegisterLayerHandler()
            {
                var layerHandler = new MetaOpenXRPassthroughLayer();
                OpenXRLayerProvider.RegisterLayerHandler(typeof(PassthroughLayerData), layerHandler);
            }

            /// <summary>
            /// Start the camera functionality.
            /// </summary>
            public override void Start()
            {
                if (!IsPassthroughLayerActive())
                    CreatePassthroughLayer();

                var defaultLayer = FindCompositionLayerType<DefaultLayerData>(CompositionLayerManager.Instance.CompositionLayers);
                defaultLayer.LayerData.BlendType = BlendType.Premultiply;

#if UNITY_ANDROID && !UNITY_EDITOR
                m_IsInitialized = NativeApi.UnityMetaQuest_InitializeCamera();
#endif//UNITY_ANDROID && !UNITY_EDITOR
            }

            public override void Stop()
            {
                m_IsInitialized = false;

#if UNITY_ANDROID && !UNITY_EDITOR
                NativeApi.UnityMetaQuest_ReleaseCamera();
#endif//UNITY_ANDROID && !UNITY_EDITOR

                if (IsPassthroughLayerActive())
                    DestroyPassthroughLayer();
            }

            /// <summary>
            /// Get the camera frame for the subsystem.
            /// </summary>
            /// <param name="cameraParams">The current Unity <c>Camera</c> parameters.</param>
            /// <param name="cameraFrame">The current camera frame returned by the method.</param>
            /// <returns><see langword="true"/> if the method successfully got a frame. Otherwise, <see langword="false"/>.</returns>
            public override bool TryGetFrame(XRCameraParams cameraParams, out XRCameraFrame cameraFrame)
            {
                cameraFrame = default;
#if UNITY_ANDROID && !UNITY_EDITOR
                return m_IsInitialized && NativeApi.UnityMetaQuest_Camera_TryGetFrame(cameraParams, out cameraFrame);
#else
                return false;
#endif//UNITY_ANDROID && !UNITY_EDITOR
            }

            /// <summary>
            /// Get the camera intrinsics information.
            /// </summary>
            /// <param name="cameraIntrinsics">The camera intrinsics information returned from the method.</param>
            /// <returns><see langword="true"/> if the method successfully gets the camera intrinsics information.
            /// Otherwise, <see langword="false"/>.</returns>
            public override bool TryGetIntrinsics(out XRCameraIntrinsics cameraIntrinsics)
            {
                cameraIntrinsics = default;
#if UNITY_ANDROID && !UNITY_EDITOR
                return m_IsInitialized && NativeApi.UnityMetaQuest_Camera_TryGetIntrinsics(out cameraIntrinsics);
#else
                return false;
#endif//UNITY_ANDROID && !UNITY_EDITOR
            }

            /// <summary>
            /// An instance of the <see cref="XRCpuImage.Api"/> used to operate on <see cref="XRCpuImage"/> objects.
            /// </summary>
            public override XRCpuImage.Api cpuImageApi => MetaOpenXRCpuImageApi.instance;

            /// <summary>
            /// Get the actual camera facing direction.
            /// </summary>
            /// <value>The current camera facing direction.</value>
            /// <seealso cref="requestedCamera"/>
            public override Feature currentCamera => Feature.WorldFacingCamera;

            /// <summary>
            /// Property to be implemented by the provider to query or set the current camera configuration.
            /// </summary>
            /// <value>The current camera configuration, if it exists. Otherwise, <see langword="null"/>.</value>
            /// <exception cref="System.NotSupportedException">Thrown when setting the current configuration if the
            /// implementation does not support camera configurations.</exception>
            public override XRCameraConfiguration? currentConfiguration
            {
                get
                {
                    if (TryGetIntrinsics(out XRCameraIntrinsics cameraIntrinsics))
                    {
                        // NOTE: the Android NDK by itself possesses no API to determine whether a depth sensor exists
                        // on the device, and the framerate is variable
                        return new (IntPtr.Zero, cameraIntrinsics.resolution, null, Supported.Unknown);
                    }

                    return null;
                }
                set => throw new NotSupportedException("setting current camera configuration is not supported by this implementation");
            }

            /// <summary>
            /// Get the latest native camera image.
            /// </summary>
            /// <param name="cameraImageCinfo">The metadata required to construct a <see cref="XRCpuImage"/>.</param>
            /// <returns><see langword="true"/> if the camera image is acquired. Otherwise, <see langword="false"/>.</returns>
            /// <exception cref="System.NotSupportedException">Thrown if the implementation does not support camera image.</exception>
            public override bool TryAcquireLatestCpuImage(out XRCpuImage.Cinfo cameraImageCinfo)
            {
                cameraImageCinfo = default;
                return m_IsInitialized && MetaOpenXRCpuImageApi.TryAcquireLatestImage(MetaOpenXRCpuImageApi.ImageType.Camera,
                    out cameraImageCinfo);
            }

            static bool IsPassthroughLayerActive()
            {
                var compositionLayerManager = CompositionLayerManager.Instance;
                if (compositionLayerManager == null)
                    return false;

                var layer = FindCompositionLayerType<PassthroughLayerData>(compositionLayerManager.CompositionLayers);
#if VERBOSE_LOGGING
                Debug.Log($"Is Passthrough Layer active: {layer != null}");
#endif
                return layer != null;
            }

            static void CreatePassthroughLayer()
            {
#if VERBOSE_LOGGING
                Debug.Log("CreatePassthroughLayer");
#endif
                var passthroughDescriptor = CompositionLayerUtils.GetLayerDescriptor(typeof(PassthroughLayerData));
                var passthroughGameObject = new GameObject("Passthrough");
                var compositionLayerComponent = passthroughGameObject.AddComponent<CompositionLayer>();
                var passthroughLayerData = CompositionLayerUtils.CreateLayerData(typeof(PassthroughLayerData));
                compositionLayerComponent.ChangeLayerDataType(passthroughLayerData);
                foreach (var extension in passthroughDescriptor.SuggestedExtensions)
                {
                    if (extension.IsSubclassOf(typeof(MonoBehaviour)))
                        passthroughGameObject.AddComponent(extension);
                }
                compositionLayerComponent.TryChangeLayerOrder(compositionLayerComponent.Order, CompositionLayerManager.GetFirstUnusedLayer(false));
            }

            static void DestroyPassthroughLayer()
            {
#if VERBOSE_LOGGING
                Debug.Log("DestroyPassthroughLayer");
#endif
                if (CompositionLayerManager.Instance == null)
                    return;

                var passthroughLayer = FindCompositionLayerType<PassthroughLayerData>(CompositionLayerManager.Instance.CompositionLayers);
                if (passthroughLayer != null)
                    UnityObjectUtils.Destroy(passthroughLayer.gameObject);
            }

            static CompositionLayer FindCompositionLayerType<T>(IReadOnlyCollection<CompositionLayer> layers)
            {
                foreach (var layer in layers)
                {
                    if (layer != null && layer.LayerData.GetType() == typeof(T))
                        return layer;
                }

                return null;
            }
        }

        internal static class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern XrResult UnityMetaQuest_xrCreatePassthroughFB(
                ulong session, in XrPassthroughCreateInfoFB createInfo, out ulong passthroughHandle);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern XrResult UnityMetaQuest_xrCreatePassthroughLayerFB(
                ulong session, in XrPassthroughLayerCreateInfoFB createInfo, out ulong passthroughLayerHandle);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern XrResult UnityMetaQuest_xrPassthroughStartFB(ulong passthroughHandle);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern XrResult UnityMetaQuest_xrPassthroughPauseFB(ulong passthroughHandle);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern XrResult UnityMetaQuest_xrDestroyPassthroughLayerFB(ulong passthroughLayerHandle);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern XrResult UnityMetaQuest_xrDestroyPassthroughFB(ulong passthroughHandle);

#if UNITY_ANDROID && !UNITY_EDITOR
            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern bool UnityMetaQuest_InitializeCamera();

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern void UnityMetaQuest_ReleaseCamera();

            [DllImport(Constants.k_ARFoundationLibrary)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool UnityMetaQuest_Camera_TryGetFrame(
                XRCameraParams cameraParams, out XRCameraFrame cameraFrame);

            [DllImport(Constants.k_ARFoundationLibrary)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool UnityMetaQuest_Camera_TryGetIntrinsics(out XRCameraIntrinsics cameraIntrinsics);
#endif//UNITY_ANDROID && !UNITY_EDITOR
        }
    }
}
