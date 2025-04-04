using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public class UsageRAM
{
    private PerformanceCounter ramCounter;

    public UsageRAM()
    {
        ramCounter = new PerformanceCounter
        {
            CategoryName = "Memory",
            CounterName = "% Committed Bytes In Use"
        };
    }

    public async Task MonitoringRAM(CancellationToken cancellationToken, Action<double> updateCallback)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            float value = ramCounter.NextValue();
        
            if (!float.IsNaN(value))
            {
                updateCallback?.Invoke(Convert.ToDouble(value));
            }
            else
            {
                updateCallback?.Invoke(double.NaN);
            }
        
            await Task.Delay(1000, cancellationToken);
        }
    }
}
