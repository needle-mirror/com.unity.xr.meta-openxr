using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR.Features.Meta;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    public class GpuImageSamples
    {
        Texture2D m_ExternalTexture;
        Texture2D m_LeftEyeTexture;
        Texture2D m_RightEyeTexture;

        public void ExampleAcquireGpuImage(ARCameraManager cameraManager)
        {
            #region acquire_gpu_image
            MetaOpenXRCameraSubsystem cameraSubsystem = cameraManager.subsystem as MetaOpenXRCameraSubsystem;
            if (cameraSubsystem != null && cameraSubsystem.TryAcquireLatestGpuImage(out XRTextureDescriptor descriptor))
            {
                // Create a new texture
                if (m_ExternalTexture == null)
                {
                    m_ExternalTexture = Texture2D.CreateExternalTexture(
                        descriptor.width,
                        descriptor.height,
                        descriptor.format,
                        false, // mipmaps
                        false, // linear
                        descriptor.nativeTexture // VkImage handle
                    );
                }
                else
                {
                    // Update the existing texture with the new native handle
                    m_ExternalTexture.UpdateExternalTexture(descriptor.nativeTexture);
                }
                // Use the texture in your materials or shaders
                cameraSubsystem.ReleaseGpuImage(descriptor);
            }
            #endregion
        }

        public void ExampleAcquireGpuImageForPosition(ARCameraManager cameraManager, MetaOpenXRCameraSubsystem.CameraPosition position)
        {
            #region acquire_gpu_image_for_position
            MetaOpenXRCameraSubsystem cameraSubsystem = cameraManager.subsystem as MetaOpenXRCameraSubsystem;
            if (cameraSubsystem == null || !cameraSubsystem.TryAcquireLatestGpuImageForPosition(position, out XRTextureDescriptor descriptor))
                return;

            bool isLeft = position == MetaOpenXRCameraSubsystem.CameraPosition.LeftEye;
            ref Texture2D targetTexture = ref isLeft ? ref m_LeftEyeTexture : ref m_RightEyeTexture;
            if (targetTexture == null)
            {
                targetTexture = Texture2D.CreateExternalTexture(
                    descriptor.width,
                    descriptor.height,
                    descriptor.format,
                    false,
                    false,
                    descriptor.nativeTexture
                );
            }
            else
            {
                targetTexture.UpdateExternalTexture(descriptor.nativeTexture);
            }
            // Use targetTexture in your materials or shaders
            cameraSubsystem.ReleaseGpuImageForPosition(position, descriptor);
            #endregion
        }

        public void ExampleAcquireStereoGpuImagePair(ARCameraManager cameraManager)
        {
            #region acquire_stereo_gpu_image_pair
            MetaOpenXRCameraSubsystem cameraSubsystem = cameraManager.subsystem as MetaOpenXRCameraSubsystem;
            if (cameraSubsystem == null || !cameraSubsystem.TryAcquireLatestStereoGpuImagePair(out MetaOpenXRCameraSubsystem.XRTextureDescriptorPair pair))
                return;

            var left = pair.leftEyeDescriptor;
            var right = pair.rightEyeDescriptor;

            if (m_LeftEyeTexture == null)
                m_LeftEyeTexture = Texture2D.CreateExternalTexture(left.width, left.height, left.format, false, false, left.nativeTexture);
            else
                m_LeftEyeTexture.UpdateExternalTexture(left.nativeTexture);

            if (m_RightEyeTexture == null)
                m_RightEyeTexture = Texture2D.CreateExternalTexture(right.width, right.height, right.format, false, false, right.nativeTexture);
            else
                m_RightEyeTexture.UpdateExternalTexture(right.nativeTexture);

            // Use m_LeftEyeTexture and m_RightEyeTexture in your materials or shaders
            cameraSubsystem.ReleaseStereoGpuImagePair(pair);
            #endregion
        }
    }
}

