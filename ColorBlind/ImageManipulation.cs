using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
namespace ColorBlind
{
    public class ImageManipulation
    {
        // writeable bitmap
        WriteableBitmap bm;
        int width;
        int height;
        int stride;
        byte[] pixels;

        // store colors
        Dictionary<Color, int> colors = new Dictionary<Color, int>();

        // constructor
        public ImageManipulation(WriteableBitmap bitmap)
        {
            bm = bitmap;
            width = bm.PixelWidth;
            height = bm.PixelHeight;
            // stride is the number of bytes in a row of pixels
            stride = width * 4; // 4 bytes per pixel (BGRA)
            pixels = GetBytes();
        }

        public byte[] GetBytes()
        {

            // byte array to hold pixel data
            byte[] pixels = new byte[height * stride];
            bm.CopyPixels(pixels, stride, 0);
            return pixels;
        }

        // temporary, reads a pixel color
        public void ReadColorTest()
        {
            // test output pixel at 0, 0
            int index = 0; // pixel at (0, 0)
            byte blue = pixels[index];
            byte green = pixels[index + 1];
            byte red = pixels[index + 2];
            byte alpha = pixels[index + 3];

            MessageBox.Show($"Pixel at (0, 0): R={red}, G={green}, B={blue}, A={alpha}");

        }

        // loops through all pixels, sees if it matches a dictionary color, adds if not
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

                    Color color = Color.FromRgb(red, green, blue);

                    if (colors.ContainsKey(color))
                    {
                        colors[color]++;
                    }
                    else
                    {
                        colors[color] = 1;
                    }
                }
            }
        }

        // list all colors and their counts
        public void ListAllColors()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var kvp in colors)
            {
                Color color = kvp.Key;
                int count = kvp.Value;
                sb.AppendLine($"Color: R={color.R}, G={color.G}, B={color.B}, Count={count}");
            }
            MessageBox.Show(sb.ToString());
        }
    }
}
