using System;
using System.Threading;

#if __ANDROID__ || __IOS__ || __WASM__
// Mobile and Web platforms
#else
// Desktop platforms (Windows, macOS, Linux)
using Microsoft.UI.Xaml;
#endif

namespace ProductManageUNO
{
#if __ANDROID__
    public static class Program
    {
        static void Main(string[] args)
        {
            // Android uses NativeApplication defined in Platforms/Android/Main.Android.cs
            // This Main method is just to satisfy the entry point requirement
        }
    }
#elif __IOS__ || __WASM__
    // iOS and WASM have their own Program.cs files in Platforms folders
#else
    // Desktop entry point (Windows, macOS, Linux)
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Microsoft.UI.Xaml.Application.Start((p) => new App());
        }
    }
#endif
}
