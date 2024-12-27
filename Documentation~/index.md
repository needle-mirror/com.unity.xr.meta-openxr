---
uid: meta-openxr-manual
---
# Unity OpenXR: Meta

Unity OpenXR: Meta (Meta OpenXR) enables Meta Quest device support for your AR Foundation projects and provides a C# interface for Meta's OpenXR runtime. This package depends on both [AR Foundation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.0) and the [OpenXR Plug-in](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.10).

Unity's [OpenXR Plug-in](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.10) integrates core OpenXR features for all XR platforms. This package integrates Meta-specific vendor extensions to provide additional features for Meta Quest devices. Refer to [Architecture](xref:meta-openxr-architecture) for a full list of OpenXR extensions that this package uses.

## Get started

To understand how to install the Meta OpenXR package, refer to [Install Meta OpenXR](xref:meta-openxr-install).

Visit the [Get started](xref:meta-openxr-get-started) section to understand how to set up your project for Meta OpenXR.

## Features

This package defines the following OpenXR Features:

<table>
  <tr>
    <td colspan="2"><strong>Feature</strong></td>
    <td><strong>Description</strong>
  </tr>
  <tr>
    <td colspan="2"><a href="features/display-utilities.md">Display utilities</a></td>
    <td>Get the supported display refresh rates for the device and request a display refresh rate.</td>
  </tr>
  <tr>
    <td colspan="2"><a href="features/boundary-visibility.md">Boundary visibility</a></td>
    <td>Suppress the visibility of the boundary.</td>
  </tr>
  <tr>
    <td colspan="2"><a href="features/ar-features.md">AR Features</a></td>
    <td>AR Features implement AR Foundation interfaces.</td>
  </tr>
  <tr>
    <td></td>
    <td><a href="features/session.md">Session</a></td>
    <td>Enable, disable, and configure AR on the target platform.</td>
  </tr>
  <tr>
    <td></td>
    <td><a href="features/camera.md">Camera</a></td>
    <td>Render images from device cameras and perform light estimation.</td>
  </tr>
  <tr>
    <td></td>
    <td><a href="features/planes.md">Planes</a></td>
    <td>Use Scene Model data to track flat surfaces.</td>
  </tr>
  <tr>
    <td></td>
    <td><a href="features/bounding-boxes.md">Bounding boxes</a></td>
    <td>Use Scene Model data to track bounding boxes of 3D objects.</td>
  </tr>
  <tr>
    <td></td>
    <td><a href="features/anchors.md">Anchors</a></td>
    <td>Track arbitrary points in space.</td>
  </tr>
  <tr>
    <td></td>
    <td><a href="features/raycasts.md">Ray casts</a></td>
    <td>Cast rays against tracked items.</td>
  </tr>
  <tr>
    <td></td>
    <td><a href="features/meshing.md">Meshing</a></td>
    <td>Generate meshes of the environment.</td>
  </tr>
  <tr>
    <td></td>
    <td><a href="features/occlusion.md">Occlusion</a></td>
    <td>Occlude virtual content with physical objects.</td>
  </tr>
</table>
