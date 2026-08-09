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

        private Random random = new Random();
        // store colors and their counts
        Dictionary<Color, int> colors = new Dictionary<Color, int>();

        // store texture type for each color found
        Dictionary<Color, string> colorTextures = new Dictionary<Color, string>();

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

            // assign textures to each color
            textureAssigner(colors);
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

        // assign a texture type to a color randomly, avoiding duplicates
        // randomly assigns a texture file to each newly-found color
        public void textureAssigner(Dictionary<Color, int> colors)
        {
            string texturesFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Textures");

            if (!System.IO.Directory.Exists(texturesFolder))
            {
                System.Diagnostics.Debug.WriteLine($"Textures folder not found: {texturesFolder}");
                return;
            }

            string[] textureFiles = System.IO.Directory.GetFiles(texturesFolder, "*.*")
                .Where(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                         || f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                         || f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (textureFiles.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine("No texture files found in Textures folder.");
                return;
            }

            // colors that still need a texture assigned
            List<Color> unassigned = colors.Keys.Where(c => !colorTextures.ContainsKey(c)).ToList();

            if (unassigned.Count == 0)
                return;

            if (unassigned.Count > textureFiles.Length)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Warning: {unassigned.Count} colors but only {textureFiles.Length} textures available — some will repeat.");
            }

            // shuffle a copy of the texture list (Fisher-Yates)
            List<string> shuffled = textureFiles.ToList();
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }

            for (int i = 0; i < unassigned.Count; i++)
            {
                // wrap around with modulo if there are more colors than textures
                string chosen = shuffled[i % shuffled.Count];
                colorTextures[unassigned[i]] = chosen;
                System.Diagnostics.Debug.WriteLine($"Assigned {System.IO.Path.GetFileName(chosen)} to color {unassigned[i]}");
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

        // applies a single texture to every color group found via flood fill
        // (temporary version — no per-color texture lookup yet)
        public WriteableBitmap ApplyTextures()
        {
            List<ColorGroup> colorGroups = FindColorGroups();

            foreach (var group in colorGroups)
            {
                if (group.Pixels.Count == 0)
                    continue;

                if (!colorTextures.ContainsKey(group.Color))
                {
                    System.Diagnostics.Debug.WriteLine($"No texture assigned for color {group.Color}, skipping.");
                    continue;
                }

                string texturePath = colorTextures[group.Color];
                BitmapSource texture = LoadTexture(texturePath);

                int tw = texture.PixelWidth;
                int th = texture.PixelHeight;
                int tStride = tw * 4;
                byte[] texPixels = new byte[th * tStride];
                texture.CopyPixels(texPixels, tStride, 0);

                int minX = group.Pixels.Min(p => (int)p.X);
                int minY = group.Pixels.Min(p => (int)p.Y);

                foreach (Point p in group.Pixels)
                {
                    int x = (int)p.X;
                    int y = (int)p.Y;
                    int i = y * stride + x * 4;

                    int tx = (x - minX) % tw;
                    int ty = (y - minY) % th;
                    if (tx < 0) tx += tw;
                    if (ty < 0) ty += th;
                    int ti = ty * tStride + tx * 4;

                    byte texB = texPixels[ti];
                    byte texG = texPixels[ti + 1];
                    byte texR = texPixels[ti + 2];
                    byte texA = texPixels[ti + 3];   // texture's alpha at this pixel

                    double alpha = texA / 255.0;

                    byte srcB = pixels[i];
                    byte srcG = pixels[i + 1];
                    byte srcR = pixels[i + 2];

                    // alpha-over blend: texture color where opaque, original color where transparent
                    pixels[i] = (byte)(texB * alpha + srcB * (1 - alpha));
                    pixels[i + 1] = (byte)(texG * alpha + srcG * (1 - alpha));
                    pixels[i + 2] = (byte)(texR * alpha + srcR * (1 - alpha));
                    // pixels[i + 3] (alpha) left untouched — stays fully opaque
                }
            }

            WriteableBitmap result = new WriteableBitmap(width, height, bm.DpiX, bm.DpiY, PixelFormats.Bgra32, null);
            result.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
            bm = result;
            return result;
        }

        // now loads from a plain file path instead of a pack URI
        private BitmapSource LoadTexture(string path)
        {
            BitmapImage img = new BitmapImage();
            img.BeginInit();
            img.UriSource = new Uri(path, UriKind.Absolute);
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.EndInit();

            return new FormatConvertedBitmap(img, PixelFormats.Bgra32, null, 0);
        }

        public string GetTextureForColor(Color color)
        {
            if (colorTextures.ContainsKey(color))
                return colorTextures[color];
            return null;
        }

        // creates a small preview bitmap showing a solid color with the texture overlaid,
        // for use in UI swatches
        public BitmapSource CreateSwatchPreview(Color baseColor, string texturePath, int size = 60)
        {
            BitmapSource texture = LoadTexture(texturePath);

            int tw = texture.PixelWidth;
            int th = texture.PixelHeight;
            int tStride = tw * 4;
            byte[] texPixels = new byte[th * tStride];
            texture.CopyPixels(texPixels, tStride, 0);

            int stride = size * 4;
            byte[] outPixels = new byte[size * stride];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int i = y * stride + x * 4;

                    int tx = x % tw;
                    int ty = y % th;
                    int ti = ty * tStride + tx * 4;

                    byte texB = texPixels[ti];
                    byte texG = texPixels[ti + 1];
                    byte texR = texPixels[ti + 2];
                    byte texA = texPixels[ti + 3];

                    double alpha = texA / 255.0;

                    outPixels[i] = (byte)(texB * alpha + baseColor.B * (1 - alpha));
                    outPixels[i + 1] = (byte)(texG * alpha + baseColor.G * (1 - alpha));
                    outPixels[i + 2] = (byte)(texR * alpha + baseColor.R * (1 - alpha));
                    outPixels[i + 3] = 255;
                }
            }

            WriteableBitmap result = new WriteableBitmap(size, size, 96, 96, PixelFormats.Bgra32, null);
            result.WritePixels(new Int32Rect(0, 0, size, size), outPixels, stride, 0);
            return result;
        }

    }
}
