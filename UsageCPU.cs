using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public class UsageCPU
{
    private PerformanceCounter totalCpuCounter;
    private PerformanceCounter idleCpuCounter;

    public UsageCPU()
    {
        totalCpuCounter = new PerformanceCounter
        {
            CategoryName = "Processor Information",
            CounterName = "% Privileged Utility",
            InstanceName = "_Total"
        };

        idleCpuCounter = new PerformanceCounter
        {
            CategoryName = "Processor Information",
            CounterName = "% Idle Time",
            InstanceName = "_Total"
        };
    }

    public async Task MonitoringCPU(CancellationToken cancellationToken, Action<double> updateCallback)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                float value = totalCpuCounter.NextValue();
                if (value > 100) { value = 100; }

                updateCallback?.Invoke(Convert.ToDouble(value));

                await Task.Delay(1000, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {

        }
    }

    public async Task GetIdleTime(CancellationToken cancellationToken, Action<double> updateCallback)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                float value = idleCpuCounter.NextValue();
                if (value > 100) { value = 100; }

                updateCallback?.Invoke(Convert.ToDouble(value));

                await Task.Delay(1000, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {

        }
    }

    public void Dispose()
    {
        totalCpuCounter.Dispose();
    }
}
