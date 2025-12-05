using UnityEngine.XR.OpenXR.NativeTypes;
using XrSpace = System.UInt64;
using XrSession = System.UInt64;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    unsafe struct XrBoundary2DFB
    {
        internal XrStructureType type;
        internal void* next;
        internal uint vertexCapacityInput;
        internal uint vertexCountOutput;
        internal XrVector2f* vertices;
    }

    unsafe struct XrSemanticLabelsFB
    {
        internal XrStructureType type;
        internal void* next;
        internal uint bufferCapacityInput;
        internal uint bufferCountOutput;
        internal byte* buffer;
    }

    unsafe delegate XrResult XrGetSpaceBoundary2DFB_Delegate(
        XrSession session,
        XrSpace space,
        XrBoundary2DFB* boundary2DOutput);

    unsafe delegate XrResult XrGetSpaceBoundingBox2DFB_Delegate(
        XrSession session,
        XrSpace space,
        XrRect2Df* boundingBox2DOutput);

    unsafe delegate XrResult XrGetSpaceSemanticLabelsFB_Delegate(
        XrSession session,
        XrSpace space,
        XrSemanticLabelsFB* semanticLabelsOutput);
}
