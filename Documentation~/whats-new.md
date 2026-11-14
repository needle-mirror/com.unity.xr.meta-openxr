---
uid: meta-openxr-whats-new
---
# What's new in version 7.0

Unity OpenXR: Meta 7.0 updates the package for AR Foundation 7.0 and Unity 7000.0, and removes support for session availability APIs that are obsolete in AR Foundation 7.0.

Discover new features and performance improvements in the latest update to Unity OpenXR: Meta.

For a full list of changes in this version including backwards-compatible bugfixes, refer to the package [changelog](xref:meta-openxr-changelog).

## Updated for AR Foundation 7.0

Unity OpenXR: Meta 7.0 requires Unity 7000.0 or newer, AR Foundation 7.0, and XR Composition Layers 7.0. AR Foundation 7.0 contains breaking changes, such as the rename of `BoundedPlane` to [XRPlane](xref:UnityEngine.XR.ARSubsystems.XRPlane), which the Meta OpenXR plane subsystem now uses. Refer to [Upgrade to AR Foundation 7.0](xref:arfoundation-upgrade-guide-7-0) for the changes you need to make to your project.

## Removed session availability checks

In AR Foundation 7.0, the APIs that check session availability separately from starting a session are obsolete and cause compiler errors. As a result, the Meta OpenXR session subsystem no longer implements a session availability check.

The following table lists the obsolete AR Foundation APIs that the Meta OpenXR session subsystem previously supported, and what to use instead:

| API | Remediation |
| :-- | :---------- |
| `XRSessionSubsystem.GetAvailabilityAsync` | Call [XRResultStatus.IsSuccess](xref:UnityEngine.XR.ARSubsystems.XRResultStatus.IsSuccess) on the result of [TryStartAsync](xref:UnityEngine.XR.ARSubsystems.XRSubsystem`3.TryStartAsync*) instead. |
| `ARSessionManager.CheckAvailability` | Call [XRResultStatus.IsSuccess](xref:UnityEngine.XR.ARSubsystems.XRResultStatus.IsSuccess) on the result of [TryStartAsync](xref:UnityEngine.XR.ARSubsystems.XRSubsystem`3.TryStartAsync*) instead. |
