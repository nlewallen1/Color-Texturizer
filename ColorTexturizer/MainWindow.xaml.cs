using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ColorTexturizer.Core; // Core engine namespace

namespace ColorTexturizer
{
    public partial class MainWindow : Window
    {
        private ImageProcessing imageProcessing;
        private int currentWidth;
        private int currentHeight;

        public MainWindow()
        {
            InitializeComponent();
        }

        // upload image button
        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                ColorPanel.Children.Clear();

                BitmapImage bitmap = new BitmapImage(new Uri(openFileDialog.FileName));
                currentWidth = bitmap.PixelWidth;
                currentHeight = bitmap.PixelHeight;

                // Show loaded image in UI
                image.Source = bitmap;

                // Extract raw pixels for the core engine
                byte[] pixels = bitmap.GetPixelBytes();

                // Create core ImageProcessing instance
                imageProcessing = new ImageProcessing(pixels, currentWidth, currentHeight);

                // Find all colors and color groups
                imageProcessing.FindColors();

                // Add colors to the ColorPanel
                AddColorPreviewsUntextured(
                    imageProcessing.GetColors(),
                    imageProcessing.ColorManager.GetLargestColor()
                );
            }
        }

        // add textures button
        private void TextureButton_Click(object sender, RoutedEventArgs e)
        {
            if (imageProcessing != null)
            {
                // Add textured colors to the color panel
                AddColorPreviews(
                    imageProcessing.GetColors(),
                    imageProcessing.ColorManager.GetLargestColor()
                );

                // Apply textures in Core library
                byte[] texturedPixels = imageProcessing.ApplyTextures();

                // Convert raw bytes back to WPF WriteableBitmap for display
                image.Source = texturedPixels.ToWriteableBitmap(currentWidth, currentHeight);
            }
            else
            {
                MessageBox.Show("Please upload an image first.");
            }
        }

        // adds untextured color previews to the ColorPanel
        private void AddColorPreviewsUntextured(Dictionary<RgbColor, int> colors, RgbColor? largestColor)
        {
            ColorPanel.Children.Clear();
            var displayColors = new Dictionary<RgbColor, int>(colors);

            if (largestColor.HasValue)
            {
                displayColors[largestColor.Value] = int.MaxValue;
            }

            var sortedColors = displayColors.OrderByDescending(c => c.Value);
            ColorIdentifier colorClassifier = new ColorIdentifier();

            foreach (var kvp in sortedColors)
            {
                RgbColor color = kvp.Key;

                Border colorSquare = new Border
                {
                    Width = 60,
                    Height = 60,
                    Margin = new Thickness(15),
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1),
                    Background = new SolidColorBrush(color.ToMediaColor()) // Convert RgbColor -> WPF Color
                };

                string name = colorClassifier.GetColorName(color);
                colorSquare.ToolTip = $"{name}\nR:{color.R} G:{color.G} B:{color.B}";
                ColorPanel.Children.Add(colorSquare);
            }
        }

        // adds textured color previews to the ColorPanel
        private void AddColorPreviews(Dictionary<RgbColor, int> colors, RgbColor? largestColor)
        {
            ColorPanel.Children.Clear();
            var displayColors = new Dictionary<RgbColor, int>(colors);

            if (largestColor.HasValue)
            {
                displayColors[largestColor.Value] = int.MaxValue;
            }

            var sortedColors = displayColors.OrderByDescending(c => c.Value);
            ColorIdentifier colorClassifier = new ColorIdentifier();

            foreach (var kvp in sortedColors)
            {
                RgbColor color = kvp.Key;

                Border colorSquare = new Border
                {
                    Width = 60,
                    Height = 60,
                    Margin = new Thickness(15),
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1)
                };

                string texturePath = imageProcessing.GetTextureForColor(color);

                if (texturePath != null)
                {
                    // Convert Core PNG bytes -> WPF BitmapImage
                    byte[] swatchBytes = imageProcessing.CreateSwatchPreview(color, texturePath, 60);
                    colorSquare.Background = new ImageBrush(swatchBytes.ToBitmapImage());
                }
                else
                {
                    colorSquare.Background = new SolidColorBrush(color.ToMediaColor());
                }

                string name = colorClassifier.GetColorName(color);
                colorSquare.ToolTip = $"{name}\nR:{color.R} G:{color.G} B:{color.B}";
                ColorPanel.Children.Add(colorSquare);
            }
        }
    }
}