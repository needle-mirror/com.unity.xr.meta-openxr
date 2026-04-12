---
uid: meta-openxr-camera
---
<a id="passthrough"></a>

# Camera (Passthrough)

The Meta Quest Camera (Passthrough) feature controls Meta [Passthrough](https://www.meta.com/help/quest/articles/in-vr-experiences/oculus-features/passthrough/), a device capability that captures real-time images of the physical environment and renders them to screen. Your app can use Passthrough to give the user a view of their surroundings while wearing a headset, enabling them to safely move through their space while using your app.

As an AR Foundation provider plug-in, this package implements Passthrough via AR Foundation's camera subsystem, and this page is a supplement to the AR Foundation [Camera](xref:arfoundation-camera) manual. The following sections only contain information about Meta Quest-specific behavior.

[!include[](../snippets/arf-docs-tip.md)]

## Enable Passthrough

To enable the Passthrough capability in your app:

1. Go to **Project Settings** > **XR Plug-in Management** > **OpenXR**.
2. Select either the **Windows, Mac, Linux settings** tab to configure Quest Link on Windows, or the **Android settings** tab to configure builds.
3. Under the **OpenXR Feature Groups** heading, select the **Meta Quest** feature group.
4. If disabled, enable the **Meta Quest: Camera (Passthrough)** OpenXR feature.

![Meta Quest feature group in the Unity Editor](../images/openxr-features-all-landscape.png)<br/>*The Meta Quest feature group, shown with all features enabled.*

<a id="passthrough-pre-splash-screen"></a>

### Enable Passthrough splash screen

If your app uses Passthrough, you can replace the default splash screen with a Passthrough splash screen when your app launches to enable a seamless Passthrough experience.

For more information, refer to [Composition Layer Splash Screen](xref:xr-layers-splash-screen).

### Enable Passthrough pre-splash screen

If your app uses Passthrough, you can enable Passthrough before the splash screen is displayed to enable a seamless Passthrough experience. This will override the default opaque startup screen, with a Passthrough background. For more information, visit [Passthrough Loading Screen](https://developers.meta.com/horizon/documentation/native/android/mobile-passthrough-loading-screens/) (Meta developer documentation).

To do this, enable **Passthrough Pre Splash Screen** as follows:

1. Select the gear icon next to the **Meta Quest: Camera (Passthrough)** feature to configure Passthrough settings.
2. Enable **Passthrough Pre Splash Screen**.

![Passthrough pre-splash screen](../images/passthrough-pre-splash-screen.png)<br/>*Passthrough Pre Splash Screen setting.*

## Scene setup

To use Passthrough in your scene, you must have an [AR Camera Manager component](xref:arfoundation-camera-components#ar-camera-manager-component) attached to your camera. Enable the AR Camera Manager component to enable Passthrough, and disable it to disable Passthrough.

### Camera background color

Passthrough requires that your Camera has a transparent background. To do this, set your **Background Color** (Universal Render Pipeline) or **Clear Flags** (Built-In Render Pipeline) to **Solid Color**, with the **Background** alpha channel value set to `0`.

> [!IMPORTANT]
> In Unity 6.5 and newer, the Built-In Render Pipeline is deprecated and will be made obsolete in a future release. For more information, refer to [Migrating from the Built-In Render Pipeline to URP](https://docs.unity3d.com/6000.5/Documentation/Manual/urp/upgrading-from-birp.html) and [Render pipeline feature comparison](https://docs.unity3d.com/6000.5/Documentation/Manual/render-pipelines-feature-comparison.html).

Refer to [Configure camera background for Passthrough](xref:meta-openxr-scene-setup#camera-background-passthrough) for step-by-step instructions.

> [!TIP]
> If Passthrough isn't working in your project after setting the alpha channel value to `0`, ensure your URP settings are correctly configured, as described in [Optimize graphics settings](xref:meta-openxr-graphics-settings#universal-render-pipeline).

<a id="composition-layers"></a>

## Composition layers

Unity doesn't have access to pixels or other image data associated with Meta Passthrough images. Instead, Meta's OpenXR runtime submits the images directly to the OpenXR compositor as composition layers. Unity's [XR Composition Layers](xref:xr-layers-index) package provides an interface for authoring your app's composition layers. The Unity OpenXR: Meta Passthrough layer is a custom layer type that you can use to control how Passthrough is composited in your app.

For more information about composition layers, refer to the [Composition layers](xref:xr-layer-overview) package documentation.

### Meta Passthrough layer

If your scene uses Passthrough, by default the camera subsystem will create a GameObject at runtime that enables the Meta Passthrough layer. This `Passthrough` GameObject appears at the root of your scene hierarchy.

![Passthrough GameObject](../images/passthrough-gameobject.png)<br/>*Passthrough GameObject created at runtime.*

The Passthrough GameObject contains a [Composition Layer component](xref:xr-layers-base-component) that controls the Passthrough layer. Its default **Layer Order** is `-1`, and it uses the **Alpha** blend type.

![Passthrough CompositionLayer](../images/passthrough-gameobject-components.png)<br/>*Passthrough GameObject shown in the Inspector window.*

### Customize your composition layers

If you prefer to author your app's composition layers in the Editor, you can [Add a layer in the Editor](xref:xr-layers-add-layer#add-a-layer-in-the-editor) as you normally would when working with the XR Composition Layers package. For your Passthrough layer, choose the **Meta OpenXR - Passthrough** layer type, and [Set the layer sort order in the Editor](xref:xr-layers-order#set-the-layer-sort-order-in-the-editor) to your desired value.

## AR Camera Background component

Meta Passthrough does not require the [AR Camera Background component](xref:arfoundation-camera-components#ar-camera-background-component). If `ARCameraBackground` is in your scene, it will have no effect on Meta Quest devices. If your scene only targets Meta Quest devices, you can safely delete the AR Camera Background component from your XR Origin's **Main Camera** GameObject.

## Image capture

This package supports AR Foundation [Image capture](xref:arfoundation-image-capture) for both CPU and GPU images. The method you choose to access device camera images depends on how you intend to process the image. There are tradeoffs to either a GPU-based or a CPU-based approach.

* **CPU:** Use CPU if you will access the image's pixel data in a C# script. This is more resource-intensive, but allows you to perform operations such as save the image to a file or pass it to a CPU-based computer vision system.
* **GPU:** GPU provides best performance if you will simply render the image or process it with a shader. GPU images provide zero-copy access to camera data, making them ideal for real-time rendering and shader-based processing.

### Enable camera image support

To enable CPU and GPU image support in your app, enable the **Camera Image Support** option as follows:

1. Go to **Project Settings** > **XR Plug-in Management** > **OpenXR**.
2. Select the **Android settings** tab.
3. Under the **OpenXR Feature Groups** heading, select the **Meta Quest** feature group.
4. Click the gear icon next to the **Meta Quest: Camera (Passthrough)** feature.
5. Enable the **Camera Image Support** option.

![Camera Feature Settings](../images/camera-feature-settings.png)<br/>*Camera feature settings.*

![Camera Image Support](../images/camera-images-setting.png)<br/>*Enable the Camera Image Support setting.*

### Access images via CPU

For information about accessing camera images on the CPU, including synchronous and asynchronous conversion, refer to AR Foundation's [Image capture](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@latest/manual/features/camera/image-capture.html#access-images-via-cpu) documentation.

On Meta Quest, the camera subsystem supports the following CPU image acquisition options:

| **Option** | **Description** |
|:---------- | :-------------- |
| **Mono (default)** | Use [MetaOpenXRCameraSubsystem.TryAcquireLatestCpuImage](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.TryAcquireLatestCpuImage(UnityEngine.XR.ARSubsystems.XRCpuImage@)) to obtain a single left eye camera image. |
| **Left or right eye Camera (single)** | Use [MetaOpenXRCameraSubsystem.TryAcquireLatestCpuImageForPosition](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.TryAcquireLatestCpuImageForPosition(UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.CameraPosition,UnityEngine.XR.ARSubsystems.XRCpuImage.Cinfo@)) with [CameraPosition.LeftEye](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.CameraPosition) or [CameraPosition.RightEye](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.CameraPosition) to acquire the latest image from one specific camera. |
| **Stereo pair (synchronized)** | Use [MetaOpenXRCameraSubsystem.TryAcquireLatestStereoCpuImagePair](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.TryAcquireLatestStereoCpuImagePair(UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.XRCpuImagePair@)) to acquire a left and right image from the same capture request. This returns an [XRCpuImagePair](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.XRCpuImagePair) with `leftEyeImageCinfo` and `rightEyeImageCinfo`. |

The following code samples demonstrate each CPU acquisition option. Dispose each `XRCpuImage` when done (e.g. with `using`).

#### Mono (default)

The following code sample demonstrates mono (single left eye) CPU image acquisition:

[!code-cs[acquire_cpu_image](../../Tests/Runtime/CodeSamples/CpuImageSamples.cs#acquire_cpu_image)]

#### Left or right eye (single)

The following code sample demonstrates single eye (left or right) CPU image acquisition:

[!code-cs[acquire_cpu_image_for_position](../../Tests/Runtime/CodeSamples/CpuImageSamples.cs#acquire_cpu_image_for_position)]

#### Stereo pair

The following code sample demonstrates stereo eye CPU image acquisition:

[!code-cs[acquire_stereo_cpu_image_pair](../../Tests/Runtime/CodeSamples/CpuImageSamples.cs#acquire_stereo_cpu_image_pair)]

> [!NOTE]
> CPU image capture for Meta Quest requires a minimum Android API level `32`. You can set the **Minimum API Level** to **Android 12L (API level 32)** in your project's Player settings. Refer to [Android Player settings](xref:um-class-player-settings-android) for more information.

### Access images via GPU

The following sections describe how to access camera images on the GPU for Meta Quest.

#### Requirements

* **Graphics API:** GPU image capture requires you to set your graphics API to Vulkan as outlined in [Vulkan Graphics API](xref:meta-openxr-graphics-settings). `TryAcquireLatestGpuImage` will return `false` if any other graphics API is selected.
* **Rendering callbacks:** GPU images must be acquired and released during Unity's rendering callbacks ([RenderPipelineManager.beginCameraRendering](xref:UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering(System.Action`2<UnityEngine.Rendering.ScriptableRenderContext,UnityEngine.Camera>)) and [RenderPipelineManager.endCameraRendering](xref:UnityEngine.Rendering.RenderPipelineManager.endCameraRendering(System.Action`2<UnityEngine.Rendering.ScriptableRenderContext,UnityEngine.Camera>))), respectively, to ensure proper synchronization with the graphics pipeline.

> [!NOTE]
> GPU image capture is not supported on Quest Link.

#### Acquire and Release GPU images

> [!NOTE]
> Only one GPU acquisition is active at a time: either one single image (default), one per-camera image, or one stereo pair. Release the current acquisition before switching to another mode or acquiring again.

On Meta Quest, the camera subsystem supports the following GPU image acquisition methods to obtain an `XRTextureDescriptor`. The descriptor contains a native Vulkan image handle that you can use to create a Unity texture. The acquisition options are:

| **Option** | **Description** |
|:---------- | :-------------- |
| **Mono (default)** | Use [MetaOpenXRCameraSubsystem.TryAcquireLatestGpuImage](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.TryAcquireLatestGpuImage(UnityEngine.XR.ARSubsystems.XRTextureDescriptor@)) to acquire a single [XRTextureDescriptor](xref:UnityEngine.XR.ARSubsystems.XRTextureDescriptor). Release with [ReleaseGpuImage](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.ReleaseGpuImage(UnityEngine.XR.ARSubsystems.XRTextureDescriptor)). |
| **Left or right eye (single)** | Use [MetaOpenXRCameraSubsystem.TryAcquireLatestGpuImageForPosition](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.TryAcquireLatestGpuImageForPosition(UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.CameraPosition,UnityEngine.XR.ARSubsystems.XRTextureDescriptor@)) with [CameraPosition.LeftEye](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.CameraPosition) or [CameraPosition.RightEye](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.CameraPosition) to acquire the latest image from one camera. Release with [ReleaseGpuImageForPosition](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.ReleaseGpuImageForPosition(UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.CameraPosition,UnityEngine.XR.ARSubsystems.XRTextureDescriptor)). <strong>Note</strong>: Right eye requires stereo image capture support. |
| **Stereo pair (synchronized)** | Use [MetaOpenXRCameraSubsystem.TryAcquireLatestStereoGpuImagePair](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.TryAcquireLatestStereoGpuImagePair(UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.XRTextureDescriptorPair@)) to acquire a left and right [XRTextureDescriptorPair](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.XRTextureDescriptorPair) from the same capture request. Release with [ReleaseStereoGpuImagePair](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.ReleaseStereoGpuImagePair(UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.XRTextureDescriptorPair)). Returns `false` if images are not synchronized. |

The following code samples demonstrate each GPU acquisition option. Each are acquired and released within the same frame.

#### Mono (default)

The following code sample demonstrates mono (single left eye) GPU image acquisition:

[!code-cs[acquire_gpu_image](../../Tests/Runtime/CodeSamples/GpuImageSamples.cs#acquire_gpu_image)]

#### Left or right eye (single)

The following code sample demonstrates single eye (left or right) GPU image acquisition:

[!code-cs[acquire_gpu_image_for_position](../../Tests/Runtime/CodeSamples/GpuImageSamples.cs#acquire_gpu_image_for_position)]

#### Stereo pair

The following code sample demonstrates stereo eye CPU image acquisition:

[!code-cs[acquire_stereo_gpu_image_pair](../../Tests/Runtime/CodeSamples/GpuImageSamples.cs#acquire_stereo_gpu_image_pair)]

> [!IMPORTANT]
> GPU images are valid only for the current frame. Do not hold references to GPU images across frames.
> Failure to release images will prevent acquiring new images and might cause resource leaks.

### Stereo sync API

When acquiring a stereo image pair, the subsystem tries to match left and right image timestamps so both images come from the same capture. [MetaOpenXRCameraSubsystem.SetMaxStereoSyncAttempts](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRCameraSubsystem.SetMaxStereoSyncAttempts(System.Int32)) sets how many times the subsystem will attempt to find a synchronized pair. The value is clamped to 1–10; the default is 3. If stereo capture fails to find a synced pair, it will return the last acquired pair.  You can call this method before or after the subsystem starts.
