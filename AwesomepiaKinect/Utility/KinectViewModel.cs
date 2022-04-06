using Microsoft.Azure.Kinect.BodyTracking;
using Microsoft.Azure.Kinect.Sensor;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;


namespace Utility
{
    public class KinectViewModel : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler KinectNoConnected;
        public event EventHandler KinectConnected;

        #endregion Events

        #region Member vars

        private Device _device;
        private Calibration _calibration;
        private Transformation _transformation;
        private int _colourWidth;
        private int _colourHeight;
        private int _depthWidth;
        private int _depthHeight;
        private SynchronizationContext _uiContext;
        private Tracker _bodyTracker;
        private ImageSource _bitmap;
        private BGRA[] _bodyColours =
        {
            new BGRA(255, 0, 0, 255),        // 0
            new BGRA(0, 255, 0, 255),        // 1
            new BGRA(0, 0, 255, 255),        // 2
            new BGRA(255, 255, 0, 255),      // 3
            new BGRA(255, 255, 255, 255),    // 4
            new BGRA(0, 255, 255, 255),      // 5
            new BGRA(128, 255, 0, 255),      // 6
            new BGRA(128, 128, 0, 255),      // 7
            new BGRA(128, 128, 128, 255),    // 8
            new BGRA(0, 0, 0, 255),          // 9
        };
        private BGRA SkeletonColour;

        #endregion Member vars

        #region Constructors

        public KinectViewModel()
        {
            SelectedOutput = new OutputOption { Name = "Skeleton tracking", OutputType = OutputType.SkeletonTracking };
        }

        #endregion Constructors

        #region VM Properties

        private OutputOption _selectedOutput;
        private bool _applicationIsRunning = true;
        public OutputOption SelectedOutput
        {
            get => _selectedOutput;
            set
            {
                CameraDetails.Clear();
                _selectedOutput = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedOutput"));
            }
        }
        public ObservableCollection<OutputOption> Outputs { get; set; }
        public ObservableCollection<CameraDetailItem> CameraDetails { get; set; } = new ObservableCollection<CameraDetailItem>();
        public ImageSource CurrentCameraImage => _bitmap;

        public List<string> ViewJoints = new List<string>();
        private int _SkeletonPixelSize = 12;
        public int SkeletonPixelSize
        {
            get { return _SkeletonPixelSize; }
            set
            {
                _SkeletonPixelSize = value;

                if (_SkeletonPixelSize == 0)
                    return;

                int temp = _SkeletonPixelSize;
                if (_SkeletonPixelSize % 2 == 0)
                    temp++;

                minPixel = (temp - 1) / 2;
                maxPixel = minPixel + 1;

            }
        }

        public enum PixelColor
        {
            Blue = 0,
            Yellow = 5,
            Red = 2,
            Green = 1,
            Black = 9,
            White = 4,
            Gray = 8


        }
        private PixelColor _SkeletonPixelColor = PixelColor.Blue;
        public PixelColor SkeletonPixelColor
        {
            get { return _SkeletonPixelColor; }
            set
            {
                _SkeletonPixelColor = value;
                SkeletonColour = _bodyColours[(int)_SkeletonPixelColor];
            }
        }

        public int recogBodyNum = 0;



        private int minPixel = 6;
        private int maxPixel = 7;

        public Dictionary<string, string> Dictionary_JointsPoint = new Dictionary<string, string>();

        #endregion VM Properties

        #region Camera control

        internal void StopCamera()
        {
            _applicationIsRunning = false;
            Task.WaitAny(Task.Delay(1000));
            _device.StopImu();
            _device.StopCameras();
            _bodyTracker.Shutdown();

            _bodyTracker.Dispose();
            _device.Dispose();

        }

