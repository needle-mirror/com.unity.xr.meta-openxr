---
uid: meta-openxr-camera-image-capture
---
# Image capture

Configure CPU and GPU image capture for your OpenXR Meta project.

OpenXR Meta supports AR Foundation [Image capture](xref:arfoundation-image-capture) for both CPU and GPU images. The method you choose to access device camera images depends on how you intend to process the image. There are tradeoffs to either a GPU-based or a CPU-based approach.

* **CPU:** Use CPU if you will access the image's pixel data in a C# script. This is more resource-intensive, but allows you to perform operations such as save the image to a file or pass it to a CPU-based computer vision system.
* **GPU:** GPU provides best performance if you will simply render the image or process it with a shader. GPU images provide zero-copy access to camera data, making them ideal for real-time rendering and shader-based processing.

## Enable camera image support

To enable CPU and GPU image support in your app, enable the **Camera Image Support** option as follows:

1. Go to **Project Settings** > **XR Plug-in Management** > **OpenXR**.
2. Select the **Android settings** tab.
3. Under the **OpenXR Feature Groups** heading, select the **Meta Quest** feature group.
4. Click the gear icon next to the **Meta Quest: Camera (Passthrough)** feature.
5. Enable the **Camera Image Support** option.

![Camera Feature Settings](../../images/camera-feature-settings.png)<br/>*Camera feature settings.*

![Camera Image Support](../../images/camera-images-setting.png)<br/>*Enable the Camera Image Support setting.*

## Access images via CPU

For information about accessing camera images on the CPU, including synchronous and asynchronous conversion, refer to AR Foundation's [Image capture](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@latest/manual/features/camera/image-capture.html#access-images-via-cpu) documentation.

On Meta Quest, the camera subsystem supports the following CPU image acquisition options:

| **Option** | **Description** |
|:---------- | :-------------- |
| **Mono (default)** | Use [MetaOpenXRCameraSubsystem.TryAcquireLatestCpuImage](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.TryAcquireLatestGpuImage(UnityEngine.XR.ARSubsystems.XRTextureDescriptor@)) to obtain a single left eye camera image. |
| **Left or right eye Camera (single)** | Use [MetaOpenXRCameraSubsystem.TryAcquireLatestCpuImageForPosition](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.TryAcquireLatestCpuImageForPosition(UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.CameraPosition,UnityEngine.XR.ARSubsystems.XRCpuImage.Cinfo@)) with [CameraPosition.LeftEye](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.CameraPosition) or [CameraPosition.RightEye](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.CameraPosition) to acquire the latest image from one specific camera. |
| **Stereo pair (synchronized)** | Use [MetaOpenXRCameraSubsystem.TryAcquireLatestStereoCpuImagePair](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.TryAcquireLatestStereoCpuImagePair(UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.XRCpuImagePair@)) to acquire a left and right image from the same capture request. This returns an [XRCpuImagePair](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.XRCpuImagePair) with `leftEyeImageCinfo` and `rightEyeImageCinfo`. |

The following code samples demonstrate each CPU acquisition option. Dispose each `XRCpuImage` when done (for example with `using`).

### Mono (default)

The following code sample demonstrates mono (single left eye) CPU image acquisition:

