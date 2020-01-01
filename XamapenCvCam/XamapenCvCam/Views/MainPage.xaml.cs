using System;
using System.Runtime.InteropServices;
using System.Text;
using Xamarin.Forms;

namespace XamapenCvCam.Views
{
    public partial class MainPage : ContentPage
    {
        private const string LibMySharedObject = "MyOpenCvWrapper"; // "libMySharedObject.so" でもOK

        [DllImport(LibMySharedObject)]  
        private static extern int GetMyInt();

        [DllImport(LibMySharedObject)]   // "libMySharedObject.so" でもOK
        private static extern double GetImageAverage();

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


        public MainPage()
        {
            InitializeComponent();

            int a0 = GetMyInt();
            var s1 = GetStringWrapper(GetString);
            var a1 = GetImageAverage();

            DisplayAlert("Title", s1, "OK");

        }
    }
}