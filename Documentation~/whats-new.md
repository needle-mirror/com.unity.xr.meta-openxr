---
uid: meta-openxr-whats-new
---
# What's new in version 2.4

## High Fidelity Scene support

* Added a selectable [plane provider type](xref:meta-openxr-planes#plane-provider-type) property to the Meta Quest Planes feature, which enables you to opt in to the new High Fidelity Scene capability in Horizon OS.

## CPU camera images

* Added support for CPU camera image capture, and camera display-related render matrices.  Refer to [Camera Image Capture](xref:meta-openxr-camera) for more information.

## More data from environment ray casts

* Added provider-specific [Raycast methods](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRRaycastSubsystem.Raycast(UnityEngine.XR.OpenXR.Features.Meta.EnvironmentRaycastHit,UnityEngine.Ray,UnityEngine.XR.ARSubsystems.TrackableType,Unity.Collections.Allocator)) for getting information about the [EnvironmentRaycastHitStatus](xref:UnityEngine.XR.OpenXR.Features.Meta.EnvironmentRaycastHitStatus) along with the XRRaycastHit.

For a full list of changes in this version including backwards-compatible bugfixes, refer to the package [changelog](xref:meta-openxr-changelog).
