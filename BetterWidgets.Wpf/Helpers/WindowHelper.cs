using System.Windows;
using BetterWidgets.Enums;
using BetterWidgets.Consts;
using System.Windows.Interop;
using BetterWidgets.Structures;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using BetterWidgets.Extensions;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace BetterWidgets.Helpers
{
    public static class WindowHelper
    {
        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;
        private const int WS_CAPTION = 0xC00000;
        private const int SM_CYCAPTION = 4;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_APPWINDOW = 0x00040000;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref WidgetCornerMode pref, int attrSize);

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern void DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowLongPtr(IntPtr hwnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hwnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern IntPtr ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        public static void SetRoundedCorners(Window window, WidgetCornerMode preference)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            if(hwnd == IntPtr.Zero) return;

            DwmSetWindowAttribute(hwnd, Values.DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
        }

        public static void SetBlurBehind(Window window, AccentState state)
        {
            var windowHelper = new WindowInteropHelper(window);
            var accent = new AccentPolicy { AccentState = (AccentState)state };
            var accentStructSize = Marshal.SizeOf(accent);
            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);
            Marshal.FreeHGlobal(accentPtr);
        }

        public static void ExtendViewIntoTitleBar(Window window, bool extend)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;

            if(hwnd == IntPtr.Zero) return;

            int style = GetWindowLongPtr(hwnd, GWL_STYLE).ToInt32();

            if(extend)
            {
                MARGINS margins = new MARGINS { Top = -GetSystemMetrics(SM_CYCAPTION) };  // Сдвиг, чтобы контент был в зоне заголовка
                DwmExtendFrameIntoClientArea(hwnd, ref margins);

                SetWindowLongPtr(hwnd, GWL_STYLE, (IntPtr)(style & ~WS_CAPTION));
            }
            else
            {
                MARGINS margins = new MARGINS { Top = 0 };
                DwmExtendFrameIntoClientArea(hwnd, ref margins);

                SetWindowLongPtr(hwnd, GWL_STYLE, (IntPtr)(style | WS_CAPTION));
            }
        }

        public static void SetAltTabVisibility(Window window, bool isVisible)
        {
            var handle = new WindowInteropHelper(window).Handle;

            IntPtr exStyle = GetWindowLongPtr(handle, GWL_EXSTYLE);

            if(isVisible)
            {
                exStyle |= WS_EX_APPWINDOW;
                exStyle &= ~WS_EX_TOOLWINDOW;
            }
            else
            {
                exStyle &= ~WS_EX_APPWINDOW;
                exStyle |= WS_EX_TOOLWINDOW;
            }

            SetWindowLongPtr(handle, GWL_EXSTYLE, exStyle);
        }

        public static BitmapImage CaptureWindowAsBitmapImage(Window window)
        {
            var hwnd = window.GetHwnd();
            GetWindowRect(hwnd, out RECT rect);

            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            using(var g = Graphics.FromImage(bitmap))
                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, new System.Drawing.Size(width, height));

            using var memory = new MemoryStream();
            bitmap.Save(memory, ImageFormat.Png);
            memory.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MARGINS
    {
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
