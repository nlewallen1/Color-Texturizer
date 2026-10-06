using System.Collections.Generic;

namespace ColorTexturizer.Core
{
    public class ImageProcessing
    {
        // managers for color and texture processing
        public ColorManager ColorManager { get; private set; }
        public TextureManager TextureManager { get; private set; }

        private readonly int width;
        private readonly int height;
        private readonly int stride;
        private readonly byte[] pixels;

        // constructor
        public ImageProcessing(byte[] pixels, int width, int height)
        {
            this.pixels = pixels;
            this.width = width;
            this.height = height;
            this.stride = width * 4;

            ColorManager = new ColorManager(pixels, width, height, stride);
            TextureManager = new TextureManager(pixels, width, height, stride);
        }

        // finds colors, color groups, assigns textures
        public void FindColors()
        {
            ColorManager.FindAllColors();
            ColorManager.FindColorGroups();
            TextureManager.TextureAssigner(ColorManager.GetColors());
        }

        // returns the pixel data of the image as a byte array
        public byte[] GetBytes()
        {
            return pixels;
        }

        // get colors with pixel counts
        public Dictionary<RgbColor, int> GetColors()
        {
            return ColorManager.GetColors();
        }

        // applies textures and returns the modified BGRA pixel array
        public byte[] ApplyTextures()
        {
            return TextureManager.ApplyTextures(ColorManager.GetColorGroups());
        }

        // get texture for a color
        public string GetTextureForColor(RgbColor color)
        {
            return TextureManager.GetTextureForColor(color);
        }

        // create color preview swatch returning raw PNG byte stream
        public byte[] CreateSwatchPreview(RgbColor baseColor, string texturePath, int size = 60)
        {
            return TextureManager.CreateSwatchPreview(baseColor, texturePath, size);
        }
    }
}