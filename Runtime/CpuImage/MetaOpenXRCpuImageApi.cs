using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    class MetaOpenXRCpuImageApi : XRCpuImage.Api
    {
        static readonly HashSet<TextureFormat> s_SupportedVideoConversionFormats = new HashSet<TextureFormat>
        {
            TextureFormat.Alpha8,
            TextureFormat.R8,
            TextureFormat.R16,
            TextureFormat.RFloat,
            TextureFormat.RGB24,
            TextureFormat.RGBA32,
            TextureFormat.ARGB32,
            TextureFormat.BGRA32,
        };

        internal enum ImageType
        {
            Camera = 0,
            EnvironmentDepth = 1,
            RawEnvironmentDepth = 2,
            RawEnvironmentDepthConfidence = 3,
        }

        internal static MetaOpenXRCpuImageApi instance { get; } = new MetaOpenXRCpuImageApi();

        internal static bool TryAcquireLatestImage(ImageType imageType, out XRCpuImage.Cinfo cinfo)
            => NativeApi.UnityMetaOpenXR_CpuImage_TryAcquireLatestImage(imageType, out cinfo);

        internal static bool TryAcquireLatestImageForPosition(int cameraPosition, out XRCpuImage.Cinfo cinfo)
            => NativeApi.UnityMetaOpenXR_CpuImage_TryAcquireLatestImageForPosition(cameraPosition, out cinfo);

        internal static bool TryAcquireLatestStereoCpuImagePair(out XRCpuImage.Cinfo leftCinfo, out XRCpuImage.Cinfo rightCinfo)
            => NativeApi.UnityMetaOpenXR_CpuImage_TryAcquireLatestStereoCpuImagePair(out leftCinfo, out rightCinfo);

        // Max stereo sync attempts (1–10, default 3).
        internal static void SetMaxStereoSyncAttempts(int maxAttempts)
            => NativeApi.UnityMetaOpenXR_CpuImage_SetMaxStereoSyncAttempts(maxAttempts);

        public override XRCpuImage.AsyncConversionStatus GetAsyncRequestStatus(int requestId)
            => NativeApi.UnityMetaOpenXR_CpuImage_GetAsyncRequestStatus(requestId);

        public override void DisposeImage(int nativeHandle)
            => NativeApi.UnityMetaOpenXR_CpuImage_DisposeImage(nativeHandle);

        public override void DisposeAsyncRequest(int requestId)
            => NativeApi.UnityMetaOpenXR_CpuImage_DisposeAsyncRequest(requestId);

        public override bool TryGetPlane(int nativeHandle, int planeIndex, out XRCpuImage.Plane.Cinfo planeCinfo)
            => NativeApi.UnityMetaOpenXR_CpuImage_TryGetPlane(nativeHandle, planeIndex, out planeCinfo);

        public override bool NativeHandleValid(int nativeHandle)
            => NativeApi.UnityMetaOpenXR_CpuImage_HandleValid(nativeHandle);

        public override bool TryGetConvertedDataSize(
            int nativeHandle, Vector2Int dimensions, TextureFormat format, out int size)
            => NativeApi.UnityMetaOpenXR_CpuImage_TryGetConvertedDataSize(nativeHandle, dimensions, format, out size);

        public override bool TryConvert(
            int nativeHandle, XRCpuImage.ConversionParams conversionParams, IntPtr destinationBuffer, int bufferLength)
            => NativeApi.UnityMetaOpenXR_CpuImage_TryConvert(
                nativeHandle, conversionParams, destinationBuffer, bufferLength);

        public override int ConvertAsync(int nativeHandle, XRCpuImage.ConversionParams conversionParams)
            => NativeApi.UnityMetaOpenXR_CpuImage_CreateAsyncConversionRequest(nativeHandle, conversionParams);

        public override bool TryGetAsyncRequestData(int requestId, out IntPtr dataPtr, out int dataLength)
            => NativeApi.UnityMetaOpenXR_CpuImage_TryGetAsyncRequestData(requestId, out dataPtr, out dataLength);

        // callback's dataPtr is only valid during the callback; data may be destroyed after the callback returns.
        public override void ConvertAsync(
            int nativeHandle, XRCpuImage.ConversionParams conversionParams,
            XRCpuImage.Api.OnImageRequestCompleteDelegate callback, IntPtr context)
            => NativeApi.UnityMetaOpenXR_CpuImage_CreateAsyncConversionRequestWithCallback(
                nativeHandle, conversionParams, callback, context);

        public override bool FormatSupported(XRCpuImage image, TextureFormat format)
            => (((image.format == XRCpuImage.Format.AndroidYuv420_888) ||
                 (image.format == XRCpuImage.Format.DepthUint16) ||
                 (image.format == XRCpuImage.Format.OneComponent8))
                && s_SupportedVideoConversionFormats.Contains(format));

        static class NativeApi
        {
#if UNITY_ANDROID
            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern bool UnityMetaOpenXR_CpuImage_TryAcquireLatestImage(
                ImageType imageType, out XRCpuImage.Cinfo cameraImageCinfo);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern bool UnityMetaOpenXR_CpuImage_TryAcquireLatestImageForPosition(
                int cameraPosition, out XRCpuImage.Cinfo cameraImageCinfo);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern bool UnityMetaOpenXR_CpuImage_TryAcquireLatestStereoCpuImagePair(
                out XRCpuImage.Cinfo leftCinfo, out XRCpuImage.Cinfo rightCinfo);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern void UnityMetaOpenXR_CpuImage_SetMaxStereoSyncAttempts(int maxAttempts);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern XRCpuImage.AsyncConversionStatus
                UnityMetaOpenXR_CpuImage_GetAsyncRequestStatus(int requestId);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern void UnityMetaOpenXR_CpuImage_DisposeImage(int nativeHandle);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern void UnityMetaOpenXR_CpuImage_DisposeAsyncRequest(int requestHandle);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern bool UnityMetaOpenXR_CpuImage_TryGetPlane(
                int nativeHandle, int planeIndex, out XRCpuImage.Plane.Cinfo planeCinfo);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern bool UnityMetaOpenXR_CpuImage_HandleValid(int nativeHandle);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern bool UnityMetaOpenXR_CpuImage_TryGetConvertedDataSize(
                int nativeHandle, Vector2Int dimensions, TextureFormat format, out int size);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern bool UnityMetaOpenXR_CpuImage_TryConvert(
                int nativeHandle, XRCpuImage.ConversionParams conversionParams, IntPtr buffer, int bufferLength);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern int UnityMetaOpenXR_CpuImage_CreateAsyncConversionRequest(
                int nativeHandle, XRCpuImage.ConversionParams conversionParams);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern bool UnityMetaOpenXR_CpuImage_TryGetAsyncRequestData(
                int requestHandle, out IntPtr dataPtr, out int dataLength);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern void UnityMetaOpenXR_CpuImage_CreateAsyncConversionRequestWithCallback(
                int nativeHandle, XRCpuImage.ConversionParams conversionParams,
                OnImageRequestCompleteDelegate callback, IntPtr context);
#else
            const string k_NotSupportedMessage = "Meta OpenXR GPU image API is only supported on Android platforms";

            public static bool UnityMetaOpenXR_CpuImage_TryAcquireLatestImage(
                ImageType imageType, out XRCpuImage.Cinfo cameraImageCinfo)
            => throw new NotSupportedException(k_NotSupportedMessage);

            public static bool UnityMetaOpenXR_CpuImage_TryAcquireLatestImageForPosition(
                int cameraPosition, out XRCpuImage.Cinfo cameraImageCinfo)
            => throw new NotSupportedException(k_NotSupportedMessage);

            public static bool UnityMetaOpenXR_CpuImage_TryAcquireLatestStereoCpuImagePair(
                out XRCpuImage.Cinfo leftCinfo, out XRCpuImage.Cinfo rightCinfo)
            => throw new NotSupportedException(k_NotSupportedMessage);

            public static void UnityMetaOpenXR_CpuImage_SetMaxStereoSyncAttempts(int maxAttempts)
            => throw new NotSupportedException(k_NotSupportedMessage);

            public static XRCpuImage.AsyncConversionStatus UnityMetaOpenXR_CpuImage_GetAsyncRequestStatus(int requestId)
            => throw new NotSupportedException(k_NotSupportedMessage);

            public static void UnityMetaOpenXR_CpuImage_DisposeImage(int nativeHandle)
            => throw new NotSupportedException(k_NotSupportedMessage);

            public static void UnityMetaOpenXR_CpuImage_DisposeAsyncRequest(int requestHandle)
            => throw new NotSupportedException(k_NotSupportedMessage);

            public static bool UnityMetaOpenXR_CpuImage_TryGetPlane(
                int nativeHandle, int planeIndex, out XRCpuImage.Plane.Cinfo planeCinfo)
            => throw new NotSupportedException(k_NotSupportedMessage);

            public static bool UnityMetaOpenXR_CpuImage_HandleValid(int nativeHandle)
            => throw new NotSupportedException(k_NotSupportedMessage);

            public static bool UnityMetaOpenXR_CpuImage_TryGetConvertedDataSize(
                int nativeHandle, Vector2Int dimensions, TextureFormat format, out int size)
            => throw new NotSupportedException(k_NotSupportedMessage);

            public static bool UnityMetaOpenXR_CpuImage_TryConvert(
                int nativeHandle, XRCpuImage.ConversionParams conversionParams, IntPtr buffer, int bufferLength)
            => throw new NotSupportedException(k_NotSupportedMessage);

            public static int UnityMetaOpenXR_CpuImage_CreateAsyncConversionRequest(
                int nativeHandle, XRCpuImage.ConversionParams conversionParams)
            => throw new NotSupportedException(k_NotSupportedMessage);

            public static bool UnityMetaOpenXR_CpuImage_TryGetAsyncRequestData(
                int requestHandle, out IntPtr dataPtr, out int dataLength)
            => throw new NotSupportedException(k_NotSupportedMessage);

            public static void UnityMetaOpenXR_CpuImage_CreateAsyncConversionRequestWithCallback(
                int nativeHandle, XRCpuImage.ConversionParams conversionParams,
                OnImageRequestCompleteDelegate callback, IntPtr context)
            => throw new NotSupportedException(k_NotSupportedMessage);
#endif
        }
    }
}
