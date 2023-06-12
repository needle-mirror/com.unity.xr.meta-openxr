---
uid: meta-openxr-plane-detection
---
# Plane detection

Plane detection with the Meta OpenXR Feature works similarly to AR Foundation [Plane detection](xref:arfoundation-plane-detection), with a couple platform-specific nuances.

## Trackable ID

Unlike other AR Foundation platforms, the [trackableId](xref:UnityEngine.XR.ARFoundation.ARTrackable`2.trackableId) property of any [ARPlane](xref:UnityEngine.XR.ARFoundation.ARPlane) from the Meta OpenXR platform persists across multiple sessions in the same room setup. This allows you to, for instance, save the `trackableId` of a user's wall to persist a virtual painting on that wall each time the user opens your app.

## Plane alignment

Also unique to the Meta OpenXR platform is the [planeAlignmentThreshold](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRPlaneProvider.planeAlignmentThreshold) property, which determines the threshold for [PlaneAlignment](xref:UnityEngine.XR.ARSubsystems.PlaneAlignment) categorization. Plane alignment is calculated by taking each component of the plane's normal vector and finding their difference from 0. If the difference is less than the `planeAlignmentThreshold`, then the `MetaOpenXRPlaneSubsystem` will categorize the plane depending on which of the normal vector's components are within the threshold. The `x` and `z` components are both checked for horizontal planes while the `y` component is checked for vertical planes.

## Plane classification

This package maps Meta's native [semantic label component](https://developer.oculus.com/documentation/native/android/mobile-scene-api-ref#getting-semantic-label-component) to AR Foundation's [PlaneClassification](xref:UnityEngine.XR.ARFoundation.ARPlane.classification). Natively, Meta supports multiple classifications per plane, but `ARPlane.classification` will only contain the first label, or `None` if the label cannot be determined.

Refer to the table below to understand the mapping between Meta's semantic labels and AR Foundation's classifications:

| Meta Label       | AR Foundation Label   |
| :--------------- | :-------------------- |
| DESK             | Table                 |
| COUCH            | Seat                  |
| FLOOR            | Floor                 |
| CEILING          | Ceiling               |
| WALL_FACE        | Wall                  |
| DOOR_FRAME       | Door                  |
| WINDOW_FRAME     | Window                |
| SCREEN           | Other                 |
| LAMP             | Other                 |
| PLANT            | Other                 |
| STORAGE          | Other                 |
| BED              | Other                 |
| OTHER            | Other                 |
