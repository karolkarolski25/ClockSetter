using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
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
        private ComputerData stopwatchData;

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
            stopwatchData = _storageService.GetComputerDatasListAsync().Result.FirstOrDefault(e => e.Date == DateTime.Now.ToString("dd.MM.yyyy"));

            if (stopwatchData != null)
            {
                timeElapsed = TimeSpan.Parse(stopwatchData.Time);
                currentDate = Convert.ToDateTime(stopwatchData.Date);
                powerOnCount = stopwatchData.PowerOnCount;
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

                totalElapsedTime = stopwatch.Elapsed + timeElapsed;

                await Task.Delay(1000);
            }
        }

        public void UpdateStopwatchData()
        {
            _logger.LogDebug($"Elapsed time: {new DateTime(stopwatch.ElapsedTicks):HH:mm:ss}");

            var stopwatchDataToUpdate = new ComputerData();

            if (stopwatchData != null)
            {
                stopwatchDataToUpdate.Date = currentDate.ToString("dd.MM.yyyy");
                stopwatchDataToUpdate.Time = new DateTime(totalElapsedTime.Ticks).ToString("HH:mm:ss");
                stopwatchDataToUpdate.PowerOnCount = ++powerOnCount;
            }
            else
            {
                stopwatchDataToUpdate.Date = DateTime.Now.Date.ToString("dd.MM.yyyy");
                stopwatchDataToUpdate.Time = "00:00:00";
                stopwatchDataToUpdate.PowerOnCount = 1;
                stopwatchDataToUpdate.GigabytesReceived = 0;
                stopwatchDataToUpdate.GigabytesSent = 0;
            }

            _storageService.UpdateData(stopwatchDataToUpdate);
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
