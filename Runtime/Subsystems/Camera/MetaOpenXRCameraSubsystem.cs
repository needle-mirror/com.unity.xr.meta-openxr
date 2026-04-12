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
// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable PossibleNullReferenceException
// ReSharper disable UnusedParameter.Local
#pragma warning disable CS0414 // Field is assigned but its value is never used

#if UNITY_ANDROID
using UnityEngine.Rendering;
#endif
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

        /// <summary>
        /// Single-camera position. Used for parameters that accept exactly one camera (e.g. <see cref="TryGetFrameForPosition"/>, <see cref="TryAcquireLatestCpuImageForPosition"/>).
        /// </summary>
        public enum CameraPosition
        {
            /// <summary>
            /// Left eye camera.
            /// </summary>
            LeftEye = 0,

            /// <summary>
            /// Right eye camera.
            /// </summary>
            RightEye = 1
        }

        /// <summary>
        /// Bitfield of available cameras. Use with <see cref="GetAvailableCameras"/>.
        /// </summary>
        [Flags]
        public enum AvailableCameras
        {
            /// <summary>
            /// No cameras available.
            /// </summary>
            None = 0,

            /// <summary>
            /// Left eye camera is available.
            /// </summary>
            LeftEye = 1 << 0,

            /// <summary>
            /// Right eye camera is available.
            /// </summary>
            RightEye = 1 << 1,

            /// <summary>
            /// Both left and right eye cameras are available.
            /// </summary>
            Both = LeftEye | RightEye
        }

        /// <summary>
        /// A synchronized stereo pair of CPU image metadata. Use with <see cref="TryAcquireLatestStereoCpuImagePair"/>.
        /// The pair's <see cref="XRCpuImage.Cinfo"/> fields can be turned into <see cref="XRCpuImage"/> instances via
        /// <see cref="cpuImageApi"/>.
        /// </summary>
        public struct XRCpuImagePair
        {
            /// <summary>
            /// Left eye image construction info.
            /// </summary>
            public XRCpuImage.Cinfo leftEyeImageCinfo;

            /// <summary>
            /// Right eye image construction info.
            /// </summary>
            public XRCpuImage.Cinfo rightEyeImageCinfo;
        }

        /// <summary>
        /// A synchronized stereo pair of GPU texture descriptors. Use with <see cref="TryAcquireLatestStereoGpuImagePair"/>.
        /// Release with <see cref="ReleaseStereoGpuImagePair"/>.
        /// </summary>
        public struct XRTextureDescriptorPair
        {
            /// <summary>
            /// Left eye texture descriptor.
            /// </summary>
            public XRTextureDescriptor leftEyeDescriptor;

            /// <summary>
            /// Right eye texture descriptor.
            /// </summary>
            public XRTextureDescriptor rightEyeDescriptor;
        }

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
        /// <returns>`true` if the method successfully acquires the native texture descriptor. Otherwise, `false`.</returns>
        public bool TryAcquireLatestGpuImage(out XRTextureDescriptor descriptor)
        {
            return (provider as MetaOpenXRProvider).TryAcquireLatestGpuImage(out descriptor);
        }

        /// <summary>
        /// Releases a previously successfully acquired native texture descriptor.
        /// </summary>
        /// <param name="descriptor">The texture descriptor that was returned by <see cref="TryAcquireLatestGpuImage"/>.</param>
        /// <exception cref="ArgumentException">Thrown when the provided descriptor does not match the currently held GPU image.</exception>
        public void ReleaseGpuImage(XRTextureDescriptor descriptor)
        {
            (provider as MetaOpenXRProvider).ReleaseGpuImage(descriptor);
        }

        /// <summary>
        /// Attempts to acquire a synchronized stereo pair of GPU images (both cameras from the same capture request).
        /// Only available when using logical camera approach. Returns `false` if images are not synchronized.
        /// </summary>
        /// <param name="descriptorPair">The texture descriptors for the left and right camera if successful.</param>
        /// <returns>`true` if both images were acquired and are synchronized (same capture request). Otherwise, `false`.</returns>
        public bool TryAcquireLatestStereoGpuImagePair(out XRTextureDescriptorPair descriptorPair)
        {
            return (provider as MetaOpenXRProvider).TryAcquireLatestStereoGpuImagePair(out descriptorPair);
        }

        /// <summary>
        /// Releases a previously acquired stereo GPU image pair.
        /// </summary>
        /// <param name="descriptorPair">The stereo pair (left and right camera texture descriptors) to release.</param>
        /// <exception cref="ArgumentException">Thrown when attempted release of pairs were not acquired together.</exception>
        public void ReleaseStereoGpuImagePair(XRTextureDescriptorPair descriptorPair)
        {
            (provider as MetaOpenXRProvider).ReleaseStereoGpuImagePair(descriptorPair);
        }

        /// <summary>
        /// Attempts to acquire the texture descriptor for GPU image for the specified camera.
        /// </summary>
        /// <param name="position">The camera position to acquire the image from.</param>
        /// <param name="descriptor">The descriptor to fill in with the texture information if successful.</param>
        /// <returns>`true` if the method successfully acquires the native texture descriptor. Otherwise, `false`.</returns>
        public bool TryAcquireLatestGpuImageForPosition(CameraPosition position, out XRTextureDescriptor descriptor)
        {
            return (provider as MetaOpenXRProvider).TryAcquireLatestGpuImageForPosition(position, out descriptor);
        }

        /// <summary>
        /// Releases a previously successfully acquired GPU image for the specified camera.
        /// </summary>
        /// <param name="position">The camera position that the image was acquired from.</param>
        /// <param name="descriptor">The texture descriptor that was returned by <see cref="TryAcquireLatestGpuImageForPosition"/>.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="position"/> doesn't match descriptor camera source.</exception>
        public void ReleaseGpuImageForPosition(CameraPosition position, XRTextureDescriptor descriptor)
        {
            (provider as MetaOpenXRProvider).ReleaseGpuImageForPosition(position, descriptor);
        }

        /// <summary>
        /// Gets the camera frame for the requested camera.
        ///
        /// Use <see cref="XRCameraSubsystem.TryGetLatestFrame"/> for the default (mono) frame or
        /// this method for a specific camera.
        /// </summary>
        /// <param name="position">Which camera to get the frame for (LeftEye or RightEye).</param>
        /// <param name="cameraParams">Parameters used for frame conversion (e.g. screen dimensions).</param>
        /// <param name="cameraFrame">The camera frame data if successful.</param>
        /// <returns>`true` if the frame was successfully retrieved. Otherwise `false`.</returns>
        public bool TryGetFrameForPosition(CameraPosition position, XRCameraParams cameraParams, out XRCameraFrame cameraFrame)
        {
            return (provider as MetaOpenXRProvider).TryGetFrameForPosition(position, cameraParams, out cameraFrame);
        }

        /// <summary>
        /// Gets the camera intrinsics for the requested camera.
        ///
        /// Use <see cref="XRCameraSubsystem.TryGetIntrinsics"/> for the default (mono) intrinsics or this method
        /// for a specific camera.
        /// </summary>
        /// <param name="position">Which camera to get the intrinsics for (LeftEye or RightEye).</param>
        /// <param name="cameraIntrinsics">The camera intrinsics if successful.</param>
        /// <returns>`true` if the intrinsics were successfully retrieved. Otherwise `false`.</returns>
        public bool TryGetIntrinsicsForPosition(CameraPosition position, out XRCameraIntrinsics cameraIntrinsics)
        {
            return (provider as MetaOpenXRProvider).TryGetIntrinsicsForPosition(position, out cameraIntrinsics);
        }

        /// <summary>
        /// Acquires the latest CPU image for the requested camera.
        ///
        /// Use <see cref="XRCameraSubsystem.TryAcquireLatestCpuImage"/> for the default (mono) image or this method
        /// for a specific camera.
        /// </summary>
        /// <param name="position">The camera position to acquire the image from (LeftEye or RightEye).</param>
        /// <param name="cameraImageCinfo">The metadata required to construct a <see cref="XRCpuImage"/>.</param>
        /// <returns>`true` if the camera image is acquired. Otherwise, `false`.</returns>
        public bool TryAcquireLatestCpuImageForPosition(CameraPosition position, out XRCpuImage.Cinfo cameraImageCinfo)
        {
            return (provider as MetaOpenXRProvider).TryAcquireLatestCpuImageForPosition(position, out cameraImageCinfo);
        }

        /// <summary>
        /// Attempts to acquire a synchronized stereo pair of CPU images (both cameras from the same capture request).
        /// Returns `false` if stereo is not available or the latest left/right frames are not synchronized.
        /// The pair can be turned into <see cref="XRCpuImage"/> instances via <see cref="cpuImageApi"/>.
        /// </summary>
        /// <param name="pairImageCinfo">The left and right image metadata if successful.</param>
        /// <returns>`true` if both images were acquired and are synchronized. Otherwise `false`.</returns>
        public bool TryAcquireLatestStereoCpuImagePair(out XRCpuImagePair pairImageCinfo)
        {
            return (provider as MetaOpenXRProvider).TryAcquireLatestStereoCpuImagePair(out pairImageCinfo);
        }

        /// <summary>
        /// Queries whether synchronized stereo image capture (both cameras from the same frame) is supported on this device.
        /// </summary>
        /// <returns>`true` if stereo capture is supported. Otherwise `false`.</returns>
        public bool SupportsStereoImageCapture()
        {
            return (provider as MetaOpenXRProvider).SupportsStereoImageCapture();
        }

        /// <summary>
        /// Gets the available cameras on this device.
        /// </summary>
        /// <returns>A bitfield of available cameras. The bits that are set indicate which cameras are available.</returns>
        public AvailableCameras GetAvailableCameras()
        {
            return (provider as MetaOpenXRProvider).GetAvailableCameras();
        }

        /// <summary>
        /// Sets the maximum number of sync attempts when matching stereo pair timestamps.
        /// Use a higher value (e.g. 5–10) if the cameras are not stereo synced and stereo acquire often fails.
        /// Clamped to 1–10. Default is 3. Can be called before or after the subsystem starts.
        /// </summary>
        /// <param name="maxAttempts">Maximum number of sync attempts (1–10)</param>
        public static void SetMaxStereoSyncAttempts(int maxAttempts)
        {
            MetaOpenXRCpuImageApi.SetMaxStereoSyncAttempts(maxAttempts);
        }

        /// <summary>
        /// The CPU image API used to construct <see cref="XRCpuImage"/> from <see cref="XRCpuImage.Cinfo"/> returned
        /// by <see cref="TryAcquireLatestCpuImageForPosition"/> or <see cref="TryAcquireLatestStereoCpuImagePair"/>.
        /// </summary>
        public XRCpuImage.Api cpuImageApi => provider.cpuImageApi;

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
            int m_LastGpuImageFrameCount = -1;  // Ensures BeginFrame is called once per Unity frame
            GpuImageHandles m_CurrentGpuImageHandles;
            GpuImageHandles m_CurrentLeftGpuImageHandles;
            GpuImageHandles m_CurrentRightGpuImageHandles;
            bool m_AcquiredAsStereoPair;

            HashSet<string> m_LoggedKeys = new();

            void LogCameraNotReadyWarning(string methodName)
            {
                var key = $"NotReady:{methodName}:{m_CameraReadyState}";
                if (m_LoggedKeys.Add(key))
                {
                    switch (m_CameraReadyState)
                    {
                        case CameraReadyState.SupportNotRequested:
                            Debug.LogWarning(
                                $"{methodName} returned false because camera image support is not enabled. Enable it in Project Settings > XR Plug-in Management > OpenXR > Meta Quest > Camera (Passthrough).");
                            break;
                        case CameraReadyState.NoPermission:
                            Debug.LogWarning(
                                $"{methodName} returned false because Android camera permissions have not been granted.");
                            break;
                        case CameraReadyState.PlatformError:
                            Debug.LogWarning(
                                $"{methodName} returned false because camera initialization failed. This may indicate a platform or hardware issue.");
                            break;
                        case CameraReadyState.SubsystemNotStarted:
                            Debug.LogWarning(
                                $"{methodName} returned false because the camera subsystem is not started.");
                            break;
                    }
                }
            }

