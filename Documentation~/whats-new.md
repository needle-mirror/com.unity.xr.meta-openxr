---
uid: meta-openxr-whats-new
---
# What's new in version 2.1

This release includes the following significant changes:

## New features

### Meta Quest Link

- Added support for Quest Link. Refer to [Meta Quest Link](xref:meta-openxr-link) for more information.

### Occlusion

- Added support for occlusion through AR Foundation's occlusion subsystem. Refer to [Occlusion](xref:meta-openxr-occlusion) for more information.

### Boundary visibility

- Added the Meta Quest Boundary Visibility feature, which enables you to suppress the visibility of the boundary so users can move freely with Passthrough enabled. Refer to [Boundary visibility](xref:meta-openxr-boundary-visibility) for more information.

### Other API additions

- Added a public class [MetaOpenXRFeature](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRFeature) as the new base class for OpenXR features in this package.

## Changes

- Added a validation rule to warn you that soft shadows can negatively affect performance for Meta Quest.
