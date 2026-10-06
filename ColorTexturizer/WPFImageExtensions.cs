using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ColorTexturizer.Core;

namespace ColorTexturizer
{
    // adatper class to convert to WPF logic
    public static class WpfImageExtensions
    {
        // Convert WPF Color to Core RgbColor
        public static RgbColor ToRgbColor(this Color c)
        {
            return new RgbColor(c.R, c.G, c.B, c.A);
        }

        // Convert Core RgbColor to WPF Color
        public static Color ToMediaColor(this RgbColor c)
        {
            return Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        // Extract raw BGRA byte[] from WPF BitmapSource
        public static byte[] GetPixelBytes(this BitmapSource bitmap)
        {
            FormatConvertedBitmap bgraBitmap = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);
            int stride = bgraBitmap.PixelWidth * 4;
            byte[] pixels = new byte[bgraBitmap.PixelHeight * stride];
            bgraBitmap.CopyPixels(pixels, stride, 0);
            return pixels;
        }

        // Convert modified BGRA byte[] back to WriteableBitmap for display
        public static WriteableBitmap ToWriteableBitmap(this byte[] pixels, int width, int height)
        {
            int stride = width * 4;
            WriteableBitmap wbm = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            wbm.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
            return wbm;
        }

        // Convert Core PNG byte[] preview stream to WPF BitmapImage for UI swatches
        public static BitmapImage ToBitmapImage(this byte[] pngBytes)
        {
            using MemoryStream ms = new MemoryStream(pngBytes);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = ms;
            image.EndInit();
            image.Freeze(); 
            return image;
        }
    }
}