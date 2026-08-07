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

namespace ColorBlind
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // store the ImageManipulation object as a class member
        private ImageProcessing imageManipulation;
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
                // Load the selected image into the Image control
                BitmapImage bitmap = new BitmapImage(new Uri(openFileDialog.FileName));
                image.Source = bitmap;

                // convert to writeable bitmap
                WriteableBitmap writeableBitmap = new WriteableBitmap(bitmap);

                // create ImageManipulation object
                imageManipulation = new ImageProcessing(writeableBitmap);

            }
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            if (imageManipulation != null)
            {
                imageManipulation.findAllColors();
                // imageManipulation.ListAllColors();

                // add colors to the ColorPanel
                AddColorBorders(imageManipulation.GetColors());
            }
            else
            {
                MessageBox.Show("Please upload an image first.");
            }
        }

        private void AddColorBorders(HashSet<Color> colors)
        {
            ColorPanel.Children.Clear();

            foreach (Color color in colors)
            {
                Border colorSquare = new Border
                {
                    Width = 60,
                    Height = 60,
                    Margin = new Thickness(15),
                    Background = new SolidColorBrush(color),
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1)
                };
                colorSquare.ToolTip =$"{GetColorName(color)}\nR:{color.R} G:{color.G} B:{color.B}";
                ColorPanel.Children.Add(colorSquare);
            }
        }

        // check which named color a color is closest to
        public static string GetColorName(Color color)
        {
            string closestName = "";
            double smallestDistance = double.MaxValue;

            foreach (var property in typeof(Colors).GetProperties())
            {
                 Color namedColor = (Color)property.GetValue(null)!;

                int redDifference = color.R - namedColor.R;
                int greenDifference = color.G - namedColor.G;
                int blueDifference = color.B - namedColor.B;

                double distance =
                    redDifference * redDifference +
                    greenDifference * greenDifference +
                    blueDifference * blueDifference;

                if (distance < smallestDistance)
                {
                    smallestDistance = distance;
                    closestName = property.Name;
                }
            }

            return closestName;
        }
    }


}