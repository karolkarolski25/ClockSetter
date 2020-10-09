using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SystemClockSetterNTP.Storage;

namespace SystemClockSetterNTP.Services
{
    public class StopwatchService : IStopwatchService
    {
        private readonly ILogger<StopwatchService> _logger;
        private readonly IStorageService _storageService;

        private readonly Stopwatch stopwatch = new Stopwatch();
        private TimeSpan timeElapsed;
        private TimeSpan totalElapsedTime;
        private DateTime currentDate;
        private int powerOnCount;

        public StopwatchService(ILogger<StopwatchService> logger, IStorageService storageService)
        {
            _logger = logger;
            _storageService = storageService;
        }

        public void ReadTimeAndDateFromDataBase()
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
                await Task.Delay(1000);

                if (Math.Abs((DateTime.Now.Date - currentDate).TotalDays) > 0)
                {
                    SaveOrEditTime();

                    timeElapsed = new TimeSpan(0, 0, 0);
                    currentDate = DateTime.Now.Date;
                    powerOnCount = 0;

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
                PowerOnCount = ++powerOnCount
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
