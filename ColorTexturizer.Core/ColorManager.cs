namespace ColorTexturizer.Core
{
    public class ColorManager
    {
        private byte[] pixels;
        private int width;
        private int height;
        private int stride;

        // dictionary to store colors and their pixel counts
        private Dictionary<RgbColor, int> colors = new Dictionary<RgbColor, int>();
        // list of color groups
        private List<ColorGroup> colorGroups = new List<ColorGroup>();
        // hold largest color (to avoid backgrounds being included) to remove later
        private RgbColor? largestColor = null;

        // constructor
        public ColorManager(byte[] pixels, int width, int height, int stride)
        {
            this.pixels = pixels;
            this.width = width;
            this.height = height;
            this.stride = stride;
        }

        // finds all colors in the image
        public Dictionary<RgbColor, int> FindAllColors()
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

                    RgbColor color = QuantizeColor(red, green, blue);

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
                RgbColor largest = colors.OrderByDescending(c => c.Value).First().Key;
                largestColor = largest;
                colors.Remove(largest);
            }
        }

        // rounds a color to the nearest multiple of `step` per channel, to group near-identical shades
        private RgbColor QuantizeColor(byte r, byte g, byte b, int step = 16)
        {
            byte qr = (byte)((r / step) * step);
            byte qg = (byte)((g / step) * step);
            byte qb = (byte)((b / step) * step);
            return new RgbColor(qr, qg, qb);
        }

        public RgbColor? GetLargestColor()
        {
            return largestColor;
        }

        public Dictionary<RgbColor, int> GetColors()
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
                    RgbColor pixelColor = QuantizeColor(red, green, blue);

                    // if this is not a significant color, move on
                    if (!colors.ContainsKey(pixelColor))
                    {
                        visited[x, y] = true;
                        continue;
                    }

                    // create a new color group and perform flood fill to find all connected pixels of the same color
                    ColorGroup colorGroup = new ColorGroup();
                    colorGroup.RgbColor = pixelColor;
                    colorGroup.Pixels = new List<PixelPoint>();
                    FloodFill(x, y, pixelColor, visited, colorGroup.Pixels);
                    colorGroups.Add(colorGroup);
                }
            }

            return colorGroups;
        }

        // try to push a pixel onto the stack for flood fill, check if its out of bounds or visited
        private void TryPush(int x, int y, Stack<PixelPoint> stack, bool[,] visited)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return;
            if (visited[x, y])
                return;

            visited[x, y] = true;
            stack.Push(new PixelPoint(x, y));
        }

        // flood fill algorithm to find all connected pixels of the same color
        private void FloodFill(int startX, int startY, RgbColor targetColor, bool[,] visited, List<PixelPoint> pixelsList)
        {
            // use a stack to avoid stack overflow with recursion
            var stack = new Stack<PixelPoint>();
            // start from the first pixel
            stack.Push(new PixelPoint(startX, startY));
            visited[startX, startY] = true;

            // loop until there are no more pixels
            while (stack.Count > 0)
            {
                // get the next pixel from the stack
                PixelPoint p = stack.Pop();
                int x = (int)p.X;
                int y = (int)p.Y;

                int index = y * stride + x * 4;
                byte blue = pixels[index];
                byte green = pixels[index + 1];
                byte red = pixels[index + 2];
                // get the color bucket
                RgbColor pixelColor = QuantizeColor(red, green, blue);

                // move on if the pixel color does not match
                if (pixelColor != targetColor)
                    continue;

                // color matches, so add it to the connected pixels list
                pixelsList.Add(new PixelPoint(x, y));

                // push neighboring pixels onto the stack
                TryPush(x + 1, y, stack, visited);
                TryPush(x - 1, y, stack, visited);
                TryPush(x, y + 1, stack, visited);
                TryPush(x, y - 1, stack, visited);
            }
        }
    }
}