#if OPENXR_PLUGIN_1_16_0_PRE_2_OR_NEWER
using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine.XR.OpenXR.NativeTypes;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    using XrSession = UInt64;
    using XrSpace = UInt64;

    static class METASpatialEntityRoomMesh
    {
        const string k_SemanticLabel = "COUCH";

        static readonly XrUuid k_TestRoomMeshFaceUuid = new (24UL, 87UL);

        static readonly XrVector3f[] s_TestRoomMeshVertices =
        {
            new(1.1F, 0.0F, 1.2F), new(0.0F, 1.1F, 1.3F), new(-0.9F, -0.1F, 1.4F)
        };

        static readonly XrRoomMeshFaceMETA[] s_TestRoomMeshFaces =
        {
            new(k_TestRoomMeshFaceUuid, XrUuid.empty, XrSemanticLabelMETA.XR_SEMANTIC_LABEL_DOOR_FRAME_META)
        };

        static readonly uint[] s_TestRoomMeshIndices = { 0, 1, 2 };
        internal static uint[] testRoomMeshIndices => s_TestRoomMeshIndices;

        // Steps to calculate this manually:
        // Find the vertices that form the boundary
        // Convert each vertex from local (room mesh) space to world space
        // Find the normal vector of the plane (use the first valid triangle)
        //      - remember to pay attention to the winding order / which way the plane is facing
        //      - don't get the normal for the backface of the plane by mistake
        // Convert to 2D coordinates on the plane: translate to (0,0,0), rotate to y=0, translate to centroid(2d)

        // Given the values in s_TestRoomMeshVertices and s_TestRoomMeshIndices, the calculations are as follows:
        // The boundary of the plane is vertices[0,1,2], a fairly simple case with just 1 triangle in total.
        // Convert from local to world: N/A, this LocateSpace mock delegate sets the room mesh position and rotation to 0
        // normalized normal vector: (-0.099,-0.009,-0.995)
        // 2D coordinates:
        // - create a translation matrix from the first point to (0,0,0) and apply the transformation to all points
        //          (1.1, 0.0, 1.2) -> (0.0, 0.0, 0.0)
        //          (0.0, 1.1, 1.3) -> (-1.1, 1.1, 0.1)
        //          (-0.9, -0.1, 1.4) -> (-2.0, -0.1, 0.2)
        // - create a rotation matrix from normal to up=(0,1,0) and apply the transformation to all points
        //          (0.0, 0.0, 0.0) -> (0.0, 0.0, 0.0)
        //          (-1.1, 1.1, 0.1) -> (-0.990, -0.0005, 1.204)
        //          (-2.0, -0.1, 0.2) -> (-2.01, -0.0001, 0.099)
        // - find the centroid of the rotated points: (-1.000, -0.0002, 0.434)
        // - create a translation matrix from (0,0,0) to the (centroid * -1) and apply the transformation to all points
        //          (0.0, 0.0, 0.0) -> (1.0, 0.0002, -0.434)
        //          (-0.990, -0.0005, 1.204) -> (0.01, -0.0003, 0.769)
        //          (-2.01, -0.0001, 0.099) -> (-1.01, 0.0001, -0.335)
        // Use only X and Z, as we are now in 2D coordinates. (Y is approximately zero anyway)
        static readonly XrVector2f[] s_TestPlanePointsRoomMesh =
        {
            new(1.0F, -0.434F), new(0.01F, 0.769F), new(-1.01F, -0.335F)
        };
        internal static XrVector2f[] TestPlanePointsRoomMesh => s_TestPlanePointsRoomMesh;

        static unsafe IntPtr s_SpaceSemanticLabelsFBPtr =
            Marshal.GetFunctionPointerForDelegate((XrGetSpaceSemanticLabelsFB_Delegate)SpaceSemanticLabelsFB_MockDelegate);
        internal static IntPtr SpaceSemanticLabelsFBPtr => s_SpaceSemanticLabelsFBPtr;

        static unsafe IntPtr s_SpaceRoomMeshMETAPtr =
            Marshal.GetFunctionPointerForDelegate((XrGetSpaceRoomMeshMETA_Delegate)SpaceRoomMeshMETA_MockDelegate);
        internal static IntPtr SpaceRoomMeshMETAPtr => s_SpaceRoomMeshMETAPtr;

        static unsafe IntPtr s_SpaceRoomMesh_NoPlanes_METAPtr =
            Marshal.GetFunctionPointerForDelegate((XrGetSpaceRoomMeshMETA_Delegate)SpaceRoomMesh_NoPlanes_META_MockDelegate);
        internal static IntPtr SpaceRoomMesh_NoPlanes_METAPtr => s_SpaceRoomMesh_NoPlanes_METAPtr;

        static unsafe IntPtr s_SpaceRoomMeshFaceIndicesMETAPtr =
            Marshal.GetFunctionPointerForDelegate((XrGetSpaceRoomMeshFaceIndicesMETA_Delegate)SpaceRoomMeshFaceIndicesMETA_MockDelegate);
        internal static IntPtr SpaceRoomMeshFaceIndicesMETAPtr => s_SpaceRoomMeshFaceIndicesMETAPtr;

        [MonoPInvokeCallback(typeof(XrGetSpaceSemanticLabelsFB_Delegate))]
        internal static unsafe XrResult SpaceSemanticLabelsFB_MockDelegate(
            XrSession session,
            XrSpace space,
            XrSemanticLabelsFB* semanticLabelsOutput)
        {
            semanticLabelsOutput->bufferCountOutput = (uint)k_SemanticLabel.Length;
            if (semanticLabelsOutput->bufferCapacityInput != 0)
            {
                for (var i = 0; i < k_SemanticLabel.Length; ++i)
                {
                    semanticLabelsOutput->buffer[i] = (byte)k_SemanticLabel[i];
                }
            }

            return XrResult.Success;
        }

        [MonoPInvokeCallback(typeof(XrGetSpaceRoomMeshMETA_Delegate))]
        internal static unsafe XrResult SpaceRoomMeshMETA_MockDelegate(
            XrSpace space,
            XrSpaceRoomMeshGetInfoMETA* getInfo,
            XrRoomMeshMETA* roomMeshOutput)
        {
            if (roomMeshOutput->vertices == null)
            {
                roomMeshOutput->vertexCountOutput = (uint)s_TestRoomMeshVertices.Length;
            }
            else
            {
                roomMeshOutput->vertexCountOutput = (uint)s_TestRoomMeshVertices.Length;
                for (var i = 0; i < s_TestRoomMeshVertices.Length; ++i)
                {
                    roomMeshOutput->vertices[i] = s_TestRoomMeshVertices[i];
                }
            }

            if (roomMeshOutput->faces == null)
            {
                roomMeshOutput->faceCountOutput = (uint)s_TestRoomMeshFaces.Length;
            }
            else
            {
                roomMeshOutput->faceCountOutput = (uint)s_TestRoomMeshFaces.Length;
                for (var i = 0; i < s_TestRoomMeshFaces.Length; ++i)
                {
                    roomMeshOutput->faces[i] = s_TestRoomMeshFaces[i];
                }
            }

            return XrResult.Success;
        }

        [MonoPInvokeCallback(typeof(XrGetSpaceRoomMeshMETA_Delegate))]
        internal static unsafe XrResult SpaceRoomMesh_NoPlanes_META_MockDelegate(
            XrSpace space,
            XrSpaceRoomMeshGetInfoMETA* getInfo,
            XrRoomMeshMETA* roomMeshOutput)
        {
            roomMeshOutput->vertexCountOutput = 0;
            roomMeshOutput->faceCountOutput = 0;
            return XrResult.Success;
        }

        [MonoPInvokeCallback(typeof(XrGetSpaceRoomMeshFaceIndicesMETA_Delegate))]
        internal static unsafe XrResult SpaceRoomMeshFaceIndicesMETA_MockDelegate(
            XrSpace space,
            XrUuid* faceUuid,
            XrRoomMeshFaceIndicesMETA* roomMeshFaceIndicesOutput)
        {
            roomMeshFaceIndicesOutput->indexCountOutput = (uint)s_TestRoomMeshIndices.Length;

            if (roomMeshFaceIndicesOutput->indices != null)
            {
                roomMeshFaceIndicesOutput->indices[0] = s_TestRoomMeshIndices[0];
                roomMeshFaceIndicesOutput->indices[1] = s_TestRoomMeshIndices[1];
                roomMeshFaceIndicesOutput->indices[2] = s_TestRoomMeshIndices[2];
            }

            return XrResult.Success;
        }
    }
}
#endif // OPENXR_PLUGIN_1_16_0_PRE_2_OR_NEWER
