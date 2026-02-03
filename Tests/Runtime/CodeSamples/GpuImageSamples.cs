using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR.Features.Meta;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    public class GpuImageSamples
    {
        Texture2D m_ExternalTexture;

        public void ExampleAcquireGpuImage(ARCameraManager cameraManager)
        {
            #region acquire_gpu_image
            var cameraSubsystem = cameraManager.subsystem as MetaOpenXRCameraSubsystem;
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
            }
            #endregion
        }
    }
}

