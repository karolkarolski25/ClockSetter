using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SystemClockSetterNTP.Storage.Models;
using SystemClockSetterNTP.Storage.Services;

namespace SystemClockSetterNTP.NetworkActivity.Services
{
    public class NicService : INicService
    {
        public double? GigabytesSent { get; set; }
        public double? GigabytesReceived { get; set; }

        private readonly List<PerformanceCounter> receivePerformanceCounters = new List<PerformanceCounter>();
        private readonly List<PerformanceCounter> sentPerformanceCounters = new List<PerformanceCounter>();

        private ComputerData nicData;
        private DateTime currentDate;
        private bool monitorNetworkActivity = false;

        private readonly ILogger<NicService> _logger;
        private readonly IStorageService _storageService;

        public NicService(ILogger<NicService> logger, IStorageService storageService)
        {
            _logger = logger;
            _storageService = storageService;
        }

        private void ReadNicDataFromDatabase()
        {
            nicData = _storageService.GetComputerDatasListAsync().Result.FirstOrDefault(e => e.Date == DateTime.Now.ToString("dd.MM.yyyy"));

            if (nicData != null)
            {
                currentDate = Convert.ToDateTime(nicData.Date);
                GigabytesReceived = nicData.GigabytesReceived;
                GigabytesSent = nicData.GigabytesSent;
            }
            else
            {
                currentDate = DateTime.Now.Date;
                GigabytesSent = 0;  
                GigabytesReceived = 0;

                nicData = new ComputerData()
                {
                    Date = currentDate.ToString("dd.MM.yyyy"),
                    GigabytesReceived = GigabytesReceived,
                    GigabytesSent = GigabytesSent
                };
            }
        }

        public void UpdateNicData()
        {
            _logger.LogDebug($"Gigabytes received and sent: {GigabytesReceived} {GigabytesSent}");

            var nicDataToUpdate = new ComputerData();

            if (nicData != null)
            {
                nicDataToUpdate.Date = currentDate.ToString("dd.MM.yyyy");
                nicDataToUpdate.GigabytesSent = (double?)Math.Round((decimal)GigabytesSent, 4);
                nicDataToUpdate.GigabytesReceived = (double?)Math.Round((decimal)GigabytesReceived, 4);
            }
            else
            {
                nicDataToUpdate.Date = DateTime.Now.Date.ToString("dd.MM.yyyy");
                nicDataToUpdate.Time = "00:00:00";
                nicDataToUpdate.PowerOnCount = 1;
                nicDataToUpdate.GigabytesSent = (double?)Math.Round((decimal)GigabytesSent, 4);
                nicDataToUpdate.GigabytesReceived = (double?)Math.Round((decimal)GigabytesReceived, 4);
            }

            _storageService.UpdateData(nicDataToUpdate);
        }

        public void InitializeNICs()
        {
            var nics = new PerformanceCounterCategory("Network Interface").GetInstanceNames();

            if (nics.Any())
            {
                foreach (var nic in nics)
                {
                    sentPerformanceCounters.Add(new PerformanceCounter("Network Interface", "Bytes Sent/sec", nic));
                    receivePerformanceCounters.Add(new PerformanceCounter("Network Interface", "Bytes Received/sec", nic));
                }

                ReadNicDataFromDatabase();
            }
            else
            {
                _logger.LogDebug("No NICs found");
            }
        }

        public async void StartNicsMonitoring()
        {
            _logger.LogDebug("Starting monitoring network activity");
            
            monitorNetworkActivity = true;

            do
            {
                if (Math.Abs((currentDate - DateTime.Now.Date).TotalDays) > 0)
                {
                    UpdateNicData();

                    GigabytesSent = 0;
                    GigabytesReceived = 0;

                    currentDate = DateTime.Now.Date;
                }

                foreach (var nic in sentPerformanceCounters)
                {
                    GigabytesSent += ConvertToGigabyte(nic.NextValue());
                }

                foreach (var nic in receivePerformanceCounters)
                {
                    GigabytesReceived += ConvertToGigabyte(nic.NextValue());
                }

                await Task.Delay(1000);

            } while (monitorNetworkActivity);
        }

        public void StopNicsMonitoring()
        {
            monitorNetworkActivity = false;

            _logger.LogDebug("Stopping monitoring network activity");

            UpdateNicData();
        }

        private double ConvertToGigabyte(double dataToConvert)
        {
            return Math.Round(((dataToConvert / 1000) / 1000) / 1000, 4);
        }
    }
}
