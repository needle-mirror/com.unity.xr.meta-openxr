---
uid: meta-openxr-whats-new
---
# What's new in version 2.0

This release includes the following significant changes:

## New features

- Added support for saving anchors, loading anchors, and erasing anchors introduced in AR Foundation 6.0.
- Added an API for the tracking of 3D bounding boxes with the new [Bounding box detection](xref:arfoundation-bounding-box-detection) feature added in AR Foundation 6.1. Refer to [Bounding boxes](xref:meta-openxr-bounding-boxes) for more information.
- Added support for the [Meshing](xref:arfoundation-meshing) subsystem.

## Added

- Added support for the [Asynchronous TryAddAnchor API](xref:arfoundation-whats-new#asynchronous-tryaddanchor-api) introduced in AR Foundation 6.
- Added [XRResultStatusExtensions](xref:UnityEngine.XR.OpenXR.Features.Meta.XRResultStatusExtensions), extending the [XRResultStatus](xref:UnityEngine.XR.ARSubsystems.XRResultStatus) type to support conversion to [XrResult](xref:UnityEngine.XR.OpenXR.NativeTypes.XrResult).
- Added support for `NotAxisAligned` plane detection mode introduced in AR Foundation 6.0.
- Added support for the [ARPlane.classifications](xref:UnityEngine.XR.ARFoundation.ARPlane.classifications) data type introduced in AR Foundation 6.0, including new classification values and support for planes with multiple classifications.
- Added explicit dependencies for the XR Plug-in Management and XR Core Utils packages, which were previously implicit.

## Changed

- Changed the minimum AR Foundation dependency version to 6.0.1.
- Changed the minimum OpenXR Plug-in dependency version to 1.10.0.
- Changed the minimum version of Unity to 6000.0, as required by AR Foundation 6.0.1.
- Changed the plane subsystem to no longer attempt to request the Android system permission `com.oculus.permission.USE_SCENE` on your app's behalf. To use planes or bounding boxes with OpenXR Plug-in version 1.11.0 or newer, your app must request this permission. Refer to [Spatial Data Permission](https://developer.oculus.com/documentation/unity/unity-spatial-data-perm/) (Meta Quest Developer Center) to learn how to request this permission.
- Changed the behavior of `MetaOpenXRSessionSubsystem.sessionId` to now return a non-empty, unique Guid value per Meta OpenXR session. You can access the session id using `XRSessionSubsystem.sessionId`.
- Changed the way the Android Manifest is written in `ModifyAndroidManifestMeta` to override `OpenXRFeatureBuildHooks` virtual functions so that direct XML manipulation is not necessary.
- Changed the names of the Meta Quest features in the Editor to remove the "AR" prefix.