#if UNITY_ANDROID
            bool LogGpuImageApiVulkanRequired(string methodName)
            {
                Debug.LogError($"{methodName}: API is only available if the graphics API is set to Vulkan");
                return false;
            }

            bool LogGpuImageApiAndroidRequired(string methodName)
            {
                Debug.LogError($"{methodName}: API is only available if the runtime platform is Android");
                return false;
            }

            bool LogGpuImageApiImageAcquisitionError(string methodName)
            {
                Debug.LogError($"{methodName}: API is trying to acquire new image(s) before the previous one(s) have been released.");
                return false;
            }

            bool LogGpuImageApiError(string message)
            {
                Debug.LogError(message);
                return false;
            }

            void EnsureGpuImageFrameAdvanced()
            {
                int frame = Time.frameCount;
                if (frame != m_LastGpuImageFrameCount)
                {
                    m_LastGpuImageFrameCount = frame;
                    MetaOpenXRGpuImageApi.BeginFrame();
                }
            }

#if !UNITY_EDITOR
            void ForceReleaseAllGpuImageHandles()
            {
                if (!m_GpuImageProviderInitialized)
                    return;
                if (m_CurrentGpuImageHandles.IsValid)
                {
                    MetaOpenXRGpuImageApi.ReleaseGpuHandles(m_CurrentGpuImageHandles);
                    m_CurrentGpuImageHandles.Reset();
                }
                if (m_CurrentLeftGpuImageHandles.IsValid)
                {
                    MetaOpenXRGpuImageApi.ReleaseGpuHandles(m_CurrentLeftGpuImageHandles);
                    m_CurrentLeftGpuImageHandles.Reset();
                }
                if (m_CurrentRightGpuImageHandles.IsValid)
                {
                    MetaOpenXRGpuImageApi.ReleaseGpuHandles(m_CurrentRightGpuImageHandles);
                    m_CurrentRightGpuImageHandles.Reset();
                }
                m_AcquiredAsStereoPair = false;
            }
