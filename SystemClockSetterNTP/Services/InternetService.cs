using Microsoft.Extensions.Logging;
using System.Net;

namespace SystemClockSetterNTP.Services
{
    public class InternetService : IInternetService
    {
        private readonly ILogger<InternetService> _logger;

        public InternetService(ILogger<InternetService> logger)
        {
            _logger = logger;
        }

        public bool IsInternetConnectionAvailable()
        {
            try
            {
                var s = new WebClient().OpenRead("http://google.com/generate_204");

                _logger.LogDebug("Internet connection available");

                return true;
            }
            catch
            {
                _logger.LogDebug("Internet connection unavailable");

                return false;
            }
        }
    }
}
