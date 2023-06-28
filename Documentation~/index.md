---
uid: meta-openxr-manual
---
# Meta OpenXR Feature

The Meta OpenXR Feature package enables Meta Quest device support for your AR Foundation projects.

This package is dependent on both [AR Foundation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.0) and the [OpenXR Plug-in](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.6), and implements the following AR Foundation features:

| Feature | Implemented | Description |
| :------ | :---------- | :---------- |
| [Session](xref:arfoundation-session) | Yes | Enable, disable, and configure AR on the target platform. |
| [Device tracking](xref:arfoundation-device-tracking) | Yes | Track the device's position and rotation in physical space. |
| [Camera](xref:arfoundation-camera) | Yes | Render images from device cameras and perform light estimation. |
| [Plane detection](xref:arfoundation-plane-detection) | Yes | Detect and track surfaces. |
| [Image tracking](xref:arfoundation-image-tracking) | | Detect and track 2D images. |
| [Object tracking](xref:arfoundation-object-tracking) | | Detect and track 3D objects. |
| [Face tracking](xref:arfoundation-face-tracking) | | Detect and track human faces. |
| [Body tracking](xref:arfoundation-body-tracking) | | Detect and track a human body. |
| [Point clouds](xref:arfoundation-point-clouds) | | Detect and track feature points. |
| [Raycasts](xref:arfoundation-raycasts) | Yes | Cast rays against tracked items. |
| [Anchors](xref:arfoundation-anchors) | Yes | Track arbitrary points in space. |
| [Meshing](xref:arfoundation-meshing) | | Generate meshes of the environment. |
| [Environment probes](xref:arfoundation-environment-probes) | | Generate cubemaps of the environment. |
| [Occlusion](xref:arfoundation-occlusion) | | Occlude AR content with physical objects and perform human segmentation. |
| [Participants](xref:arfoundation-participant-tracking) | | Track other devices in a shared AR session. |

## Requirements

To use the Meta OpenXR Feature package, your project must meet the following requirements:

* Unity 2021.3 or newer
* AR Foundation 5.1.0-pre.6 or newer
* OpenXR 1.7 or newer

When you install this package, if you don't already have AR Foundation or OpenXR in your project, the Package Manager will automatically install these dependencies.
