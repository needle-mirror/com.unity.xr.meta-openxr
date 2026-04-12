using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR.Features.Meta;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    public class MetaCpuImageSamples
    {


        public void ExampleAcquireCpuImage(ARCameraManager cameraManager)
        {
            #region acquire_cpu_image
            MetaOpenXRCameraSubsystem cameraSubsystem = cameraManager.subsystem as MetaOpenXRCameraSubsystem;

            if (!cameraSubsystem.TryAcquireLatestCpuImage(out XRCpuImage image))
                return;

            using (image)
            {
                // Use image (e.g. image.dimensions, image.GetPlane(), image.Convert(), image.ConvertAsync())
            }
            #endregion
        }

        public void ExampleAcquireCpuImageForPosition(ARCameraManager cameraManager, MetaOpenXRCameraSubsystem.CameraPosition position)
        {
            #region acquire_cpu_image_for_position
            MetaOpenXRCameraSubsystem cameraSubsystem = cameraManager.subsystem as MetaOpenXRCameraSubsystem;

            if (cameraSubsystem == null || !cameraSubsystem.TryAcquireLatestCpuImageForPosition(position, out XRCpuImage.Cinfo cinfo))
                return;

            using (var image = new XRCpuImage(cameraSubsystem.cpuImageApi, cinfo))
            {
                // Use image (e.g. image.dimensions, image.GetPlane(), image.Convert(), image.ConvertAsync())
            }
            #endregion
        }

        public void ExampleAcquireStereoCpuImagePair(ARCameraManager cameraManager)
        {
            #region acquire_stereo_cpu_image_pair
            MetaOpenXRCameraSubsystem cameraSubsystem = cameraManager.subsystem as MetaOpenXRCameraSubsystem;

            if (cameraSubsystem == null || !cameraSubsystem.TryAcquireLatestStereoCpuImagePair(out MetaOpenXRCameraSubsystem.XRCpuImagePair pair))
                return;

            using (var leftImage = new XRCpuImage(cameraSubsystem.cpuImageApi, pair.leftEyeImageCinfo))
            using (var rightImage = new XRCpuImage(cameraSubsystem.cpuImageApi, pair.rightEyeImageCinfo))
            {
                // Use leftImage and rightImage (e.g. dimensions, GetPlane(), Convert(), ConvertAsync())
            }
            #endregion
        }
    }
}
