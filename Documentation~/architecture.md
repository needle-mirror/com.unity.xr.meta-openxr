---
uid: meta-openxr-architecture
---
# Architecture

Unity OpenXR: Meta functions as both an OpenXR Feature Group and an AR Foundation [provider plug-in](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.0?subfolder=/manual/architecture/subsystems.html).

## OpenXR extensions

Meta's OpenXR extensions can be found in the Khronos Group [OpenXR Specification](https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html).

This package enables support for the following OpenXR extensions in your project:

| Extension | Usage | Description |
| :-------- | :---- | :---------- |
| [XR_FB_scene_capture](https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_FB_scene_capture) | [Session](xref:meta-openxr-session) | This extension allows an application to request that the system begin capturing information about what is in the environment around the user. |
| [XR_FB_passthrough](https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_FB_passthrough) | [Camera](xref:meta-openxr-camera) | Passthrough is a way to show a user their physical environment in a light-blocking VR headset. |
| [XR_FB_spatial_entity](https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_FB_spatial_entity) | [Planes](xref:meta-openxr-planes), [Anchors](xref:meta-openxr-anchors), [Meshing](xref:meta-openxr-meshing), [Bounding boxes](xref:meta-openxr-bounding-boxes) | This extension enables applications to use spatial entities to specify world-locked frames of reference. It enables applications to persist the real world location of content over time. All Facebook spatial entity and scene extensions are dependent on this one. |
| [XR_FB_scene](https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_FB_scene) | [Planes](xref:meta-openxr-planes), [Meshing](xref:meta-openxr-meshing), [Bounding boxes](xref:meta-openxr-bounding-boxes) | This extension expands on the concept of spatial entities to include a way for a spatial entity to represent rooms, objects, or other boundaries in a scene. |
| [XR_FB_display_refresh_rate](https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_FB_display_refresh_rate) | [Display Utilities](xref:meta-openxr-display-utilities) | On platforms that support dynamically adjusting the display refresh rate, application developers may request a specific display refresh rate in order to improve the overall user experience. |
| [XR_META_spatial_entity_mesh](https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_META_spatial_entity_mesh) | [Meshing](xref:meta-openxr-meshing) | This extension expands on the concept of spatial entities to include a way for a spatial entity to represent a triangle mesh that describes 3D geometry of the spatial entity in a scene. |
| [XR_META_spatial_entity_discovery](https://developer.oculus.com/documentation/native/android/openxr-spatial-anchors-api-ref/) | [Planes](xref:meta-openxr-planes), [Anchors](xref:meta-openxr-anchors), [Meshing](xref:meta-openxr-meshing), [Bounding boxes](xref:meta-openxr-bounding-boxes) | This extension enables finding and loading persisted Spatial Entities which can then be tracked across different sessions and over time by applications. |
| [XR_META_spatial_entity_persistence](https://developer.oculus.com/documentation/native/android/openxr-spatial-anchors-api-ref/) | [Anchors](xref:meta-openxr-anchors) | This extension enables saving and erasing Spatial Entities, allowing them to be retrieved and tracked across different sessions and over time. |
