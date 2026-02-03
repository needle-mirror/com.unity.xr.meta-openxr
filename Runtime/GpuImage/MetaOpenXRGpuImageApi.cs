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
        }
    }

    static class MetaOpenXRGpuImageApi
    {
        [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_GpuImage_Initialize")]
        internal static extern bool Initialize();

        [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_GpuImage_Release")]
        internal static extern void Release();

        [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_GpuImage_TryAcquireLatest")]
        internal static extern bool TryAcquireLatestGpuHandles(out GpuImageHandles outHandles);

        [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_GpuImage_ReleaseHandles")]
        internal static extern void ReleaseGpuHandles(GpuImageHandles handles);
    }
}
