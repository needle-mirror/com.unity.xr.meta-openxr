---
uid: meta-openxr-changelog
---
# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.1.2] - 2023-06-28

### Fixed

- Fixed math operations for comparing structs with single-precision values.
- Fixed an issue where it was possible to incorrectly remove or update planes before they were added in some uncommon situations. This resulted in an `InvalidOperationException` in AR Foundation that is now no longer thrown.

## [0.1.1] - 2023-06-12

### Added

- Added support for getting plane boundary vertices via [XRPlaneSubsystem.GetBoundary](xref:UnityEngine.XR.ARSubsystems.XRPlaneSubsystem.GetBoundary(UnityEngine.XR.ARSubsystems.TrackableId,Unity.Collections.Allocator,Unity.Collections.NativeArray{UnityEngine.Vector2}@)).
- Added support for getting plane alignment via [ARPlane.alignment](xref:UnityEngine.XR.ARFoundation.ARPlane.alignment) as well as [planeAlignmentThreshold](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRPlaneSubsystem.planeAlignmentThreshold). Refer to [Plane alignment](xref:meta-openxr-plane-detection#plane-alignment) for more information.
- Added support for [plane classification](xref:UnityEngine.XR.ARSubsystems.PlaneClassification).
- Added support for planes to be updated and removed.
- Added a custom Editor for [ARCameraManager](xref:UnityEngine.XR.ARFoundation.ARCameraManager) to clarify that its serialized properties have no effect on Meta Passthrough.
- Added a custom Editor for [ARCameraBackground](xref:UnityEngine.XR.ARFoundation.ARCameraBackground) to clarify that it has no effect on Meta Quest devices.
- Added support for [requestedDetectionMode](xref:UnityEngine.XR.ARFoundation.ARPlaneManager.requestedDetectionMode) to filter horizontal and vertical planes.

### Changed

- Changed the `trackableId` property of planes to be backed by OpenXR's `uuid` field, instead of a monotonically increasing counter.
- Changed the display name of the AR Foundation Meta Quest feature.
- Grouped the Meta Quest OpenXR features in an OpenXR feature group.
- Passthrough layer is now composited behind AR content rather than alpha blended in front of it. This fixes an issue where AR opaques always appeared semi-transparent.

### Fixed

- Fixed an issue where the `trackingState` property of planes could be incorrectly reported in some rare cases.
- Fixed an issue where the center of a plane in plane space was incorrectly reported as a value other than (0, 0).
- Fixed an issue where `ARPlane`s were not removed if the `Redo Walls` action or the `Clear Room Set Up` action was performed through the Meta Quest menu.

## [0.1.0] - 2023-04-04

- This is the first release of Meta OpenXR Plugin <com.unity.xr.meta-openxr>.
