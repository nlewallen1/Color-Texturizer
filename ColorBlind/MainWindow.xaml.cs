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
                // Load the selected image into the Image control
                BitmapImage bitmap = new BitmapImage(new Uri(openFileDialog.FileName));

                // convert to writeable bitmap
                WriteableBitmap writeableBitmap = new WriteableBitmap(bitmap);
                image.Source = writeableBitmap;

                // create ImageManipulation object
                imageProcessing = new ImageProcessing(writeableBitmap);

            }
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            if (imageProcessing != null)
            {
                imageProcessing.findAllColors();
                // imageProcessing.ListAllColors();

                // add colors to the ColorPanel
                AddColorBorders(imageProcessing.GetColors());
                imageProcessing.AssignTextures(imageProcessing.FindColorGroups());
            }
            else
            {
                MessageBox.Show("Please upload an image first.");
            }
        }

        private void AddColorBorders(Dictionary<Color, int> colors)
        {
            ColorPanel.Children.Clear();

            foreach (Color color in colors.Keys)
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
                ColorClassifier colorClassifier = new ColorClassifier();
                string name = colorClassifier.GetColorName(color);

                colorSquare.ToolTip =$"{name}\nR:{color.R} G:{color.G} B:{color.B}";
                ColorPanel.Children.Add(colorSquare);
            }
        }

    }


}