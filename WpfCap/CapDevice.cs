using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Interop;
using System.Windows;
using System.Diagnostics;

namespace WpfCap
{
    public class CapDevice : DependencyObject, IDisposable
    {
        #region Variables

        private double _frames;
        private CapGrabber _capGrabber;
        private CancellationTokenSource _cancelToken;
        private readonly string _monikerString = string.Empty;
        private readonly Stopwatch _timer = Stopwatch.StartNew();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes the default capture device
        /// </summary>
        public CapDevice()
            : this(DeviceMonikers[0].MonikerString) { }

        /// <summary>
        /// Initializes a specific capture device
        /// </summary>
        /// <param name="moniker">Moniker string that represents a specific device</param>
        public CapDevice(string moniker)
        {
            // Store moniker (since dependency properties are not thread-safe, store it locally as well)
            _monikerString = moniker;
            MonikerString = moniker;

            // Find the name
            var filterInfo = DeviceMonikers.FirstOrDefault(fi => fi.MonikerString == moniker);
            if (filterInfo != null)
            {
                Name = filterInfo.Name;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Event that is invoked when a new bitmap is ready
        /// </summary>
        public event EventHandler NewBitmapReady;

        void OnNewBitmapReady()
        {
            var bit = NewBitmapReady;
            if (bit != null)
            {
                bit(this, null);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the device monikers
        /// </summary>
        public static FilterInfo[] DeviceMonikers
        {
            get
            {
                List<FilterInfo> filters = new List<FilterInfo>();
                IMoniker[] ms = new IMoniker[1];
                ICreateDevEnum enumD = Activator.CreateInstance(Type.GetTypeFromCLSID(SystemDeviceEnum)) as ICreateDevEnum;
                IEnumMoniker moniker;
                Guid g = VideoInputDevice;
                if (enumD.CreateClassEnumerator(ref g, out moniker, 0) == 0)
                {
                    while (true)
                    {
                        int r = moniker.Next(1, ms, IntPtr.Zero);
                        if (r != 0 || ms[0] == null)
                            break;
                        filters.Add(new FilterInfo(ms[0]));
                        Marshal.ReleaseComObject(ms[0]);
                        ms[0] = null;
                    }
                }

                return filters.ToArray();
            }
        }

        /// <summary>
        /// Gets the available devices
        /// </summary>
        public static CapDevice[] Devices
        {
            get
            {
                // Declare variables
                List<CapDevice> devices = new List<CapDevice>();

                // Loop all monikers
                foreach (FilterInfo moniker in DeviceMonikers)
                {
                    devices.Add(new CapDevice(moniker.MonikerString));
                }

                // Return result
                return devices.ToArray();
            }
        }

        /// <summary>
        /// Wrapper for the BitmapSource dependency property
        /// </summary>
        public InteropBitmap BitmapSource
        {
            get { return (InteropBitmap)GetValue(BitmapSourceProperty); }
            private set { SetValue(BitmapSourcePropertyKey, value); }
        }

        private static readonly DependencyPropertyKey BitmapSourcePropertyKey = DependencyProperty.RegisterReadOnly("BitmapSource", typeof(InteropBitmap), typeof(CapDevice), new UIPropertyMetadata(default(InteropBitmap)));

        public static readonly DependencyProperty BitmapSourceProperty = BitmapSourcePropertyKey.DependencyProperty;

        /// <summary>
        /// Wrapper for the Name dependency property
        /// </summary>
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(CapDevice), new UIPropertyMetadata(""));

        /// <summary>
        /// Wrapper for the MonikerString dependency property
        /// </summary>
        public string MonikerString
        {
            get { return (string)GetValue(MonikerStringProperty); }
            set { SetValue(MonikerStringProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MonikerString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MonikerStringProperty = DependencyProperty.Register("MonikerString", typeof(string), typeof(CapDevice), new UIPropertyMetadata(""));

        /// <summary>
        /// Wrapper for the Framerate dependency property
        /// </summary>
        public float Framerate
        {
            get { return (float)GetValue(FramerateProperty); }
            set { SetValue(FramerateProperty, value); }
        }

        public static readonly DependencyProperty FramerateProperty = DependencyProperty.Register("Framerate", typeof(float), typeof(CapDevice), new UIPropertyMetadata(default(float)));

        #endregion

        #region Methods

        /// <summary>
        /// Invoked when a new frame arrived
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">EventArgs</param>
        private void capGrabber_NewFrameArrived(object sender, EventArgs e)
        {
            // Make sure to be thread safe
            if (Dispatcher != null)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, (SendOrPostCallback)delegate
                {
                    if (BitmapSource != null)
                    {
                        BitmapSource.Invalidate();
                        UpdateFramerate();
                    }
                }, null);
            }
        }

        /// <summary>
        /// Updates the framerate
        /// </summary>
        private void UpdateFramerate()
        {
            // Increase the frames
            _frames++;

            // Check the timer
            if (_timer.ElapsedMilliseconds < 1000) return;
            // Set the framerate
            Framerate = (float)Math.Round(_frames * 1000 / _timer.ElapsedMilliseconds);

            // Reset the timer again so we can count the framerate again
            _timer.Reset();
            _timer.Start();
            _frames = 0;
        }

        /// <summary>
        /// Invoked when a property of the CapGrabber object has changed
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">PropertyChangedEventArgs</param>
        private void capGrabber_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.DataBind, (SendOrPostCallback)delegate
            {
                try
                {
                    if ((_capGrabber.Width != default(int)) && (_capGrabber.Height != default(int)))
                    {
                        // Get the pixel count
                        uint pcount = (uint)(_capGrabber.Width * _capGrabber.Height * PixelFormats.Bgr32.BitsPerPixel / 8);

                        // Create a file mapping
                        var section = CreateFileMapping(new IntPtr(-1), IntPtr.Zero, 0x04, 0, pcount, null);
                        var map = MapViewOfFile(section, 0xF001F, 0, 0, pcount);

                        // Get the bitmap
                        BitmapSource = Imaging.CreateBitmapSourceFromMemorySection(section, _capGrabber.Width, _capGrabber.Height, PixelFormats.Bgr32, _capGrabber.Width * PixelFormats.Bgr32.BitsPerPixel / 8, 0) as InteropBitmap;
                        _capGrabber.Map = map;
                        OnNewBitmapReady();
                    }
                }
                catch (Exception ex)
                {
                    // Trace
                    Trace.TraceError(ex.Message);
                }
            }, null);
        }

        /// <summary>;
        /// Starts grabbing images from the capture device
        /// </summary>
        public void Start()
        {
            Stop();
            _cancelToken = new CancellationTokenSource();
            Task.Factory.StartNew(() =>
            {
                // Create new grabber
                _capGrabber = new CapGrabber();
                _capGrabber.PropertyChanged += capGrabber_PropertyChanged;
                _capGrabber.NewFrameArrived += capGrabber_NewFrameArrived;

                var graph = Activator.CreateInstance(Type.GetTypeFromCLSID(FilterGraph)) as IGraphBuilder;

                var sourceObject = FilterInfo.CreateFilter(_monikerString);

                var grabber = Activator.CreateInstance(Type.GetTypeFromCLSID(SampleGrabber)) as ISampleGrabber;
                var grabberObject = grabber as IBaseFilter;

                if (graph == null) return;

                graph.AddFilter(sourceObject, "source");
                graph.AddFilter(grabberObject, "grabber");
                using (var mediaType = new AMMediaType())
                {
                    mediaType.MajorType = MediaTypes.Video;
                    mediaType.SubType = MediaSubTypes.RGB32;
                    if (grabber != null)
                    {
                        grabber.SetMediaType(mediaType);

                        if (graph.Connect(sourceObject.GetPin(PinDirection.Output, 0), grabberObject.GetPin(PinDirection.Input, 0)) >= 0)
                        {
                            if (grabber.GetConnectedMediaType(mediaType) == 0)
                            {
                                var header = (VideoInfoHeader)Marshal.PtrToStructure(mediaType.FormatPtr, typeof(VideoInfoHeader));
                                _capGrabber.Width = header.BmiHeader.Width;
                                _capGrabber.Height = header.BmiHeader.Height;
                            }
                        }
                        graph.Render(grabberObject.GetPin(PinDirection.Output, 0));
                        grabber.SetBufferSamples(false);
                        grabber.SetOneShot(false);
                        grabber.SetCallback(_capGrabber, 1);
                    }

                    // Get the video window
                    var wnd = (IVideoWindow)graph;
                    wnd.put_AutoShow(false);

                    // Create the control and run
                    var control = (IMediaControl)graph;
                    control.Run();

                    // Wait for the stop signal
                    var stopSignal = new ManualResetEventSlim(false);
                    using (_cancelToken.Token.Register(stopSignal.Set))
                        stopSignal.Wait();

                    // Stop when ready
                    control.StopWhenReady();
                    _capGrabber = null;
                }
            }, _cancelToken.Token);
        }

        /// <summary>
        /// Stops grabbing images from the capture device
        /// </summary>
        public void Stop()
        {
            if (_cancelToken != null)
            {
                _cancelToken.Cancel();
            }
        }

        #endregion

        #region Win32

        static readonly Guid FilterGraph = new Guid(0xE436EBB3, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);

        static readonly Guid SampleGrabber = new Guid(0xC1F400A0, 0x3F08, 0x11D3, 0x9F, 0x0B, 0x00, 0x60, 0x08, 0x03, 0x9E, 0x37);

        public static readonly Guid SystemDeviceEnum = new Guid(0x62BE5D10, 0x60EB, 0x11D0, 0xBD, 0x3B, 0x00, 0xA0, 0xC9, 0x11, 0xCE, 0x86);

        public static readonly Guid VideoInputDevice = new Guid(0x860BB310, 0x5D01, 0x11D0, 0xBD, 0x3B, 0x00, 0xA0, 0xC9, 0x11, 0xCE, 0x86);

        [ComVisible(false)]
        internal class MediaTypes
        {
            public static readonly Guid Video = new Guid(0x73646976, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

            public static readonly Guid Interleaved = new Guid(0x73766169, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

            public static readonly Guid Audio = new Guid(0x73647561, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

            public static readonly Guid Text = new Guid(0x73747874, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

            public static readonly Guid Stream = new Guid(0xE436EB83, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);
        }

        [ComVisible(false)]
        internal class MediaSubTypes
        {
            public static readonly Guid YUYV = new Guid(0x56595559, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

            public static readonly Guid IYUV = new Guid(0x56555949, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

            public static readonly Guid DVSD = new Guid(0x44535644, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

            public static readonly Guid RGB1 = new Guid(0xE436EB78, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);

            public static readonly Guid RGB4 = new Guid(0xE436EB79, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);

            public static readonly Guid RGB8 = new Guid(0xE436EB7A, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);

            public static readonly Guid RGB565 = new Guid(0xE436EB7B, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);

            public static readonly Guid RGB555 = new Guid(0xE436EB7C, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);

            public static readonly Guid RGB24 = new Guid(0xE436Eb7D, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);

            public static readonly Guid RGB32 = new Guid(0xE436EB7E, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);

            public static readonly Guid Avi = new Guid(0xE436EB88, 0x524F, 0x11CE, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70);

            public static readonly Guid Asf = new Guid(0x3DB80F90, 0x9412, 0x11D1, 0xAD, 0xED, 0x00, 0x00, 0xF8, 0x75, 0x4B, 0x99);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateFileMapping(IntPtr hFile, IntPtr lpFileMappingAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Stop();
        }

        #endregion
    }
}