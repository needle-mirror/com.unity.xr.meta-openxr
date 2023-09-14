| Feature | Description | Implemented | Manual |
| :------ | :---------- | :---------- | :----- |
| [Session](xref:arfoundation-session) | Enable, disable, and configure AR on the target platform. | Yes | [Session](xref:meta-openxr-session) (Meta) |
| [Device tracking](xref:arfoundation-device-tracking) | Track the device's position and rotation in physical space. | Yes | N/A |
| [Camera](xref:arfoundation-camera) | Render images from device cameras and perform light estimation. | Yes | [Camera](xref:meta-openxr-camera) (Meta) |
| [Plane detection](xref:arfoundation-plane-detection) | Detect and track flat surfaces. | Yes | [Plane detection](xref:meta-openxr-plane-detection) (Meta) |
| [Image tracking](xref:arfoundation-image-tracking) | Detect and track 2D images. | | |
| [Object tracking](xref:arfoundation-object-tracking) | Detect and track 3D objects. | | |
| [Face tracking](xref:arfoundation-face-tracking) | Detect and track human faces. | | |
| [Body tracking](xref:arfoundation-body-tracking) | Detect and track a human body. | | |
| [Point clouds](xref:arfoundation-point-clouds) | Detect and track feature points. | | |
| [Raycasts](xref:arfoundation-raycasts) | Cast rays against tracked items. | Yes | N/A |
| [Anchors](xref:arfoundation-anchors) | Track arbitrary points in space. | Yes | N/A |
| [Meshing](xref:arfoundation-meshing) | Generate meshes of the environment. | | |
| [Environment probes](xref:arfoundation-environment-probes) | Generate cubemaps of the environment. | | |
| [Occlusion](xref:arfoundation-occlusion) | Occlude AR content with physical objects and perform human segmentation. | | |
| [Participants](xref:arfoundation-participant-tracking) | Track other devices in a shared AR session. | | |

## Meta OpenXR manual

The [AR Foundation](xref:arfoundation-manual) package manual defines each AR Feature in the table above and explains how to use them. This Unity OpenXR: Meta package manual is a supplement to AR Foundation, and only includes information regarding APIs where the Meta OpenXR Feature exhibits unique platform-specific behavior.

> [!TIP]
> When developing an AR app, refer to both the AR Foundation documentation as well as the [required packages](xref:arfoundation-manual#required-packages) for each platform you support.
