---
uid: meta-openxr-project-setup
---
# Project setup

To enable the Unity OpenXR: Meta in your project, follow the steps below:

1. Go to **Edit** > **Project Settings...** > **XR Plug-in Management**.
2. In the Android tab, under **Plug-in Providers**, enable **OpenXR**.
3. While still in the Project Settings window, go to **XR Plug-in Management** > **OpenXR**.
4. In the Android tab, under **Interaction Profiles**, add **Oculus Touch Controller Profile**.
5. Under **OpenXR Feature Groups**, enable the **Meta Quest** feature group.

> [!NOTE]
> XR Plug-in Management supports only one enabled plug-in provider at a time per build target. If your project also targets ARCore using the [Google ARCore XR Plug-in](https://docs.unity3d.com/Packages/com.unity.xr.arcore@5.0), to avoid errors you should disable the **Google ARCore** plug-in provider before you build an APK for Meta Quest devices. Likewise, you should disable **OpenXR** before you build for ARCore.

## Scene setup

To set up your scene for Meta OpenXR, first follow the standard AR Foundation [scene setup](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.0?subfolder=/manual/project-setup/scene-setup.html). If your scene will use the Meta Quest device's Passthrough camera, see the additional information below.

Ensure that the **Tracked Pose Driver** component on your camera is has "centerEyePositon [XR HMD]" included in the position and rotation input actions.

### Camera clear flags

Meta Passthrough requires that your Camera's **Clear Flags** are set to **Solid Color**, with the **Background** color alpha channel value set to zero.

If you have completed AR Foundation scene setup, follow these instructions to set your Camera's **Clear Flags**:

- Your GameObject hierarchy should contain a GameObject named **XR Origin**. Expand its children to reveal the **Camera Offset** and **Main Camera** GameObjects.
- Inspect the **Main Camera** GameObject.
- The Camera component's **Clear Flags** should already be set to **Solid Color**. Select the **Background** color to open the color picker.
- Set the color's **A** value to 0.

Your scene is now configured to support Meta Passthrough.
