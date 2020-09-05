using System;
using System.Threading.Tasks;
using SystemClockSetterNTP.Models;
using Microsoft.Extensions.Logging;

namespace SystemClockSetterNTP.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly ILogger<ApplicationService> _logger;
        private readonly ITimeService _timeService;
        private readonly IWindowService _windowService;
        private readonly DateAndTimeFormat _dateAndTimeFormat;
        private readonly WindowConfiguration _windowConfiguration;
        private readonly ApplicationConfiguration _applicationConfiguration;

        public ApplicationService(ILogger<ApplicationService> logger, ITimeService timeService,
            IWindowService windowService, DateAndTimeFormat dateAndTimeFormat, WindowConfiguration windowConfiguration, 
            ApplicationConfiguration applicationConfiguration)
        {
            _logger = logger;
            _timeService = timeService;
            _windowService = windowService;
            _dateAndTimeFormat = dateAndTimeFormat;
            _windowConfiguration = windowConfiguration;
            _applicationConfiguration = applicationConfiguration;
        }

        public void ApplicationShutdown()
        {
            _logger.LogDebug("Application shutdown");
            _logger.LogDebug("");

            Environment.Exit(1);
        }

        public void ApplicationStartup()
        {
            _logger.LogDebug("Application startup");

            _windowService.WindowServiceStartup();

            if (_timeService.IsComputerTimeCorrect())
            {
                _logger.LogDebug("Time is correnct, no need to set it up once again");

                ApplicationShutdown();
            }
            else
            {
                _logger.LogDebug($"Selected date format: {_dateAndTimeFormat.DateFormat}, time format: {_dateAndTimeFormat.TimeFormat}");

                string networkTime = _timeService.GetNetworkTime();

                if (networkTime != null)
                {
                    _timeService.SetSystemClock(networkTime).GetAwaiter().GetResult();

                    ApplicationShutdown();
                }
                else
                {
                    _logger.LogDebug("Error setting system time, time hasn't been set");

                    if (_windowConfiguration.Beep)
                    {
                        Console.Beep(_windowConfiguration.FailureBeepFrequency, _windowConfiguration.FailureBeepDuration);
                    }

                    PrintErrorSettingUpSystemTimeAsync().GetAwaiter().GetResult();

                    ApplicationShutdown();
                }
            }
        }

        public async Task PrintErrorSettingUpSystemTimeAsync()
        {
            Console.WriteLine(new string('\n', 5));
            Console.WriteLine("Wystąpił błąd podczas ustawiania zegara");
            Console.WriteLine("Zegar nie został ustawiony");
            Console.WriteLine(new string('\n', 5));

            await Task.Delay(_applicationConfiguration.ErrorMessageTime);
        }
    }
}
