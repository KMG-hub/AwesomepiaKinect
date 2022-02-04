using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Azure.Kinect.BodyTracking;


namespace AwesomepiaKinect
{
    /// <summary>
    /// StartWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
            initFileList();

            Debug.WriteLine("Debug WriteLine");
            System.Console.WriteLine("Console WriteLine");
            Trace.WriteLine("Trace WriteLine");

            combobox_firstitem.ItemsSource = Enum.GetValues(typeof(JointId));
            combobox_seconditem.ItemsSource = Enum.GetValues(typeof(JointId));
            combobox_firstitem.SelectedIndex = 0;
            combobox_seconditem.SelectedIndex = 0;

            //combobox_ratio_firstitem.ItemsSource = Enum.GetValues(typeof(JointId));
            //combobox_ratio_seconditem.ItemsSource = Enum.GetValues(typeof(JointId));
            //combobox_ratio_firstitem.SelectedIndex = 0;
            //combobox_ratio_seconditem.SelectedIndex = 0;
        }

        private void button_Open_Click(object sender, RoutedEventArgs e)
        {
            _viewModel = new Utility.KinectViewModel();
            DataContext = _viewModel;

            IsConnected = true;
        }

        private void button_Close_Click(object sender, RoutedEventArgs e)
        {
            IsConnected = false;
        }

        private void button_Joints_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            string name = btn.Name.Replace("button_", "");
            if (btn.Background.ToString() == ButtonColorList[1].ToString())
            {
                btn.Background = ButtonColorList[0];

                if (!_viewModel.ViewJoints.Contains(name))
                    _viewModel.ViewJoints.Add(name);
            }
            else if (btn.Background.ToString() == ButtonColorList[0].ToString())
            {
                btn.Background = ButtonColorList[1];
                if (_viewModel.ViewJoints.Contains(name))
                    _viewModel.ViewJoints.Remove(name);
            }
        }

        private List<Brush> ButtonColorList = new List<Brush>()
        {
            new SolidColorBrush(Color.FromArgb(0xFF, 0x3D, 0x99, 0x62)),    // Open
            new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x3D, 0x3D)),    // Close
            new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x90, 0x3D)),    // Pause
            new SolidColorBrush(Color.FromArgb(0xFF, 0xA0, 0xA0, 0xA0)),    // Disable
        };

        Utility.KinectViewModel _viewModel;

        private bool _IsConnected;
        private bool IsConnected
        {
            get { return _IsConnected; }
            set
            {
                _IsConnected = value;
                if (_IsConnected)
                {
                    IsLoading = true;

                    _viewModel.StartCamera();
                    IsLoading = false;
                    grid_Connect.Visibility = Visibility.Hidden;
                }
                else
                {
                    _viewModel.StopCamera();
                    grid_Connect.Visibility = Visibility.Visible;
                }
            }
        }

        private bool _IsLoading;
        private bool IsLoading
        {
            get { return _IsLoading; }
            set
            {
                // in the case of already value is true
                if (_IsLoading == true && value == true)
                {
                    return;
                }

                _IsLoading = value;

                if (_IsLoading)
                {
                    grid_loading.Visibility = Visibility.Visible;
                }
                else
                {
                    grid_loading.Visibility = Visibility.Hidden;
                }

            }
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
                    button_capture.IsEnabled = false;
                }
                else
                {
                    button_capture.IsEnabled = true;
                }

            }
        }
        private void combobox_Pixelsize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.SelectedItem is null)
                return;

            if (IsConnected is false)
                return;

            _viewModel.SkeletonPixelSize = Convert.ToInt32((comboBox.SelectedItem as ComboBoxItem).Content);
        }

        private void combobox_Time_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { 

        }
        private void combobox_Interval_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void combobox_Pixelcolor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.SelectedItem is null)
                return;
            if (IsConnected is false)
                return;

            switch ((comboBox.SelectedItem as ComboBoxItem).Content)
            {
                case "Blue":
                    _viewModel.SkeletonPixelColor = Utility.KinectViewModel.PixelColor.Blue;
                    break;
                case "Black":
                    _viewModel.SkeletonPixelColor = Utility.KinectViewModel.PixelColor.Black;
                    break;
                case "White":
                    _viewModel.SkeletonPixelColor = Utility.KinectViewModel.PixelColor.White;
                    break;
                case "Red":
                    _viewModel.SkeletonPixelColor = Utility.KinectViewModel.PixelColor.Red;
                    break;
                case "Yellow":
                    _viewModel.SkeletonPixelColor = Utility.KinectViewModel.PixelColor.Yellow;
                    break;
                case "Gray":
                    _viewModel.SkeletonPixelColor = Utility.KinectViewModel.PixelColor.Gray;
                    break;
            }
        }
        private void button_capture_Click(object sender, RoutedEventArgs e)
        {
            IsCapture = !IsCapture;

            if (IsCapture)
            {
                string tempDuration = combobox_Time.Text;
                string tempInterval = combobox_Interval.Text;

                Task.Run(() => Capturing(Convert.ToDouble(tempDuration.Replace("seconds", "")), Convert.ToDouble(tempInterval.Replace("seconds", ""))));
            }
        }


        DataTable dataTable;

        string folderPath = "";
        private void Capturing(double duration, double interval)
        {
            duration = duration * 1000;
            interval = interval * 1000;


            folderPath = Environment.CurrentDirectory + "/Capture" + DateTime.Now.ToString("yyMMdd_HHmmss");
            DirectoryInfo di = new DirectoryInfo(folderPath);

            if (!di.Exists)
                di.Create();


            dataTable = new DataTable();
            foreach (var item in Enum.GetValues(typeof(JointId)))
            {
                dataTable.Columns.Add(item.ToString());
            }

           

            int capture_cnt = 0;

            Stopwatch interval_sw = new Stopwatch();
            Stopwatch duration_sw = new Stopwatch();
            interval_sw.Start();
            duration_sw.Start();
            while (duration_sw.IsRunning)
            {
                if (interval_sw.ElapsedMilliseconds > interval && duration_sw.ElapsedMilliseconds <= duration)
                {
                    interval_sw.Stop();
                    duration_sw.Stop();

                    // To Do...
                    capture_cnt++;

                    Task.Run(() => Capture(capture_cnt.ToString("0000") + ".png", folderPath + "/"));
                    //Dispatcher.Invoke(() => Capture(folderPath + "/", capture_cnt.ToString("0000") + ".png"));
                   
                    DataRow dt = dataTable.NewRow();
                    foreach (var key in _viewModel.Dictionary_JointsPoint.Keys)
                    {
                        dt[key] = _viewModel.Dictionary_JointsPoint[key];
                        
                    }
                    dataTable.Rows.Add(dt);

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
            DataSave(dataTable);
            Dispatcher?.Invoke(() =>
            {
                IsCapture = false;
            });
        }
        private void Capture(string filename, string filepath, PngInterlaceOption pngInterlaceOption = PngInterlaceOption.Default)
        {
            BitmapSource source = null;
            ImageSource image;
            Dispatcher?.Invoke(() =>
            {
                image = image_kinect.Source.Clone();
                source = image.Clone() as BitmapSource;
                source.Freeze();
            });
            
            if (source == null)
                return;

            string filePath = filepath + "/" + filename;
            PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
            pngBitmapEncoder.Interlace = pngInterlaceOption;
            pngBitmapEncoder.Frames.Add(BitmapFrame.Create(source));
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                pngBitmapEncoder.Save(fileStream);
            }
        }

        private void button_listfresh_Click(object sender, RoutedEventArgs e)
        {
            ButtonDictionary.Clear();
            stackpanel_folder.Children.Clear();
            initFileList();
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

                stackpanel_folder.Children.Add(button);
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            if (!ButtonDictionary.ContainsValue(btn))
                return;

            var key = ButtonDictionary.FirstOrDefault(x => x.Value == btn).Key;
            textblock_selected.Text = key;
            initPictureList(key);
        }

        Dictionary<string, Button> ImageButtonDictionary = new Dictionary<string, Button>();
        private void initPictureList(string foldername)
        {
            stackpanel_image.Children.Clear();
            ImageButtonDictionary.Clear();
            var imagepath = Environment.CurrentDirectory + "\\" + foldername;


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

                stackpanel_image.Children.Add(button);
            }
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        { 
            Button btn = sender as Button;

            if (!ImageButtonDictionary.ContainsValue(btn))
                return;

            var key = ImageButtonDictionary.FirstOrDefault(x => x.Value == btn).Key;
           
            image_saved.Source = new BitmapImage(new Uri(key, UriKind.RelativeOrAbsolute)); ;

            Debug.WriteLine(key);


            

            initJointViewList(key.Substring(0, key.Length - key.Split("\\").Last().Length), Convert.ToInt32(key.Split("\\").Last().Replace(".png", "")));
            //initPictureList(key);
        }

        private void DataSave(DataTable dataTable)
        {
            using (StreamWriter file = new StreamWriter(folderPath + "/data.txt"))
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    
                    string temp = "";
                    foreach (var item in row.ItemArray)
                    {
                        temp += item.ToString() + "/";
                    }
                    file.WriteLine(temp);
                }
            }
        }


        string[] viewdata;
        List<Point> jointPoints = new List<Point>();
        string[] selectedpoints;
        private void initJointViewList(string path, int row)
        {
            int cnt = 0;
            ReadTextFile(path);
            string[] points = viewdata[row -1].Split("/");
            selectedpoints = points;

            statckpanel_viewjoints.Children.Clear();
            jointPoints.Clear();
            foreach (JointId item in Enum.GetValues(typeof(JointId)))
            {
                if ((JointId)item == JointId.Count)
                    continue;

                cnt++;
                StackPanel stackPanel = new StackPanel();
                stackPanel.Orientation = Orientation.Horizontal;
                stackPanel.Margin = new Thickness(0, 5, 0, 0);
                stackPanel.VerticalAlignment = VerticalAlignment.Center;

                TextBlock tb_no = new TextBlock();
                tb_no.Width = 30;
                tb_no.FontSize = 14;
                tb_no.Foreground = Brushes.White;
                tb_no.TextAlignment = TextAlignment.Center;
                tb_no.Text = cnt.ToString("00");
                stackPanel.Children.Add(tb_no);

                CheckBox cb = new CheckBox();
                cb.Name = "cb" + (cnt - 1).ToString();
                cb.Width = 120;
                cb.FontSize = 14;
                cb.Foreground = Brushes.White;
                cb.VerticalContentAlignment = VerticalAlignment.Center;
                cb.Content = item.ToString();
                
                stackPanel.Children.Add(cb);

                TextBlock tb_point = new TextBlock();
                tb_point.FontSize = 14;
                tb_point.Foreground = Brushes.White;
                tb_point.TextAlignment = TextAlignment.Center;
                tb_point.Text = points[cnt - 1];
                if (string.IsNullOrEmpty(tb_point.Text))
                {
                    jointPoints.Add(new Point(0, 0));
                }
                else
                {
                    string value = tb_point.Text.Replace("<", "").Replace(">", "");
                    Point pointValue = Point.Parse(value);
                    jointPoints.Add(pointValue);

                    canvas_draw.Children.Clear();

                    if (item == JointId.EarLeft)
                    {
                        //PointEarleft = new Point(pointValue.X - 20, pointValue.Y);
                        PointEarleft = pointValue;
                        textblock_tnfirst.Text = value;
                    }
                    else if (item == JointId.ShoulderLeft)
                    {
                        PointShoulderleft = pointValue;
                        textblock_tnsecond.Text = value;
                    }
                    else if (item == JointId.Neck)
                    {
                        PointNeck = pointValue;
                        textblock_bkfirst.Text = value;
                    }
                    else if (item == JointId.Pelvis)
                    {
                        PointPelvis = pointValue;
                        textblock_bksecond.Text = value;
                    }
                }

                stackPanel.Children.Add(tb_point);

                cb.Checked += Cb_Checked;
                cb.Unchecked += Cb_Unchecked;

                statckpanel_viewjoints.Children.Add(stackPanel);
            }
            textblock_tnresult.Text = Math.Abs(PointEarleft.X - PointShoulderleft.X).ToString();

            var PointPerpen = new Point(PointPelvis.X, PointNeck.Y);
            var thetaa = Math.Atan2(PointPerpen.Y - PointPelvis.Y * 2, PointPerpen.X - PointPelvis.X * 2);
            var thetab = Math.Atan2(PointNeck.Y - PointPelvis.Y * 2, PointNeck.X - PointPelvis.X * 2);

            var theta = thetaa - thetab;

            theta = theta * 180 / Math.PI;

            //if (theta < 0)
            //    theta = 360 + theta;



            textblock_bkresult.Text = theta.ToString("00.00 ˚");
        }
        private void Cb_Checked(object sender , RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb is null)
            {
                Debug.WriteLine("CheckBox is null !");
                return;
            }

            Debug.WriteLine(jointPoints[Convert.ToInt32(cb.Name.Remove(0, 2))]);
        }

        private void Cb_Unchecked(object sender, RoutedEventArgs e)
        {
           
        }

        private void ReadTextFile(string path)
        {
            viewdata = File.ReadAllLines(path + "data.txt");
        }

        Ellipse selectPoint = new Ellipse()
        {
            Fill = Brushes.Yellow,
            Width = 10,
            Height = 10
        };
        Point PointSelect = new Point();
        Point PointFirst = new Point();
        Point PointSecond = new Point();
        Point PointRatioFirst = new Point();
        Point PointRatioSecond = new Point();


        Point PointEarleft = new Point();
        Point PointShoulderleft = new Point();
        Point PointNeck = new Point();
        Point PointPelvis = new Point();

        private void canvas_draw_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(sender as Canvas);

            textblock_selectpoint.Text = $"({point.X * 2.0}, {point.Y * 2.0})";

            if (!canvas_draw.Children.Contains(selectPoint))
                canvas_draw.Children.Add(selectPoint);

            PointSelect = point;

            Canvas.SetLeft(selectPoint, point.X - selectPoint.Width / 2);
            Canvas.SetTop(selectPoint, point.Y - selectPoint.Height / 2);

            Firstline.X2 = PointSelect.X;
            Firstline.Y2 = PointSelect.Y;

            Secondline.X2 = PointSelect.X;
            Secondline.Y2 = PointSelect.Y;
            UpdateAngle();
            UpdateRatio();
        }

        Ellipse FirstSelectPoint = new Ellipse()
        {
            Fill = Brushes.Cyan,
            Width = 10,
            Height = 10
        };
        Line Firstline = new Line()
        {
            Stroke = Brushes.Yellow,
            StrokeThickness = 2,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeDashArray = new DoubleCollection() { 1, 1 },
        };
        private void combobox_firstitem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selectedpoints is null || selectedpoints.Length == 0 || combobox_firstitem.SelectedIndex == 32)
                return;

            if (!canvas_draw.Children.Contains(FirstSelectPoint))
                canvas_draw.Children.Add(FirstSelectPoint);


            if (!canvas_draw.Children.Contains(Firstline))
                canvas_draw.Children.Add(Firstline);


            var point = Point.Parse(selectedpoints[combobox_firstitem.SelectedIndex].Replace("<", "").Replace(">", ""));
            PointFirst = point;

            Firstline.X1 = point.X /2 ;
            Firstline.X2 = PointSelect.X;

            Firstline.Y1 = point.Y /2 ;
            Firstline.Y2 = PointSelect.Y;

            Canvas.SetLeft(FirstSelectPoint, (point.X/2) - (FirstSelectPoint.Width / 2));
            Canvas.SetTop(FirstSelectPoint, (point.Y/2) - (FirstSelectPoint.Height / 2));

            textblock_firstitem.Text = point.ToString();
            UpdateAngle();
            UpdateRatio();
        }
        Ellipse SecondSelectPoint = new Ellipse()
        {
            Fill = Brushes.Purple,
            Width = 10,
            Height = 10
        };
        Line Secondline = new Line()
        {
            Stroke = Brushes.Yellow,
            StrokeThickness = 2,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeDashArray = new DoubleCollection() { 1, 1 },
        };
        private void combobox_seconditem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selectedpoints is null || selectedpoints.Length == 0 || combobox_seconditem.SelectedIndex == 32)
                return;

            if (!canvas_draw.Children.Contains(SecondSelectPoint))
                canvas_draw.Children.Add(SecondSelectPoint);

            if (!canvas_draw.Children.Contains(Secondline))
                canvas_draw.Children.Add(Secondline);

            var point = Point.Parse(selectedpoints[combobox_seconditem.SelectedIndex].Replace("<", "").Replace(">", ""));
            PointSecond = point;

            Secondline.X1 = point.X /2 ;
            Secondline.X2 = PointSelect.X;

            Secondline.Y1 = point.Y /2 ;
            Secondline.Y2 = PointSelect.Y;

            Canvas.SetLeft(SecondSelectPoint, (point.X/2) - (SecondSelectPoint.Width / 2));
            Canvas.SetTop(SecondSelectPoint, (point.Y/2) - (SecondSelectPoint.Height / 2));

            textblock_seconditem.Text = point.ToString();
            UpdateAngle();
            UpdateRatio();
        }

        private void UpdateAngle()
        {
            var thetaa = Math.Atan2(PointFirst.Y - PointSelect.Y * 2, PointFirst.X - PointSelect.X * 2);
            var thetab = Math.Atan2(PointSecond.Y - PointSelect.Y * 2, PointSecond.X - PointSelect.X * 2);

            var theta = thetaa - thetab;

            theta = theta * 180 / Math.PI;

            if (theta < 0)
                theta = 360 + theta;

            textblock_angleview.Text = theta.ToString("00.00 도");
        }

        private void UpdateRatio()
        {  
            var distancea = Math.Abs(PointFirst.X - (PointSelect.X * 2));
            var distanceb = Math.Abs(PointSecond.X - (PointSelect.X * 2));

            if (distancea == 0 || distanceb == 0)
                return;

            textblock_ratioview.Text = $"{distancea.ToString("0.00")} : {distanceb.ToString("0.00")} -> {(distancea / distanceb).ToString("0.00")} ";
        }



        Ellipse ellipse_tnfirst = new Ellipse()
        {
            Fill = Brushes.Purple,
            Width = 10,
            Height = 10
        };

        Ellipse ellipse_tnsecond = new Ellipse()
        {
            Fill = Brushes.DarkCyan,
            Width = 10,
            Height = 10
        };

        Line line_tn_horizontal = new Line()
        {
            Stroke = Brushes.Red,
            StrokeThickness = 2,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeStartLineCap = PenLineCap.Round,
            //StrokeDashArray = new DoubleCollection() { 1, 1 },
        };
        Line line_tn_vertical = new Line()
        {
            Stroke = Brushes.Gray,
            StrokeThickness = 2,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeDashArray = new DoubleCollection() { 1, 1 },
        };

        private bool tnview_flag = false;
        private void button_tnview_Click(object sender, RoutedEventArgs e)
        {
            tnview_flag = !tnview_flag;

            Button button = (Button)sender;

            if (!canvas_draw.Children.Contains(ellipse_tnfirst))
            {
                canvas_draw.Children.Add(ellipse_tnfirst);
                Canvas.SetLeft(ellipse_tnfirst, PointEarleft.X / 2 - ellipse_tnfirst.Width / 2);
                Canvas.SetTop(ellipse_tnfirst, PointEarleft.Y / 2 - ellipse_tnfirst.Height / 2);
            }
            if (!canvas_draw.Children.Contains(ellipse_tnsecond))
            {
                canvas_draw.Children.Add(ellipse_tnsecond);
                Canvas.SetLeft(ellipse_tnsecond, PointShoulderleft.X / 2 - ellipse_tnsecond.Width / 2);
                Canvas.SetTop(ellipse_tnsecond, PointShoulderleft.Y / 2 - ellipse_tnsecond.Height / 2);
            }
            if (!canvas_draw.Children.Contains(line_tn_horizontal))
            {
                canvas_draw.Children.Add(line_tn_horizontal);
                line_tn_horizontal.X1 = PointEarleft.X / 2;
                line_tn_horizontal.Y1 = PointShoulderleft.Y / 2;
                line_tn_horizontal.X2 = PointShoulderleft.X / 2;
                line_tn_horizontal.Y2 = PointShoulderleft.Y / 2;
            }
            if (!canvas_draw.Children.Contains(line_tn_vertical))
            {
                canvas_draw.Children.Add(line_tn_vertical);
                line_tn_vertical.X1 = PointEarleft.X / 2;
                line_tn_vertical.Y1 = PointEarleft.Y / 2;
                line_tn_vertical.X2 = PointEarleft.X / 2;
                line_tn_vertical.Y2 = PointShoulderleft.Y / 2;
            }

            if (tnview_flag)
            {
                button.Content = "끄기";
                ellipse_tnfirst.Visibility = Visibility.Visible;
                ellipse_tnsecond.Visibility = Visibility.Visible;
                line_tn_horizontal.Visibility = Visibility.Visible;
                line_tn_vertical.Visibility = Visibility.Visible;
            }
            else
            {
                button.Content = "켜기";
                ellipse_tnfirst.Visibility = Visibility.Hidden;
                ellipse_tnsecond.Visibility = Visibility.Hidden;
                line_tn_horizontal.Visibility = Visibility.Hidden;
                line_tn_vertical.Visibility = Visibility.Hidden;
            }

        }


        Ellipse ellipse_bkfirst = new Ellipse()
        {
            Fill = Brushes.Purple,
            Width = 10,
            Height = 10
        };

        Ellipse ellipse_bksecond = new Ellipse()
        {
            Fill = Brushes.DarkCyan,
            Width = 10,
            Height = 10
        };

        Line line_bk_vertical = new Line()
        {
            Stroke = Brushes.Gray,
            StrokeThickness = 2,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeDashArray = new DoubleCollection() { 1, 1 },
        };
        Line line_bk_neckpelvis = new Line()
        {
            Stroke = Brushes.Red,
            StrokeThickness = 2,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeStartLineCap = PenLineCap.Round,
            //StrokeDashArray = new DoubleCollection() { 1, 1 },
        };
        private bool bkview_flag = false;
        private void button_bkview_Click(object sender, RoutedEventArgs e)
        {
            bkview_flag = !bkview_flag;
            Button button = (Button)sender;

            if (!canvas_draw.Children.Contains(ellipse_bkfirst))
            {
                canvas_draw.Children.Add(ellipse_bkfirst);
                Canvas.SetLeft(ellipse_bkfirst, PointNeck.X / 2 - ellipse_bkfirst.Width / 2);
                Canvas.SetTop(ellipse_bkfirst, PointNeck.Y / 2 - ellipse_bkfirst.Height / 2);
            }
            if (!canvas_draw.Children.Contains(ellipse_bksecond))
            {
                canvas_draw.Children.Add(ellipse_bksecond);
                Canvas.SetLeft(ellipse_bksecond, PointPelvis.X / 2 - ellipse_bksecond.Width / 2);
                Canvas.SetTop(ellipse_bksecond, PointPelvis.Y / 2 - ellipse_bksecond.Height / 2);
            }
            if (!canvas_draw.Children.Contains(line_bk_vertical))
            {
                canvas_draw.Children.Add(line_bk_vertical);
                line_bk_vertical.X1 = PointPelvis.X / 2;
                line_bk_vertical.Y1 = PointNeck.Y / 2;
                line_bk_vertical.X2 = PointPelvis.X / 2;
                line_bk_vertical.Y2 = PointPelvis.Y / 2;
            }
            if (!canvas_draw.Children.Contains(line_bk_neckpelvis))
            {
                canvas_draw.Children.Add(line_bk_neckpelvis);
                line_bk_neckpelvis.X1 = PointNeck.X / 2;
                line_bk_neckpelvis.Y1 = PointNeck.Y / 2;
                line_bk_neckpelvis.X2 = PointPelvis.X / 2;
                line_bk_neckpelvis.Y2 = PointPelvis.Y / 2;
            }

            if (bkview_flag)
            {
                button.Content = "끄기";
                ellipse_bkfirst.Visibility = Visibility.Visible;
                ellipse_bksecond.Visibility = Visibility.Visible;
                line_bk_vertical.Visibility = Visibility.Visible;
                line_bk_neckpelvis.Visibility = Visibility.Visible;
            }
            else
            {
                button.Content = "켜기";
                ellipse_bkfirst.Visibility = Visibility.Hidden;
                ellipse_bksecond.Visibility = Visibility.Hidden;
                line_bk_vertical.Visibility = Visibility.Hidden;
                line_bk_neckpelvis.Visibility = Visibility.Hidden;
            }
        }
    }
}