using System.Windows;
using System.Windows.Media.Imaging;

namespace AvCapWPF
{
    /// <summary>
    /// Interaction logic for CameraWindow.xaml
    /// </summary>
    public partial class CameraWindow : Window
    {
        public CameraWindow()
        {
            InitializeComponent();
            Closed += (o, e) => StopDevice();
            Preview.CaptureButton.Click += Photo_Captured;

            Capture.RetakeButton.Click += Photo_Released;
            Capture.UseButton.Click += (o, e) => OnPhotoCaptured();
        }

        private void Photo_Captured(object sender, RoutedEventArgs e)
        {
            Capture.Photo.Source = Preview.CapturedImage;
            Preview.Visibility = Visibility.Hidden;
            Capture.Visibility = Visibility.Visible;
            StopDevice();
        }

        private void Photo_Released(object sender, RoutedEventArgs e)
        {
            StartDevice();
            Capture.Visibility = Visibility.Hidden;
            Preview.Visibility = Visibility.Visible;
        }

        private void StopDevice()
        {
            var device = Preview.Player.Device;
            if (device != null)
                device.Stop();
        }
        private void StartDevice()
        {
            var device = Preview.Player.Device;
            if (device != null)
                device.Start();
        }

        public delegate void CapturePhoto(BitmapSource image);

        public event CapturePhoto PhotoCaptured;

        protected virtual void OnPhotoCaptured()
        {
            var handler = PhotoCaptured;
            if (handler != null) handler(Preview.CapturedImage);
        }
    }
}