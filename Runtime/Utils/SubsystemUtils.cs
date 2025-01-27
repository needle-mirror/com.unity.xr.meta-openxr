using UnityEngine.SubsystemsImplementation;
using UnityEngine.XR.Management;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// A utility class for checking loaded subsystems.
    /// </summary>
    static class SubsystemUtils
    {
        /// <summary>
        /// Returns <see langword="true"/> if there is a loaded <see cref="SubsystemWithProvider"/> of
        /// type <typeparamref name="TSubsystem"/>, and writes it to the <see langword="out"/> parameter.
        /// </summary>
        /// <typeparam name="TSubsystemBase">The base subsystem type, ie `XRSessionSubsystem`.</typeparam>
        /// <typeparam name="TSubsystem">The derived subsystem type to match.</typeparam>
        /// <param name="subsystem">The loaded subsystem, if this method returns <see langword="true"/>.</param>
        /// <returns><see langword="true"/> if there exists a loaded <typeparamref name="TSubsystemBase"/> of type
        /// <typeparamref name="TSubsystem"/>. Otherwise, <see langword="false"/>.</returns>
        public static bool TryGetLoadedSubsystem<TSubsystemBase, TSubsystem>(out TSubsystem subsystem)
            where TSubsystemBase : SubsystemWithProvider, new()
            where TSubsystem : TSubsystemBase
        {
            TryGetLoadedSubsystem<TSubsystemBase>(out var baseSubsystem);
            subsystem = baseSubsystem as TSubsystem;
            return subsystem != null;
        }

        /// <summary>
        /// Returns <see langword="true"/> if there is a loaded <see cref="SubsystemWithProvider"/> of
        /// type <typeparamref name="TSubsystemBase"/>. Otherwise, <see langword="false"/>.
        /// </summary>
        /// <typeparam name="TSubsystemBase">The base subsystem type, ie `XRSessionSubsystem`.</typeparam>
        /// <param name="subsystem">The loaded subsystem, if this method returns <see langword="true"/>.</param>
        /// <returns><see langword="true"/> if there exists a loaded <typeparamref name="TSubsystemBase"/>.
        /// Otherwise, <see langword="false"/>.</returns>
        public static bool TryGetLoadedSubsystem<TSubsystemBase>(out TSubsystemBase subsystem)
            where TSubsystemBase : SubsystemWithProvider, new()
        {
            if (XRGeneralSettings.Instance == null || XRGeneralSettings.Instance.Manager == null)
            {
                subsystem = null;
                return false;
            }

            var loader = XRGeneralSettings.Instance.Manager.activeLoader;

            // Query the currently active loader for the created subsystem, if one exists.
            subsystem = loader != null ? loader.GetLoadedSubsystem<TSubsystemBase>() : null;

            return subsystem != null;
        }

        /// <summary>
        /// Returns <see langword="true"/> if there is a loaded integrated subsystem of
        /// type <typeparamref name="TIntegratedSubsystem"/>. Otherwise, <see langword="false"/>.
        /// </summary>
        /// <typeparam name="TIntegratedSubsystem">The integrated subsystem type.</typeparam>
        /// <param name="subsystem">The loaded subsystem, if this method returns <see langword="true"/>.</param>
        /// <returns><see langword="true"/> if there exists a loaded <typeparamref name="TIntegratedSubsystem"/>.
        /// Otherwise, <see langword="false"/>.</returns>
        internal static bool TryGetLoadedIntegratedSubsystem<TIntegratedSubsystem>(out TIntegratedSubsystem subsystem)
            where TIntegratedSubsystem : IntegratedSubsystem, new()
        {
            if (XRGeneralSettings.Instance == null || XRGeneralSettings.Instance.Manager == null)
            {
                subsystem = null;
                return false;
            }

            var loader = XRGeneralSettings.Instance.Manager.activeLoader;

            // Query the currently active loader for the created subsystem, if one exists.
            subsystem = loader != null ? loader.GetLoadedSubsystem<TIntegratedSubsystem>() : null;

            return subsystem != null;
        }
    }
}
