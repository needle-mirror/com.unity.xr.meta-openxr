---
uid: meta-openxr-anchors
---
# Anchors

This page is a supplement to the AR Foundation [Anchors](xref:arfoundation-anchors) manual. The following sections only contain information about APIs where Meta Quest exhibits unique platform-specific behavior.

[!include[](../snippets/arf-docs-tip.md)]

## Trackable ID

Unlike some other AR platforms, Meta's OpenXR runtime only supports creating anchors asynchronously. To fulfill AR Foundation's synchronous API design for [TryAddAnchor](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystem.TryAddAnchor), this package generates monotonically increasing `TrackableId`s for anchors.

## Native pointer

[XRAnchor.nativePtr](xref:UnityEngine.XR.ARSubsystems.XRAnchor.nativePtr) values returned by this package contain a pointer to the following struct:

```c
typedef struct UnityXRNativeAnchor
{
    int version;
    void* referencePointPtr;
} UnityXRNativeAnchor;
```

Cast the `void* referencePointPtr` to an [XrSpace](https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#spaces) handle in C++ using the following example code:

```cpp
// Marhshal the native anchor data from the XRAnchor.nativePtr in C#
UnityXRNativeAnchor nativeAnchor;
XrSpace* anchorXrSpaceHandle = static_cast<XrSpace*>(nativeAnchor.referencePointPtr);
```

To learn more about native pointers and their usage, refer to [Extending AR Foundation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.1/manual/architecture/extensions.html).
