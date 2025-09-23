using System;
using System.Diagnostics;
using System.Management;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace OtakAgent.Core.Services
{
    public interface ISystemResourceService
    {
        double CpuUsage { get; }
        double MemoryUsagePercentage { get; }
        long MemoryUsedMB { get; }
        long MemoryTotalMB { get; }
        void StartMonitoring();
        void StopMonitoring();
        event EventHandler<ResourceUpdateEventArgs>? ResourcesUpdated;
    }

    public class ResourceUpdateEventArgs : EventArgs
    {
        public double CpuUsage { get; set; }
        public double MemoryUsagePercentage { get; set; }
        public long MemoryUsedMB { get; set; }
        public long MemoryTotalMB { get; set; }
    }

    [SupportedOSPlatform("windows")]
    public class SystemResourceService : ISystemResourceService, IDisposable
    {
        private readonly PerformanceCounter _cpuCounter;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _monitoringTask;

        public double CpuUsage { get; private set; }
        public double MemoryUsagePercentage { get; private set; }
        public long MemoryUsedMB { get; private set; }
        public long MemoryTotalMB { get; private set; }

        public event EventHandler<ResourceUpdateEventArgs>? ResourcesUpdated;

        public SystemResourceService()
        {
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _cpuCounter.NextValue(); // Initialize
                Console.WriteLine("SystemResourceService: CPU counter initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SystemResourceService: Failed to initialize CPU counter - {ex.Message}");
                throw;
            }
        }

        public void StartMonitoring()
        {
            if (_monitoringTask != null && !_monitoringTask.IsCompleted)
                return;

            Console.WriteLine("SystemResourceService: Starting monitoring");
            _cancellationTokenSource = new CancellationTokenSource();
            _monitoringTask = Task.Run(async () => await MonitorResourcesAsync(_cancellationTokenSource.Token));
        }

        public void StopMonitoring()
        {
            _cancellationTokenSource?.Cancel();
            try
            {
                _monitoringTask?.Wait(TimeSpan.FromSeconds(2));
            }
            catch (AggregateException) { }

            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            _monitoringTask = null;
        }

        private async Task MonitorResourcesAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("SystemResourceService: MonitorResourcesAsync started");

            try
            {
                // First CPU reading is always 0, so wait and discard it
                await Task.Delay(100, cancellationToken);
                _cpuCounter.NextValue();
                await Task.Delay(1000, cancellationToken);

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // Get CPU usage
                        CpuUsage = Math.Round(_cpuCounter.NextValue(), 1);

                        // Get memory information
                        UpdateMemoryInfo();

                        Console.WriteLine($"Resource Update: CPU={CpuUsage}%, Memory={MemoryUsedMB}/{MemoryTotalMB} MB ({MemoryUsagePercentage}%)");

                        // Raise event
                        ResourcesUpdated?.Invoke(this, new ResourceUpdateEventArgs
                        {
                            CpuUsage = CpuUsage,
                            MemoryUsagePercentage = MemoryUsagePercentage,
                            MemoryUsedMB = MemoryUsedMB,
                            MemoryTotalMB = MemoryTotalMB
                        });

                        await Task.Delay(1000, cancellationToken); // Update every second
                    }
                    catch (TaskCanceledException)
                    {
                        Console.WriteLine("SystemResourceService: Monitoring cancelled");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Resource monitoring error: {ex.Message}");
                        await Task.Delay(5000, cancellationToken); // Wait longer on error
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SystemResourceService: Fatal error in MonitorResourcesAsync - {ex.Message}");
            }
        }

        private void UpdateMemoryInfo()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                using var collection = searcher.Get();

                foreach (ManagementObject mo in collection)
                {
                    if (mo["TotalVisibleMemorySize"] != null && mo["FreePhysicalMemory"] != null)
                    {
                        var totalKB = Convert.ToUInt64(mo["TotalVisibleMemorySize"]);
                        var freeKB = Convert.ToUInt64(mo["FreePhysicalMemory"]);

                        MemoryTotalMB = (long)(totalKB / 1024);
                        var freeMB = (long)(freeKB / 1024);
                        MemoryUsedMB = MemoryTotalMB - freeMB;

                        MemoryUsagePercentage = MemoryTotalMB > 0
                            ? Math.Round((double)MemoryUsedMB / MemoryTotalMB * 100, 1)
                            : 0;

                        System.Diagnostics.Debug.WriteLine($"Memory WMI: Total={totalKB}KB, Free={freeKB}KB, TotalMB={MemoryTotalMB}, UsedMB={MemoryUsedMB}");
                        break;
                    }
                }

                // Fallback to Process memory if WMI fails
                if (MemoryTotalMB == 0)
                {
                    using var process = Process.GetCurrentProcess();
                    var totalMemory = GC.GetTotalMemory(false);

                    // Get available physical memory using PerformanceCounter
                    try
                    {
                        using var availMemCounter = new PerformanceCounter("Memory", "Available MBytes");
                        var availableMB = (long)availMemCounter.NextValue();

                        // Get total physical memory from Environment
                        var totalPhysicalMemory = Environment.WorkingSet / (1024 * 1024);

                        // Use process info as approximation
                        MemoryUsedMB = process.WorkingSet64 / (1024 * 1024);
                        MemoryTotalMB = Math.Max(totalPhysicalMemory, availableMB + MemoryUsedMB);

                        if (MemoryTotalMB > 0)
                        {
                            MemoryUsagePercentage = Math.Round((double)MemoryUsedMB / MemoryTotalMB * 100, 1);
                        }

                        System.Diagnostics.Debug.WriteLine($"Memory Fallback: TotalMB={MemoryTotalMB}, UsedMB={MemoryUsedMB}");
                    }
                    catch (Exception ex2)
                    {
                        System.Diagnostics.Debug.WriteLine($"Memory counter error: {ex2.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Memory info error: {ex.Message}");
            }
        }

        public void Dispose()
        {
            StopMonitoring();
            _cpuCounter?.Dispose();
        }
    }
}