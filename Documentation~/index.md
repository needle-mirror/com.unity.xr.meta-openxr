---
uid: meta-openxr-manual
---
# Unity OpenXR: Meta

Unity OpenXR: Meta enables Meta Quest device support for your AR Foundation projects and provides a C# interface for Meta's OpenXR runtime. This package depends on both [AR Foundation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.0) and the [OpenXR Plug-in](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.9).

## Installation

Unity OpenXR: Meta is an official Unity package available from the [Package Manager](https://docs.unity3d.com/6000.0/Documentation/Manual/upm-ui.html). To understand how to install a package via the Package Manager, refer to [Install a UPM package from a registry](https://docs.unity3d.com/6000.0/Documentation/Manual/upm-ui-install.html).

### Requirements

To use Unity OpenXR: Meta 2.0, your project must meet the following requirements:

* Unity 6 (6000.0)
* AR Foundation 6.0.1 or newer
* OpenXR Plug-in 1.10.0 or newer

The Package Manager will automatically install these dependencies if you don't already have them installed.

> [!NOTE]
> You can visit the Unity Forum for more information on the [Unity 6 New Naming Convention](https://forum.unity.com/threads/unity-6-new-naming-convention.1558592/).

## Get started

To enable the features in this package, go to **Project Settings** > **XR Plug-in Management** > **OpenXR** > **OpenXR Feature Groups** and enable the **Meta Quest** feature group as shown below:

![Unity's Project Settings window is open to XR Plug-in Management > OpenXR, showing a list of enabled features in the Meta Quest feature group](images/openxr-features-all.png)<br/>*The Meta Quest OpenXR feature group, shown with all features enabled*

> [!IMPORTANT]
> To use any feature in this package, you must enable the corresponding OpenXR Feature in the **Meta Quest** feature group. Otherwise, your app cannot access these features.

### Setup instructions

Refer to [Device setup](xref:meta-openxr-device-setup) and [Project setup](xref:meta-openxr-project-setup), respectively, for more detailed information about how to set up your Meta Quest device and Unity project to use this package.

## Features

This package defines the following OpenXR Features:

<table>
  <tr>
  	<td colspan="2"><strong>Feature</strong></td>
  	<td><strong>Description</strong>
  </tr>
  <tr>
  	<td colspan="2"><a href="features/display-utilities.md">Display Utilities</a></td>
  	<td>Get the supported display refresh rates for the device and request a display refresh rate.</td>
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
  	<td><a href="features/raycasts.md">Raycasts</a></td>
  	<td>Cast rays against tracked items.</td>
  </tr>
  <tr>
  	<td></td>
  	<td><a href="features/meshing.md">Meshing</a></td>
  	<td>Generate meshes of the environment.</td>
  </tr>
</table>

### Unsupported AR features

This package does not implement the following AR features:

| Feature | Description |
| :------ | :---------- |
| [Image tracking](xref:arfoundation-image-tracking) | Detect and track 2D images. |
| [Object tracking](xref:arfoundation-object-tracking) | Detect and track 3D objects. |
| [Face tracking](xref:arfoundation-face-tracking) | Detect and track human faces. |
| [Body tracking](xref:arfoundation-body-tracking) | Detect and track a human body. |
| [Point clouds](xref:arfoundation-point-clouds) | Detect and track feature points. |
| [Environment probes](xref:arfoundation-environment-probes) | Generate cubemaps of the environment. |
| [Occlusion](xref:arfoundation-occlusion) | Occlude AR content with physical objects and perform human segmentation. |
| [Participants](xref:arfoundation-participant-tracking) | Track other devices in a shared AR session. |

## A primer on OpenXR

OpenXR is an open-source standard that defines an interface between XR apps and platform runtimes. The OpenXR specification contains two categories of features: 

* Core features: present on every platform
* Extensions: optional and may not be implemented by some platforms.

Unity's [OpenXR Plug-in](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.9) integrates core features, while this package integrates Meta-specific vendor extensions. Refer to [Architecture](xref:meta-openxr-architecture) for a full list of OpenXR extensions that this package uses.
