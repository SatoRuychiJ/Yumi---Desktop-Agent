using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VPet.Plugin.AIPet
{
    /// <summary>
    /// Captures the primary screen to a downscaled JPEG and returns it as base64.
    /// Uses Win32 BitBlt plus the WPF imaging encoder, so it needs no extra dependency.
    /// </summary>
    internal static class ScreenCapture
    {
        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;
        private const uint SRCCOPY = 0x00CC0020;

        [DllImport("user32.dll")] private static extern int GetSystemMetrics(int index);
        [DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")] private static extern IntPtr GetWindowDC(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleDC(IntPtr hDC);
        [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int w, int h);
        [DllImport("gdi32.dll")] private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObj);
        [DllImport("gdi32.dll")] private static extern bool BitBlt(IntPtr dst, int x, int y, int w, int h, IntPtr src, int sx, int sy, uint rop);
        [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr hObj);
        [DllImport("gdi32.dll")] private static extern bool DeleteDC(IntPtr hDC);

        /// <summary>
        /// Grab the primary screen, scale it down to <paramref name="maxWidth"/>, and return a base64 JPEG.
        /// Returns null on any failure (the caller simply proceeds without an image).
        /// </summary>
        public static string CaptureJpegBase64(int maxWidth = 1280, int quality = 55)
        {
            IntPtr desktop = IntPtr.Zero, srcDc = IntPtr.Zero, memDc = IntPtr.Zero, hbmp = IntPtr.Zero;
            try
            {
                int w = GetSystemMetrics(SM_CXSCREEN);
                int h = GetSystemMetrics(SM_CYSCREEN);
                if (w <= 0 || h <= 0) return null;

                desktop = GetDesktopWindow();
                srcDc = GetWindowDC(desktop);
                memDc = CreateCompatibleDC(srcDc);
                hbmp = CreateCompatibleBitmap(srcDc, w, h);
                IntPtr old = SelectObject(memDc, hbmp);
                BitBlt(memDc, 0, 0, w, h, srcDc, 0, 0, SRCCOPY);
                SelectObject(memDc, old);

                BitmapSource img = Imaging.CreateBitmapSourceFromHBitmap(
                    hbmp, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                if (w > maxWidth)
                {
                    double scale = (double)maxWidth / w;
                    img = new TransformedBitmap(img, new ScaleTransform(scale, scale));
                }
                img.Freeze();

                var encoder = new JpegBitmapEncoder { QualityLevel = quality };
                encoder.Frames.Add(BitmapFrame.Create(img));
                using var ms = new MemoryStream();
                encoder.Save(ms); // reads the pixels here, before the HBITMAP is freed below
                return Convert.ToBase64String(ms.ToArray());
            }
            catch
            {
                return null;
            }
            finally
            {
                if (hbmp != IntPtr.Zero) DeleteObject(hbmp);
                if (memDc != IntPtr.Zero) DeleteDC(memDc);
                if (srcDc != IntPtr.Zero && desktop != IntPtr.Zero) ReleaseDC(desktop, srcDc);
            }
        }
    }
}
