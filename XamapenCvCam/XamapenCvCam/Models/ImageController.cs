using Android.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamapenCvCam.Models
{
    internal static class NativeMethods
    {
        private const string LibName = "MyOpenCvWrapper"; // "libMySharedObject.so" でもOK

        [DllImport(LibName, EntryPoint = "OpenCv_GetAverageG")]
        public static extern double GetAverageG(ref ImagePixels pixels);

        [DllImport(LibName, EntryPoint = "OpenCv_ToNegaPosi")]
        public static extern void ToNegaPosi(ref ImagePixels pixels);
    }

    class ImageController : IDisposable
    {
        private readonly ImagePixelsContainer _imageContainer;

        private ImageController(Bitmap bitmap)
        {
            _imageContainer = new ImagePixelsContainer(bitmap);
        }

        public static async Task<ImageController> CreateInstance(Stream stream)
        {
            var bitmap = await BitmapFactory.DecodeStreamAsync(stream);
            var controller = new ImageController(bitmap);

            bitmap.Recycle();
            bitmap.Dispose();

            return controller;
        }

        public double GetAverageG()
        {
            var payload = _imageContainer.Payload;
            return NativeMethods.GetAverageG(ref payload);
        }

        public void ToNegaPosi()
        {
            var payload = _imageContainer.Payload;
            NativeMethods.ToNegaPosi(ref payload);
        }

        public ImageSource GetImageSource()
        {
            return _imageContainer.ToImageSource();
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                    _imageContainer.Dispose();
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~ImageController()
        // {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
