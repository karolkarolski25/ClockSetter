using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SystemClockSetterNTP.Models;
using SystemClockSetterNTP.Storage;
using SystemClockSetterNTP.Storage.Services;

namespace SystemClockSetterNTP.Services
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
        private int powerOnCount;

        public StopwatchService(ILogger<StopwatchService> logger, IStorageService storageService, 
            INicService nicService, ApplicationConfiguration applicationConfiguration)
        {
            _logger = logger;
            _storageService = storageService;
            _nicService = nicService;
            _applicationConfiguration = applicationConfiguration;
        }

        public void ReadTimeAndDateFromDataBase()
        {
            var computerData = _storageService.GetComputerDatasListAsync().Result.FirstOrDefault(e => e.Date == DateTime.Now.ToString("dd.MM.yyyy"));

            if (computerData != null)
            {
                timeElapsed = TimeSpan.Parse(computerData.Time);
                currentDate = Convert.ToDateTime(computerData.Date);
                powerOnCount = computerData.PowerOnCount;
                _nicService.GigabytesReceived = computerData.GigabytesReceived;
                _nicService.GigabytesSent = computerData.GigabytesSent;
            }
            else
            {
                timeElapsed = new TimeSpan(0, 0, 0);
                currentDate = DateTime.Now.Date;
                powerOnCount = 0;
                _nicService.GigabytesSent = 0;
                _nicService.GigabytesReceived = 0;
            }
        }

        public async Task RunTimer()
        {
            while (true)
            {
                await Task.Delay(1000);

                if (Math.Abs((DateTime.Now.Date - currentDate).TotalDays) > 0)
                {
                    SaveOrEditTime();

                    timeElapsed = new TimeSpan(0, 0, 0);
                    currentDate = DateTime.Now.Date;
                    powerOnCount = 0;
                    _nicService.GigabytesSent = 0;
                    _nicService.GigabytesReceived = 0;

                    stopwatch.Reset();
                }

                totalElapsedTime = stopwatch.Elapsed + timeElapsed;
            }
        }

        public void SaveOrEditTime()
        {
            _logger.LogDebug($"Elapsed time: {new DateTime(stopwatch.ElapsedTicks):HH:mm:ss}");

            var computerDataToEdit = new ComputerData()
            {
                Date = currentDate.ToString("dd.MM.yyyy"),
                Time = new DateTime(totalElapsedTime.Ticks).ToString("HH:mm:ss"),
                PowerOnCount = ++powerOnCount,
                GigabytesReceived = Math.Round(_nicService.GigabytesReceived, 4),
                GigabytesSent = Math.Round(_nicService.GigabytesSent, 4)
            };

            var computerData = _storageService.GetComputerDatasListAsync().Result.FirstOrDefault(e => e.Date == computerDataToEdit.Date);

            if (computerData != null)
            {
                _storageService.EditComputerData(computerDataToEdit);
            }
            else
            {
                _storageService.AddComputerDataAsync(computerDataToEdit);
            }
        }

        public void StartTimer()
        {
            _logger.LogDebug("Starting stopwatch");

            ReadTimeAndDateFromDataBase();

            if (_applicationConfiguration.CountNetworkActivity)
            {
                Task.Run(() =>
                {
                    _nicService.InitializeNICs();
                    _nicService.RunMonitoringNics();
                });
            }

            stopwatch.Start();
        }

        public void StopTimer()
        {
            _logger.LogDebug("Stopping stopwatch");

            stopwatch.Stop();

            SaveOrEditTime();
        }
    }
}
