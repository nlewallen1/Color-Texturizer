using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ColorBlind
{
    public class TextureManager
    {
        private byte[] pixels;
        private int width;
        private int height;
        private int stride;
        private double dpiX;
        private double dpiY;

        private Random random = new Random();
        // dictionary to store color-texture assignments
        private Dictionary<Color, string> colorTextures = new Dictionary<Color, string>();

        // constructor
        public TextureManager(byte[] pixels, int width, int height, int stride, double dpiX = 96, double dpiY = 96)
        {
            this.pixels = pixels;
            this.width = width;
            this.height = height;
            this.stride = stride;
            this.dpiX = dpiX;
            this.dpiY = dpiY;
        }

        // randomly assigns a texture file to each newly-found color, avoiding duplicates
        public void TextureAssigner(Dictionary<Color, int> colors)
        {
            // get textures path
            string texturesFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Textures");

            // get all texture files in the folder
            string[] textureFiles = System.IO.Directory.GetFiles(texturesFolder, "*.*")
                .Where(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                         || f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                         || f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            // find colors that don't have an assigned texture yet
            List<Color> unassigned = colors.Keys.Where(c => !colorTextures.ContainsKey(c)).ToList();
            if (unassigned.Count == 0)
                return;

            // shuffle the texture files
            List<string> shuffled = textureFiles.ToList();
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }

            for (int i = 0; i < unassigned.Count; i++)
            {
                string chosen = shuffled[i % shuffled.Count];
                colorTextures[unassigned[i]] = chosen;
            }
        }

        // applies each color's assigned texture to its pixel group
        public WriteableBitmap ApplyTextures(List<ColorGroup> colorGroups)
        {
            // give each color group a texture
            foreach (var group in colorGroups)
            {
                if (group.Pixels.Count == 0)
                    continue;

                // load the texture for this color
                string texturePath = colorTextures[group.Color];
                // load the texture image
                BitmapSource texture = LoadTexture(texturePath);

                // get texture pixel data
                int tw = texture.PixelWidth;
                int th = texture.PixelHeight;
                int tStride = tw * 4;
                byte[] texPixels = new byte[th * tStride];
                texture.CopyPixels(texPixels, tStride, 0);

                // find the minimum x and y coordinates of the pixels in this group
                int minX = group.Pixels.Min(p => (int)p.X);
                int minY = group.Pixels.Min(p => (int)p.Y);

                // loop through each pixel in the group and apply the texture
                foreach (Point p in group.Pixels)
                {
                    int x = (int)p.X;
                    int y = (int)p.Y;
                    int i = y * stride + x * 4;

                    // use pixel positions relative to top left corner of the region,
                    // wrapping around with modulo
                    int tx = (x - minX) % tw;
                    int ty = (y - minY) % th;
                    if (tx < 0) tx += tw;
                    if (ty < 0) ty += th;
                    int ti = ty * tStride + tx * 4;

                    // get texture pixel color
                    byte texB = texPixels[ti];
                    byte texG = texPixels[ti + 1];
                    byte texR = texPixels[ti + 2];
                    byte texA = texPixels[ti + 3];

                    double alpha = texA / 255.0;

                    // get source pixel color
                    byte srcB = pixels[i];
                    byte srcG = pixels[i + 1];
                    byte srcR = pixels[i + 2];

                    // blend the texture pixel with the source pixel based on alpha, keeping orginal pixel's color if transparent
                    pixels[i] = (byte)(texB * alpha + srcB * (1 - alpha));
                    pixels[i + 1] = (byte)(texG * alpha + srcG * (1 - alpha));
                    pixels[i + 2] = (byte)(texR * alpha + srcR * (1 - alpha));
                }
            }

            // create a new WriteableBitmap with the modified pixel data
            WriteableBitmap result = new WriteableBitmap(width, height,  dpiX, dpiY, PixelFormats.Bgra32, null);
            result.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
            return result;
        }

        // loads a texture image from file and converts it to Bgra32 format
        private BitmapSource LoadTexture(string path)
        {
            BitmapImage img = new BitmapImage();
            img.BeginInit();
            img.UriSource = new Uri(path, UriKind.Absolute);
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.EndInit();

            return new FormatConvertedBitmap(img, PixelFormats.Bgra32, null, 0);
        }
        
        // get assigned texture for a color
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
            // load the texture image
            BitmapSource texture = LoadTexture(texturePath);

            int tw = texture.PixelWidth;
            int th = texture.PixelHeight;
            int tStride = tw * 4;
            byte[] texPixels = new byte[th * tStride];
            texture.CopyPixels(texPixels, tStride, 0);

            int previewStride = size * 4;
            // set up blank output
            byte[] outPixels = new byte[size * previewStride];

            // loop through square of pixels
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int i = y * previewStride + x * 4;

                    // tiling and texture blending
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

            // return bitmap of the preview
            WriteableBitmap result = new WriteableBitmap(size, size, 96, 96, PixelFormats.Bgra32, null);
            result.WritePixels(new Int32Rect(0, 0, size, size), outPixels, previewStride, 0);
            return result;
        }
    }
}