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

        // store colors and their counts
        Dictionary<Color, int> colors = new Dictionary<Color, int>();

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

        // get colors dictonary
        public Dictionary<Color, int> GetColors()
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

                    // ignore near-white and near-black pixels
                    if (red > 245 && green > 245 && blue > 245)
                        continue;

                    if (red < 10 && green < 10 && blue < 10)
                        continue;

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
            // TODO: this is only temporary, refine later
            trimColors(2500);
        }

        // remove noise colors
        public void trimColors(int threshold)
        {
            List<Color> colorsToRemove = new List<Color>();
            foreach (var color in colors)
            {
                if (color.Value < threshold)
                {
                    colorsToRemove.Add(color.Key);
                }
            }
            foreach (var color in colorsToRemove)
            {
                colors.Remove(color);
            }

        }

        // list all colors and their counts
        public void ListAllColors()
        {
            foreach (var color in colors)
            {
                MessageBox.Show($"Color: R={color.Key.R}, G={color.Key.G}, B={color.Key.B} (Count: {color.Value})");
            }
        }

        // modified flood fill algorithm to find connected pixels of the same color
        private void FloodFill(int startX, int startY, Color targetColor, bool[,] visited, List<Point> pixelsList)
        {
            var stack = new Stack<Point>();
            stack.Push(new Point(startX, startY));

            while (stack.Count > 0)
            {
                Point p = stack.Pop();
                int x = (int)p.X;
                int y = (int)p.Y;

                // check bounds
                if (x < 0 || x >= width || y < 0 || y >= height)
                    continue;

                // check if already visited
                if (visited[x, y])
                    continue;

                // get pixel color
                int index = y * stride + x * 4;
                byte blue = pixels[index];
                byte green = pixels[index + 1];
                byte red = pixels[index + 2];
                Color pixelColor = Color.FromRgb(red, green, blue);

                // check if the pixel color matches the target color
                if (pixelColor != targetColor)
                    continue;

                // mark as visited
                visited[x, y] = true;

                // add to the list of pixels in the color group
                pixelsList.Add(new Point(x, y));

                // push neighboring pixels instead of recursing
                stack.Push(new Point(x + 1, y));
                stack.Push(new Point(x - 1, y));
                stack.Push(new Point(x, y + 1));
                stack.Push(new Point(x, y - 1));
            }
        }

        // find groups of color using flood fill algorithm
        public List<ColorGroup> FindColorGroups()
        {
            // 2d array to keep track of visited pixels
            bool[,] visited = new bool[width, height];
            // list to hold color groups
            List<ColorGroup> colorGroups = new List<ColorGroup>();
            // only process colors that are in the colors dictionary
            foreach (var color in colors)
            {
                // if the color is not visited, start a new color group
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int index = y * stride + x * 4;
                        byte blue = pixels[index];
                        byte green = pixels[index + 1];
                        byte red = pixels[index + 2];
                        Color pixelColor = Color.FromRgb(red, green, blue);
                        if (pixelColor == color.Key && !visited[x, y])
                        {
                            // start a new color group
                            ColorGroup colorGroup = new ColorGroup();
                            colorGroup.Color = pixelColor;
                            colorGroup.Pixels = new List<Point>();
                            // perform flood fill to find all connected pixels of the same color
                            FloodFill(x, y, pixelColor, visited, colorGroup.Pixels);
                            // add the color group to the list
                            colorGroups.Add(colorGroup);
                        }
                    }
                }
            }
            return colorGroups;
        }
        // list all color groups and their pixel counts
        public void ListColorGroups(List<ColorGroup> colorGroups)
        {
            foreach (var group in colorGroups)
            {
                MessageBox.Show($"Color: R={group.Color.R}, G={group.Color.G}, B={group.Color.B} (Pixel Count: {group.Pixels.Count})");
            }
        }

        // test color groups by randomly assigning new color to each group and changing
        public void TestColorGroups(List<ColorGroup> colorGroups)
        {
            Random random = new Random();

            // dictionary to hold if this color has already been assigned a new color
            Dictionary<Color, Color> colorMapping = new Dictionary<Color, Color>();

            foreach (var group in colorGroups)
            {
                Color newColor;
                // generate random color if this pixel's color has not been seen yet
                if (!colorMapping.ContainsKey(group.Color)) {
                    // generate a random new color
                    newColor = Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));
                    colorMapping[group.Color] = newColor;
                } else
                {
                    newColor = colorMapping[group.Color];
                }
                // change the color of all pixels in the group to the new color
                foreach (var pixel in group.Pixels)
                {
                    int x = (int)pixel.X;
                    int y = (int)pixel.Y;
                    int index = y * stride + x * 4;
                    pixels[index] = newColor.B; // blue
                    pixels[index + 1] = newColor.G; // green
                    pixels[index + 2] = newColor.R; // red

                }


            }
            // update the pixel in the bitmap
            bm.WritePixels(
                new Int32Rect(0, 0, width, height),
                pixels,
                stride,
                0);
        }

    }
}
