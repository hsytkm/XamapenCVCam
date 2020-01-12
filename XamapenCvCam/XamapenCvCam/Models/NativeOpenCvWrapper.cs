using System;
using System.Runtime.InteropServices;
using System.Text;

namespace XamapenCvCam.Models
{
    class NativeOpenCvWrapper
    {
        private const string LibMySharedObject = "MyOpenCvWrapper"; // "libMySharedObject.so" でもOK

        [DllImport(LibMySharedObject)]
        private static extern int GetMyInt();

        [DllImport(LibMySharedObject, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private extern static bool GetString(
            [MarshalAs(UnmanagedType.LPUTF8Str), Out] StringBuilder str, int length);

        private static string GetStringWrapper(Func<StringBuilder, int, bool> func, int maxLength = 256)
        {
            var buff = new StringBuilder(maxLength);
            if (func.Invoke(buff, buff.Capacity))
                throw new ExternalException($"String Buffer is short. ({buff.ToString()})");

            return buff.ToString();
        }

        public NativeOpenCvWrapper()
        {
            int a0 = GetMyInt();
            var s1 = GetStringWrapper(GetString);

        }

    }
}
