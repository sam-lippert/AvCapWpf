using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfCap;

namespace AvCapWPF
{
    /// <summary>
    /// Interaction logic for CameraView
    /// </summary>
    public partial class CameraView : UserControl
    {
        public CameraView()
        {
            InitializeComponent();

            DeviceBox.ItemsSource = CapDevice.Devices;
            DeviceBox.DisplayMemberPath = "Name";
            DeviceBox.SelectionChanged += (o, e) =>
            {
                Player.Device = (CapDevice)DeviceBox.SelectedItem;
            };
            DeviceBox.SelectedIndex = 0;
            CaptureButton.Click += CaptureButton_Click;
        }

        private void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            // Store current image from the webcam
            var bitmap = Player.CurrentBitmap;
            if (bitmap == null) return;

            Transform tr = new ScaleTransform(-1, 1);
            var transformedBmp = new TransformedBitmap();
            transformedBmp.BeginInit();
            transformedBmp.Source = bitmap;
            transformedBmp.Transform = tr;
            transformedBmp.EndInit();
            bitmap = transformedBmp;

            CapturedImage = bitmap;
        }

        public BitmapSource CapturedImage { get; set; }
    }
}