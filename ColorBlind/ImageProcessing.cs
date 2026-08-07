using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
namespace ColorBlind
{
    public class ImageProcessing
    {
        // writeable bitmap
        WriteableBitmap bm;
        int width;
        int height;
        int stride;
        byte[] pixels;

        // store colors
        HashSet<Color> colors = new HashSet<Color>();

        // constructor
        public ImageProcessing(WriteableBitmap bitmap)
        {
            bm = bitmap;
            width = bm.PixelWidth;
            height = bm.PixelHeight;
            // stride is the number of bytes in a row of pixels
            stride = width * 4; // 4 bytes per pixel (BGRA)
            pixels = GetBytes();
        }

        // get pixel byte data
        public byte[] GetBytes()
        {

            // byte array to hold pixel data
            byte[] pixels = new byte[height * stride];
            bm.CopyPixels(pixels, stride, 0);
            return pixels;
        }

        // get colors dictionary
        public HashSet<Color> GetColors()
        {
            return colors;
        }

        // loops through all pixels, adds unique colors to the list
        public void findAllColors()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * stride + x * 4;

                    byte blue = pixels[index];
                    byte green = pixels[index + 1];
                    byte red = pixels[index + 2];
                    byte alpha = pixels[index + 3];

                    Color color = Color.FromRgb(red, green, blue);

                    colors.Add(color);
                }
            }
        }

        // list all colors and their counts
        public void ListAllColors()
        { 
            foreach (var color in colors)
            {
                MessageBox.Show($"Color: R={color.R}, G={color.G}, B={color.B}");
            }
        }
    }
}
