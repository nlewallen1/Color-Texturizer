using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ColorTexturizer
{
    public class ImageProcessing
    {
        // managers for color and texture processing
        public ColorManager colorManager;
        public TextureManager textureManager;

        private WriteableBitmap bm;
        private int width;
        private int height;
        private int stride;
        private byte[] pixels;

        // constructor
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

        // finds colors, color groups, assigns textures
        public void FindColors()
        {
            colorManager.FindAllColors();
            colorManager.FindColorGroups();
            textureManager.TextureAssigner(colorManager.GetColors());
        }

        // returns the pixel data of the image as a byte array
        public byte[] GetBytes()
        {
            byte[] buffer = new byte[height * stride];
            bm.CopyPixels(buffer, stride, 0);
            return buffer;
        }

        // get colors with pixel counts
        public Dictionary<Color, int> GetColors()
        {
            return colorManager.GetColors();
        }

        // get the texture bitmap from TextureManager
        public WriteableBitmap ApplyTextures()
        {
            WriteableBitmap result = textureManager.ApplyTextures(colorManager.GetColorGroups());
            bm = result;
            return result;
        }

        // get texture for a color
        public string GetTextureForColor(Color color)
        {
            return textureManager.GetTextureForColor(color);
        }

        // create color preview swatch
        public BitmapSource CreateSwatchPreview(Color baseColor, string texturePath, int size = 60)
        {
            return textureManager.CreateSwatchPreview(baseColor, texturePath, size);
        }
    }
}