        internal void StartCamera()
        {
            if (Device.GetInstalledCount() == 0)
            {
                MessageBox.Show("연결된 키넥트가 없습니다.");
                KinectNoConnected?.Invoke(this, new EventArgs());
                //MessageBox.Show("프로그램을 종료합니다.");
                //Application.Current.Shutdown();
                return;
            }

            _device = Device.Open();
            KinectConnected?.Invoke(this, new EventArgs());
            var configuration = new DeviceConfiguration
            {
                ColorFormat = ImageFormat.ColorBGRA32,
                ColorResolution = ColorResolution.R720p,
                DepthMode = DepthMode.WFOV_2x2Binned,
                SynchronizedImagesOnly = true,
                CameraFPS = FPS.FPS30
            };

            _device.StartCameras(configuration);

            //_device.StartImu();

            _calibration = _device.GetCalibration(configuration.DepthMode, configuration.ColorResolution);
            _transformation = _calibration.CreateTransformation();
            _colourWidth = _calibration.ColorCameraCalibration.ResolutionWidth;
            _colourHeight = _calibration.ColorCameraCalibration.ResolutionHeight;
            _depthWidth = _calibration.DepthCameraCalibration.ResolutionWidth;
            _depthHeight = _calibration.DepthCameraCalibration.ResolutionHeight;

            _uiContext = SynchronizationContext.Current;

            _bodyTracker = Tracker.Create(_calibration, new TrackerConfiguration
            {
                ProcessingMode = TrackerProcessingMode.Gpu,
                SensorOrientation = SensorOrientation.Default
            });

            _bodyTracker.SetTemporalSmooting(1);
        
            //Task.Run(() => { ImuCapture(); });
            Task.Run(() => { CameraCapture(); });
        }

        private void CameraCapture()
        {
            while (_applicationIsRunning)
            {
                try
                {
                    using (var capture = _device.GetCapture())
                    {
                        switch (SelectedOutput.OutputType)
                        {
                            case OutputType.SkeletonTracking:
                                //PresentSkeletonTracking(capture);
                                PresentDepthSkeletonTracking(capture);
                                break;                        
                        }

                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentCameraImage"));
                    }
                }
                catch (Exception ex)
                {
                    _applicationIsRunning = false;
                    MessageBox.Show($"An error occurred {ex.Message}");
                }
            }
        }

