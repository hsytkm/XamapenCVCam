using Android.Graphics;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamapenCvCam.Models
{
    internal static class NativeMethods
    {
        private const string LibName = "MyOpenCvWrapper"; // "libMySharedObject.so" でもOK

        [DllImport(LibName, EntryPoint = "Lib_GetInt123")]
        public static extern int GetInt123();
        
        [DllImport(LibName, EntryPoint = "Lib_GetAverageG")]
        public static extern double GetAverageG(ref ImagePixels pixels);

        [DllImport(LibName, EntryPoint = "Lib_ToNegaPosi")]
        public static extern void ToNegaPosiLibTest(ref ImagePixels pixels);

        [DllImport(LibName, EntryPoint = "OpenCv_ToNegaPosi")]
        public static extern void ToNegaPosiOpenCv(ref ImagePixels pixels);

        [DllImport(LibName, EntryPoint = "OpenCv_DrawFaceFrames", CharSet = CharSet.Unicode)]
        public static extern int DrawFaceFrames(ref ImagePixels pixels,
            [MarshalAs(UnmanagedType.LPUTF8Str), In]string path);
    }

    class ImageController : IDisposable
    {
        private readonly ImagePixelsContainer _imageContainer;

        public int SourceWidth { get; }
        public int SourceHeight { get; }

        public static async Task<ImageController> CreateInstance(Stream stream)
        {
            var bitmap = await BitmapFactory.DecodeStreamAsync(stream);
            var controller = new ImageController(bitmap);

            bitmap.Recycle();
            bitmap.Dispose();
            return controller;
        }

        private ImageController(Bitmap bitmap)
        {
            _imageContainer = new ImagePixelsContainer(bitmap);

            SourceWidth = bitmap.Width;
            SourceHeight = bitmap.Height;

            // lib read test
            Debug.Assert(123 == NativeMethods.GetInt123());
        }

        public double GetAverageG()
        {
            var payload = _imageContainer.Payload;
            return NativeMethods.GetAverageG(ref payload);
        }

        public void ToNegaPosi()
        {
            var payload = _imageContainer.Payload;
            //NativeMethods.ToNegaPosiLibTest(ref payload);
            NativeMethods.ToNegaPosiOpenCv(ref payload);
        }

        #region FaceDetect
        private readonly static string FaceCascadeFilename =
            "haarcascade_frontalface_alt.xml";

        private readonly static string FaceCascadeResourcePath =
            $"XamapenCvCam.Resources.{FaceCascadeFilename}";

        private static string PrepareFaceCascadeFile()
        {
            var dir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string dstPath = System.IO.Path.Combine(dir, FaceCascadeFilename);

            //new FileInfo(dstPath).Delete(); // for test

            if (!File.Exists(dstPath))
            {
                var assembly = Assembly.GetExecutingAssembly();
                var stream = assembly.GetManifestResourceStream(FaceCascadeResourcePath);
                using var src = new StreamReader(stream);
                using var dst = new StreamWriter(dstPath);

                while (src.Peek() >= 0)
                    dst.WriteLine(src.ReadLine());
            }
            return dstPath;
        }

        public int DrawFaceFrames()
        {
            // 顔検出用ファイルの準備(リソースを端末内に保存)
            var cascadePath = PrepareFaceCascadeFile();
            if (string.IsNullOrEmpty(cascadePath)) return -1;

            var payload = _imageContainer.Payload;
            return NativeMethods.DrawFaceFrames(ref payload, cascadePath);
        }
        #endregion

        public ImageSource GetImageSource()
            => _imageContainer.Payload.ToImageSource();

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
