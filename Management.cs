using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.IO; //temp
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.ObjectModel;

namespace Monitoring
{
    public class Management
    {
        private MainWindow _mainWindow;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private UsageCPU _usageCPU = new UsageCPU();
        private UsageGPU _usageGPU = new UsageGPU();
        private UsageRAM _usageRAM = new UsageRAM();

        //графики
        public ChartValues<double> CpuValues { get; set; } = new ChartValues<double>();
        public ChartValues<double> GpuValues { get; set; } = new ChartValues<double>();
        public ObservableCollection<string> TimeLabels { get; set; } = new ObservableCollection<string>();
        private int currentTime = 0;

        // CPU static info
        private ManagementObjectSearcher cpuSearcher; //хранит WMI запрос; при вызове .Get() вернёт
                                                      //коллекцию типа ManagementObject, каждый элемент которой хранит данные
                                                      //соотвествующие одному экземпляру запрошенного WMI-класса;
                                                      //данные представлены в виде свойств, названия которых
                                                      //соответствуют атрибутам WMI-класса. 
        public ManagementObjectCollection processorCollection;
        public Management(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;

            // CPU static info
            cpuSearcher = getCPUSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");

            _mainWindow.LbCPUstatic.Content = $"{getProcessorInfo("Name")}\n" + $"{getProcessorInfo("MaxClockSpeed")}MHz \n"
                                                + $"{getProcessorInfo("NumberOfCores")} corses " + $"{getProcessorInfo("ThreadCount")} threads ";

            Task t = StartMonitoringAsync(_cts);
        }

        // CPU static info
        public ManagementObjectSearcher getCPUSearcher(string _namespace, string query)
        {
            return new ManagementObjectSearcher(_namespace, query);
        }

        public string getProcessorCollectionProperty(string property)
        {
            string answer = "";
            foreach (ManagementObject queryObj in processorCollection) //queryObj должен быть один элемент в коллекции
            {
                answer += $"{queryObj[property]}";
            }
            return answer;
        }

        public async Task StartMonitoringAsync(CancellationTokenSource cts)
        {
            // CPU
            var cpuMonitoringTask = _usageCPU.MonitoringCPU(cts.Token, cpuUsage =>
            {
                if (double.IsNaN(cpuUsage))
                {

                    _mainWindow.LbCPUinfo.Content = "CPU:...";
                }
                else
                {
                    _mainWindow.Dispatcher.Invoke(() =>
                    {
                        _mainWindow.LbCPUinfo.Content = $"CPU: {cpuUsage:F0}%";
                        _mainWindow.UpdateGraph(cpuUsage, _mainWindow.GpuValues.LastOrDefault());
                    });
                }
            });

            // IDLE
            var IdleTimeTask = _usageCPU.GetIdleTime(cts.Token, idleTime =>
            {
                if (double.IsNaN(idleTime))
                {

                    _mainWindow.LbIdleTime.Content = "Idle:...";
                }
                else
                {
                    _mainWindow.Dispatcher.Invoke(() =>
                    {
                        _mainWindow.LbIdleTime.Content = $"Idle: {idleTime:F0}%";
                    });
                }
            });

            //GPU
            var gpuMonitoringTask = _usageGPU.MonitoringGPU(cts.Token, gpuUsage =>
            {
                if (double.IsNaN(gpuUsage))
                {
                    _mainWindow.LbGPUinfo.Content = "GPU:...";
                }
                else
                {
                    _mainWindow.Dispatcher.Invoke(() =>
                    {
                        _mainWindow.LbGPUinfo.Content = $"GPU: {gpuUsage:F0}%";
                        _mainWindow.UpdateGraph(_mainWindow.CpuValues.LastOrDefault(), gpuUsage);
                    });
                }
            });

            // RAM
            await _usageRAM.MonitoringRAM(cts.Token, ramUsage =>
            {
                if (double.IsNaN(ramUsage))
                {
                    _mainWindow.Dispatcher.Invoke(() =>
                    {
                        _mainWindow.LbRAMinfo.Content = "RAM:...";
                    });
                }
                else
                {
                    _mainWindow.Dispatcher.Invoke(() =>
                    {
                        _mainWindow.LbRAMinfo.Content = $"RAM: {ramUsage:F0}%";
                    });
                }
            });

            await Task.WhenAll(cpuMonitoringTask, gpuMonitoringTask);
        }

        public void StopMonitoring()
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        // STATIC info
        public string getProcessorInfo(String value)
        {
            String result = "";
            foreach (ManagementObject obj in cpuSearcher.Get())
            {
                result = obj[value].ToString();
            }

            return result;
        }
    }
}