#endif // !UNITY_EDITOR
#endif // UNITY_ANDROID

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

#endif //UNITY_ANDROID && !UNITY_EDITOR
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
                if (m_GpuImageProviderInitialized && m_CurrentLeftGpuImageHandles.IsValid)
                {
                    MetaOpenXRGpuImageApi.ReleaseGpuHandles(m_CurrentLeftGpuImageHandles);
                    m_CurrentLeftGpuImageHandles.Reset();
                }
                if (m_GpuImageProviderInitialized && m_CurrentRightGpuImageHandles.IsValid)
                {
                    MetaOpenXRGpuImageApi.ReleaseGpuHandles(m_CurrentRightGpuImageHandles);
                    m_CurrentRightGpuImageHandles.Reset();
                }
                m_AcquiredAsStereoPair = false;

                NativeApi.UnityMetaQuest_ReleaseCamera();
                MetaOpenXRGpuImageApi.Release();
                m_GpuImageProviderInitialized = false;
#endif //UNITY_ANDROID && !UNITY_EDITOR

                if (IsPassthroughLayerActive())
                    DestroyPassthroughLayer();
            }

            /// <summary>
            /// Get the camera frame for the subsystem.
            /// </summary>
            /// <param name="cameraParams">The current Unity <c>Camera</c> parameters.</param>
            /// <param name="cameraFrame">The current camera frame returned by the method.</param>
            /// <returns>`true` if the method successfully got a frame. Otherwise, `false`.</returns>
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
#endif //UNITY_ANDROID && !UNITY_EDITOR
            }

            /// <summary>
            /// Get the camera frame for a specific eye (left or right). Same as <see cref="TryGetFrame"/>
            /// but for the requested camera.
            /// </summary>
            internal bool TryGetFrameForPosition(CameraPosition position, XRCameraParams cameraParams, out XRCameraFrame cameraFrame)
            {
                cameraFrame = default;
#if UNITY_ANDROID && !UNITY_EDITOR
                if (m_CameraReadyState != CameraReadyState.Ready)
                {
                    LogCameraNotReadyWarning("TryGetFrameForPosition");
                    return false;
                }
                int index = (int)position;
                return NativeApi.UnityMetaQuest_Camera_TryGetFrameForPosition(index, cameraParams, out cameraFrame);
#else
                return false;
#endif
            }

            /// <summary>
            /// Get the camera intrinsics information.
            /// </summary>
            /// <param name="cameraIntrinsics">The camera intrinsics information returned from the method.</param>
            /// <returns>`true` if the method successfully gets the camera intrinsics information.
            /// Otherwise, `false`.</returns>
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
#endif //UNITY_ANDROID && !UNITY_EDITOR
            }

            /// <summary>
            /// Get the camera intrinsics for a specific eye (left or right). Same as <see cref="TryGetIntrinsics"/>
            /// but for the requested camera.
            /// </summary>
            internal bool TryGetIntrinsicsForPosition(CameraPosition position, out XRCameraIntrinsics cameraIntrinsics)
            {
                cameraIntrinsics = default;
#if UNITY_ANDROID && !UNITY_EDITOR
                if (m_CameraReadyState != CameraReadyState.Ready)
                {
                    LogCameraNotReadyWarning("TryGetIntrinsicsForPosition");
                    return false;
                }
                int index = (int)position;
                return NativeApi.UnityMetaQuest_Camera_TryGetIntrinsicsForPosition(index, out cameraIntrinsics);
#else
                return false;
#endif
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
            /// <value>The current camera configuration, if it exists. Otherwise, `null`.</value>
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
            /// <returns>`true` if the camera image is acquired. Otherwise, `false`.</returns>
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

            /// <summary>
            /// Acquire the latest CPU image for the specified camera.
            /// </summary>
            internal bool TryAcquireLatestCpuImageForPosition(CameraPosition position, out XRCpuImage.Cinfo cameraImageCinfo)
            {
                cameraImageCinfo = default;
#if UNITY_ANDROID && !UNITY_EDITOR
                if (m_CameraReadyState != CameraReadyState.Ready)
                {
                    LogCameraNotReadyWarning("TryAcquireLatestCpuImageForPosition");
                    return false;
                }
                return MetaOpenXRCpuImageApi.TryAcquireLatestImageForPosition((int)position, out cameraImageCinfo);
#else
                return false;
#endif
            }

            internal bool TryAcquireLatestStereoCpuImagePair(out XRCpuImagePair pairImageCinfo)
            {
                pairImageCinfo = default;
#if UNITY_ANDROID && !UNITY_EDITOR
                if (m_CameraReadyState != CameraReadyState.Ready)
                {
                    LogCameraNotReadyWarning("TryAcquireLatestStereoCpuImagePair");
                    return false;
                }
                return MetaOpenXRCpuImageApi.TryAcquireLatestStereoCpuImagePair(
                    out pairImageCinfo.leftEyeImageCinfo,
                    out pairImageCinfo.rightEyeImageCinfo);
#else
                return false;
#endif
            }

            internal bool SupportsStereoImageCapture()
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                return NativeApi.UnityMetaQuest_Camera_SupportsStereoImageCapture();
#else
                return false;
#endif
            }

            internal AvailableCameras GetAvailableCameras()
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                return (AvailableCameras)NativeApi.UnityMetaQuest_Camera_GetAvailableCameras();
#else
                return AvailableCameras.None;
