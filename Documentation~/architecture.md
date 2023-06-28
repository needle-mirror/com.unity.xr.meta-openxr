---
uid: meta-openxr-architecture
---
# Architecture

The Meta OpenXR Feature package functions as both an [OpenXR Feature](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.6?subfolder=/manual/features.html) and an AR Foundation [provider plug-in](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.0?subfolder=/manual/architecture/subsystems.html), and primarily contains subsystem providers that are implemented via Meta's OpenXR extensions.

## Meta OpenXR extensions

Meta's OpenXR extensions can be found in the Khronos Group [OpenXR Specification](https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html).

This package enables the following OpenXR extension strings in your project:

| Extension String | Purpose | Description |
| :--------------- | :------ | :---------- |
| [XR_FB_passthrough](https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_FB_passthrough) | Passthrough | Passthrough is a way to show a user their physical environment in a light-blocking VR headset. |
| [XR_FB_spatial_entity](https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_FB_spatial_entity) | Plane detection, Anchors | This extension enables applications to use spatial entities to specify world-locked frames of reference. It enables applications to persist the real world location of content over time and contains definitions for the Entity-Component System. All Facebook spatial entity and scene extensions are dependent on this one. |
| [XR_FB_spatial_entity_query](https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_FB_spatial_entity_query) | Plane detection | This extension enables an application to discover persistent spatial entities in the area and restore them. Using the query system, the application can load persistent spatial entities from storage. |
| [XR_FB_spatial_entity_storage](https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_FB_spatial_entity_storage) | Plane detection | This extension enables spatial entities to be stored and persisted across sessions. |
| [XR_FB_scene](https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_FB_scene) | Plane detection | This extension expands on the concept of spatial entities to include a way for a spatial entity to represent rooms, objects, or other boundaries in a scene. |
