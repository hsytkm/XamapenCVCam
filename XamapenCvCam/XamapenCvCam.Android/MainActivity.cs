using Android.App;
using Android.Content.PM;
using Android.OS;
using Prism;
using Prism.Ioc;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace XamapenCvCam.Droid
{
    [Activity(Label = "XamapenCvCam", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private const string LibMySharedObject = "MyOpenCvWrapper"; // "libMyOpenCvWrapper.so" でもOK

        [DllImport(LibMySharedObject)]
        private static extern int GetMyInt();

        [DllImport(LibMySharedObject)]
        private static extern double GetMatMeanY(int black_length, int white_length);

        [DllImport(LibMySharedObject, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private extern static bool GetString([MarshalAs(UnmanagedType.LPUTF8Str), Out] StringBuilder str, int length);

        private static string GetStringWrapper(Func<StringBuilder, int, bool> func, int maxLength = 256)
        {
            var buff = new StringBuilder(maxLength);
            if (func.Invoke(buff, buff.Capacity))
                throw new ExternalException($"String Buffer is short. ({buff.ToString()})");

            return buff.ToString();
        }


        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            // for Media Plugin
            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App(new AndroidInitializer()));


            int a0 = GetMyInt();
            var s1 = GetStringWrapper(GetString);

            // 黒画像と白画像の割合を指定して、平均輝度値を求める
            var y0 = GetMatMeanY(1, 1);     // 255 * 1/2 = 127.5
            var y1 = GetMatMeanY(2, 1);     // 255 * 1/3 =  85.0
            var y2 = GetMatMeanY(200, 300); // 255 * 3/5 = 153.0
        }

        // for Media Plugin
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }

    public class AndroidInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Register any platform specific implementations
        }
    }
}

