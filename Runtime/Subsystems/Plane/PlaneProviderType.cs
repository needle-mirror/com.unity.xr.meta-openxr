namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// Represents the strategy for identifying planes using Meta's OpenXR extensions.
    /// </summary>
    public enum PlaneProviderType
    {
        /// <summary>
        /// Use `XR_FB_scene` to identify planes.
        /// </summary>
        XrFbScene = 1,

        /// <summary>
        /// Use `XR_META_spatial_entity_room_mesh` to identify planes.
        /// </summary>
        XrMetaSpatialEntityRoomMesh = 2,
    }
}
