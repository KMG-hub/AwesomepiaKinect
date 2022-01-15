using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Azure.Kinect.BodyTracking;

namespace AwesomepiaKinect
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Utility.KinectViewModel _viewModel;
        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new Utility.KinectViewModel();
            DataContext = _viewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            initCheckBoxList();
            _DeviceOpenned = false;

            initFileList();
        }

        private List<Brush> ButtonColorList = new List<Brush>()
        {
            new SolidColorBrush(Color.FromArgb(0xFF, 0x3D, 0x99, 0x62)),    // Open
            new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x3D, 0x3D)),    // Close
            new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x90, 0x3D)),    // Pause
            new SolidColorBrush(Color.FromArgb(0xFF, 0xA0, 0xA0, 0xA0)),    // Disable
        };

        private bool _DeviceOpenned { get; set; }
        private bool DeviceOpenned
        {
            get { return _DeviceOpenned; }
            set 
            { 
                _DeviceOpenned = value;

                if (_DeviceOpenned)
                {
                    button_Open.Background = ButtonColorList[1];
                    button_Open.Content = "Close";
                }
                else
                {
                    button_Open.Background = ButtonColorList[0];
                    button_Open.Content = "Open";
                }
            }
        }

        private void button_Open_Click(object sender, RoutedEventArgs e)
        {
            if (DeviceOpenned == false)
            {
                _viewModel.StartCamera();
            }
            else
            {
                _viewModel.StopCamera();
                image_kinect.Source = null;
            }
            DeviceOpenned = !DeviceOpenned;
        }

        private void checkbox_Joints_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;
            string name = checkbox.Content.ToString();
            if (checkbox.IsChecked == true)
                _viewModel.ViewJoints.Add(name);
            else
            {
                if (_viewModel.ViewJoints.Contains(name))
                    _viewModel.ViewJoints.Remove(name);
            }
        }

        private void initCheckBoxList()
        {
            foreach (JointId jointType in Enum.GetValues(typeof(JointId)))
            {
                string name = Enum.GetName(typeof(JointId), jointType);
                if (name == "Count")
                    continue;
                CheckBox checkBox = new CheckBox();
                checkBox.Content = name;
                checkBox.Foreground = Brushes.White;
                checkBox.Checked += checkbox_Joints_Checked;
                checkBox.Unchecked += checkbox_Joints_Checked;
                stackpanel_Joints.Children.Add(checkBox);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string stringValue = (sender as ComboBox).SelectedValue.ToString();
            int intValue = Convert.ToInt32(stringValue.Remove(0, 38));
            if (_viewModel is null)
                return;
            _viewModel.SkeletonPixelSize = intValue;
            Debug.WriteLine(intValue);
        }

        private bool _IsCapture = false;
        private bool IsCapture
        {
            get { return _IsCapture; }
            set
            {
                _IsCapture = value;

                if (_IsCapture)
                {
                    button_Capture.Background = ButtonColorList[0];
                    button_Capture.Content = "Capture";
                }
                else
                {
                    button_Capture.Background = ButtonColorList[3];
                    button_Capture.Content = "Capturing...";
                }
            }
        }
        private void button_Capture_Click(object sender, RoutedEventArgs e)
        {
            if (IsCapture)
            {
                string tempDuration = combobox_Duration.Text;
                string tempInterval = combobox_Interval.Text;

                Task.Run(() => Capturing(Convert.ToDouble(tempDuration.Replace("seconds", "")), Convert.ToDouble(tempInterval.Replace("seconds", ""))));
            }
            else
            {
                IsCapture = true;
            }
        }

        string folderPath = "";
        private void Capturing(double duration, double interval)
        {
            duration = duration * 1000;
            interval = interval * 1000;


            folderPath = Environment.CurrentDirectory +  "/Capture" + DateTime.Now.ToString("yyMMdd_HHmmss");
            DirectoryInfo di = new DirectoryInfo(folderPath);
            
            if (!di.Exists)
                di.Create();

            int capture_cnt = 0;

            Stopwatch interval_sw = new Stopwatch();
            Stopwatch duration_sw = new Stopwatch();
            interval_sw.Start();
            duration_sw.Start();
            while (duration_sw.IsRunning)
            {
                if (interval_sw.ElapsedMilliseconds > interval && duration_sw.ElapsedMilliseconds <= duration )
                {
                    interval_sw.Stop();
                    duration_sw.Stop();

                    // To Do...
                    capture_cnt++;

                    Task.Run(() => Capture(capture_cnt.ToString("0000") + ".png", folderPath + "/"));

                    //Dispatcher.Invoke(() => Capture(folderPath + "/", capture_cnt.ToString("0000") + ".png"));


                    interval_sw.Reset();

                    interval_sw.Start();
                    duration_sw.Start();
                }
                else if (duration_sw.ElapsedMilliseconds > duration)
                {
                    interval_sw.Stop();
                    duration_sw.Stop();
                }
            }
        }

        private void Capture(string filename, string filepath, PngInterlaceOption pngInterlaceOption = PngInterlaceOption.Default)
        {
            BitmapSource source = null;
            Dispatcher.Invoke(() =>
            {
                
                source = image_kinect.Source.Clone() as BitmapSource;
                if (source == null)
                    return;
              
            });

            string filePath = filepath + "/" + filename;
            PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
            pngBitmapEncoder.Interlace = pngInterlaceOption;
            pngBitmapEncoder.Frames.Add(BitmapFrame.Create(source));

            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                pngBitmapEncoder.Save(fileStream);
            }

        }



        Dictionary<string, Button> ButtonDictionary = new Dictionary<string, Button>();
        private void initFileList()
        {
            List<string> CaptureDirectoryList = new List<string>();
            foreach (var item in Directory.GetDirectories(Environment.CurrentDirectory).Reverse())
            {
                
                if (!item.Contains("Capture"))
                    continue;
                
                CaptureDirectoryList.Add(item);

                StackPanel stackPanel = new StackPanel();
                stackPanel.Orientation = Orientation.Vertical;

               

                Image image = new Image();
                image.Source = new BitmapImage(new Uri("./Resources/icon_folder.png", UriKind.RelativeOrAbsolute));
                image.Stretch = Stretch.None;
                stackPanel.Children.Add(image);


                TextBlock textBlock = new TextBlock();
                textBlock.Text = item.Split("\\").Last();
                textBlock.Foreground = Brushes.White;
                stackPanel.Children.Add(textBlock);


                Button button = new Button();
                button.Margin = new Thickness(5, 0, 5, 10);
                button.BorderThickness = new Thickness(0);
                button.Content = stackPanel;
                button.Background = null;
                ButtonDictionary.Add(textBlock.Text, button);
                button.Click += Button_Click;

                folderViewer.Children.Add(button);
            }
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            if (!ButtonDictionary.ContainsValue(btn))
                return;

            var key = ButtonDictionary.FirstOrDefault(x => x.Value == btn).Key;
            initPictureList(key);
        }


        Dictionary<string, Button> ImageButtonDictionary = new Dictionary<string, Button>();
        private void initPictureList(string foldername)
        {
            folderViewer.Children.Clear();
            ImageButtonDictionary.Clear();
            var imagepath = Environment.CurrentDirectory + "\\" + foldername;

            Button backbtn = new Button();
            backbtn.Content = "Back";
            backbtn.Click += BackButton_Click;
            backbtn.Width = 80;
            backbtn.Height = 30;
            backbtn.Foreground = Brushes.White;
            backbtn.Background = ButtonColorList[2];
            backbtn.Margin = new Thickness(0, 0, 0, 10);
            folderViewer.Children.Add(backbtn);

            foreach (var item in Directory.GetFiles(imagepath))
            {

                if (!item.Contains(".png"))
                    continue;
               

                StackPanel stackPanel = new StackPanel();
                stackPanel.Orientation = Orientation.Vertical;
                

                Image image = new Image();
                image.Source = new BitmapImage(new Uri(item, UriKind.RelativeOrAbsolute));
                image.Width = 80;
                image.Height = 45;
                image.Stretch = Stretch.Uniform;
                stackPanel.Children.Add(image);


                TextBlock textBlock = new TextBlock();
                textBlock.Text = item.Split("\\").Last();
                textBlock.Foreground = Brushes.White;
                stackPanel.Children.Add(textBlock);


                Button button = new Button();
                button.Margin = new Thickness(5, 0, 5, 10);
                button.BorderThickness = new Thickness(0);
                button.Content = stackPanel;
                button.Background = null;
                ImageButtonDictionary.Add(item, button);
                button.Click += ImageButton_Click;

                folderViewer.Children.Add(button);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            folderViewer.Children.Clear();
            ButtonDictionary.Clear();
            initFileList();
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            if (!ImageButtonDictionary.ContainsValue(btn))
                return;

            var key = ImageButtonDictionary.FirstOrDefault(x => x.Value == btn).Key;

            image_kinect.Source = new BitmapImage(new Uri(key, UriKind.RelativeOrAbsolute)); ;

            Debug.WriteLine(key);
            //initPictureList(key);
        }
    }
}
