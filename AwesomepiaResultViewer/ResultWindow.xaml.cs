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
using System.Windows.Shapes;

namespace AwesomepiaResultViewer
{
    /// <summary>
    /// ResultWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ResultWindow : Window
    {
        public ResultWindow()
        {
            InitializeComponent();
        }
        DataTable datatable_total = new DataTable();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            datatable_total = Utility.SQLHelper.GetResultScoreTbl();
            datagrid_phone.ItemsSource = datatable_total.DefaultView;
        }

        private int selectedIndex = -1;
        private void datagrid_phone_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(dataGrid.SelectedIndex);
            DataGridCell RowColumn = dataGrid.Columns[0].GetCellContent(row).Parent as DataGridCell;
            string CellValue = ((TextBlock)RowColumn.Content).Text;
            Debug.WriteLine(CellValue.ToString());
            selectedIndex = row.GetIndex();
            if (string.IsNullOrWhiteSpace(CellValue))
                return;

            ViewData(selectedIndex);
        }

        private void ViewData(int index)
        {
            if (selectedIndex == -1)
                return;

            grid_nodatas.Visibility = Visibility.Hidden;
            image_save.Source = null;
            DataTable dt = Utility.SQLHelper.GetPoint(DirectionFlag, datatable_total.Rows[index]["TestID"].ToString()); // front, side
            Uri uri = new Uri(Environment.CurrentDirectory + dt.Rows[0]["SavePath"].ToString().Replace("./", "/").Replace("/", "\\"), UriKind.RelativeOrAbsolute);

            List<string> list_joints = new List<string>();
            object[] temparray = dt.Rows[0].ItemArray;
            for (int i = 6; i < 39; i++)
            {
                list_joints.Add(temparray[i].ToString());
            }
            textbox_fp.Text = datatable_total.Rows[index]["ScoreFA"].ToString();
            textbox_sp.Text = datatable_total.Rows[index]["ScoreSA"].ToString();
            textbox_sn.Text = datatable_total.Rows[index]["ScoreSN"].ToString();
            DrawJoints(list_joints);

            if (!File.Exists(uri.AbsolutePath))
            {
                grid_nodatas.Visibility = Visibility.Visible;
                return;
            }
                
            image_save.Source = new BitmapImage(uri);

            
        }

        private void DrawJoints(List<string> joints)
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

      

        private string DirectionFlag = "front";
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton.Content.ToString().ToLower() != DirectionFlag)
            {
                DirectionFlag = radioButton.Content.ToString().ToLower();
                ViewData(selectedIndex);
            }
        }
    }
}
