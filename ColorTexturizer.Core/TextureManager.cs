using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ColorTexturizer.Core
{
    public class TextureManager
    {
        private readonly byte[] pixels;
        private readonly int width;
        private readonly int height;
        private readonly int stride;

        private readonly Random random = new Random();
        // dictionary to store color-texture assignments
        private readonly Dictionary<RgbColor, string> colorTextures = new Dictionary<RgbColor, string>();

        // constructor
        public TextureManager(byte[] pixels, int width, int height, int stride)
        {
            this.pixels = pixels;
            this.width = width;
            this.height = height;
            this.stride = stride;
        }

        // randomly assigns a texture file to each newly-found color, avoiding duplicates
        public void TextureAssigner(Dictionary<RgbColor, int> colors)
        {
            // get textures path
            string texturesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Textures");

            if (!Directory.Exists(texturesFolder))
                return;

            // get all texture files in the folder
            string[] textureFiles = Directory.GetFiles(texturesFolder, "*.*")
                .Where(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                         || f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                         || f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            // find colors that don't have an assigned texture yet
            List<RgbColor> unassigned = colors.Keys.Where(c => !colorTextures.ContainsKey(c)).ToList();
            if (unassigned.Count == 0 || textureFiles.Length == 0)
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

        // applies each color's assigned texture to its pixel group and returns the modified pixel array
        public byte[] ApplyTextures(List<ColorGroup> colorGroups)
        {
            // give each color group a texture
            foreach (var group in colorGroups)
            {
                if (group.Pixels.Count == 0)
                    continue;

                // check if texture exists for color property
                if (!colorTextures.TryGetValue(group.RgbColor, out string texturePath))
                    continue;

                // load texture using ImageSharp helper
                var (texPixels, tw, th) = LoadTexturePixels(texturePath);
                int tStride = tw * 4;

                // find minimum x and y coordinates
                int minX = group.Pixels.Min(p => p.X);
                int minY = group.Pixels.Min(p => p.Y);

                // loop through each pixel in the group and apply texture
                foreach (PixelPoint p in group.Pixels)
                {
                    int x = p.X;
                    int y = p.Y;
                    int i = y * stride + x * 4;

                    int tx = (x - minX) % tw;
                    int ty = (y - minY) % th;
                    if (tx < 0) tx += tw;
                    if (ty < 0) ty += th;
                    int ti = ty * tStride + tx * 4;

                    // texture pixel color (BGRA format)
                    byte texB = texPixels[ti];
                    byte texG = texPixels[ti + 1];
                    byte texR = texPixels[ti + 2];
                    byte texA = texPixels[ti + 3];

                    double alpha = texA / 255.0;

                    // source pixel color
                    byte srcB = pixels[i];
                    byte srcG = pixels[i + 1];
                    byte srcR = pixels[i + 2];

                    // blend texture pixel with source pixel based on alpha
                    pixels[i] = (byte)(texB * alpha + srcB * (1 - alpha));
                    pixels[i + 1] = (byte)(texG * alpha + srcG * (1 - alpha));
                    pixels[i + 2] = (byte)(texR * alpha + srcR * (1 - alpha));
                }
            }

            return pixels;
        }

        // loads a texture image from file into BGRA byte array
        private (byte[] Pixels, int Width, int Height) LoadTexturePixels(string path)
        {
            using Image<Bgra32> image = Image.Load<Bgra32>(path);
            byte[] pixelBytes = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(pixelBytes);
            return (pixelBytes, image.Width, image.Height);
        }

        // get assigned texture for a color
        public string GetTextureForColor(RgbColor color)
        {
            if (colorTextures.TryGetValue(color, out string path))
                return path;
            return null;
        }

        // creates a small preview swatch returning a PNG byte stream
        public byte[] CreateSwatchPreview(RgbColor baseColor, string texturePath, int size = 60)
        {
            var (texPixels, tw, th) = LoadTexturePixels(texturePath);
            int tStride = tw * 4;

            int previewStride = size * 4;
            byte[] outPixels = new byte[size * previewStride];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int i = y * previewStride + x * 4;

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

            using Image<Bgra32> previewImage = Image.LoadPixelData<Bgra32>(outPixels, size, size);
            using MemoryStream ms = new MemoryStream();
            previewImage.SaveAsPng(ms);
            return ms.ToArray();
        }
    }
}