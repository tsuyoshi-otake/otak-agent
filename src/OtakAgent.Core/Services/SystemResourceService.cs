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
        private readonly object _lock = new object();
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _monitoringTask;
        private bool _disposed;

        private double _cpuUsage;
        private double _memoryUsagePercentage;
        private long _memoryUsedMB;
        private long _memoryTotalMB;

        public double CpuUsage
        {
            get { lock (_lock) { return _cpuUsage; } }
            private set { lock (_lock) { _cpuUsage = value; } }
        }

        public double MemoryUsagePercentage
        {
            get { lock (_lock) { return _memoryUsagePercentage; } }
            private set { lock (_lock) { _memoryUsagePercentage = value; } }
        }

        public long MemoryUsedMB
        {
            get { lock (_lock) { return _memoryUsedMB; } }
            private set { lock (_lock) { _memoryUsedMB = value; } }
        }

        public long MemoryTotalMB
        {
            get { lock (_lock) { return _memoryTotalMB; } }
            private set { lock (_lock) { _memoryTotalMB = value; } }
        }

        public event EventHandler<ResourceUpdateEventArgs>? ResourcesUpdated;

        public SystemResourceService()
        {
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _cpuCounter.NextValue(); // Initialize
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Failed to initialize CPU performance counter. Ensure performance counters are enabled.", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException("Access denied to performance counters. Run as administrator or grant necessary permissions.", ex);
            }
        }

        public void StartMonitoring()
        {
            lock (_lock)
            {
                if (_monitoringTask != null && !_monitoringTask.IsCompleted)
                    return;

                _cancellationTokenSource = new CancellationTokenSource();
                _monitoringTask = Task.Run(async () => await MonitorResourcesAsync(_cancellationTokenSource.Token).ConfigureAwait(false));
            }
        }

        public void StopMonitoring()
        {
            CancellationTokenSource? cts;
            Task? task;

            lock (_lock)
            {
                cts = _cancellationTokenSource;
                task = _monitoringTask;
                _cancellationTokenSource = null;
                _monitoringTask = null;
            }

            if (cts != null)
            {
                cts.Cancel();
                try
                {
                    task?.Wait(TimeSpan.FromSeconds(2));
                }
                catch (AggregateException)
                {
                    // Expected when task is cancelled
                }
                finally
                {
                    cts.Dispose();
                }
            }
        }

        private async Task MonitorResourcesAsync(CancellationToken cancellationToken)
        {
            try
            {
                // First CPU reading is always 0, so wait and discard it
                await Task.Delay(100, cancellationToken).ConfigureAwait(false);
                _cpuCounter.NextValue();
                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // Get CPU usage
                        CpuUsage = Math.Round(_cpuCounter.NextValue(), 1);

                        // Get memory information
                        UpdateMemoryInfo();

                        // Raise event (thread-safe)
                        var handler = ResourcesUpdated;
                        if (handler != null)
                        {
                            var args = new ResourceUpdateEventArgs
                            {
                                CpuUsage = CpuUsage,
                                MemoryUsagePercentage = MemoryUsagePercentage,
                                MemoryUsedMB = MemoryUsedMB,
                                MemoryTotalMB = MemoryTotalMB
                            };

                            // Invoke on ThreadPool to avoid blocking
                            _ = Task.Run(() => handler.Invoke(this, args), CancellationToken.None);
                        }

                        await Task.Delay(1000, cancellationToken).ConfigureAwait(false); // Update every second
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Resource monitoring error: {ex.Message}");
                        await Task.Delay(5000, cancellationToken).ConfigureAwait(false); // Wait longer on error
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Expected during shutdown
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SystemResourceService: Fatal error in MonitorResourcesAsync - {ex.Message}");
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
                    using (mo)
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

                            break;
                        }
                    }
                }

                // Fallback to Process memory if WMI fails
                if (MemoryTotalMB == 0)
                {
                    using var process = Process.GetCurrentProcess();

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
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Memory counter error: {ex.Message}");
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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    StopMonitoring();
                    _cpuCounter?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}