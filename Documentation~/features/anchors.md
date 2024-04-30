---
uid: meta-openxr-anchors
---
# Anchors

This page is a supplement to the AR Foundation [Anchors](xref:arfoundation-anchors) manual. The following sections only contain information about APIs where Meta Quest exhibits unique platform-specific behavior.

[!include[](../snippets/arf-docs-tip.md)]

## Optional feature support

Meta OpenXR implements the following optional features of AR Foundation's [XRAnchorSubsystem](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystem):

| Feature | Descriptor Property | Supported |
| :------ | :--------------- | :--------: |
| **Trackable Attachment** | [supportsTrackableAttachments](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsTrackableAttachments) | |
| **Synchronous Add Anchor** | [supportsSynchronousAdd](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsSynchronousAdd) | |
| **Save Anchor** | [supportsSaveAnchor](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsSaveAnchor) | Yes |
| **Load Anchor** | [supportsLoadAnchor](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsLoadAnchor) | Yes |
| **Erase Anchor** | [supportsEraseAnchor](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsEraseAnchor) | Yes |
| **Get Saved Anchor Ids** | [supportsGetSavedAnchorIds](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsGetSavedAnchorIds) | |
| **Async Cancellation** | [supportsAsyncCancellation](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsAsyncCancellation) | |

## Platform-specific success and error codes

On OpenXR platforms, the [XRResultStatus.nativeStatusCode](xref:UnityEngine.XR.ARSubsystems.XRResultStatus.nativeStatusCode) returned by AR Foundation's [ARAnchorManager.TryAddAnchorAsync](xref:UnityEngine.XR.ARFoundation.ARAnchorManager.TryAddAnchorAsync(UnityEngine.Pose)) is a wrapper around OpenXR's [XrResult](xref:UnityEngine.XR.OpenXR.NativeTypes.XrResult).

You can use the `XRResultStatus.nativeStatusCode` property to access the underlying `XrResult` value, as shown in the example below:

[!code-cs[anchors_resultstatus_xrresult](../../Tests/CodeSamples/AnchorsSamples.cs#anchors_resultstatus_xrresult)]

## Persistent anchor GUIDs

On Meta's OpenXR runtime, the `SerializableGuid` returned by [ARAnchorManager.TrySaveAnchorAsync](xref:UnityEngine.XR.ARFoundation.ARAnchorManager.TrySaveAnchorAsync) is the same value as the input anchor's [trackableId](xref:UnityEngine.XR.ARSubsystems.ITrackable.trackableId).
