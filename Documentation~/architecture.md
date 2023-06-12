---
uid: meta-openxr-architecture
---
# Architecture

The Meta OpenXR Feature package functions as both an [OpenXR Feature](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.6?subfolder=/manual/features.html) and an AR Foundation [provider plug-in](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.0?subfolder=/manual/architecture/subsystems.html), and primarily contains subsystem providers that are implemented via Meta's OpenXR extensions.

## Meta OpenXR extensions

Meta's OpenXR extensions can be found in the Khronos Group [OpenXR Specification](https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html).

This package enables the following OpenXR extension strings in your project:

| Extension String | Description |
| :--------------- | :---------- |
| [XR_FB_passthrough](https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_FB_passthrough) | Passthrough is a way to show a user their physical environment in a light-blocking VR headset. |
| [XR_FB_composition_layer_alpha_blend](https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_FB_composition_layer_alpha_blend) | This extension provides explicit control over source and destination blend factors, with separate controls for color and alpha. |
