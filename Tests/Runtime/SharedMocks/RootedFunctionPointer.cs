using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    /// <summary>
    /// GetFunctionPointerForDelegate doesn't keep the delegate alive. Under CoreCLR the GC can collect it while the
    /// native mock runtime still holds the pointer, crashing the Editor, so every mock delegate is held here for the
    /// domain's lifetime.
    /// </summary>
    static class RootedFunctionPointer
    {
#if UNITY_6000_5_OR_NEWER
        [NoAutoStaticsCleanup]
#endif
        static readonly List<Delegate> k_RootedDelegates = new();

        internal static IntPtr For(Delegate function)
        {
            lock (k_RootedDelegates)
            {
                k_RootedDelegates.Add(function);
            }

            return Marshal.GetFunctionPointerForDelegate(function);
        }
    }
}
