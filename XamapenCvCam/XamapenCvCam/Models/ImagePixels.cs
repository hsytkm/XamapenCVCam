using Android.Graphics;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Xamarin.Forms;

namespace XamapenCvCam.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal readonly struct ImagePixels
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

        // https://www.sawalemontea.com/entry/2018/04/29/183000
        private byte[] MakeBuffer()
        {
            var pixels = Payload;
            var width = pixels.Width;
            var height = pixels.Height;
            var bytesPerPixel = pixels.BytesPerPixel;
            var stride = pixels.Stride;

            var headerSize = 54;
            var numPixelBytes = (bytesPerPixel * width) * height;
            var filesize = headerSize + numPixelBytes;
            var buffer = new byte[filesize];

            //bufferにheader情報を書き込む
            using var ms = new MemoryStream(buffer);
            using var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8);

            writer.Write(new char[] { 'B', 'M' });
            writer.Write(filesize);
            writer.Write((short)0);
            writer.Write((short)0);
            writer.Write(headerSize);

            writer.Write(40);
            writer.Write(width);
            writer.Write(height);
            writer.Write((short)1);
            writer.Write((short)32);
            writer.Write(0);
            writer.Write(numPixelBytes);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);

            // 画像は左下から右上に向かって記録する
            var startPtr = pixels.PixelsPtr;
            for (int y = height * stride - 1; y >= 0; y -= stride)
            {
                for (int n = y; n < y + (width * bytesPerPixel); n +=bytesPerPixel)
                {
                    writer.Write(Marshal.ReadInt32(startPtr, n));
                }
            }
            return buffer;
        }

        public ImageSource ToImageSource()
        {
            var buffer = MakeBuffer();
            var ms = new MemoryStream(buffer);  // ◆DisposeしたらExceptionでちゃう
            return ImageSource.FromStream(() => ms);
        }

        public void Dispose()
        {
            if (UnmanagedPtr != IntPtr.Zero)
                Marshal.FreeHGlobal(UnmanagedPtr);
        }
    }
}
