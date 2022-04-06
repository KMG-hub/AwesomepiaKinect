using System;
using System.Collections.Generic;
using System.Data;
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

namespace AwesomepiaResultViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            initPictureList();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            datagrid_phone.ItemsSource = Utility.SQLHelper.GetPhone().DefaultView;
        }

        private string BodyDirection = "front";
        private string TestId = "";
        private string ScoreFA = "";
        private string ScoreSA = "";
        private string ScoreSN = "";

        private void datagrid_phone_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(dataGrid.SelectedIndex);
            DataGridCell RowColumn = dataGrid.Columns[1].GetCellContent(row).Parent as DataGridCell;
            string CellValue = ((TextBlock)RowColumn.Content).Text;
            Debug.WriteLine(CellValue.ToString()); ;

            DataTable dt = Utility.SQLHelper.GetPoint(BodyDirection, CellValue);
            datagrid_position.ItemsSource = dt.DefaultView;

            List<string> list_joints = new List<string>();
            object[] temparray = dt.Rows[0].ItemArray;
            for (int i = 6; i < 39; i++)
            {
                list_joints.Add(temparray[i].ToString());
            }
            TestId = temparray[1].ToString();
            SetJoints(list_joints);
            if (!SelectImage(temparray[5].ToString().Split("/").Last()))
                MessageBox.Show("no data");



        }
        private void SetJoints(List<string> joints)
        {
            canvas_draw.Children.Clear();
            foreach (var joint in joints)
            {
                var temp = joint.Replace("<", "").Replace(">", "").Replace("(", "").Replace(")", "").Replace(" ", "");

                if (string.IsNullOrWhiteSpace(temp))
                    return;

                Point point = Point.Parse(temp);
                Ellipse ellipse = new Ellipse();
                ellipse.Width = 5;
                ellipse.Height = 5;
                ellipse.Fill = Brushes.Red;

                Canvas.SetLeft(ellipse, point.X - (ellipse.Width / 2));
                Canvas.SetTop(ellipse, point.Y - (ellipse.Height / 2));
                canvas_draw.Children.Add(ellipse);
            }
        }

        Dictionary<string, Button> ImageButtonDictionary = new Dictionary<string, Button>();
        private void initPictureList()
        {
            var imagepath = Environment.CurrentDirectory + "\\" + "kinectcapture";
            //var imagepath = "https://minehealth.awesomeserver.kr/kinectcapture";
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

                stackpanel_picture.Children.Add(button);
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

            //initJointViewList(key.Substring(0, key.Length - key.Split("\\").Last().Length), Convert.ToInt32(key.Split("\\").Last().Replace(".png", "")));
        }
        private bool SelectImage(string filename)
        {
            bool result = true;

            //foreach (var key in ImageButtonDictionary.Keys)
            //{
            //    if (key.Contains(filename))
            //    {
            //        result = true;
            //        //image_saved.Source = new BitmapImage(new Uri(key, UriKind.RelativeOrAbsolute));
                    
            //    }

                
            //}

            image_saved.Source = new BitmapImage(new Uri($"https://minehealth.awesomeserver.kr/kinectcapture/{filename}", UriKind.RelativeOrAbsolute));
            Debug.WriteLine(image_saved.Source.ToString());
            return result;
        }
        private void radiobutton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            string name = rb.Name;

            if (name.Contains("front"))
            {
                BodyDirection = "front";
            }
            else if (name.Contains("side"))
            {
                BodyDirection = "side";
            }

        }

        enum SelectPoint
        {
            Default = 0,
            ClavicleLeft = 1,
            ClavicleRight = 2,
            SpineChest = 3,
            SpineNaval = 4,
            EarLeft = 5,
            Neck = 6,
            Pelvis = 7,
            Sacrum = 8,
            Count = 9
        }
        private SelectPoint _mSelectPoint = SelectPoint.Default;
        private SelectPoint mSelectPoint
        {
            get { return _mSelectPoint; }
            set
            {
                _mSelectPoint = value;
                textblock_selectbutton.Text = value.ToString();

            }
        }
        Ellipse ellipse_default = new Ellipse()
        {
            Width = 10,
            Height = 10,
            Fill = Brushes.Black,
        };
        private void canvas_draw_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(sender as Canvas);
            switch (mSelectPoint)
            {
                case SelectPoint.ClavicleLeft:
                    point_clavicleleft = point;
                    SetPositionEllipse(ellipse_clavicleleft, point_clavicleleft);
                    break;
                case SelectPoint.ClavicleRight:
                    point_clavicleright = point;
                    SetPositionEllipse(ellipse_clavicleright, point_clavicleright);
                    break;
                case SelectPoint.SpineChest:
                    point_spinechest = point;
                    SetPositionEllipse(ellipse_spinechest, point_spinechest);
                    break;
                case SelectPoint.SpineNaval:
                    point_spinenaval = point;
                    SetPositionEllipse(ellipse_spinenaval, point_spinenaval);
                    break;

                case SelectPoint.EarLeft:
                    point_earleft = point;
                    SetPositionEllipse(ellipse_earleft, point_earleft);
                    break;
                case SelectPoint.Neck:
                    point_neck = point;
                    SetPositionEllipse(ellipse_neck, point_neck);
                    break;
                case SelectPoint.Pelvis:
                    point_pelvis = point;
                    SetPositionEllipse(ellipse_pelvis, point_pelvis);
                    break;
                case SelectPoint.Sacrum:
                    point_sacrum = point;
                    SetPositionEllipse(ellipse_sacrum, point_sacrum);
                    break;

                default:
                    SetPositionEllipse(ellipse_default, point);
                    break;
            }

            Cal_YAData();
            Cal_TNData();
            Cal_RTData();
        }
        private void canvas_draw_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            mSelectPoint = SelectPoint.Default;
        }
        private void button_setposition_Click(object sender, RoutedEventArgs e)
        {
            string name = (sender as Button).Name;

            foreach (var item in Enum.GetValues(typeof(SelectPoint)))
            {
                if (name.Contains(item.ToString().ToLower()))
                {
                    mSelectPoint = (SelectPoint)item;
                }
            }
        }

        Ellipse ellipse_clavicleleft = new Ellipse()
        {
            Width = 10, Height = 10,
            Fill = Brushes.DarkOrange,
        };
        Ellipse ellipse_clavicleright = new Ellipse()
        {
            Width = 10,
            Height = 10,
            Fill = Brushes.DarkOrange,
        };
        Ellipse ellipse_spinechest = new Ellipse()
        {
            Width = 10,
            Height = 10,
            Fill = Brushes.DarkOrange,
        };
        Ellipse ellipse_spinenaval = new Ellipse()
        {
            Width = 10,
            Height = 10,
            Fill = Brushes.DarkOrange,
        };

        Point point_clavicleleft = new Point();
        Point point_clavicleright = new Point();
        Point point_spinechest = new Point();
        Point point_spinenaval = new Point();
        private void Cal_YAData()
        {
            Point ClavicleRight_Point = point_clavicleleft;
            Point ClavicleLeft_Point = point_clavicleright;
            Point SpineChest_Point = point_spinechest;
            Point SpineNavalPoint = point_spinenaval;

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
            Btheta = 360 - Btheta * 180 / Math.PI;

            textblock_ya.Text = $"{Atheta:0.00}:{Btheta:0.00},{Math.Abs(Btheta - Atheta):0.00}";

            ScoreFA = Math.Abs(Btheta - Atheta).ToString("0.00");
        }

        Ellipse ellipse_neck = new Ellipse()
        {
            Width = 10, Height = 10,
            Fill = Brushes.DarkCyan,
        };
        Ellipse ellipse_earleft = new Ellipse()
        {
            Width = 10,
            Height = 10,
            Fill = Brushes.DarkCyan,
        };
        Ellipse ellipse_pelvis = new Ellipse()
        {
            Width = 10,
            Height = 10,
            Fill = Brushes.DarkCyan,
        };
        Ellipse ellipse_sacrum = new Ellipse()
        {
            Width = 10,
            Height = 10,
            Fill = Brushes.DarkCyan,
        };
        Point point_neck = new Point();
        Point point_earleft = new Point();
        Point point_pelvis = new Point();
        Point point_sacrum = new Point();
        private void Cal_TNData()
        {
            Point C7_Point = point_neck;
            Point EarLeft_Point = point_earleft;

            var xa = EarLeft_Point.X;
            var ya = EarLeft_Point.Y;
            var xb = C7_Point.X;
            var yb = C7_Point.Y;
            var xc = EarLeft_Point.X;
            var yc = C7_Point.Y;

            var theta = Math.Atan2(yb - ya, xb - xa) - Math.Atan2(yb - yc, xb - xc);
            theta = theta * 180 / Math.PI;

            textblock_tn.Text = theta.ToString("0.00");

            ScoreSN = theta.ToString("0.00");
        }

        private void Cal_RTData()
        {
            Point C7_Point = point_neck;
            Point Pelvis_Point = point_pelvis;
            Point Sacrum_Point = point_sacrum;

            var c7_sacrum_length = C7_Point.X - Sacrum_Point.X;
            var pelvis_sacrum_length = Pelvis_Point.X - Sacrum_Point.X;
            var ratio = c7_sacrum_length / pelvis_sacrum_length;

            textblock_rt.Text = ratio.ToString("0.00");

            ScoreSA = ratio.ToString("0.00");
        }
        private void SetPositionEllipse(Ellipse ellipse, Point point)
        {
            if (!canvas_draw.Children.Contains(ellipse))
            {
                canvas_draw.Children.Add(ellipse);
            }

            Canvas.SetLeft(ellipse, point.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, point.Y - ellipse.Height / 2);
        }


        private void button_ya_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TestId))
                MessageBox.Show("no selected");

            Utility.SQLHelper.SaveDatas(TestId, Utility.SQLHelper.ScoreCategory.FrontAngle, ScoreFA);
        }

        private void button_tn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TestId))
                MessageBox.Show("no selected");

            Utility.SQLHelper.SaveDatas(TestId, Utility.SQLHelper.ScoreCategory.SideNeck, ScoreSN);
        }

        private void button_rt_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TestId))
                MessageBox.Show("no selected");

            Utility.SQLHelper.SaveDatas(TestId, Utility.SQLHelper.ScoreCategory.SideAngle, ScoreSA);
        }

       
    }
}
