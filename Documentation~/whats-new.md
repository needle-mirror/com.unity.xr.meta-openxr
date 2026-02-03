---
uid: meta-openxr-whats-new
---
# What's new in version 2.5

## GPU Camera Images

* Added support for GPU camera image capture via [MetaOpenXRCameraSubsystem.TryAcquireLatestGpuImage](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.TryAcquireLatestGpuImage(UnityEngine.XR.ARSubsystems.XRTextureDescriptor@)) and [MetaOpenXRCameraSubsystem.ReleaseGpuImage](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.ReleaseGpuImage(UnityEngine.XR.ARSubsystems.XRTextureDescriptor)). Refer to [Access images via GPU](xref:meta-openxr-camera#access-images-via-gpu) for more information.

* Both CPU and GPU image capture functionality accessed through the [MetaOpenXRCameraSubsystem](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem) now require opting in to the feature.  Refer to [Image Capture](xref:meta-openxr-camera#image-capture) for details.

## Passthrough pre-splash screen

* Added a setting to the AR Camera Feature that allows you to enable passthrough before the splash screen is displayed. Refer to [Passthrough pre-splash screen](xref:meta-openxr-camera#passthrough-pre-splash-screen) for details.

For a full list of changes in this version including backwards-compatible bugfixes, refer to the package [changelog](xref:meta-openxr-changelog).
