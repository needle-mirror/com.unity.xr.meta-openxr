---
uid: meta-openxr-bounding-boxes
---
# Bounding boxes

This page is a supplement to the AR Foundation [Bounding box detection](xref:arfoundation-bounding-box-detection) manual. The following sections only contain information about APIs where Meta Quest exhibits unique platform-specific behavior.

[!include[](../snippets/arf-docs-tip.md)]

## Space setup

Bounding boxes on Meta Quest devices requires that the user first completes [Space Setup](xref:meta-openxr-device-setup#space-setup) before any 3D bounding box data can be used.

Meta OpenXR does not dynamically discover bounding boxes at runtime. Instead, this provider queries the device's Space Setup data and returns all bounding box components that are stored in its [Scene Model](https://developer.oculus.com/documentation/native/android/openxr-scene-overview#scene-model). Some entities in the Scene Model, such as Tables or Lamps, include bounding boxes, while others do not.

> [!Important]
> If Space Setup is not complete, the user's Scene Model will not contain any 3D bounding boxes. If your app requires bounding boxes, you can use [scene capture](xref:meta-openxr-session#scene-capture) to prompt the user to complete Space Setup.

## Trackable ID

Unlike other AR platforms, the [trackableId](xref:UnityEngine.XR.ARFoundation.ARTrackable`2.trackableId) property of any [ARBoundingBox](xref:UnityEngine.XR.ARFoundation.ARBoundingBox) from the Meta OpenXR platform persists across multiple sessions in the same space setup. This allows you to, for instance, save the `trackableId` of a user's bed to persist a virtual pet sleeping on the bed each time the user runs your app.

## Bounding box classifications

This package maps Meta's native [semantic labels](https://developer.oculus.com/documentation/native/android/mobile-scene-api-ref#getting-semantic-label-component) to AR Foundation's [BoundingBoxClassifications](xref:UnityEngine.XR.ARFoundation.ARBoundingBox.classifications). Meta supports multiple classifications per bounding box.

Refer to the table below to understand the mapping between AR Foundation's classifications and Meta's semantic labels:

| AR Foundation Label   | Meta Label          |
| :-------------------- | :------------------ |
| Couch                 | COUCH               |
| Table                 | TABLE               |
| Bed                   | BED                 |
| Lamp                  | LAMP                |
| Plant                 | PLANT               |
| Screen                | SCREEN              |
| Storage               | STORAGE             |
| Other                 | OTHER               |
