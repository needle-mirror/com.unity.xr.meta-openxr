---
uid: meta-openxr-features
---
# Features

This package defines the following OpenXR Features:

<table>
  <tr>
  	<td colspan="2"><strong>Feature</strong></td>
  	<td><strong>Description</strong>
  </tr>
  <tr>
  	<td colspan="2"><a href="display-utilities.md">Display Utilities</a></td>
  	<td>Get the supported display refresh rates for the device and request a display refresh rate.</td>
  </tr>
  <tr>
  	<td colspan="2"><a href="ar-features.md">AR Features</a></td>
  	<td>AR Features implement AR Foundation interfaces.</td>
  </tr>
  <tr>
  	<td></td>
  	<td><a href="session.md">Session</a></td>
  	<td>Enable, disable, and configure AR on the target platform.</td>
  </tr>
  <tr>
  	<td></td>
  	<td><a href="camera.md">Camera</a></td>
  	<td>Render images from device cameras and perform light estimation.</td>
  </tr>
  <tr>
  	<td></td>
  	<td><a href="planes.md">Planes</a></td>
  	<td>Use Scene Model data to track flat surfaces.</td>
  </tr>
  <tr>
  	<td></td>
  	<td><a href="bounding-boxes.md">Bounding boxes</a></td>
  	<td>Use Scene Model data to track bounding boxes around 3D objects.</td>
  </tr>
  <tr>
  	<td></td>
  	<td><a href="anchors.md">Anchors</a></td>
  	<td>Track arbitrary points in space.</td>
  </tr>
  <tr>
  	<td></td>
  	<td><a href="raycasts.md">Raycasts</a></td>
  	<td>Cast rays against tracked items.</td>
  </tr>
  <tr>
  	<td></td>
  	<td><a href="meshing.md">Meshing</a></td>
  	<td>Generate meshes of the environment.</td>
  </tr>
</table>

> [!TIP]
> If your app does not use a feature, it's best practice to disable that feature. In some cases doing so may improve your app's performance.
