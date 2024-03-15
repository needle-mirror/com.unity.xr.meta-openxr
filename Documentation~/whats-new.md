---
uid: meta-openxr-whats-new
---
# What's new in version 2.0

This release includes the following significant changes:

## Added

- Added support for the [ARPlane.classifications](xref:UnityEngine.XR.ARFoundation.ARPlane.classifications) data type introduced in AR Foundation 6.0, including new classification values and support for planes with multiple classifications.
- Added support for the [Asynchronous TryAddAnchor API](xref:arfoundation-whats-new#asynchronous-tryaddanchor-api) introduced in AR Foundation 6.
- Added an API for the tracking of 3D bounding boxes with the new [Bounding box detection](xref:arfoundation-bounding-box-detection) feature added in AR Foundation 6.1. Refer to [Bounding boxes](xref:meta-openxr-bounding-boxes) for more information.
- Added [XRResultStatusExtensions](xref:UnityEngine.XR.OpenXR.Features.Meta.XRResultStatusExtensions), extending the [XRResultStatus](xref:UnityEngine.XR.ARSubsystems.XRResultStatus) type to support conversion to [XrResult](xref:UnityEngine.XR.OpenXR.NativeTypes.XrResult).
- Added support for `NotAxisAligned` plane detection mode introduced in AR Foundation 6.0.
- Added support for the [Meshing](xref:arfoundation-meshing) subsystem.

## Changed

- Changed the AR Foundation dependency version to 6.0.0-pre.7.
- Changed the minimum version of Unity to 2023.3, as required by AR Foundation 6.0.0-pre.7.
- Changed the behavior of `MetaOpenXRSessionSubsystem.sessionId` to now return a non-empty, unique Guid value per Meta OpenXR session. You can access the session id using `XRSessionSubsystem.sessionId`.
- Changed the way the Android Manifest is written in `ModifyAndroidManifestMeta` to override `OpenXRFeatureBuildHooks` virtual functions so that direct XML manipulation is not necessary.
- Changed the names of the Meta Quest features in the Editor to remove the "AR" prefix.
