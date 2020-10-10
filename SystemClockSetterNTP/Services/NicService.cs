using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SystemClockSetterNTP.Services
{
    public class NicService : INicService
    {
        public double GigabytesSent { get; set; } = 0;
        public double GigabytesReceived { get; set; } = 0;


        private List<PerformanceCounter> receivePerformanceCounters = new List<PerformanceCounter>();
        private List<PerformanceCounter> sentPerformanceCounters = new List<PerformanceCounter>();


        public void InitializeNICs()
        {
            foreach (var nic in new PerformanceCounterCategory("Network Interface").GetInstanceNames())
            {
                sentPerformanceCounters.Add(new PerformanceCounter("Network Interface", "Bytes Sent/sec", nic));
                receivePerformanceCounters.Add(new PerformanceCounter("Network Interface", "Bytes Received/sec", nic));
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
