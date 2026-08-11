using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace ColorBlind
{
    public class ColorManager
    {
        private byte[] pixels;
        private int width;
        private int height;
        private int stride;

        private Dictionary<Color, int> colors = new Dictionary<Color, int>();
        private List<ColorGroup> colorGroups = new List<ColorGroup>();
        private Color? largestColor = null;

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

            trimColors();
            return colors;
        }

        // trims noisy colors and pulls out the single largest (background) color
        public void trimColors(double minPixelPercent = 0.5)
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
            bool[,] visited = new bool[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (visited[x, y])
                        continue;

                    int index = y * stride + x * 4;
                    byte blue = pixels[index];
                    byte green = pixels[index + 1];
                    byte red = pixels[index + 2];

                    Color pixelColor = QuantizeColor(red, green, blue);

                    if (!colors.ContainsKey(pixelColor))
                    {
                        visited[x, y] = true;
                        continue;
                    }

                    ColorGroup colorGroup = new ColorGroup();
                    colorGroup.Color = pixelColor;
                    colorGroup.Pixels = new List<Point>();
                    FloodFill(x, y, pixelColor, visited, colorGroup.Pixels);
                    colorGroups.Add(colorGroup);
                }
            }

            return colorGroups;
        }

        private void TryPush(int x, int y, Stack<Point> stack, bool[,] visited)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return;
            if (visited[x, y])
                return;

            visited[x, y] = true;
            stack.Push(new Point(x, y));
        }

        private void FloodFill(int startX, int startY, Color targetColor, bool[,] visited, List<Point> pixelsList)
        {
            var stack = new Stack<Point>();
            stack.Push(new Point(startX, startY));
            visited[startX, startY] = true;

            while (stack.Count > 0)
            {
                Point p = stack.Pop();
                int x = (int)p.X;
                int y = (int)p.Y;

                int index = y * stride + x * 4;
                byte blue = pixels[index];
                byte green = pixels[index + 1];
                byte red = pixels[index + 2];
                Color pixelColor = QuantizeColor(red, green, blue);

                if (pixelColor != targetColor)
                    continue;

                pixelsList.Add(new Point(x, y));

                TryPush(x + 1, y, stack, visited);
                TryPush(x - 1, y, stack, visited);
                TryPush(x, y + 1, stack, visited);
                TryPush(x, y - 1, stack, visited);
            }
        }
    }
}