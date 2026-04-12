using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    struct GpuImageHandles
    {
        internal UInt64 m_HardwareBuffer;
        internal int m_SyncFenceFd;
        internal IntPtr m_VkImage;          // VkImage handle imported from AHardwareBuffer
        internal UInt64 m_VkSemaphore;       // VkSemaphore handle imported from sync fence
        internal int m_Width;
        internal int m_Height;
        // VkFormat enum value representing the pixel format of the image see https://docs.vulkan.org/refpages/latest/refpages/source/VkFormat.html
        internal uint m_Format;
        /// Camera position index from the native layer: 0 = left eye, 1 = right eye.
        internal int m_CameraPosition;

        internal bool IsValid => m_VkImage != IntPtr.Zero;

        internal void Reset()
        {
            m_HardwareBuffer = 0;
            m_SyncFenceFd = 0;
            m_VkImage = IntPtr.Zero;
            m_VkSemaphore = 0;
            m_Width = 0;
            m_Height = 0;
            m_Format = 0;
            m_CameraPosition = 0;
        }
    }

    static class MetaOpenXRGpuImageApi
    {
#if UNITY_ANDROID
        [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_GpuImage_Initialize")]
        internal static extern bool Initialize();

        [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_GpuImage_Release")]
        internal static extern void Release();

        [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_GpuImage_BeginFrame")]
        internal static extern void BeginFrame();

        [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_GpuImage_TryAcquireLatest")]
        internal static extern bool TryAcquireLatestGpuHandles(out GpuImageHandles outHandles);

        [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_GpuImage_TryAcquireLatestForPosition")]
        internal static extern bool TryAcquireLatestGpuHandlesForPosition(int cameraPosition, out GpuImageHandles outHandles);

        [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_GpuImage_TryAcquireLatestStereo")]
        internal static extern bool TryAcquireLatestStereoGpuHandles(out GpuImageHandles outLeftHandles, out GpuImageHandles outRightHandles);

        [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_GpuImage_ReleaseHandles")]
        internal static extern void ReleaseGpuHandles(GpuImageHandles handles);
#else
        const string k_NotSupportedMessage = "Meta OpenXR GPU image API is only supported on Android platforms.";

        internal static bool Initialize() => throw new NotSupportedException(k_NotSupportedMessage);
        internal static void Release() => throw new NotSupportedException(k_NotSupportedMessage);
        internal static void BeginFrame() => throw new NotSupportedException(k_NotSupportedMessage);
        internal static bool TryAcquireLatestGpuHandles(out GpuImageHandles outHandles) => throw new NotSupportedException(k_NotSupportedMessage);
        internal static bool TryAcquireLatestGpuHandlesForPosition(int cameraPosition, out GpuImageHandles outHandles) => throw new NotSupportedException(k_NotSupportedMessage);
        internal static bool TryAcquireLatestStereoGpuHandles(out GpuImageHandles outLeftHandles, out GpuImageHandles outRightHandles) => throw new NotSupportedException(k_NotSupportedMessage);
        internal static void ReleaseGpuHandles(GpuImageHandles handles) => throw new NotSupportedException(k_NotSupportedMessage);
#endif
    }
}
