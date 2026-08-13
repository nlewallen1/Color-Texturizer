using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace ColorTexturizer
{
    public class ColorManager
    {
        private byte[] pixels;
        private int width;
        private int height;
        private int stride;

        // dictionary to store colors and their pixel counts
        private Dictionary<Color, int> colors = new Dictionary<Color, int>();
        // list of color groups
        private List<ColorGroup> colorGroups = new List<ColorGroup>();
        // hold largest color (to avoid backgrounds being included) to remove later
        private Color? largestColor = null;

        // constructor
        public ColorManager(byte[] pixels, int width, int height, int stride)
        {
            this.pixels = pixels;
            this.width = width;
            this.height = height;
            this.stride = stride;
        }

        // finds all colors in the image
        public Dictionary<Color, int> FindAllColors()
        {
            colors.Clear();
            largestColor = null;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * stride + x * 4;
                    byte blue = pixels[index];
                    byte green = pixels[index + 1];
                    byte red = pixels[index + 2];

                    Color color = QuantizeColor(red, green, blue);

                    if (color.R > 220 && color.G > 220 && color.B > 220) continue;
                    if (color.R < 30 && color.G < 30 && color.B < 30) continue;

                    if (colors.ContainsKey(color))
                        colors[color]++;
                    else
                        colors[color] = 1;
                }
            }

            TrimColors();
            return colors;
        }

        // trims noisy colors and pulls out the single largest (background) color
        public void TrimColors(double minPixelPercent = 0.2)
        {
            int totalPixels = width * height;
            int minPixelCount = (int)(totalPixels * (minPixelPercent / 100.0));

            var noise = colors.Where(c => c.Value < minPixelCount).Select(c => c.Key).ToList();
            foreach (var color in noise)
                colors.Remove(color);

            if (colors.Count > 1)
            {
                Color largest = colors.OrderByDescending(c => c.Value).First().Key;
                largestColor = largest;
                colors.Remove(largest);
            }
        }

        // rounds a color to the nearest multiple of `step` per channel, to group near-identical shades
        private Color QuantizeColor(byte r, byte g, byte b, int step = 16)
        {
            byte qr = (byte)((r / step) * step);
            byte qg = (byte)((g / step) * step);
            byte qb = (byte)((b / step) * step);
            return Color.FromRgb(qr, qg, qb);
        }

        public Color? GetLargestColor()
        {
            return largestColor;
        }

        public Dictionary<Color, int> GetColors()
        {
            return colors;
        }

        public List<ColorGroup> GetColorGroups()
        {
            return colorGroups;
        }

        // find groups of colors using flood fill
        public List<ColorGroup> FindColorGroups()
        {
            colorGroups.Clear();
            // create a visited array to keep track of which pixels have been processed
            bool[,] visited = new bool[width, height];

            // loop through each pixel
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (visited[x, y])
                        continue;

                    // get the color of the current pixel
                    int index = y * stride + x * 4;
                    byte blue = pixels[index];
                    byte green = pixels[index + 1];
                    byte red = pixels[index + 2];

                    // quantize the color to group similar shades
                    Color pixelColor = QuantizeColor(red, green, blue);

                    // if this is not a significant color, move on
                    if (!colors.ContainsKey(pixelColor))
                    {
                        visited[x, y] = true;
                        continue;
                    }

                    // create a new color group and perform flood fill to find all connected pixels of the same color
                    ColorGroup colorGroup = new ColorGroup();
                    colorGroup.Color = pixelColor;
                    colorGroup.Pixels = new List<Point>();
                    FloodFill(x, y, pixelColor, visited, colorGroup.Pixels);
                    colorGroups.Add(colorGroup);
                }
            }

            return colorGroups;
        }

        // try to push a pixel onto the stack for flood fill, check if its out of bounds or visited
        private void TryPush(int x, int y, Stack<Point> stack, bool[,] visited)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return;
            if (visited[x, y])
                return;

            visited[x, y] = true;
            stack.Push(new Point(x, y));
        }

        // flood fill algorithm to find all connected pixels of the same color
        private void FloodFill(int startX, int startY, Color targetColor, bool[,] visited, List<Point> pixelsList)
        {
            // use a stack to avoid stack overflow with recursion
            var stack = new Stack<Point>();
            // start from the first pixel
            stack.Push(new Point(startX, startY));
            visited[startX, startY] = true;

            // loop until there are no more pixels
            while (stack.Count > 0)
            {
                // get the next pixel from the stack
                Point p = stack.Pop();
                int x = (int)p.X;
                int y = (int)p.Y;

                int index = y * stride + x * 4;
                byte blue = pixels[index];
                byte green = pixels[index + 1];
                byte red = pixels[index + 2];
                // get the color bucket
                Color pixelColor = QuantizeColor(red, green, blue);

                // move on if the pixel color does not match
                if (pixelColor != targetColor)
                    continue;

                // color matches, so add it to the connected pixels list
                pixelsList.Add(new Point(x, y));

                // push neighboring pixels onto the stack
                TryPush(x + 1, y, stack, visited);
                TryPush(x - 1, y, stack, visited);
                TryPush(x, y + 1, stack, visited);
                TryPush(x, y - 1, stack, visited);
            }
        }
    }
}