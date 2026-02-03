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
using UnityEngine.Scripting;
using UnityEngine.Rendering;

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// The Meta-OpenXR implementation of the <see cref="XRCameraSubsystem"/>.
    /// Do not create this directly. Use the <see cref="SubsystemManager"/> instead.
    /// </summary>
    [Preserve]
    public sealed class MetaOpenXRCameraSubsystem : XRCameraSubsystem
    {
        internal const string k_SubsystemId = "Meta-Camera";

#if UNITY_ANDROID && !UNITY_EDITOR
        const string k_HorizonCameraPermission = "horizonos.permission.HEADSET_CAMERA";
#endif


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

        /// <summary>
        /// Attempts to acquire the texture descriptor for a camera GPU image as a native texture.
        /// </summary>
        /// <param name="descriptor">The descriptor to fill in with the texture information if successful.</param>
        /// <returns><see langword="true"/> if the method successfully acquires the native texture descriptor. Otherwise, <see langword="false"/>.</returns>
        public bool TryAcquireLatestGpuImage(out XRTextureDescriptor descriptor)
        {
            return ((MetaOpenXRProvider)provider).TryAcquireLatestGpuImage(out descriptor);
        }

        /// <summary>
        /// Releases a previously successfully acquired native texture descriptor.
        /// </summary>
        /// <param name="descriptor">The texture descriptor that was returned by <see cref="TryAcquireLatestGpuImage"/>.</param>
        public void ReleaseGpuImage(XRTextureDescriptor descriptor)
        {
            ((MetaOpenXRProvider)provider).ReleaseGpuImage(descriptor);
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnStop()
        {
            base.OnStop();
        }

        class MetaOpenXRProvider : Provider
        {
            enum CameraReadyState
            {
                Ready,
                SupportNotRequested,
                SubsystemNotStarted,
                NoPermission,
                PlatformError
            }

            CameraReadyState m_CameraReadyState = CameraReadyState.SubsystemNotStarted;

            bool m_GpuImageProviderInitialized;
            GpuImageHandles m_CurrentGpuImageHandles;

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
                CompositionLayerManager.PassthroughLayerType = typeof(PassthroughLayerData);
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
                var cameraFeature = OpenXRSettings.Instance.GetFeature<ARCameraFeature>();

                if (!cameraFeature.cameraImageSupportEnabled)
                {
                    m_CameraReadyState = CameraReadyState.SupportNotRequested;
                }
                else if (!Permission.HasUserAuthorizedPermission(k_HorizonCameraPermission))
                {
                    m_CameraReadyState = CameraReadyState.NoPermission;
                }
                else if (!NativeApi.UnityMetaQuest_InitializeCamera())
                {
                    m_CameraReadyState = CameraReadyState.PlatformError;
                }
                else
                {
                    m_CameraReadyState = CameraReadyState.Ready;
                }

#endif//UNITY_ANDROID && !UNITY_EDITOR
            }

            public override void Stop()
            {
                m_CameraReadyState = CameraReadyState.SubsystemNotStarted;

#if UNITY_ANDROID && !UNITY_EDITOR
                // Release any held GPU image handles before stopping
                if (m_GpuImageProviderInitialized && m_CurrentGpuImageHandles.IsValid)
                {
                    MetaOpenXRGpuImageApi.ReleaseGpuHandles(m_CurrentGpuImageHandles);
                    m_CurrentGpuImageHandles.Reset();
                }

                NativeApi.UnityMetaQuest_ReleaseCamera();
                MetaOpenXRGpuImageApi.Release();
                m_GpuImageProviderInitialized = false;
#endif//UNITY_ANDROID && !UNITY_EDITOR

                if (IsPassthroughLayerActive())
                    DestroyPassthroughLayer();
            }

            /// <summary>
            /// Get the camera frame for the subsystem.
            /// </summary>
            /// <param name="cameraParams">The current Unity <c>Camera</c> parameters.</param>
            /// <param name="cameraFrame">The current camera frame returned by the method.</param>
            /// <returns><`true`/> if the method successfully got a frame. Otherwise, <see langword="false"/>.</returns>
            public override bool TryGetFrame(XRCameraParams cameraParams, out XRCameraFrame cameraFrame)
            {
                cameraFrame = default;

#if UNITY_ANDROID && !UNITY_EDITOR
                if (m_CameraReadyState != CameraReadyState.Ready)
                {
                    LogCameraNotReadyWarning("TryGetFrame");
                    return false;
                }
                return NativeApi.UnityMetaQuest_Camera_TryGetFrame(cameraParams, out cameraFrame);
#else
                return false;
#endif//UNITY_ANDROID && !UNITY_EDITOR
            }

            /// <summary>
            /// Get the camera intrinsics information.
            /// </summary>
            /// <param name="cameraIntrinsics">The camera intrinsics information returned from the method.</param>
            /// <returns><`true`/> if the method successfully gets the camera intrinsics information.
            /// Otherwise, <`false`/>.</returns>
            public override bool TryGetIntrinsics(out XRCameraIntrinsics cameraIntrinsics)
            {
                cameraIntrinsics = default;
#if UNITY_ANDROID && !UNITY_EDITOR
                if (m_CameraReadyState != CameraReadyState.Ready)
                {
                    LogCameraNotReadyWarning("TryGetIntrinsics");
                    return false;
                }
                return NativeApi.UnityMetaQuest_Camera_TryGetIntrinsics(out cameraIntrinsics);
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
            /// <value>The current camera configuration, if it exists. Otherwise, <`null`/>.</value>
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
            /// <returns><`true`/> if the camera image is acquired. Otherwise, <`false`/>.</returns>
            /// <exception cref="System.NotSupportedException">Thrown if the implementation does not support camera image.</exception>
            public override bool TryAcquireLatestCpuImage(out XRCpuImage.Cinfo cameraImageCinfo)
            {
                cameraImageCinfo = default;

                if (m_CameraReadyState != CameraReadyState.Ready)
                {
                    LogCameraNotReadyWarning("TryAcquireLatestCpuImage");
                    return false;
                }

                return MetaOpenXRCpuImageApi.TryAcquireLatestImage(MetaOpenXRCpuImageApi.ImageType.Camera,
                    out cameraImageCinfo);
            }

            internal bool TryAcquireLatestGpuImage(out XRTextureDescriptor descriptor)
            {
                descriptor = default;
#if !UNITY_ANDROID
                return false;
#endif

                if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Vulkan)
                {
                    Debug.LogError("TryAcquireLatestGpuImage: API is only available if the graphics API is set to Vulkan");
                    return false;
                }

                if (Application.platform != RuntimePlatform.Android)
                {
                    Debug.LogError("TryAcquireLatestGpuImage: API is only available if the runtime platform is Android");
                    return false;
                }

                // Initialize GPU image provider for Vulkan texture support.
                // We do not do this in the Start() method because we need to have
                // the Vulkan support in place first
#if UNITY_ANDROID && !UNITY_EDITOR
                if (m_CameraReadyState != CameraReadyState.Ready)
                {
                    LogCameraNotReadyWarning("TryAcquireLatestGpuImage");
                    return false;
                }

                if (!m_GpuImageProviderInitialized)
                {
                    m_GpuImageProviderInitialized = MetaOpenXRGpuImageApi.Initialize();
                }
#endif

                if (m_GpuImageProviderInitialized)
                {
                    if (m_CurrentGpuImageHandles.IsValid)
                    {
                        Debug.LogError("TryAcquireLatestGpuImage: API is trying to acquire an image before releasing the previous one.");
                        return false;
                    }

                    var success = MetaOpenXRGpuImageApi.TryAcquireLatestGpuHandles(out m_CurrentGpuImageHandles);
                    if(!success)
                    {
                        return false;
                    }

                    descriptor = new XRTextureDescriptor(
                        m_CurrentGpuImageHandles.m_VkImage,
                        m_CurrentGpuImageHandles.m_Width,
                        m_CurrentGpuImageHandles.m_Height,
                        1,
                        TextureFormat.RGBA32,
                        0, // propertyNameId not needed as there is no use case for it
                        0,
                        XRTextureType.Texture2D
                    );

                    return success;
                }

                return false;
            }

            internal void ReleaseGpuImage(XRTextureDescriptor descriptor)
            {
                if (!m_GpuImageProviderInitialized || !m_CurrentGpuImageHandles.IsValid)
                {
                    return;
                }

                if (descriptor.nativeTexture != m_CurrentGpuImageHandles.m_VkImage)
                {
                    Debug.LogError("ReleaseGpuImage: The provided descriptor does not match the currently held GPU image." +
                        "Please release the descriptor returned from the most recent TryAcquireLatestGpuImage call. " +
                        "Otherwise, the current image handle will remain held and prevent future image acquisitions" +
                        "until properly released or the subsystem stops.");
                    return;
                }

                MetaOpenXRGpuImageApi.ReleaseGpuHandles(m_CurrentGpuImageHandles);
                m_CurrentGpuImageHandles.Reset();
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
                {
                    UnityObjectUtils.Destroy(passthroughLayer.gameObject);

                    // notify the CompLayerManager that the comp layer has been destroyed immediately since
                    // relying on UnityEngine destruction of the comp layer GO can take place _later_ than
                    // this subsystem possibly re-starting and needing to create a new passthrough layer.
                    // it is safe to call this public API multiple times with the same input comp layer since
                    // subsequent calls will be no-ops.
                    CompositionLayerManager.Instance.CompositionLayerDestroyed(passthroughLayer);
                }
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

            void LogCameraNotReadyWarning(string methodName)
            {
                switch (m_CameraReadyState)
                {
                    case CameraReadyState.SupportNotRequested:
                        Debug.LogWarning($"{methodName} returned false because camera image support is not enabled. " +
                            "Enable Camera Image Support in Project Settings > XR Plug-in Management > OpenXR > Meta Quest > Camera (Passthrough).");
                        break;
                    case CameraReadyState.NoPermission:
                        Debug.LogWarning($"{methodName} returned false because andriod camera permissions have not been granted.");
                        break;
                    case CameraReadyState.PlatformError:
                        Debug.LogWarning($"{methodName} returned false because camera initialization failed. This may indicate a platform or hardware issue.");
                        break;
                    case CameraReadyState.SubsystemNotStarted:
                        Debug.LogWarning($"{methodName} returned false because the camera subsystem is not started.");
                        break;
                }
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
