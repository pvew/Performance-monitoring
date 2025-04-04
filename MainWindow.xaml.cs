using System.Text;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.ComponentModel;
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.ObjectModel;

namespace Monitoring
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Management Management;

            // const для SetWindowPos
        private const int HWND_TOPMOST = -1;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOACTIVATE = 0x0010;
        private const int SWP_SHOWWINDOW = 0x0040;

        // графики
        public ChartValues<double> CpuValues { get; set; } = new ChartValues<double>();
        public ChartValues<double> GpuValues { get; set; } = new ChartValues<double>();
        public ObservableCollection<string> TimeLabels { get; set; } = new ObservableCollection<string>();
        private int currentTime = 0; // Время для оси X

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        public MainWindow()
        {
            InitializeComponent();
            Management = new Management(this);
            this.WindowStyle = WindowStyle.ThreeDBorderWindow;
            this.Topmost = true;
            WindowStartupLocation = WindowStartupLocation;
            Loaded += MainWindow_Loaded;
            DataContext = this;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            int attrValue = 2; // 2 - цветная область
            DwmSetWindowAttribute(hwnd, DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, ref attrValue, sizeof(int)); //title bar

            // handle окна
            IntPtr hWnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;

            // поверх всех окон
            SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }
        public void UpdateGraph(double cpuUsage, double gpuUsage)
        {
            CpuValues.Add(cpuUsage);
            GpuValues.Add(gpuUsage);
            TimeLabels.Add((currentTime++).ToString());

            if (CpuValues.Count > 20) // Ограничиваем график 20 точками
            {
                CpuValues.RemoveAt(0);
                GpuValues.RemoveAt(0);
                TimeLabels.RemoveAt(0);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Management.StopMonitoring();
        }

        private enum DWMWINDOWATTRIBUTE
        {
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20
        }


        //dark title
        [DllImport("dwmapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, ref int pvAttribute, int cbAttribute);

        private void GpuChart_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}