using System;
using UnityEngine.XR.ARSubsystems;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// Represents a potential raycast intersection with the environment,
    /// including a provider-specific <see cref="hitStatus"/> which indicates a hit or miss.
    /// </summary>
    public readonly struct EnvironmentRaycastHit : IEquatable<EnvironmentRaycastHit>
    {
        /// <summary>
        /// A default-initialized environment raycast hit,
        /// using `Pose.identity` instead of zero-initialization.
        /// </summary>
        public static EnvironmentRaycastHit defaultValue = new EnvironmentRaycastHit(
            0,
            new XRRaycastHit(TrackableId.invalidId, Pose.identity, 0, TrackableType.None)
        );

        /// <summary>
        /// The Environment Raycast Hit Status that gives additional info about the hit.
        /// Should only be read if <see cref="IsHit()"/> returns true.
        /// </summary>
        public EnvironmentRaycastHitStatus hitStatus { get; }

        /// <summary>
        /// The XR Raycast Hit against the environment.
        /// Should only be read if <see cref="IsHit()"/> returns true.
        /// </summary>
        public XRRaycastHit hit { get; }

        /// <summary>
        /// Constructs an <see cref="EnvironmentRaycastHit"/>.
        /// </summary>
        /// <param name="hitStatus">The environmental raycast hit status of the environment intersection.</param>
        /// <param name="hit">The XRRaycastHit of the intersection with the environment.</param>
        public EnvironmentRaycastHit(EnvironmentRaycastHitStatus hitStatus, XRRaycastHit hit)
        {
            this.hitStatus = hitStatus;
            this.hit = hit;
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`true` if <paramref name="obj"/> is of type <see cref="EnvironmentRaycastHit"/> and
        /// <see cref="Equals(EnvironmentRaycastHit)"/> also returns `true`. Otherwise, `false`.</returns>
        public override bool Equals(object obj) => (obj is EnvironmentRaycastHit other) && Equals(other);

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="EnvironmentRaycastHit"/> to compare against.</param>
        /// <returns>`true` if every field in <paramref name="other"/> is equal to this <see cref="EnvironmentRaycastHit"/>. Otherwise, false.</returns>
        public bool Equals(EnvironmentRaycastHit other)
        {
            return
                (hit.Equals(other.hit)) &&
                (hitStatus.Equals(other.hitStatus));
        }

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(EnvironmentRaycastHit)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`true` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>. Otherwise, `false`.</returns>
        public static bool operator ==(EnvironmentRaycastHit lhs, EnvironmentRaycastHit rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(EnvironmentRaycastHit)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`true` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>. Otherwise, `false`.</returns>
        public static bool operator !=(EnvironmentRaycastHit lhs, EnvironmentRaycastHit rhs) => !lhs.Equals(rhs);

        /// <summary>
        /// Is this hit considered a valid hit, also read as "did the raycast hit the environment?"
        /// </summary>
        /// <returns>`true` if this is considered a hit, and the pose can be read. Otherwise, `false`.</returns>
        public bool IsHit()
        {
            switch (hitStatus)
            {
                case EnvironmentRaycastHitStatus.Hit:
                case EnvironmentRaycastHitStatus.HitPointOccluded:
                case EnvironmentRaycastHitStatus.HitInvalidOrientation:
                    return true;
                case EnvironmentRaycastHitStatus.NoHit:
                case EnvironmentRaycastHitStatus.HitPointOutsideOfFov:
                case EnvironmentRaycastHitStatus.RayOccluded:
                    return false;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(hit, hitStatus);
        }
    }
}
