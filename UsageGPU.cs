using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public class UsageGPU
{
    private List<PerformanceCounter> gpuCounters;

    public UsageGPU()
    {
        InitializeGpuCounters();
    }

    private void InitializeGpuCounters()
    {
        gpuCounters = new List<PerformanceCounter>();

        try
        {
            PerformanceCounterCategory category = new PerformanceCounterCategory("GPU Engine");
            string[] counterNames = category.GetInstanceNames();

            foreach (string name in counterNames)
            {
                foreach (PerformanceCounter counter in category.GetCounters(name))
                {
                    if (counter.CounterName == "Utilization Percentage")
                    {
                        gpuCounters.Add(counter);
                    }
                }
            }

            gpuCounters.ForEach(counter => counter.NextValue());
        }
        catch (Exception ex) { throw new Exception($"Eroor: {ex.Message}"); }
    }

    public float GetGPUUsage()
    {
        float result = 0f;

        try
        {
            gpuCounters.ForEach(counter => result += counter.NextValue());
        }
        catch (Exception ex) { throw new Exception($"Error: {ex.Message}"); }

        return result;
    }

    public async Task MonitoringGPU(CancellationToken cancellationToken, Action<double> updateCallback)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            float gpuUsage = GetGPUUsage();
            if (gpuUsage > 100) { gpuUsage = 100; }

            updateCallback?.Invoke(gpuUsage);

            await Task.Delay(3000, cancellationToken);

            InitializeGpuCounters();
        }
    }
}