#endif
            }

            internal bool TryAcquireLatestGpuImage(out XRTextureDescriptor descriptor)
            {
                descriptor = default;
#if !UNITY_ANDROID
                return false;
#else
                if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Vulkan)
                    return LogGpuImageApiVulkanRequired("TryAcquireLatestGpuImage");

                if (Application.platform != RuntimePlatform.Android)
                    return LogGpuImageApiAndroidRequired("TryAcquireLatestGpuImage");

#if !UNITY_EDITOR
                // Initialize GPU image provider for Vulkan texture support.
                // We do not do this in the Start() method because we need to have
                // the Vulkan support in place first

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
                    EnsureGpuImageFrameAdvanced();
                    if (m_CurrentGpuImageHandles.IsValid || m_CurrentLeftGpuImageHandles.IsValid || m_CurrentRightGpuImageHandles.IsValid)
                        return LogGpuImageApiError("TryAcquireLatestGpuImage");

                    var success = MetaOpenXRGpuImageApi.TryAcquireLatestGpuHandles(out m_CurrentGpuImageHandles);
                    if(!success)
                        return false;

                    descriptor = new XRTextureDescriptor(
                        nativeTexture: m_CurrentGpuImageHandles.m_VkImage,
                        width: m_CurrentGpuImageHandles.m_Width,
                        height: m_CurrentGpuImageHandles.m_Height,
                        mipmapCount: 1,
                        format: TextureFormat.RGBA32,
                        propertyNameId: 0,
                        depth: 0,
                        textureType: XRTextureType.Texture2D
                    );

                    return true;
                }

                return false;
