using Android.Graphics;
using System;
using System.Runtime.InteropServices;

namespace XamapenCvCam.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct ImagePixels
    {
        public readonly IntPtr PixelsPtr;
        public readonly int PixelsSize;
        public readonly int Width;
        public readonly int Height;
        public readonly int BytesPerPixel;
        public readonly int Stride;

        public ImagePixels(int width, int height, int bytesPerPixel, int stride, IntPtr ptr)
        {
            Width = width;
            Height = height;
            BytesPerPixel = bytesPerPixel;
            Stride = stride;
            PixelsSize = height * stride;
            PixelsPtr = ptr;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal readonly struct ImagePixelsContainer : IDisposable
    {
        private readonly IntPtr UnmanagedPtr;
        public readonly ImagePixels Payload;

        public ImagePixelsContainer(Bitmap bitmap)
        {
            var bytesPerPixel = 4;
            var width = bitmap.Width;
            var height = bitmap.Height;
            var stride = bitmap.Width * bytesPerPixel;
            var size = stride * height;

            // camera image
            var pixels = new int[size / sizeof(int)];
            bitmap.GetPixels(pixels, 0, stride / sizeof(int), 0, 0, width, height);

            // copy to unmanaged memory
            UnmanagedPtr = Marshal.AllocHGlobal(size);
            Marshal.Copy(pixels, 0, UnmanagedPtr, pixels.Length);

            Payload = new ImagePixels(width, height, bytesPerPixel, stride, UnmanagedPtr);
        }

        public void Dispose()
        {
            if (UnmanagedPtr != IntPtr.Zero)
                Marshal.FreeHGlobal(UnmanagedPtr);
        }
    }
}
