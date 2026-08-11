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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // store the ImageManipulation object as a class member
        private ImageProcessing imageProcessing;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            // Open a file dialog to select an image
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                // clear color panel
                ColorPanel.Children.Clear();

                // Load the selected image into the Image control
                BitmapImage bitmap = new BitmapImage(new Uri(openFileDialog.FileName));

                // convert to writeable bitmap
                WriteableBitmap writeableBitmap = new WriteableBitmap(bitmap);
                image.Source = writeableBitmap;

                // create ImageManipulation object
                imageProcessing = new ImageProcessing(writeableBitmap);

                imageProcessing.findAllColors();
                imageProcessing.FindColorGroups();

                // add colors to the ColorPanel
                AddColorBordersUntextured(imageProcessing.GetColors(), imageProcessing.GetLargestColor());

            }
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            if (imageProcessing != null)
            {

                // test texture
                AddColorBorders(imageProcessing.GetColors(), imageProcessing.GetLargestColor());
                WriteableBitmap textured = imageProcessing.ApplyTextures();
                image.Source = textured;
            }
            else
            {
                MessageBox.Show("Please upload an image first.");
            }
        }

        private void AddColorBordersUntextured(Dictionary<Color, int> colors, Color? largestColor)
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
                    BorderThickness = new Thickness(1),
                    Background = new SolidColorBrush(color)
                };

                ColorClassifier colorClassifier = new ColorClassifier();
                string name = colorClassifier.GetColorName(color);

                colorSquare.ToolTip = $"{name}\nR:{color.R} G:{color.G} B:{color.B}";
                ColorPanel.Children.Add(colorSquare);
            }
        }

        private void AddColorBorders(Dictionary<Color, int> colors, Color? largestColor)
        {
            ColorPanel.Children.Clear();

            // add largest color back
            if (largestColor.HasValue)
            {
                colors.Add(largestColor.Value, 0);
            }

            foreach (Color color in colors.Keys)
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
                    // fallback if no texture was assigned to this color
                    colorSquare.Background = new SolidColorBrush(color);
                }

                ColorClassifier colorClassifier = new ColorClassifier();
                string name = colorClassifier.GetColorName(color);

                colorSquare.ToolTip = $"{name}\nR:{color.R} G:{color.G} B:{color.B}";
                ColorPanel.Children.Add(colorSquare);
            }
        }

    }
}

