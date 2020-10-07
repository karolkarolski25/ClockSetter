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

        public StopwatchService(ILogger<StopwatchService> logger)
        {
            _logger = logger;
        }

        public void ReadTimeFromDataBase()
        {
            timeElapsed = new TimeSpan(4, 1, 8);
        }

        public async Task RunTimer()
        {
            while (true)
            {
                await Task.Delay(1000);

                totalElapsedTime = stopwatch.Elapsed + timeElapsed;
            }
        }

        public void SaveTime()
        {
            var time = new DateTime(totalElapsedTime.Ticks).ToString("HH:mm:ss");

            _logger.LogDebug($"Elapsed time: {time}");
        }

        public void StartTimer()
        {
            _logger.LogDebug("Starting stopwatch");

            ReadTimeFromDataBase();

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
