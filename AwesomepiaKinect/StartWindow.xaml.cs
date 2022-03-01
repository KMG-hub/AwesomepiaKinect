//#define origin

#if origin
#region origin
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
                        textblock_rtfirst.Text = value;
                    }
                    else if (item == JointId.Pelvis)
                    {
                        PointPelvis = pointValue;
                        textblock_bksecond.Text = value;
                        textblock_rtsecond.Text = value;
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

            textblock_rtthird.Text = $"{point.X * 2}, {point.Y * 2}";

            Firstline.X2 = PointSelect.X;
            Firstline.Y2 = PointSelect.Y;

            Secondline.X2 = PointSelect.X;
            Secondline.Y2 = PointSelect.Y;
            UpdateAngle();
            UpdateRatio(PointSelect);
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
            UpdateRatio(point);
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
            UpdateRatio(PointSelect);
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

        private void UpdateRatio(Point point)
        {

            if (!canvas_draw.Children.Contains(line_rt_sacrum))
            {
                canvas_draw.Children.Add(line_rt_sacrum);
            }

            line_rt_sacrum.X1 = point.X;
            line_rt_sacrum.Y1 = PointNeck.Y / 2;

            line_rt_sacrum.X2 = point.X;
            line_rt_sacrum.Y2 = point.Y;
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


        Ellipse ellipse_rtfirst = new Ellipse()
        {
            Fill = Brushes.Purple,
            Width = 10,
            Height = 10
        };
        Ellipse ellipse_rtsecond = new Ellipse()
        {
            Fill = Brushes.DarkCyan,
            Width = 10,
            Height = 10
        };
        //Ellipse ellipse_rtthird = new Ellipse()
        //{
        //    Fill = Brushes.DarkOrange,
        //    Width = 10,
        //    Height = 10
        //};
        Line line_rt_c7 = new Line()
        {
            Stroke = Brushes.Yellow,
            StrokeThickness = 2,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeDashArray = new DoubleCollection() { 1, 1 },
        };
        Line line_rt_pelvis = new Line()
        {
            Stroke = Brushes.Yellow,
            StrokeThickness = 2,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeDashArray = new DoubleCollection() { 1, 1 },
        };
        Line line_rt_sacrum = new Line()
        {
            Stroke = Brushes.Yellow,
            StrokeThickness = 2,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeDashArray = new DoubleCollection() { 1, 1 },
        };

        private bool rtview_flag = false;
        private void button_rtview_Click(object sender, RoutedEventArgs e)
        {
            rtview_flag = !rtview_flag;
            Button button = (Button)sender;

            if (!canvas_draw.Children.Contains(ellipse_rtfirst))
            {
                canvas_draw.Children.Add(ellipse_rtfirst);
                Canvas.SetLeft(ellipse_rtfirst, PointNeck.X / 2 - ellipse_rtfirst.Width / 2);
                Canvas.SetTop(ellipse_rtfirst, PointNeck.Y / 2 - ellipse_rtfirst.Height / 2);
            }
            if (!canvas_draw.Children.Contains(ellipse_rtsecond))
            {
                canvas_draw.Children.Add(ellipse_rtsecond);
                Canvas.SetLeft(ellipse_rtsecond, PointPelvis.X / 2 - ellipse_rtsecond.Width / 2);
                Canvas.SetTop(ellipse_rtsecond, PointPelvis.Y / 2 - ellipse_rtsecond.Height / 2);
            }
            //if (!canvas_draw.Children.Contains(ellipse_rtthird))
            //{
            //    canvas_draw.Children.Add(ellipse_rtthird);
            //    Canvas.SetLeft(ellipse_rtthird, Canvas.GetLeft(selectPoint) - ellipse_rtthird.Width / 2);
            //    Canvas.SetTop(ellipse_rtthird, Canvas.GetTop(selectPoint) - ellipse_rtthird.Height / 2);
            //}

            if (!canvas_draw.Children.Contains(line_rt_c7))
            {
                canvas_draw.Children.Add(line_rt_c7);
                line_rt_c7.X1 = PointNeck.X / 2;
                line_rt_c7.Y1 = PointNeck.Y / 2;

                line_rt_c7.X2 = PointNeck.X / 2;
                if (PointPelvis.Y / 2 < Canvas.GetTop(selectPoint))
                    line_rt_c7.Y2 = Canvas.GetTop(selectPoint);
                else
                    line_rt_c7.Y2 = PointPelvis.Y / 2;
            }
            if (!canvas_draw.Children.Contains(line_rt_pelvis))
            {
                canvas_draw.Children.Add(line_rt_pelvis);

                line_rt_pelvis.X1 = PointPelvis.X / 2;
                line_rt_pelvis.Y1 = PointNeck.Y / 2;

                line_rt_pelvis.X2 = PointPelvis.X / 2;
                line_rt_pelvis.Y2 = PointPelvis.Y / 2;
            }

            if (!canvas_draw.Children.Contains(line_rt_sacrum))
            {
                canvas_draw.Children.Add(line_rt_sacrum);

                line_rt_sacrum.X1 = Canvas.GetLeft(selectPoint);
                line_rt_sacrum.Y1 = PointNeck.Y / 2;

                line_rt_sacrum.X2 = Canvas.GetLeft(selectPoint);
                line_rt_sacrum.Y2 = Canvas.GetTop(selectPoint);
            }


            if (rtview_flag)
            {
                button.Content = "끄기";
                ellipse_rtfirst.Visibility = Visibility.Visible;
                ellipse_rtsecond.Visibility = Visibility.Visible;
                //ellipse_rtthird.Visibility = Visibility.Visible;

                line_rt_c7.Visibility = Visibility.Visible;
                line_rt_pelvis.Visibility = Visibility.Visible;
                line_rt_sacrum.Visibility = Visibility.Visible;
            }
            else
            {
                button.Content = "켜기";
                ellipse_rtfirst.Visibility = Visibility.Hidden;
                ellipse_rtsecond.Visibility = Visibility.Hidden;
                //ellipse_rtthird.Visibility = Visibility.Hidden;

                line_rt_c7.Visibility = Visibility.Hidden;
                line_rt_pelvis.Visibility = Visibility.Hidden;
                line_rt_sacrum.Visibility = Visibility.Hidden;
            }
        }

    }
}
#endregion
#else
#region new
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
            
        }
        #region ** Kinect **
        Utility.KinectViewModel _viewModel;
        private void button_Open_Click(object sender, RoutedEventArgs e)
        {
            _viewModel = new Utility.KinectViewModel();
            DataContext = _viewModel;
            initSelectViewPoint();
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

        private void initSelectViewPoint()
        {
            button_Joints_Click(button_Head, new System.Windows.RoutedEventArgs());
            button_Joints_Click(button_Pelvis, new System.Windows.RoutedEventArgs());
            button_Joints_Click(button_Neck, new System.Windows.RoutedEventArgs());
            button_Joints_Click(button_EarLeft, new System.Windows.RoutedEventArgs());
        }

        private List<Brush> ButtonColorList = new List<Brush>()
        {
            new SolidColorBrush(Color.FromArgb(0xFF, 0x3D, 0x99, 0x62)),    // Open
            new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x3D, 0x3D)),    // Close
            new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x90, 0x3D)),    // Pause
            new SolidColorBrush(Color.FromArgb(0xFF, 0xA0, 0xA0, 0xA0)),    // Disable
        };

        private bool _IsConnected;
        private bool IsConnected
        {
            get { return _IsConnected; }
            set
            {
                _IsConnected = value;
                if (_IsConnected)
                {
                    _viewModel.StartCamera();
                    grid_Connect.Visibility = Visibility.Hidden;
                }
                else
                {
                    _viewModel.StopCamera();
                    grid_Connect.Visibility = Visibility.Visible;
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
            if (_viewModel.recogBodyNum == 0)
            {
                MessageBox.Show("Body 가 감지되지 않았습니다.");
                IsCapture = false;
                return;
            }
            else if (_viewModel.recogBodyNum > 1)
            {
                MessageBox.Show("Body 가 2명이상 감지되었습니다.");
                IsCapture = false;
                return;
            }

            IsCapture = !IsCapture;

            if (IsCapture)
            {
                string tempDuration = combobox_Time.Text;
                string tempInterval = combobox_Interval.Text;

                Task.Run(() => Capturing(Convert.ToDouble(tempDuration.Replace("seconds", "")), Convert.ToDouble(tempInterval.Replace("seconds", ""))));
            }
        }

        private void Capturing(double duration, double interval)
        {
            duration = duration * 1000;
            interval = interval * 1000;


            string folderPath = Environment.CurrentDirectory + "/Capture" + DateTime.Now.ToString("yyMMdd_HHmmss");
            DirectoryInfo di = new DirectoryInfo(folderPath);

            if (!di.Exists)
                di.Create();


            DataTable dataTable = new DataTable();
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
            DataSave(dataTable, folderPath);
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
        private void DataSave(DataTable dataTable, string folderPath)
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
        #endregion
        #region ** File View **
        // Folder Path, Button
        Dictionary<string, Button> ButtonDictionary = new Dictionary<string, Button>();
        private void button_refresh_Click(object sender, RoutedEventArgs e)
        {
            ButtonDictionary.Clear();
            stackpanel_folder.Children.Clear();
            initFileList();
        }
        // File list view
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
                button.Click += FileButton_Click;

                stackpanel_folder.Children.Add(button);
            }
        }
        private Button previousFileButton;
        private void FileButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            if (previousFileButton != null)
            {
                previousFileButton.Background = Brushes.Transparent;
            }
            previousFileButton = btn;
            btn.Background = Brushes.SkyBlue;

            if (!ButtonDictionary.ContainsValue(btn))
                return;

            var key = ButtonDictionary.FirstOrDefault(x => x.Value == btn).Key;
            textblock_selected.Text = key;
            initPictureList(key);
        }
        // Image Path, Button
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


        private Button previousImageButton;
        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            if (previousImageButton != null)
            {
                previousImageButton.Background = Brushes.Transparent;
            }
            previousImageButton = btn;
            btn.Background = Brushes.SkyBlue;


            if (!ImageButtonDictionary.ContainsValue(btn))
                return;

            var key = ImageButtonDictionary.FirstOrDefault(x => x.Value == btn).Key;

            image_saved.Source = new BitmapImage(new Uri(key, UriKind.RelativeOrAbsolute)); ;

            Debug.WriteLine(key);

            initJointViewList(key.Substring(0, key.Length - key.Split("\\").Last().Length), Convert.ToInt32(key.Split("\\").Last().Replace(".png", "")));
        }
        #endregion
        #region ** Get Test Datas **
        string[] AllJointDatas; // 전체 관절별 포인트 데이터
        string[] SelJointDatas; // 선택한 사진의 관절별 포인트 데이터
        List<Point> List_SeleJointDatas = new List<Point>();
        private void initJointViewList(string path, int row)
        {
            int cnt = 0;

            // 선택한 데이터를 전역변수에 할당.
            AllJointDatas = ReadTextFile(path);
            SelJointDatas = AllJointDatas[row - 1].Split("/");

            List_SeleJointDatas.Clear();
            SelJointDatas.ToList().ForEach(x =>
            {
                if (string.IsNullOrEmpty(x))
                    List_SeleJointDatas.Add(new Point(0, 0));
                else
                    List_SeleJointDatas.Add(Point.Parse(x.Replace("<", "").Replace(">", "")));
            });

            statckpanel_viewjoints.Children.Clear();
            
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
                tb_point.Text = SelJointDatas[cnt - 1];

                stackPanel.Children.Add(tb_point);
                cb.Checked += Cb_Joint_Checked;
                cb.Unchecked += Cb_Joint_Unchecked;
                statckpanel_viewjoints.Children.Add(stackPanel);
            }
            tnview_IsVisible = false;
            bkview_IsVisible = false;
            rtview_IsVisible = false;
            Cal_TNData();
            Cal_BKData();
            Cal_YAData();
        }
        private void Cb_Joint_Checked(object sender, RoutedEventArgs e)
        {
        }
        private void Cb_Joint_Unchecked(object sender, RoutedEventArgs e)
        {
        }
        private string[] ReadTextFile(string path)
        {
            return File.ReadAllLines(path + "data.txt");
        }
        #endregion

        Point LeftPoint = new Point();
        Point RightPoint = new Point();
        private void canvas_draw_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(sender as Canvas);
            LeftPoint = point;
            Cal_RTData();
        }
        private void canvas_draw_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(sender as Canvas);
            RightPoint = point;
            //List_SeleJointDatas[(int)JointId.Neck] = new Point(point.X * 2, point.Y * 2);
            Cal_TNData();
            Cal_BKData();
            Cal_RTData();
           
        }

        #region ** TN **
        private void Cal_TNData()
        {
            // 값을 구해서, UI에 값 표시, 사진에 데이터 표시

            // EARLEFT, C7, SHOULDERLEFT, || 3개의 점 간의 각도 OR EARLEFT -- C7 직선 수평선과의 각도
            // EARLEFT는 보정이 들어가야함.
            // C7은 NECK에서 보정이 들어간 값.
            Point C7_Point = new Point();
            switch (c7def)
            {
                case C7DEFINITION.MANUAL:
                    C7_Point = new(RightPoint.X * 2, RightPoint.Y * 2);
                    if (C7_Point == new Point(0, 0))
                    {
                        tnview_IsVisible = false;
                    }
                    break;
                case C7DEFINITION.NECK:
                    C7_Point = List_SeleJointDatas[(int)JointId.Neck];
                    break;
                case C7DEFINITION.HEAD:
                    C7_Point = List_SeleJointDatas[(int)JointId.Head];
                    break;
            }
            Point EarLeft_Point = List_SeleJointDatas[(int)JointId.EarLeft];
            Point ShoulderLeft_Point = List_SeleJointDatas[(int)JointId.ShoulderLeft];

            var xa = EarLeft_Point.X;
            var ya = EarLeft_Point.Y;
            var xb = C7_Point.X;
            var yb = C7_Point.Y;
            var xc = EarLeft_Point.X;
            var yc = C7_Point.Y;

            var theta = Math.Atan2(yb - ya, xb - xa) - Math.Atan2(yb - yc, xb - xc);
            theta = theta * 180 / Math.PI;
            Debug.WriteLine(theta);


            // UI 표시
            textblock_tnfirst.Text = EarLeft_Point.ToString();
            textblock_tnsecond.Text = C7_Point.ToString();
            textblock_tnresult.Text = theta.ToString("0.00 도");
            SetPositionEllipse(ellipse_tnfirst, canvas_draw, new Point(EarLeft_Point.X / 2, EarLeft_Point.Y / 2));
            SetPositionEllipse(ellipse_tnsecond, canvas_draw, new Point(C7_Point.X / 2, C7_Point.Y / 2));

            SetPositionLine(line_tnfirst, canvas_draw, new Point(EarLeft_Point.X / 2, EarLeft_Point.Y / 2), new Point(C7_Point.X / 2, C7_Point.Y / 2));
            SetPositionLine(line_tnsecond, canvas_draw, new Point(C7_Point.X / 2 - 100, C7_Point.Y / 2), new Point(EarLeft_Point.X / 2 + 100, C7_Point.Y / 2));
        }
        Ellipse ellipse_tnfirst = new Ellipse()
        {
            Fill = Brushes.Purple,
            Width = 10,
            Height = 10,
            Visibility = Visibility.Hidden

        };
        Ellipse ellipse_tnsecond = new Ellipse()
        {
            Fill = Brushes.DarkCyan,
            Width = 10,
            Height = 10,
            Visibility = Visibility.Hidden
        };
        Line line_tnfirst = new Line()
        {
            Stroke = Brushes.Yellow,
            StrokeThickness = 2,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeDashArray = new DoubleCollection() { 1, 1 },
            Visibility = Visibility.Hidden
        };
        Line line_tnsecond = new Line()
        {
            Stroke = Brushes.Black,
            StrokeThickness = 2,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeDashArray = new DoubleCollection() { 1, 1 },
            Visibility = Visibility.Hidden
        };

        private bool _tnview_IsVisible = false;
        private bool tnview_IsVisible
        {
            get { return _tnview_IsVisible; }
            set
            {
                _tnview_IsVisible = value;
                if (_tnview_IsVisible == false)
                {
                    button_tnview.Content = "보기";
                    ellipse_tnfirst.Visibility = Visibility.Hidden;
                    ellipse_tnsecond.Visibility = Visibility.Hidden;
                    line_tnfirst.Visibility = Visibility.Hidden;
                    line_tnsecond.Visibility = Visibility.Hidden;
                }
                else
                {
                    button_tnview.Content = "끄기";
                    ellipse_tnfirst.Visibility = Visibility.Visible;
                    ellipse_tnsecond.Visibility = Visibility.Visible;
                    line_tnfirst.Visibility = Visibility.Visible;
                    line_tnsecond.Visibility = Visibility.Visible;
                }
            }
        }
        private void button_tnview_Click(object sender, RoutedEventArgs e)
        {
            tnview_IsVisible = !tnview_IsVisible;
        }
        #endregion
        #region ** RT **
        private void Cal_RTData()
        {
            // 값을 구해서, UI에 값 표시, 사진에 데이터 표시

            // C7, PELVIS, SACRUM || C7 ~ SACRUM, PELVIS ~ SACRUM 2 직선의 비율을 구해야함.
            // C7은 NECK에서 보정이 들어간 값.
            // SACRUM은 사용자가 선택한 값.
            Point C7_Point = new Point();
            switch (c7def)
            {
                case C7DEFINITION.MANUAL:
                    C7_Point = new(RightPoint.X * 2 , RightPoint.Y * 2);
                    if (C7_Point == new Point(0, 0))
                    {
                        tnview_IsVisible = false;
                    }
                    break;
                case C7DEFINITION.NECK:
                    C7_Point = List_SeleJointDatas[(int)JointId.Neck];
                    break;
                case C7DEFINITION.HEAD:
                    C7_Point = List_SeleJointDatas[(int)JointId.Head];
                    break;
            }

            Point Pelvis_Point = List_SeleJointDatas[(int)JointId.Pelvis];
            Point Sacrum_Point = new(LeftPoint.X * 2, LeftPoint.Y * 2); ;
            if (Sacrum_Point == new Point(0, 0))
                rtview_IsVisible = false;

            
            var c7_sacrum_length = Math.Abs(C7_Point.X - Sacrum_Point.X);
            var pelvis_sacrum_length = Math.Abs(Pelvis_Point.X - Sacrum_Point.X);

            var ratio = pelvis_sacrum_length / c7_sacrum_length;

            // UI 표시
            textblock_rtfirst.Text = C7_Point.ToString();
            textblock_rtsecond.Text = Pelvis_Point.ToString();
            textblock_rtthird.Text = Sacrum_Point.ToString();
            textblock_rtresult.Text = ratio.ToString("0.00 (P-S:C-S)");

            SetPositionEllipse(ellipse_rtfirst, canvas_draw, new Point(C7_Point.X / 2, C7_Point.Y / 2));
            SetPositionEllipse(ellipse_rtsecond, canvas_draw, new Point(Pelvis_Point.X / 2, Pelvis_Point.Y / 2));
            SetPositionEllipse(ellipse_rtthird, canvas_draw, new Point(Sacrum_Point.X / 2, Sacrum_Point.Y / 2));

            SetPositionLine(line_rt_c7, canvas_draw, new Point(C7_Point.X / 2, C7_Point.Y / 2), new Point(C7_Point.X / 2, Sacrum_Point.Y / 2));
            SetPositionLine(line_rt_pelvis, canvas_draw, new Point(Pelvis_Point.X / 2, Pelvis_Point.Y / 2), new Point(Pelvis_Point.X / 2, Sacrum_Point.Y / 2));
            SetPositionLine(line_rt_sacrum, canvas_draw, new Point(Sacrum_Point.X / 2, C7_Point.Y / 2), new Point(Sacrum_Point.X / 2, Pelvis_Point.Y / 2));

            SetPositionLine(line_rt_c7_sacrum, canvas_draw, new Point(C7_Point.X / 2, (C7_Point.Y + Sacrum_Point.Y) / 2 /2), new Point(Sacrum_Point.X / 2, (C7_Point.Y + Sacrum_Point.Y) / 2 /2));
            SetPositionLine(line_rt_pelvis_sacrum, canvas_draw, new Point(Pelvis_Point.X / 2, (Pelvis_Point.Y + Sacrum_Point.Y) / 2 / 2), new Point(Sacrum_Point.X / 2, (Pelvis_Point.Y + Sacrum_Point.Y) / 2 / 2));
        }
        Ellipse ellipse_rtfirst = new Ellipse()
        {
            Fill = Brushes.DarkCyan,
            Width = 10,
            Height = 10,
            Visibility = Visibility.Hidden
        };
        Ellipse ellipse_rtsecond = new Ellipse()
        {
            Fill = Brushes.DeepPink,
            Width = 10,
            Height = 10,
            Visibility = Visibility.Hidden
        };
        Ellipse ellipse_rtthird = new Ellipse()
        {
            Fill = Brushes.DarkOrange,
            Width = 10,
            Height = 10,
            Visibility = Visibility.Hidden
        };

        Line line_rt_c7 = new Line()
        {
            Stroke = Brushes.Yellow,
            StrokeThickness = 2,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeDashArray = new DoubleCollection() { 1, 1 },
            Visibility = Visibility.Hidden
        };
        Line line_rt_pelvis = new Line()
        {
            Stroke = Brushes.Yellow,
            StrokeThickness = 2,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeDashArray = new DoubleCollection() { 1, 1 },
            Visibility = Visibility.Hidden
        };
        Line line_rt_sacrum = new Line()
        {
            Stroke = Brushes.Yellow,
            StrokeThickness = 2,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeDashArray = new DoubleCollection() { 1, 1 },
            Visibility = Visibility.Hidden
        };

        Line line_rt_c7_sacrum = new Line()
        {
            Stroke = Brushes.IndianRed,
            StrokeThickness = 2,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeStartLineCap = PenLineCap.Round,
            Visibility = Visibility.Hidden
            //StrokeDashArray = new DoubleCollection() { 1, 1 },
        };

        Line line_rt_pelvis_sacrum = new Line()
        {
            Stroke = Brushes.IndianRed,
            StrokeThickness = 2,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeStartLineCap = PenLineCap.Round,
            Visibility = Visibility.Hidden
            // StrokeDashArray = new DoubleCollection() { 1, 1 },
        };
        private bool _rtview_IsVisible = false;
        private bool rtview_IsVisible
        {
            get { return _rtview_IsVisible; }
            set
            {
                _rtview_IsVisible = value;
                if (_rtview_IsVisible == false)
                {
                    button_rtview.Content = "보기";
                    ellipse_rtfirst.Visibility = Visibility.Hidden;
                    ellipse_rtsecond.Visibility = Visibility.Hidden;
                    ellipse_rtthird.Visibility = Visibility.Hidden;
                    line_rt_c7.Visibility = Visibility.Hidden;
                    line_rt_pelvis.Visibility = Visibility.Hidden;
                    line_rt_sacrum.Visibility = Visibility.Hidden;
                    line_rt_c7_sacrum.Visibility = Visibility.Hidden;
                    line_rt_pelvis_sacrum.Visibility = Visibility.Hidden;
                }
                else
                {
                    button_rtview.Content = "끄기";
                    ellipse_rtfirst.Visibility = Visibility.Visible;
                    ellipse_rtsecond.Visibility = Visibility.Visible;
                    ellipse_rtthird.Visibility = Visibility.Visible;
                    line_rt_c7.Visibility = Visibility.Visible;
                    line_rt_pelvis.Visibility = Visibility.Visible;
                    line_rt_sacrum.Visibility = Visibility.Visible;
                    line_rt_c7_sacrum.Visibility = Visibility.Visible;
                    line_rt_pelvis_sacrum.Visibility = Visibility.Visible;
                }
            }
        }

      
        private void button_rtview_Click(object sender, RoutedEventArgs e)
        {
            rtview_IsVisible = !rtview_IsVisible;
        }
        #endregion
        #region ** BK **
        private void Cal_BKData()
        {
            // 값을 구해서, UI에 값 표시, 사진에 데이터 표시

            // C7, PELVIS || C7 ~ PELVIS의 직선과 PELVIS의 수직선이 이루는 각도.
            // C7은 NECK에서 보정이 들어간 값.
            // 좌, 우가 표시되어야 함.

            Point C7_Point = new Point();
            switch (c7def)
            {
                case C7DEFINITION.MANUAL:
                    C7_Point = new (RightPoint.X * 2, RightPoint.Y * 2);
                    if (C7_Point == new Point(0, 0))
                        bkview_IsVisible = false;
                    break;
                case C7DEFINITION.NECK:
                    C7_Point = List_SeleJointDatas[(int)JointId.Neck];
                    break;
                case C7DEFINITION.HEAD:
                    C7_Point = List_SeleJointDatas[(int)JointId.Head];
                    break;
            }
            Point Pelvis_Point = List_SeleJointDatas[(int)JointId.Pelvis];


            var xa = C7_Point.X;
            var ya = C7_Point.Y;
            var xb = Pelvis_Point.X;
            var yb = Pelvis_Point.Y;
            var xc = Pelvis_Point.X;
            var yc = C7_Point.Y;

            var theta = Math.Atan2(yb - ya, xb - xa) - Math.Atan2(yb - yc, xb - xc);
            theta = theta * 180 / Math.PI;
            //Debug.WriteLine(theta);

            // UI 표시
            textblock_bkfirst.Text = C7_Point.ToString();
            textblock_bksecond.Text = Pelvis_Point.ToString();
            textblock_bkresult.Text = theta.ToString("0.00 도");

            SetPositionEllipse(ellipse_bkfirst, canvas_draw, new Point(C7_Point.X / 2, C7_Point.Y / 2));
            SetPositionEllipse(ellipse_bksecond, canvas_draw, new Point(Pelvis_Point.X / 2, Pelvis_Point.Y / 2));

            SetPositionLine(line_bk_vertical, canvas_draw, new Point(Pelvis_Point.X / 2, Pelvis_Point.Y / 2), new Point(Pelvis_Point.X / 2, C7_Point.Y / 2));
            SetPositionLine(line_bk_neckpelvis, canvas_draw, new Point(Pelvis_Point.X / 2, Pelvis_Point.Y / 2), new Point(C7_Point.X / 2, C7_Point.Y / 2));
        }
        Ellipse ellipse_bkfirst = new Ellipse()
        {
            Fill = Brushes.Purple,
            Width = 10,
            Height = 10,
            Visibility = Visibility.Hidden
        };

        Ellipse ellipse_bksecond = new Ellipse()
        {
            Fill = Brushes.DarkCyan,
            Width = 10,
            Height = 10,
            Visibility = Visibility.Hidden
        };

        Line line_bk_vertical = new Line()
        {
            Stroke = Brushes.Yellow,
            StrokeThickness = 2,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeDashArray = new DoubleCollection() { 1, 1 },
            Visibility = Visibility.Hidden
        };
        Line line_bk_neckpelvis = new Line()
        {
            Stroke = Brushes.LightSteelBlue,
            StrokeThickness = 2,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeStartLineCap = PenLineCap.Round,
            Visibility = Visibility.Hidden
            //StrokeDashArray = new DoubleCollection() { 1, 1 },
        };
        private bool _bkview_IsVisible = false;
        private bool bkview_IsVisible
        {
            get { return _bkview_IsVisible; }
            set
            {
                _bkview_IsVisible = value;
                if (_bkview_IsVisible == false)
                {
                    button_bkview.Content = "보기";
                    ellipse_bkfirst.Visibility = Visibility.Hidden;
                    ellipse_bksecond.Visibility = Visibility.Hidden;
                    line_bk_vertical.Visibility = Visibility.Hidden;
                    line_bk_neckpelvis.Visibility = Visibility.Hidden;
                }
                else
                {
                    button_bkview.Content = "끄기";
                    ellipse_bkfirst.Visibility = Visibility.Visible;
                    ellipse_bksecond.Visibility = Visibility.Visible;
                    line_bk_vertical.Visibility = Visibility.Visible;
                    line_bk_neckpelvis.Visibility = Visibility.Visible;
                }
            }
        }
        private void button_bkview_Click(object sender, RoutedEventArgs e)
        {
            bkview_IsVisible = !bkview_IsVisible;
        }
        #endregion
        #region ** YA **
        private void Cal_YAData()
        {
            // 값을 구해서, UI에 값 표시, 사진에 데이터 표시
            // 양쪽의 쇄골에 명치가 이루는 2개의 앵글을 구한다.
            // ClavicleRight -- SpineChest -- SpineNaval
            // ClavicleLeft -- SpineCHest -- SpineNaval
            // 위 2개의 성분이 이루는 각도

            Point ClavicleRight_Point = List_SeleJointDatas[(int)JointId.ClavicleRight];
            Point ClavicleLeft_Point = List_SeleJointDatas[(int)JointId.ClavicleLeft];
            Point SpineChest_Point = List_SeleJointDatas[(int)JointId.SpineChest];
            Point SpineNavalPoint = List_SeleJointDatas[(int)JointId.SpineNavel];

            // xa, ya 첫번째 포인트의 좌표 x, y성분값
            // xb, yb 가운데 포인트의 좌표 x, y성분값
            // xc, uc 세번째 포인트의 좌표 x, y성분값

            var Axa = ClavicleRight_Point.X;
            var Aya = ClavicleRight_Point.Y;
            var Axb = SpineChest_Point.X;
            var Ayb = SpineChest_Point.Y;
            var Axc = SpineNavalPoint.X;
            var Ayc = SpineNavalPoint.Y;

            var Atheta = Math.Atan2(Ayb - Aya, Axb - Axa) - Math.Atan2(Ayb - Ayc, Axb - Axc);
            Atheta = Atheta * 180 / Math.PI;


            var Bxa = ClavicleLeft_Point.X;
            var Bya = ClavicleLeft_Point.Y;
            var Bxb = SpineChest_Point.X;
            var Byb = SpineChest_Point.Y;
            var Bxc = SpineNavalPoint.X;
            var Byc = SpineNavalPoint.Y;

            var Btheta = Math.Atan2(Byb - Bya, Bxb - Bxa) - Math.Atan2(Byb - Byc, Bxb - Bxc);
            Btheta = 360 - Btheta * 180 / Math.PI ;

            textblock_yafirst.Text = ClavicleRight_Point.ToString();
            textblock_yasecond.Text = ClavicleLeft_Point.ToString();
            textblock_yathird.Text = SpineChest_Point.ToString();
            textblock_yafourth.Text = SpineNavalPoint.ToString();

            textblock_yaresultfirst.Text = Atheta.ToString("0.00 도");
            textblock_yaresultsecond.Text = Btheta.ToString("0.00 도");
            textblock_yaresultthird.Text = (Atheta - Btheta).ToString("0.00 도");

            SetPositionEllipse(ellipse_yafirst, canvas_draw, new (ClavicleRight_Point.X / 2, ClavicleRight_Point.Y / 2));
            SetPositionEllipse(ellipse_yasecond, canvas_draw, new(ClavicleLeft_Point.X / 2, ClavicleLeft_Point.Y / 2));
            SetPositionEllipse(ellipse_yathird, canvas_draw, new(SpineChest_Point.X / 2, SpineChest_Point.Y / 2));
            SetPositionEllipse(ellipse_yafourth, canvas_draw, new(SpineNavalPoint.X / 2, SpineNavalPoint.Y / 2));

            SetPositionLine(line_yafirst, canvas_draw, new(ClavicleRight_Point.X / 2, ClavicleRight_Point.Y / 2), new(SpineChest_Point.X / 2, SpineChest_Point.Y / 2));
            SetPositionLine(line_yasecond, canvas_draw, new(ClavicleLeft_Point.X / 2, ClavicleLeft_Point.Y / 2), new(SpineChest_Point.X / 2, SpineChest_Point.Y / 2));
            SetPositionLine(line_yathird, canvas_draw, new(SpineChest_Point.X / 2, SpineChest_Point.Y / 2), new(SpineNavalPoint.X / 2, SpineNavalPoint.Y / 2));
        }

        Ellipse ellipse_yafirst = new Ellipse()
        {
            Fill = Brushes.Purple,
            Width = 10,
            Height = 10,
            Visibility = Visibility.Hidden

        };
        Ellipse ellipse_yasecond = new Ellipse()
        {
            Fill = Brushes.DarkOrange,
            Width = 10,
            Height = 10,
            Visibility = Visibility.Hidden
        };
        Ellipse ellipse_yathird = new Ellipse()
        {
            Fill = Brushes.DarkCyan,
            Width = 10,
            Height = 10,
            Visibility = Visibility.Hidden
        };
        Ellipse ellipse_yafourth = new Ellipse()
        {
            Fill = Brushes.DarkCyan,
            Width = 10,
            Height = 10,
            Visibility = Visibility.Hidden
        };

        Line line_yafirst = new Line()
        {
            Stroke = Brushes.Yellow,
            StrokeThickness = 2,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeDashArray = new DoubleCollection() { 1, 1 },
            Visibility = Visibility.Hidden
        };
        Line line_yasecond = new Line()
        {
            Stroke = Brushes.Yellow,
            StrokeThickness = 2,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeDashArray = new DoubleCollection() { 1, 1 },
            Visibility = Visibility.Hidden
        };
        Line line_yathird = new Line()
        {
            Stroke = Brushes.Yellow,
            StrokeThickness = 2,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeDashArray = new DoubleCollection() { 1, 1 },
            Visibility = Visibility.Hidden
        };
        private bool _yaview_IsVisible = false;
        private bool yaview_IsVisible
        {
            get { return _yaview_IsVisible; }
            set
            {
                _yaview_IsVisible = value;
                if (_yaview_IsVisible == false)
                {
                    button_yaview.Content = "보기";
                    ellipse_yafirst.Visibility = Visibility.Hidden;
                    ellipse_yasecond.Visibility = Visibility.Hidden;
                    ellipse_yathird.Visibility = Visibility.Hidden;
                    ellipse_yafourth.Visibility = Visibility.Hidden;

                    line_yafirst.Visibility = Visibility.Hidden;
                    line_yasecond.Visibility = Visibility.Hidden;
                    line_yathird.Visibility = Visibility.Hidden;
                }
                else
                {
                    button_yaview.Content = "끄기";

                    ellipse_yafirst.Visibility = Visibility.Visible;
                    ellipse_yasecond.Visibility = Visibility.Visible;
                    ellipse_yathird.Visibility = Visibility.Visible;
                    ellipse_yafourth.Visibility = Visibility.Visible;

                    line_yafirst.Visibility = Visibility.Visible;
                    line_yasecond.Visibility = Visibility.Visible;
                    line_yathird.Visibility = Visibility.Visible;
                }
            }
        }
        private void button_yaview_Click(object sender, RoutedEventArgs e)
        {
            yaview_IsVisible = !yaview_IsVisible;
        }
        #endregion


        private void SetPositionEllipse(Ellipse ellipse, Canvas canvas, Point point)
        {
            if (!canvas.Children.Contains(ellipse))
                canvas.Children.Add(ellipse);

            Canvas.SetLeft(ellipse, point.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, point.Y - ellipse.Height / 2);
        }

        private void SetPositionLine(Line line, Canvas canvas, Point point1, Point point2)
        {
            if (!canvas.Children.Contains(line))
                canvas.Children.Add(line);

            line.X1 = point1.X;
            line.Y1 = point1.Y;

            line.X2 = point2.X;
            line.Y2 = point2.Y;
        }


        private enum C7DEFINITION
        {
            MANUAL = 0,
            HEAD = 1,
            NECK = 2 
        }

        private C7DEFINITION c7def = C7DEFINITION.HEAD;
        private void radiobutton_selectpoint_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton.Name == "radiobutton_select_head")
            {
                c7def = C7DEFINITION.HEAD;
            }
            else if (radioButton.Name == "radiobutton_select_manual")
            {
                c7def = C7DEFINITION.MANUAL;
            }
            else if (radioButton.Name == "radiobutton_select_neck")
            {
                c7def = C7DEFINITION.NECK;
            }

            if (List_SeleJointDatas.Count == 0)
                return;
            Cal_TNData();
            Cal_BKData();
            Cal_RTData();

        }
    }
}

#endregion
#endif