#endif // UNITY_ANDROID
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

            internal bool TryAcquireLatestGpuImageForPosition(CameraPosition position, out XRTextureDescriptor descriptor)
            {
                descriptor = default;
#if !UNITY_ANDROID
                return false;
#else
                if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Vulkan)
                    return LogGpuImageApiVulkanRequired("TryAcquireLatestGpuImageForPosition");
#if !UNITY_EDITOR
                if (m_CameraReadyState != CameraReadyState.Ready)
                {
                    LogCameraNotReadyWarning("TryAcquireLatestGpuImageForPosition");
                    return false;
                }
                if (!m_GpuImageProviderInitialized)
                    m_GpuImageProviderInitialized = MetaOpenXRGpuImageApi.Initialize();
#endif
                if (!m_GpuImageProviderInitialized)
                    return false;

                EnsureGpuImageFrameAdvanced();
                int cameraPos = (int)position;
                if (m_CurrentGpuImageHandles.IsValid)
                    return LogGpuImageApiImageAcquisitionError("TryAcquireLatestGpuImageForPosition");
                if (m_AcquiredAsStereoPair)
                    return LogGpuImageApiError("TryAcquireLatestGpuImageForPosition: Release the current stereo GPU image pair before acquiring per-camera.");

                ref GpuImageHandles targetHandle = ref (cameraPos == 0 ? ref m_CurrentLeftGpuImageHandles : ref m_CurrentRightGpuImageHandles);
                if (targetHandle.IsValid)
                    return LogGpuImageApiError("TryAcquireLatestGpuImageForPosition");
                if (!MetaOpenXRGpuImageApi.TryAcquireLatestGpuHandlesForPosition(cameraPos, out GpuImageHandles handles))
                    return false;

                targetHandle = handles;
                m_AcquiredAsStereoPair = false;

                descriptor = new XRTextureDescriptor(
                    nativeTexture: targetHandle.m_VkImage,
                    width: targetHandle.m_Width,
                    height: targetHandle.m_Height,
                    mipmapCount: 1,
                    format: TextureFormat.RGBA32,
                    propertyNameId: 0,
                    depth: 0,
                    textureType: XRTextureType.Texture2D
                );
                return true;
