using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ColorBlind
{
    public class ImageProcessing
    {
        public ColorManager colorManager;
        public TextureManager textureManager;

        private WriteableBitmap bm;
        private int width;
        private int height;
        private int stride;
        private byte[] pixels;

        public ImageProcessing(WriteableBitmap bitmap)
        {
            bm = bitmap;
            width = bm.PixelWidth;
            height = bm.PixelHeight;
            stride = width * 4;
            pixels = GetBytes();

            colorManager = new ColorManager(pixels, width, height, stride);
            textureManager = new TextureManager(pixels, width, height, stride, bm.DpiX, bm.DpiY);
        }

        // runs color detection + grouping + texture assignment in one call
        public void FindColors()
        {
            colorManager.FindAllColors();
            colorManager.FindColorGroups();
            textureManager.TextureAssigner(colorManager.GetColors());
        }

        public byte[] GetBytes()
        {
            byte[] buffer = new byte[height * stride];
            bm.CopyPixels(buffer, stride, 0);
            return buffer;
        }

        public List<ColorGroup> GetColorGroups()
        {
            return colorManager.GetColorGroups();
        }

        public Dictionary<Color, int> GetColors()
        {
            return colorManager.GetColors();
        }

        public WriteableBitmap ApplyTextures()
        {
            WriteableBitmap result = textureManager.ApplyTextures(colorManager.GetColorGroups());
            bm = result;
            return result;
        }

        public string GetTextureForColor(Color color)
        {
            return textureManager.GetTextureForColor(color);
        }

        public BitmapSource CreateSwatchPreview(Color baseColor, string texturePath, int size = 60)
        {
            return textureManager.CreateSwatchPreview(baseColor, texturePath, size);
        }
    }
}