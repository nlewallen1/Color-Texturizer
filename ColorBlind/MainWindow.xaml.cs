using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Linq;

namespace ColorBlind
{

    public partial class MainWindow : Window
    {
        // store the ImageManipulation object as a class member
        private ImageProcessing imageProcessing;
        public MainWindow()
        {
            InitializeComponent();
        }

        // upload image button
        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            // open a file dialog to select an image
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                // clear color panel
                ColorPanel.Children.Clear();

                // load the selected image into the Image control
                BitmapImage bitmap = new BitmapImage(new Uri(openFileDialog.FileName));

                // convert to writeable bitmap
                WriteableBitmap writeableBitmap = new WriteableBitmap(bitmap);
                image.Source = writeableBitmap;

                // create ImageManipulation object
                imageProcessing = new ImageProcessing(writeableBitmap);

                // find all colors and color groups
                imageProcessing.FindColors();

                // add colors to the ColorPanel
                AddColorPreviewsUntextured(imageProcessing.GetColors(), imageProcessing.colorManager.GetLargestColor());

            }
        }

        // add textures button
        private void TextureButton_Click(object sender, RoutedEventArgs e)
        {
            if (imageProcessing != null)
            {
                // add textured colors to the color panel
                AddColorPreviews(imageProcessing.GetColors(), imageProcessing.colorManager.GetLargestColor());
                WriteableBitmap textured = imageProcessing.ApplyTextures();
                // update the image source to the textured image
                image.Source = textured;
            }
            else
            {
                MessageBox.Show("Please upload an image first.");
            }
        }

        // adds untextured color previews to the ColorPanel
        private void AddColorPreviewsUntextured(Dictionary<Color, int> colors, Color? largestColor)
        {
            // clear colorpanel
            ColorPanel.Children.Clear();

            var displayColors = new Dictionary<Color, int>(colors);

            // add largest color back
            if (largestColor.HasValue)
            {
                displayColors[largestColor.Value] = 0;
            }

            // add color squares to the ColorPanel
            foreach (Color color in displayColors.Keys)
            {
                Border colorSquare = new Border
                {
                    Width = 60,
                    Height = 60,
                    Margin = new Thickness(15),
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1),
                    Background = new SolidColorBrush(color)
                };

                // add tooltip with color name and RGB values
                ColorIdentifier colorClassifier = new ColorIdentifier();
                string name = colorClassifier.GetColorName(color);

                colorSquare.ToolTip = $"{name}\nR:{color.R} G:{color.G} B:{color.B}";
                ColorPanel.Children.Add(colorSquare);
            }
        }

        // adds textured color previews to the ColorPanel
        private void AddColorPreviews(Dictionary<Color, int> colors, Color? largestColor)
        {
            ColorPanel.Children.Clear();

            var displayColors = new Dictionary<Color, int>(colors);

            if (largestColor.HasValue)
            {
                displayColors[largestColor.Value] = 0;
            }

            foreach (Color color in displayColors.Keys)
            {
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
                    BitmapSource swatch = imageProcessing.CreateSwatchPreview(color, texturePath, 60);
                    colorSquare.Background = new ImageBrush(swatch);
                }
                else
                {
                    colorSquare.Background = new SolidColorBrush(color);
                }

                ColorIdentifier colorClassifier = new ColorIdentifier();
                string name = colorClassifier.GetColorName(color);

                colorSquare.ToolTip = $"{name}\nR:{color.R} G:{color.G} B:{color.B}";
                ColorPanel.Children.Add(colorSquare);
            }
        }

    }
}

