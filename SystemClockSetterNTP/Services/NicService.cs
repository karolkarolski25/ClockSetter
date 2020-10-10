using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SystemClockSetterNTP.Services
{
    public class NicService : INicService
    {
        public double GigabytesSent { get; set; }
        public double GigabytesReceived { get; set; }


        private List<PerformanceCounter> receivePerformanceCounters = new List<PerformanceCounter>();
        private List<PerformanceCounter> sentPerformanceCounters = new List<PerformanceCounter>();

        private readonly ILogger<NicService> _logger;

        public NicService(ILogger<NicService> logger)
        {
            _logger = logger;
        }

        public void InitializeNICs()
        {
            var nics = new PerformanceCounterCategory("Network Interface").GetInstanceNames();

            if (nics.Any())
            {
                foreach (var nic in new PerformanceCounterCategory("Network Interface").GetInstanceNames())
                {
                    sentPerformanceCounters.Add(new PerformanceCounter("Network Interface", "Bytes Sent/sec", nic));
                    receivePerformanceCounters.Add(new PerformanceCounter("Network Interface", "Bytes Received/sec", nic));
                }
            }
            else
            {
                _logger.LogDebug("No NICs found");
            }
        }

        public async void RunMonitoringNics()
        {
            while (true)
            {
                foreach (var nic in sentPerformanceCounters)
                {
                    GigabytesSent += ConvertToGigabyte(nic.NextValue());
                }

                foreach (var nic in receivePerformanceCounters)
                {
                    GigabytesReceived += ConvertToGigabyte(nic.NextValue());
                }

                await Task.Delay(1000);
            }
        }

        private double ConvertToGigabyte(double dataToConvert)
        {
            return Math.Round(((dataToConvert / 1000) / 1000) / 1000, 4);
        }
    }
}
