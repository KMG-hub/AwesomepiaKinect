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

        private void datagrid_phone_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(dataGrid.SelectedIndex);
            DataGridCell RowColumn = dataGrid.Columns[1].GetCellContent(row).Parent as DataGridCell;
            string CellValue = ((TextBlock)RowColumn.Content).Text;
            Debug.WriteLine(CellValue.ToString()); ;

            DataTable dt = Utility.SQLHelper.GetPoint("", CellValue);
            datagrid_position.ItemsSource = dt.DefaultView;

            List<string> list_joints = new List<string>();
            object[] temparray = dt.Rows[0].ItemArray;
            for (int i = 6; i < 39; i++)
            {
                list_joints.Add(temparray[i].ToString());
            }
            SetJoints(list_joints);
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
    }
}
