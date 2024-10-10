---
uid: meta-openxr-camera
---
# Camera

On Meta Quest devices, AR Foundation's Camera subsystem controls Meta [Passthrough](https://www.meta.com/help/quest/articles/in-vr-experiences/oculus-features/passthrough/).

This page is a supplement to the AR Foundation [Camera](xref:arfoundation-camera) manual. The following sections only contain information about APIs where Meta Quest exhibits unique platform-specific behavior.

[!include[](../snippets/arf-docs-tip.md)]

## Enable passthrough

To use Passthrough in your Meta project, you must have an [AR Camera Manager component](xref:arfoundation-camera-components#ar-camera-manager-component) attached to your camera. Enable the AR Camera Manager component to enable Passthrough, and disable it to disable Passthrough.

## AR Camera Background component

Meta Passthrough does not require the [AR Camera Background component](xref:arfoundation-camera-components#ar-camera-background-component). If `ARCameraBackground` is in your scene, it will have no effect on Meta Quest devices. If your scene only targets Meta Quest devices, you can safely delete the AR Camera Background component from your XR Origin's **Main Camera** GameObject.

## Scene setup

Meta Passthrough requires that you set your Camera's **Background Color** (Universal Render Pipeline) or **Clear Flags** (Built-In Render Pipeline) to **Solid Color**, with the **Background** alpha channel value set to `0`. Refer to [Scene setup](xref:meta-openxr-project-setup#scene-setup) to learn how to set the **Background** alpha channel value.

> [!NOTE]
> If Passthrough isn't working in your project after setting the alpha channel value to `0`, ensure your URP settings are correctly configured, as described in [Project setup](xref:meta-openxr-project-setup#universal-render-pipeline)

## Image capture

This package does not support AR Foundation [Image capture](xref:arfoundation-image-capture).
