using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Timers;

namespace SystemClockSetterNTP.Services
{
    public class InternetService : IInternetService
    {
        private readonly ILogger<InternetService> _logger;

        private readonly Timer periodicInternetCheckingTimer = new Timer();

        public event EventHandler InternetConnectionAvailable;

        public InternetService(ILogger<InternetService> logger)
        {
            _logger = logger;          
        }

        public bool IsInternetConnectionAvailable()
        {
            try
            {
                var connectionChecker = new WebClient().OpenRead("http://google.com/generate_204");
                return true;
            }
            catch
            {
                _logger.LogDebug("Internet connection unavailable");

                return false;
            }
        }

        public void CheckInternetConnectionPerdiodically()
        {
            periodicInternetCheckingTimer.Interval = 20000;
            periodicInternetCheckingTimer.Elapsed += PeriodicInternetCheckingTimer_Elapsed;
            periodicInternetCheckingTimer.Enabled = true;
        }

        private void PeriodicInternetCheckingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (IsInternetConnectionAvailable())
            {
                _logger.LogDebug("Internet connection has been estabilished, trying to set up system clock again");

                periodicInternetCheckingTimer.Stop();

                InternetConnectionAvailable?.Invoke(this, null);
            }
        }
    }
}
