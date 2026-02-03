#if OPENXR_PLUGIN_1_16_0_PRE_2_OR_NEWER
using UnityEngine.XR.OpenXR.NativeTypes;
using XrSpace = System.UInt64;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    unsafe struct XrRoomMeshFaceMETA
    {
        internal XrStructureType type;
        internal void* next;
        internal XrUuid uuid;
        internal XrUuid parentUuid;
        internal XrSemanticLabelMETA semanticLabel;

        public XrRoomMeshFaceMETA(XrUuid uuid, XrUuid parentUuid, XrSemanticLabelMETA semanticLabel)
        {
            type = XrStructureType.XR_TYPE_ROOM_MESH_META;
            next = null;
            this.uuid = uuid;
            this.parentUuid = parentUuid;
            this.semanticLabel = semanticLabel;
        }
    }

    unsafe struct XrRoomMeshFaceIndicesMETA {
        internal XrStructureType type;
        internal void* next;
        internal uint indexCapacityInput;
        internal uint indexCountOutput;
        internal uint* indices;
    }

    unsafe struct XrSpaceRoomMeshGetInfoMETA {
        internal XrStructureType type;
        internal void* next;
        internal uint recognizedSemanticLabelCount;
        internal XrSemanticLabelMETA* recognizedSemanticLabels;
    }

    unsafe struct XrRoomMeshMETA {
        internal XrStructureType type;
        internal void* next;
        internal uint vertexCapacityInput;
        internal uint vertexCountOutput;
        internal XrVector3f* vertices;
        internal uint faceCapacityInput;
        internal uint faceCountOutput;
        internal XrRoomMeshFaceMETA* faces;
    }

    unsafe delegate XrResult XrGetSpaceRoomMeshMETA_Delegate(
        XrSpace space,
        XrSpaceRoomMeshGetInfoMETA* getInfo,
        XrRoomMeshMETA* roomMeshOutput);

    unsafe delegate XrResult XrGetSpaceRoomMeshFaceIndicesMETA_Delegate(
        XrSpace space,
        XrUuid* faceUuid,
        XrRoomMeshFaceIndicesMETA* roomFaceIndicesOutput);
}
#endif // OPENXR_PLUGIN_1_16_0_PRE_2_OR_NEWER