#endif
            }

            void ReleaseGpuImageForHandle(ref GpuImageHandles handles, IntPtr descriptorNativeTexture, CameraPosition position)
            {
                if (!handles.IsValid)
                    return;
                if (descriptorNativeTexture != handles.m_VkImage)
                {
                    throw new ArgumentException(
                        $"Camera position doesn't match descriptor camera source. This descriptor was not acquired for {position}.",
                        nameof(descriptorNativeTexture));
                }
                MetaOpenXRGpuImageApi.ReleaseGpuHandles(handles);
                handles.Reset();
            }

            internal void ReleaseGpuImageForPosition(CameraPosition position, XRTextureDescriptor descriptor)
            {
                if (!m_GpuImageProviderInitialized)
                    return;

                if (position == CameraPosition.LeftEye)
                    ReleaseGpuImageForHandle(ref m_CurrentLeftGpuImageHandles, descriptor.nativeTexture, position);
                else
                    ReleaseGpuImageForHandle(ref m_CurrentRightGpuImageHandles, descriptor.nativeTexture, position);
            }

            internal bool TryAcquireLatestStereoGpuImagePair(out XRTextureDescriptorPair descriptor)
            {
                descriptor = default;
#if !UNITY_ANDROID
                return false;
#else
                if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Vulkan)
                    return LogGpuImageApiVulkanRequired("TryAcquireLatestStereoGpuImagePair");

#if UNITY_ANDROID && !UNITY_EDITOR
                if (m_CameraReadyState != CameraReadyState.Ready)
                {
                    LogCameraNotReadyWarning("TryAcquireLatestStereoGpuImagePair");
                    return false;
                }

                if (!SupportsStereoImageCapture())
                {
                    Debug.LogError("TryAcquireLatestStereoGpuImagePair: Stereo image capture is not supported on this device.");
                    return false;
                }

                if (!m_GpuImageProviderInitialized)
                {
                    m_GpuImageProviderInitialized = MetaOpenXRGpuImageApi.Initialize();
                }
