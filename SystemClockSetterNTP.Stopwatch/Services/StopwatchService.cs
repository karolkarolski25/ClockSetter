using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SystemClockSetterNTP.StopwatchLibrary.Services
{
    public class StopwatchService : IStopwatchService
    {
        private readonly ILogger<StopwatchService> _logger;

        private readonly Stopwatch stopwatch = new Stopwatch();
        private TimeSpan timeElapsed;
        private TimeSpan totalElapsedTime;
        private DateTime currentDate;

        public StopwatchService(ILogger<StopwatchService> logger)
        {
            _logger = logger;
        }

        public void ReadTimeAndDateFromDataBase()
        {
            timeElapsed = new TimeSpan(4, 1, 8);
            currentDate = new DateTime(2020, 3, 20);
        }

        public async Task RunTimer()
        {
            while (true)
            {
                await Task.Delay(1000);

                if (Math.Abs((DateTime.Now.Date - currentDate).TotalDays) > 0)
                {
                    SaveTime();

                    totalElapsedTime = new TimeSpan();
                    timeElapsed = new TimeSpan();
                    currentDate = DateTime.Now.Date;
                }

                totalElapsedTime = stopwatch.Elapsed + timeElapsed;
            }
        }

        public void SaveTime()
        {
            var time = new DateTime(totalElapsedTime.Ticks).ToString("HH:mm:ss");

            _logger.LogDebug($"{currentDate} - elapsed time: {time}");
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

            SaveTime();
        }
    }
}
