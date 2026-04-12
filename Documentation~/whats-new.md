---
uid: meta-openxr-whats-new
---
# What's new in version 2.6

## Stereo Image Capture

* Added support for CPU camera image left/right single acquire and synchronized stereo pair via [MetaOpenXRCameraSubsystem.TryAcquireLatestCpuImageForPosition](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.TryAcquireLatestCpuImageForPosition(UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.CameraPosition,UnityEngine.XR.ARSubsystems.XRCpuImage.Cinfo@)), [MetaOpenXRCameraSubsystem.TryAcquireLatestStereoCpuImagePair](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.TryAcquireLatestStereoCpuImagePair(UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.XRCpuImagePair@)), and [MetaOpenXRCameraSubsystem.SetMaxStereoSyncAttempts](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.SetMaxStereoSyncAttempts(System.Int32)) for tuning stereo timestamp matching. Refer to [Access images via CPU](xref:meta-openxr-camera#access-images-via-cpu) for more information.

* Added support for GPU camera image left/right single acquire and synchronized stereo pair via [MetaOpenXRCameraSubsystem.TryAcquireLatestGpuImageForPosition](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.TryAcquireLatestGpuImageForPosition(UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.CameraPosition,UnityEngine.XR.ARSubsystems.XRTextureDescriptor@)), [MetaOpenXRCameraSubsystem.ReleaseGpuImageForPosition](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.ReleaseGpuImageForPosition(UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.CameraPosition,UnityEngine.XR.ARSubsystems.XRTextureDescriptor)), [MetaOpenXRCameraSubsystem.TryAcquireLatestStereoGpuImagePair](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.TryAcquireLatestStereoGpuImagePair(UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.XRTextureDescriptorPair@)), and [MetaOpenXRCameraSubsystem.ReleaseStereoGpuImagePair](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.ReleaseStereoGpuImagePair(UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.XRTextureDescriptorPair)). Refer to [Access images via GPU](xref:meta-openxr-camera#access-images-via-gpu) for more information.

* Both CPU and GPU image capture functionality accessed through the [MetaOpenXRCameraSubsystem](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem) now require opting in to the feature.  Refer to [Image Capture](xref:meta-openxr-camera#image-capture) for details.

For a full list of changes in this version including backwards-compatible bugfixes, refer to the package [changelog](xref:meta-openxr-changelog).