#endif

                if (m_GpuImageProviderInitialized)
                {
                    EnsureGpuImageFrameAdvanced();
                    if (m_CurrentGpuImageHandles.IsValid)
                        return LogGpuImageApiError("TryAcquireLatestStereoGpuImagePair");
                    if (m_CurrentLeftGpuImageHandles.IsValid || m_CurrentRightGpuImageHandles.IsValid)
                        return LogGpuImageApiError("TryAcquireLatestStereoGpuImagePair");

                    var success = MetaOpenXRGpuImageApi.TryAcquireLatestStereoGpuHandles(out m_CurrentLeftGpuImageHandles, out m_CurrentRightGpuImageHandles);
                    if (success)
                    {
                        m_AcquiredAsStereoPair = true;
                        descriptor.leftEyeDescriptor = new XRTextureDescriptor(
                            nativeTexture: m_CurrentLeftGpuImageHandles.m_VkImage,
                            width: m_CurrentLeftGpuImageHandles.m_Width,
                            height: m_CurrentLeftGpuImageHandles.m_Height,
                            mipmapCount: 1,
                            format: TextureFormat.RGBA32,
                            propertyNameId: 0,
                            depth: 0,
                            textureType: XRTextureType.Texture2D
                        );
                        descriptor.rightEyeDescriptor = new XRTextureDescriptor(
                            nativeTexture: m_CurrentRightGpuImageHandles.m_VkImage,
                            width: m_CurrentRightGpuImageHandles.m_Width,
                            height: m_CurrentRightGpuImageHandles.m_Height,
                            mipmapCount: 1,
                            format: TextureFormat.RGBA32,
                            propertyNameId: 0,
                            depth: 0,
                            textureType: XRTextureType.Texture2D
                        );
                    }
                    return success;
                }

                return false;
#endif
            }

            internal void ReleaseStereoGpuImagePair(XRTextureDescriptorPair descriptorPair)
            {
                if (!m_GpuImageProviderInitialized)
                    return;
                if (!m_CurrentLeftGpuImageHandles.IsValid && !m_CurrentRightGpuImageHandles.IsValid)
                    return;
                if (!m_AcquiredAsStereoPair)
                {
                    Debug.LogError(
                        "ReleaseStereoGpuImagePair: The current GPU images were not acquired as a stereo pair. " +
                        "Use ReleaseGpuImageForPosition for per-camera release.");
                    return;
                }

                if (descriptorPair.leftEyeDescriptor.nativeTexture != m_CurrentLeftGpuImageHandles.m_VkImage ||
                    descriptorPair.rightEyeDescriptor.nativeTexture != m_CurrentRightGpuImageHandles.m_VkImage)
                {
                    throw new ArgumentException(
                        "The provided pair does not match the currently held stereo GPU images. " +
                        "Release the pair returned from the most recent TryAcquireLatestStereoGpuImagePair call.",
                        nameof(descriptorPair));
                }

                MetaOpenXRGpuImageApi.ReleaseGpuHandles(m_CurrentLeftGpuImageHandles);
                MetaOpenXRGpuImageApi.ReleaseGpuHandles(m_CurrentRightGpuImageHandles);
                m_CurrentLeftGpuImageHandles.Reset();
                m_CurrentRightGpuImageHandles.Reset();
                m_AcquiredAsStereoPair = false;
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
                compositionLayerComponent.TryChangeLayerOrder(
                    compositionLayerComponent.Order, CompositionLayerManager.GetFirstUnusedLayer(false));
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

            [DllImport(Constants.k_ARFoundationLibrary)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool UnityMetaQuest_Camera_TryGetFrameForPosition(
                int cameraPosition, XRCameraParams cameraParams, out XRCameraFrame cameraFrame);

            [DllImport(Constants.k_ARFoundationLibrary)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool UnityMetaQuest_Camera_TryGetIntrinsicsForPosition(
                int cameraPosition, out XRCameraIntrinsics cameraIntrinsics);

            [DllImport(Constants.k_ARFoundationLibrary)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool UnityMetaQuest_Camera_SupportsStereoImageCapture();

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern int UnityMetaQuest_Camera_GetAvailableCameras();
#endif //UNITY_ANDROID && !UNITY_EDITOR
        }
    }
}
