---
uid: meta-openxr-planes
---
# Planes

This page is a supplement to the AR Foundation [Plane detection](xref:arfoundation-plane-detection) manual. The following sections only contain information about APIs where Meta Quest exhibits unique platform-specific behavior.

[!include[](../snippets/arf-docs-tip.md)]

## Space setup

To use planes on Meta Quest devices, the user must first complete [Space Setup](xref:meta-openxr-device-setup#space-setup).

Unlike other AR platforms, Meta OpenXR does not dynamically discover planes at runtime. Instead, the Unity OpenXR: Meta queries the device's Space Setup data and returns all plane components that are stored in its [Scene Model](https://developer.oculus.com/documentation/native/android/openxr-scene-overview#scene-model). Some entities in the Scene Model, such as Tables or Couches, include planes, while others do not.

> [!Important]
> If Space Setup is not complete, AR Foundation cannot use plane data. If your app requires planes, you can use [scene capture](xref:meta-openxr-session#scene-capture) to prompt the user to complete Space Setup.

## Trackable ID

Unlike other AR platforms, the [trackableId](xref:UnityEngine.XR.ARFoundation.ARTrackable`2.trackableId) property of any [ARPlane](xref:UnityEngine.XR.ARFoundation.ARPlane) from the Meta OpenXR platform persists across multiple sessions in the same space setup. This allows you to, for instance, save the `trackableId` of a user's table to persist a virtual centerpiece on the table each time the user runs your app.

## Plane alignment

Also unique to the Meta OpenXR platform is the [planeAlignmentThreshold](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRPlaneSubsystem.planeAlignmentThreshold) property, which determines the threshold for [PlaneAlignment](xref:UnityEngine.XR.ARSubsystems.PlaneAlignment) categorization. Plane alignment is calculated by taking each component of the plane's normal vector and finding their difference from 0. If the difference is less than the `planeAlignmentThreshold`, then the `MetaOpenXRPlaneSubsystem` will categorize the plane depending on which of the normal vector's components are within the threshold. The `x` and `z` components are both checked for horizontal planes while the `y` component is checked for vertical planes.

## Plane classifications

This package maps Meta's native [semantic label component](https://developer.oculus.com/documentation/native/android/mobile-scene-api-ref#getting-semantic-label-component) to AR Foundation's [PlaneClassifications](xref:UnityEngine.XR.ARFoundation.ARPlane.classifications). Meta supports multiple classifications per plane.

Refer to the table below to understand the mapping between AR Foundation's classifications and Meta's semantic labels:

| AR Foundation Label   | Meta Label          |
| :-------------------- | :------------------ |
| Table                 | TABLE               |
| Couch                 | COUCH               |
| Floor                 | FLOOR               |
| Ceiling               | CEILING             |
| WallFace              | WALL_FACE           |
| WallArt               | WALL_ART            |
| DoorFrame             | DOOR_FRAME          |
| WindowFrame           | WINDOW_FRAME        |
| InvisibleWallFace     | INVISIBLE_WALL_FACE |
| Other                 | OTHER               |