[!code-cs[acquire_cpu_image](../../../Tests/Runtime/CodeSamples/CpuImageSamples.cs#acquire_cpu_image)]

### Left or right eye (single)

The following code sample demonstrates single eye (left or right) CPU image acquisition:

[!code-cs[acquire_cpu_image_for_position](../../../Tests/Runtime/CodeSamples/CpuImageSamples.cs#acquire_cpu_image_for_position)]

### Stereo pair

The following code sample demonstrates stereo eye CPU image acquisition:

[!code-cs[acquire_stereo_cpu_image_pair](../../../Tests/Runtime/CodeSamples/CpuImageSamples.cs#acquire_stereo_cpu_image_pair)]

> [!NOTE]
> CPU image capture for Meta Quest requires a minimum Android API level `32`. You can set the **Minimum API Level** to **Android 12L (API level 32)** in your project's Player settings. Refer to [Android Player settings](xref:um-class-player-settings-android) for more information.

## Access images via GPU

The following sections describe how to access camera images on the GPU for Meta Quest.

### Requirements

* **Graphics API:** GPU image capture requires you to set your graphics API to Vulkan as outlined in [Vulkan Graphics API](xref:meta-openxr-graphics-settings). `TryAcquireLatestGpuImage` will return `false` if any other graphics API is selected.
* **Rendering callbacks:** GPU images must be acquired and released during Unity's rendering callbacks ([RenderPipelineManager.beginCameraRendering](xref:UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering(System.Action`2<UnityEngine.Rendering.ScriptableRenderContext,UnityEngine.Camera>)) and [RenderPipelineManager.endCameraRendering](xref:UnityEngine.Rendering.RenderPipelineManager.endCameraRendering(System.Action`2<UnityEngine.Rendering.ScriptableRenderContext,UnityEngine.Camera>))), respectively, to ensure proper synchronization with the graphics pipeline.

> [!NOTE]
> GPU image capture is not supported on Quest Link.

### Acquire and Release GPU images

> [!NOTE]
> Only one GPU acquisition is active at a time: either one single image (default), one per-camera image, or one stereo pair. Release the current acquisition before switching to another mode or acquiring again.

On Meta Quest, the camera subsystem supports the following GPU image acquisition methods to obtain an `XRTextureDescriptor`. The descriptor contains a native Vulkan image handle that you can use to create a Unity texture. The acquisition options are:

| **Option** | **Description** |
|:---------- | :-------------- |
| **Mono (default)** | Use [MetaOpenXRCameraSubsystem.TryAcquireLatestGpuImage](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.TryAcquireLatestGpuImage(UnityEngine.XR.ARSubsystems.XRTextureDescriptor@)) to acquire a single [XRTextureDescriptor](xref:UnityEngine.XR.ARSubsystems.XRTextureDescriptor). Release with [ReleaseGpuImage](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.ReleaseGpuImage(UnityEngine.XR.ARSubsystems.XRTextureDescriptor)). |
| **Left or right eye (single)** | Use [MetaOpenXRCameraSubsystem.TryAcquireLatestGpuImageForPosition](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.TryAcquireLatestGpuImageForPosition(UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.CameraPosition,UnityEngine.XR.ARSubsystems.XRTextureDescriptor@)) with [CameraPosition.LeftEye](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.CameraPosition) or [CameraPosition.RightEye](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.CameraPosition) to acquire the latest image from one camera. Release with [ReleaseGpuImageForPosition](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.ReleaseGpuImageForPosition(UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.CameraPosition,UnityEngine.XR.ARSubsystems.XRTextureDescriptor)). <strong>Note</strong>: Right eye requires stereo image capture support. |
| **Stereo pair (synchronized)** | Use [MetaOpenXRCameraSubsystem.TryAcquireLatestStereoGpuImagePair](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.TryAcquireLatestStereoGpuImagePair(UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.XRTextureDescriptorPair@)) to acquire a left and right [XRTextureDescriptorPair](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.XRTextureDescriptorPair) from the same capture request. Release with [ReleaseStereoGpuImagePair](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.ReleaseStereoGpuImagePair(UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.XRTextureDescriptorPair)). Returns `false` if images are not synchronized. |

The following code samples demonstrate each GPU acquisition option. Each sample acquires and releases within the same frame.

### Mono (default)

The following code sample demonstrates mono (single left eye) GPU image acquisition:

[!code-cs[acquire_gpu_image](../../../Tests/Runtime/CodeSamples/GpuImageSamples.cs#acquire_gpu_image)]

### Left or right eye (single)

The following code sample demonstrates single eye (left or right) GPU image acquisition:

[!code-cs[acquire_gpu_image_for_position](../../../Tests/Runtime/CodeSamples/GpuImageSamples.cs#acquire_gpu_image_for_position)]

### Stereo pair

The following code sample demonstrates stereo eye GPU image acquisition:

[!code-cs[acquire_stereo_gpu_image_pair](../../../Tests/Runtime/CodeSamples/GpuImageSamples.cs#acquire_stereo_gpu_image_pair)]

> [!IMPORTANT]
> GPU images are valid only for the current frame. Do not hold references to GPU images across frames.
> Failure to release images will prevent acquiring new images and might cause resource leaks.

## Stereo sync API

When acquiring a stereo image pair, the subsystem tries to match left and right image timestamps so both images come from the same capture. [MetaOpenXRCameraSubsystem.SetMaxStereoSyncAttempts](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.SetMaxStereoSyncAttempts(System.Int32)) sets how many times the subsystem will attempt to find a synchronized pair. The value is clamped to `1`-`10`. The default value is `3`.

If stereo capture fails to find a synced pair, it will return the last acquired pair. You can call this method before or after the subsystem starts.

## Additional resources

* [Image capture](xref:arfoundation-image-capture) (AR Foundation)
