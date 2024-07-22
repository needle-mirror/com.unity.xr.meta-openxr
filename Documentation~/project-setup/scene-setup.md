---
uid: meta-openxr-scene-setup
---
# Scene setup

To set up your scene for Meta OpenXR, first follow the standard AR Foundation [Scene setup](xref:arfoundation-scene-setup) instructions. The following sections detail additional setup steps specific to projects that target Meta's OpenXR runtime.

## Input actions

Ensure that the **Tracked Pose Driver** component on your camera has "centerEyePosition [XR HMD]" included in the position and rotation input actions.

## Camera background

Meta Passthrough requires that your Camera's **Background Color** (Universal Render Pipeline) or **Clear Flags** (Built-In Render Pipeline) are set to **Solid Color**, with the **Background** color alpha channel value set to zero.

> [!NOTE]
> The passthrough video is layered behind the image rendered by the scene camera. If you configure the camera's background color (or clear flags) to use a skybox or an opaque solid color, then the passthrough video is covered up by the camera background.

If you have completed the AR Foundation scene setup steps, follow these instructions to configure your scene to render with a transparent camera background:

1. Locate the GameObject named **XR Origin** in your GameObject hierarchy. (Refer to [Scene setup](xref:arfoundation-scene-setup) in the AR Foundation manual for instructions on how to set up the scene if it does not already contain an **XR Origin**.) 
2. Expand the hierarchy to reveal the **Camera Offset** and **Main Camera** GameObjects.
3. Inspect the **Main Camera** GameObject.
4. Select from the following options. The options differ based on the render pipeline you're using:
    * URP: In the **Environment** section, set the **Background Type** to **Solid Color**.
    * Built-In Render Pipeline: Set **Clear Flags** to **Solid Color**. 
5. Select the **Background** color to open the color picker.
6. Set the color's **A** value to `0`. 

Your scene is now configured to support Meta Passthrough.

<a id="permissions"/>

## Permissions

Meta's OpenXR runtime requires your app to obtain an Android system permission before you can use any [Space Setup](xref:meta-openxr-device-setup#space-setup) data. Your user must grant your app the `com.oculus.permission.USE_SCENE` permission before you can access any data associated with planes, bounding boxes, or meshes.

If your scene uses AR Foundation's [ARPlaneManager](xref:arfoundation-plane-arplanemanager), [ARBoundingBoxManager](xref:arfoundation-bounding-box-arboundingboxmanager), or [ARMeshManager](xref:arfoundation-meshing) components, you should disable them, because they can't do any work without the required system permission. After your app receives permission, you can safely enable these manager components.

### Example code

The following example code shows you one way to implement permission management with a MonoBehaviour:

[!code-cs[scene-permission](../../Tests/CodeSamples/PermissionsCheck.cs#scene-permission)]

> [!NOTE]
> The AR Foundation Samples GitHub repository includes a working demo of the [PermissionsCheck](https://github.com/Unity-Technologies/arfoundation-samples/blob/main/Assets/Scenes/Meta/PermissionsCheck.cs) component.

To use this example code, add the Permissions Check component to a GameObject in your AR scene. Then in the **Inspector**, subscribe to either the **Permission Denied** or **Permission Granted** events by clicking the **Add** (**+**) button.

![Permissions Check component](../images/permissions-check.png)<br/>*Permissions Check component set up to enable the AR Plane Manager component when permission is granted*
