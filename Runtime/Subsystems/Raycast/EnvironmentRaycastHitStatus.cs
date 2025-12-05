using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// An enum representing the status of an environmental raycast as matching the Meta OpenXR Environment Raycast plugin enum.
    /// </summary>
    public enum EnvironmentRaycastHitStatus
    {
        /// <summary>
        /// The intersection with the environment is found.
        /// </summary>
        Hit = 1,

        /// <summary>
        /// The intersection with the environment is not found.
        /// </summary>
        NoHit = 2,

        /// <summary>
        /// The ray intersects with the environment, but the actual hit point is invisible.
        /// You can use this information to inform the user that they may need to move to see the hit point.
        /// </summary>
        HitPointOccluded = 3,

        /// <summary>
        /// The hit point can't be determined because it lies outside of the field of view. No hit is found.
        /// You can use this information to request that the user reposition the ray and try casting again.
        /// </summary>
        HitPointOutsideOfFov = 4,

        /// <summary>
        /// The ray is completely occluded by the environment. No hit is found.
        /// You can use this information to request that the user reposition the ray and try casting again.
        /// </summary>
        RayOccluded = 5,

        /// <summary>
        /// The intersection with the environment is found, but the hit orientation cannot be determined.
        /// In this case, Unity will populate related rotation data with the value `Quaternion.identity`.
        /// </summary>
        HitInvalidOrientation = 6,
    }
}
