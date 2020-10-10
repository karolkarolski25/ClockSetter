using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SystemClockSetterNTP.Models;
using SystemClockSetterNTP.NetworkActivity.Services;
using SystemClockSetterNTP.Storage.Models;
using SystemClockSetterNTP.Storage.Services;

namespace SystemClockSetterNTP.SystemStopwatch.Services
{
    public class StopwatchService : IStopwatchService
    {
        private readonly ILogger<StopwatchService> _logger;
        private readonly IStorageService _storageService;
        private readonly INicService _nicService;
        private readonly ApplicationConfiguration _applicationConfiguration;

        private readonly Stopwatch stopwatch = new Stopwatch();
        private TimeSpan timeElapsed;
        private TimeSpan totalElapsedTime;
        private DateTime currentDate;
        private int? powerOnCount;

        public StopwatchService(ILogger<StopwatchService> logger, IStorageService storageService,
            INicService nicService, ApplicationConfiguration applicationConfiguration)
        {
            _logger = logger;
            _storageService = storageService;
            _nicService = nicService;
            _applicationConfiguration = applicationConfiguration;
        }

        public void ReadStopwatchDataFromDatabase()
        {
            var computerData = _storageService.GetComputerDatasListAsync().Result.FirstOrDefault(e => e.Date == DateTime.Now.ToString("dd.MM.yyyy"));

            if (computerData != null)
            {
                timeElapsed = TimeSpan.Parse(computerData.Time);
                currentDate = Convert.ToDateTime(computerData.Date);
                powerOnCount = computerData.PowerOnCount;
            }
            else
            {
                timeElapsed = new TimeSpan(0, 0, 0);
                currentDate = DateTime.Now.Date;
                powerOnCount = 0;
            }
        }

        public async Task RunTimer()
        {
            while (true)
            {
                if (Math.Abs((DateTime.Now.Date - currentDate).TotalDays) > 0)
                {
                    UpdateStopwatchData();

                    timeElapsed = new TimeSpan(0, 0, 0);
                    currentDate = DateTime.Now.Date;
                    powerOnCount = 0;
                    _nicService.GigabytesSent = 0;
                    _nicService.GigabytesReceived = 0;

                    stopwatch.Reset();
                }

                _nicService.StartNicsMonitoring();

                totalElapsedTime = stopwatch.Elapsed + timeElapsed;

                await Task.Delay(1000);
            }
        }

        public void UpdateStopwatchData()
        {
            _logger.LogDebug($"Elapsed time: {new DateTime(stopwatch.ElapsedTicks):HH:mm:ss}");

            var computerData = new ComputerData()
            {
                Date = currentDate.ToString("dd.MM.yyyy"),
                Time = new DateTime(totalElapsedTime.Ticks).ToString("HH:mm:ss"),
                PowerOnCount = ++powerOnCount,
            };

            _storageService.UpdateData(computerData);
        }

        public void StartTimer()
        {
            _logger.LogDebug("Starting stopwatch");

            ReadStopwatchDataFromDatabase();

            stopwatch.Start();
        }

        public void StopTimer()
        {
            _logger.LogDebug("Stopping stopwatch");

            stopwatch.Stop();

            UpdateStopwatchData();
        }
    }
}
