using Android.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace XamapenCvCam.Models
{
    internal class NativeMethods
    {
        private const string LibName = "MyOpenCvWrapper"; // "libMySharedObject.so" でもOK

        [DllImport(LibName, EntryPoint = "GetAverageG")]
        public static extern double GetAverageG(ref ImagePixels pixels);

    }

    class ImageController
    {
        private readonly ImagePixelsContainer _imageContainer;

        public ImageController(Stream stream)
        {
            var bitmap = BitmapFactory.DecodeStream(stream);
            _imageContainer = new ImagePixelsContainer(bitmap);
        }

        public double GetAverageG()
        {
            var aa = new NativeOpenCvWrapper();

            var payload = _imageContainer.Payload;
            return NativeMethods.GetAverageG(ref payload);
        }

    }
}
