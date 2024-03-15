---
uid: meta-openxr-anchors
---
# Anchors

This page is a supplement to the AR Foundation [Anchors](xref:arfoundation-anchors) manual. The following sections only contain information about APIs where Meta Quest exhibits unique platform-specific behavior.

[!include[](../snippets/arf-docs-tip.md)]

## Platform-specific success and error codes

On OpenXR platforms, the [XRResultStatus](xref:UnityEngine.XR.ARSubsystems.XRResultStatus) object returned by AR Foundation's [ARAnchorManager.TryAddAnchorAsync](xref:UnityEngine.XR.ARFoundation.ARAnchorManager.TryAddAnchorAsync(UnityEngine.Pose)) is a wrapper around OpenXR's [XrResult](xref:UnityEngine.XR.OpenXR.NativeTypes.XrResult).

You can use the `AsXrResult` extension method to access the underlying `XrResult` value, as shown in the example below:

[!code-cs[anchors_resultstatus_xrresult](../../Tests/CodeSamples/AnchorsSamples.cs#anchors_resultstatus_xrresult)]

The `AsXrResult` extension method is only available wherever this package's namespace is used. Add the following line to your C# script to make the extension method available:

```
using UnityEngine.XR.OpenXR.Features.Meta;
```
