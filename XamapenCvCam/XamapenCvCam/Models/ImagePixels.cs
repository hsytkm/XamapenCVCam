using Android.Graphics;
using System;
using System.Diagnostics;
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
    internal readonly struct BitmapHeader
    {
        // Bitmap File Header
        public readonly Int16 FileType;
        public readonly Int32 FileSize;
        public readonly Int16 Reserved1;
        public readonly Int16 Reserved2;
        public readonly Int32 OffsetBytes;

        // Bitmap Information Header
        public readonly Int32 InfoSize;
        public readonly Int32 Width;
        public readonly Int32 Height;
        public readonly Int16 Planes;
        public readonly Int16 BitCount;
        public readonly Int32 Compression;
        public readonly Int32 SizeImage;
        public readonly Int32 XPixPerMete;
        public readonly Int32 YPixPerMete;
        public readonly Int32 ClrUsed;
        public readonly Int32 CirImportant;

        public BitmapHeader(int width, int height, int pixelsSize)
        {
            var fileHeaderSize = 14;
            var infoHeaderSize = 40; 
            var totalHeaderSize = fileHeaderSize + infoHeaderSize;
            var fileSize = totalHeaderSize + pixelsSize;

            FileType = 0x4d42;  // 'B','M'
            FileSize = fileSize;
            Reserved1 = 0;
            Reserved2 = 0;
            OffsetBytes = totalHeaderSize;

            InfoSize = infoHeaderSize;
            Width = width;
            Height = height;
            Planes = 1;
            BitCount = 32;
            Compression = 0;
            SizeImage = pixelsSize;
            XPixPerMete = 0;
            YPixPerMete = 0;
            ClrUsed = 0;
            CirImportant = 0;
        }
    }

    // Bitmap File Format http://www.umekkii.jp/data/computer/file_format/bitmap.cgi
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

    internal static class ImagePixelsExtensions
    {
        public static ImageSource ToImageSource(this in ImagePixels pixels)
        {
            var buffer = GetBitmapBuffer(pixels);

            // ◆DisposeしたらExceptionでちゃう。どこで解放したら良いの？？
            var ms = new MemoryStream(buffer);

            return ImageSource.FromStream(() => ms);
        }


        // https://www.sawalemontea.com/entry/2018/04/29/183000
        private static byte[] GetBitmapBuffer(in ImagePixels pixels)
        {
            var width = pixels.Width;
            var height = pixels.Height;
            var stride = pixels.Stride;
            var pixelsSize = pixels.PixelsSize;

            var header = new BitmapHeader(width, height, pixelsSize);
            var headerSize = Marshal.SizeOf(header);
            var filesize = headerSize + pixelsSize;
            var buffer = new byte[filesize];

            //bufferにheader情報を書き込む
            var ptr = Marshal.AllocHGlobal(headerSize);
            try
            {
                Marshal.StructureToPtr(header, ptr, false);
                Marshal.Copy(ptr, buffer, 0, headerSize);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            // 画像は左下から右上に向かって記録する
            var startPtr = pixels.PixelsPtr;
            var spanPixels = new Span<byte>(buffer).Slice(headerSize);
            unsafe
            {
                var srcHead = (byte*)startPtr.ToPointer();
                fixed (byte* destHead = spanPixels)
                {
                    for (int y = 0; y < height; ++y)
                    {
                        var src = srcHead + (height - y - 1) * stride;
                        var dst = destHead + (y * stride);
                        var last = dst + stride;

                        while (dst + 7 < last)
                        {
                            *(ulong*)dst = *(ulong*)src;
                            src += 8;
                            dst += 8;
                        }
                        if (dst + 3 < last)
                        {
                            *(uint*)dst = *(uint*)src;
                            src += 4;
                            dst += 4;
                        }
                        while (dst < last)
                        {
                            *dst = *src;
                            ++src;
                            ++dst;
                        }
                    }
                }
            }
            return buffer;
        }
    }

}
