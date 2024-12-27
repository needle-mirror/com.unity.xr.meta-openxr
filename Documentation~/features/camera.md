---
uid: meta-openxr-camera
---
# Camera

On Meta Quest devices, AR Foundation's Camera subsystem controls Meta [Passthrough](https://www.meta.com/help/quest/articles/in-vr-experiences/oculus-features/passthrough/).

This page is a supplement to the AR Foundation [Camera](xref:arfoundation-camera) manual. The following sections only contain information about APIs where Meta Quest exhibits unique platform-specific behavior.

[!include[](../snippets/arf-docs-tip.md)]

<a id="passthrough"/>

## Passthrough

The Meta Passthrough camera captures real-time images of the external environment to provide the user with a view of their surroundings while wearing a headset. You can use Passthrough to implement immersive mixed-reality experiences, by layering virtual content on top of Passthrough images of the surrounding environment.

### Enable Passthrough

To use Passthrough in your Meta project, you must have an [AR Camera Manager component](xref:arfoundation-camera-components#ar-camera-manager-component) attached to your camera. Enable the AR Camera Manager component to enable Passthrough, and disable it to disable Passthrough.

### AR Camera Background component

Meta Passthrough does not require the [AR Camera Background component](xref:arfoundation-camera-components#ar-camera-background-component). If `ARCameraBackground` is in your scene, it will have no effect on Meta Quest devices. If your scene only targets Meta Quest devices, you can safely delete the AR Camera Background component from your XR Origin's **Main Camera** GameObject.

### Camera background

Meta Passthrough requires that your Camera has a transparent background. To do this, set your **Background Color** (Universal Render Pipeline) or **Clear Flags** (Built-In Render Pipeline) to **Solid Color**, with the **Background** alpha channel value set to `0`.

Refer to [Configure camera background for Passthrough](xref:meta-openxr-scene-setup#camera-background-passthrough) to understand how to set your camera background.

> [!NOTE]
> If Passthrough isn't working in your project after setting the alpha channel value to `0`, ensure your URP settings are correctly configured, as described in [Optimize graphics settings](xref:meta-openxr-graphics-settings#universal-render-pipeline).

## Image capture

This package does not support AR Foundation [Image capture](xref:arfoundation-image-capture).
