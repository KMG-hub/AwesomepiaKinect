using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
            _viewModel = new Utility.KinectViewModel();
            DataContext = _viewModel;

            
        }

        private void button_Open_Click(object sender, RoutedEventArgs e)
        {
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

        
    }
}