        private void PresentSkeletonTracking(Capture capture)
        {
            _bodyTracker.EnqueueCapture(capture);

            using (var frame = _bodyTracker.PopResult())
            using (Image outputImage = new Image(ImageFormat.ColorBGRA32, _colourWidth, _colourHeight))
            {
                AddOrUpdateDeviceData("Number of bodies", frame.NumberOfBodies.ToString(), "");

                _uiContext.Send(d =>
                {
                    // Colour camera pixels.
                    var colourBuffer = capture.Color.GetPixels<BGRA>().Span;

                    // What we'll output.
                    var outputBuffer = outputImage.GetPixels<BGRA>().Span;

                    // Create a new image with data from the depth and colour image.
                    for (int i = 0; i < colourBuffer.Length; i++)
                    {
                        // We'll use the colour image if a joint isn't found.
                        outputBuffer[i] = colourBuffer[i];
                    }

                    // Get all of the bodies.
                    for (uint b = 0; b < frame.NumberOfBodies && b < _bodyColours.Length; b++)
                    {
                        var body = frame.GetBody(b);
                        //var colour = _bodyColours[b];

                        foreach (JointId jointType in Enum.GetValues(typeof(JointId)))
                        {
                            if (jointType == JointId.Count)
                            {
                                continue; // This isn't really a joint.
                            }

                            var joint = body.Skeleton.GetJoint(jointType);

                            // Get the position in 2d coords.
                            var jointPosition = _calibration.TransformTo2D(joint.Position, CalibrationDeviceType.Depth, CalibrationDeviceType.Color);

                            AddOrUpdateDeviceData(
                                $"Body: {b + 1} Joint: {Enum.GetName(typeof(JointId), jointType)}",
                                Enum.GetName(typeof(JointConfidenceLevel), joint.ConfidenceLevel),
                                jointPosition.ToString());

                            if (!Dictionary_JointsPoint.Keys.Contains(Enum.GetName(typeof(JointId), jointType)))
                            {
                                Dictionary_JointsPoint.Add(Enum.GetName(typeof(JointId), jointType), jointPosition.ToString());
                            }
                            else
                            {
                                Dictionary_JointsPoint[Enum.GetName(typeof(JointId), jointType)] = jointPosition.ToString();
                            }

                            if (!ViewJoints.Contains(Enum.GetName(typeof(JointId), jointType)))
                                continue;

                            if (jointPosition.HasValue)
                            {
                                // Set a 12x12 pixel value on the buffer.
                                var xR = Convert.ToInt32(Math.Round(Convert.ToDecimal(jointPosition.Value.X)));
                                var yR = Convert.ToInt32(Math.Round(Convert.ToDecimal(jointPosition.Value.Y)));

                                for (int x = xR - minPixel; x < xR + maxPixel; x++)
                                {
                                    for (int y = yR - minPixel; y < yR + maxPixel; y++)
                                    {
                                        if (x > 0 && x < (outputImage.WidthPixels) && y > 0 && (y < outputImage.HeightPixels))
                                        {
                                            outputImage.SetPixel(y, x, SkeletonColour);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    _bitmap = outputImage.CreateBitmapSource();
                    _bitmap.Freeze();
                }, null);
            }
        }

        private void PresentDepthSkeletonTracking(Capture capture)
        {
            _bodyTracker.EnqueueCapture(capture);

            using (var frame = _bodyTracker.PopResult())
            using (Image outputImage = new Image(ImageFormat.ColorBGRA32, _colourWidth, _colourHeight))
            {
                AddOrUpdateDeviceData("Number of bodies", frame.NumberOfBodies.ToString(), "");

                _uiContext.Send(d =>
                {
                    // Colour camera pixels.
                    var colourBuffer = capture.Color.GetPixels<BGRA>().Span;

                    // What we'll output.
                    var outputBuffer = outputImage.GetPixels<BGRA>().Span;

                    // Create a new image with data from the depth and colour image.
                    for (int i = 0; i < colourBuffer.Length; i++)
                    {
                        // We'll use the colour image if a joint isn't found.
                        outputBuffer[i] = colourBuffer[i];
                    }
                    recogBodyNum = (int)frame.NumberOfBodies;
                    // Get all of the bodies.
                    for (uint b = 0; b < frame.NumberOfBodies && b < _bodyColours.Length; b++)
                    {

                        var body = frame.GetBody(b);
                        //var colour = _bodyColours[b];

                        foreach (JointId jointType in Enum.GetValues(typeof(JointId)))
                        {
                            if (jointType == JointId.Count)
                            {
                                continue; // This isn't really a joint.
                            }

                            var joint = body.Skeleton.GetJoint(jointType);


                            if (jointType == JointId.EarLeft)
                            {
                                joint.Position.X = joint.Position.X - 20;
                            }

                            //if (jointType == JointId.Head)
                            //{
                            //    joint.Position.X = joint.Position.X + 55;
                            //}

                            if (jointType == JointId.Neck)
                            {
                                joint.Position.X = joint.Position.X + 80;
                            }

                            //Get the position in 2d coords.
                            var jointPosition = _calibration.TransformTo2D(joint.Position, CalibrationDeviceType.Depth, CalibrationDeviceType.Color);

                            AddOrUpdateDeviceData(
                                $"Body: {b + 1} Joint: {Enum.GetName(typeof(JointId), jointType)}",
                                Enum.GetName(typeof(JointConfidenceLevel), joint.ConfidenceLevel),
                                jointPosition.ToString());

                            if (!Dictionary_JointsPoint.Keys.Contains(Enum.GetName(typeof(JointId), jointType)))
                            {
                                Dictionary_JointsPoint.Add(Enum.GetName(typeof(JointId), jointType), jointPosition.ToString());
                            }
                            else
                            {
                                Dictionary_JointsPoint[Enum.GetName(typeof(JointId), jointType)] = jointPosition.ToString();
                            }

                            if (!ViewJoints.Contains(Enum.GetName(typeof(JointId), jointType)))
                                continue;

                            if (jointPosition.HasValue)
                            {
                                // Set a 12x12 pixel value on the buffer.
                                var xR = Convert.ToInt32(Math.Round(Convert.ToDecimal(jointPosition.Value.X)));
                                var yR = Convert.ToInt32(Math.Round(Convert.ToDecimal(jointPosition.Value.Y)));

                                for (int x = xR - minPixel; x < xR + maxPixel; x++)
                                {
                                    for (int y = yR - minPixel; y < yR + maxPixel; y++)
                                    {
                                        if (x > 0 && x < outputImage.WidthPixels && y > 0 && (y < outputImage.HeightPixels))
                                        {
                                            outputImage.SetPixel(y, x, SkeletonColour);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    _bitmap = outputImage.CreateBitmapSource();
                    _bitmap.Freeze();
                }, null);
            }
        }

        private void AddOrUpdateDeviceData(string key, string value, string position)
        {
            var detail = CameraDetails.FirstOrDefault(i => i.Name == key);

            if (detail == null)
            {
                detail = new CameraDetailItem { Name = key, Value = value, Position = position };
                _uiContext.Send(x => CameraDetails.Add(detail), null);
            }

            detail.Value = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CameraDetails"));

            detail.Position = position;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Position"));
        }

        #endregion Camera control
    }


    public enum OutputType
    {
        Colour,
        Depth,
        IR,
        BodyTracking,
        SkeletonTracking
    }
    public class OutputOption
    {
        public string Name { get; set; }
        public OutputType OutputType { get; set; }
    }
    public class DeviceInformation
    {
        public string SerialNumber { get; set; }
    }
}
