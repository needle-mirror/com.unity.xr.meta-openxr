---
uid: meta-openxr-boundary-visibility
---
# Boundary visibility

The Meta Quest Boundary Visibility feature enables you to suppress the visibility of the boundary so users can move freely with Passthrough enabled.

![A Meta Quest 3 screenshot shows a Passthrough image of a room with a system warning displayed. The warning reads, "Return to your boundary"](../images/return-to-boundary.png)<br/>*When boundary visibility isn't suppressed, the system may warn the user when they leave the boundary.*

## Enable boundary visibility

To enable boundary visibility in your app:

1. Go to **Project Settings** > **XR Plug-in Management** > **OpenXR**.
2. Select either the **Windows, Mac, Linux settings** tab to configure Quest Link on Windows, or the **Android settings** tab to configure builds.
3. Under the **OpenXR Feature Groups** heading, select the **Meta Quest** feature group.
4. If disabled, enable the **Meta Quest: Boundary Visibility** OpenXR feature.

![Meta Quest feature group in the Unity Editor](../images/openxr-features-all-landscape.png)<br/>*The Meta Quest feature group, shown with all features enabled.*

## Example code

Refer to the following example code to understand how to suppress boundary visibility:

[!code-cs[boundary_visibility](../../Tests/Runtime/CodeSamples/BoundaryVisibilitySample.cs#boundary_visibility)]
