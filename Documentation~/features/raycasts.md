---
uid: meta-openxr-raycasts
---
# Ray casts

This page is a supplement to the AR Foundation [Ray casts](xref:arfoundation-raycasts) manual. The following sections only contain information about APIs where Meta Quest exhibits unique platform-specific behavior.

[!include[](../snippets/arf-docs-tip.md)]

## Optional feature support

This package implements the following optional features of AR Foundation's [XRRaycastSubsystem](xref:UnityEngine.XR.ARSubsystems.XRRaycastSubsystem):

| Feature                    | Descriptor Property | Supported |
| :------------------------- | :------------------ | :-------: |
| **Viewport based raycast** | [supportsViewportBasedRaycast](xref:UnityEngine.XR.ARSubsystems.XRRaycastSubsystemDescriptor.supportsViewportBasedRaycast)|  |
| **World based raycast**    | [supportsWorldBasedRaycast](xref:UnityEngine.XR.ARSubsystems.XRRaycastSubsystemDescriptor.supportsWorldBasedRaycast)   | Yes |
| **Tracked raycasts**       | [supportsTrackedRaycasts](xref:UnityEngine.XR.ARSubsystems.XRRaycastSubsystemDescriptor.supportsTrackedRaycasts) |  |

> [!NOTE]
> Refer to AR Foundation [Ray cast platform support](xref:arfoundation-raycasts-platform-support) for more information on the optional features of the Raycast subsystem.

## Supported trackables

This package supports ray casting against the following [trackable types](xref:UnityEngine.XR.ARSubsystems.TrackableType):

| TrackableType           | Supported |
| :---------------------- | :-------: |
| **BoundingBox**         | Fallback  |
| **Depth**               |    Yes    |
| **Face**                |           |
| **FeaturePoint**        |           |
| **Image**               |           |
| **Planes**              |           |
| **PlaneEstimated**      |           |
| **PlaneWithinBounds**   | Fallback  |
| **PlaneWithinInfinity** |           |
| **PlaneWithinPolygon**  | Fallback  |

### Fallback ray casts

Ray cast functionality in Unity OpenXR: Meta relies on both Meta's OpenXR runtime and AR Foundation's [Fallback ray casts](xref:arfoundation-raycasts-raycastmanager#fallback-ray-casts) implementation. Meta's OpenXR runtime supports ray casts against the **Depth** trackable type. AR Foundation additionally enables you to Unity-world-space ray cast against the **PlaneWithinPolygon**, **PlaneWithinBounds**, and **BoundingBox** trackable types.

>[!IMPORTANT]
> If your app uses AR ray casts, you should use the [ARRaycastManager](xref:UnityEngine.XR.ARFoundation.ARRaycastManager) APIs. Don't access `MetaOpenXRRaycastSubsystem` directly.