using System;
using System.Threading.Tasks;
using SystemClockSetterNTP.Models;
using Microsoft.Extensions.Logging;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using System.Diagnostics;
using Microsoft.Extensions.Hosting;

namespace SystemClockSetterNTP.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly ILogger<ApplicationService> _logger;
        private readonly ITimeService _timeService;
        private readonly IWindowService _windowService;
        private readonly IHostApplicationLifetime _applicationLifetime;

        private readonly DateAndTimeFormat _dateAndTimeFormat;
        private readonly WindowConfiguration _windowConfiguration;
        private readonly ApplicationConfiguration _applicationConfiguration;

        public event EventHandler UserActivityDetected;

        public ApplicationService(ILogger<ApplicationService> logger, ITimeService timeService,
            IWindowService windowService, DateAndTimeFormat dateAndTimeFormat, WindowConfiguration windowConfiguration,
            ApplicationConfiguration applicationConfiguration, IHostApplicationLifetime applicationLifetime)
        {
            _logger = logger;
            _timeService = timeService;
            _windowService = windowService;
            _dateAndTimeFormat = dateAndTimeFormat;
            _windowConfiguration = windowConfiguration;
            _applicationConfiguration = applicationConfiguration;
            _applicationLifetime = applicationLifetime;
        }

        public void ApplicationShutdown()
        {
            _applicationLifetime.StopApplication();
        }

        public void ApplicationStartup()
        {
            _windowService.WindowServiceStartup();

            Wait().GetAwaiter().GetResult();

            try
            {
                if (_timeService.IsComputerTimeCorrect())
                {
                    _logger.LogDebug("Time is correnct, no need to set it up once again");

                    if (!_applicationConfiguration.UserActivityIntegration)
                    {
                        ApplicationShutdown();
                    }
                }

                else
                {
                    _logger.LogDebug($"Selected date format: {_dateAndTimeFormat.DateFormat}, time format: {_dateAndTimeFormat.TimeFormat}");

                    _timeService.SetSystemClock().GetAwaiter().GetResult();

                    if (!_applicationConfiguration.UserActivityIntegration)
                    {
                        ApplicationShutdown();
                    }
                }
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occured during time operation, time hasn't been set");

                if (_windowConfiguration.Beep)
                {
                    Console.Beep(_windowConfiguration.FailureBeepFrequency, _windowConfiguration.FailureBeepDuration);
                }

                PrintErrorSettingUpSystemTimeAsync().GetAwaiter().GetResult();

                if (!_applicationConfiguration.UserActivityIntegration)
                {
                    ApplicationShutdown();
                }
            }
        }

        private async Task Wait()
        {
            int startupDelay = _applicationConfiguration.StartupDelayInSeconds;

            if (startupDelay > 0)
            {
                _logger.LogDebug($"Startup delay: {startupDelay} second(s)");

                await Task.Delay(startupDelay * 1000);

                _logger.LogDebug("Startup delay passed, continue work");
            }
        }

        private void OnMouseActivityDetected(object sender, MouseEventArgs e)
        {
            UserActivityDetected?.Invoke(this, null);
        }

        private void OnKeyboardActivityDetected(object sender, KeyPressEventArgs e)
        {
            UserActivityDetected?.Invoke(this, null);
        }

        public async Task PrintErrorSettingUpSystemTimeAsync()
        {
            Console.WriteLine(new string('\n', 5));
            Console.WriteLine("Wystąpił błąd podczas ustawiania zegara");
            Console.WriteLine("Zegar nie został ustawiony");
            Console.WriteLine(new string('\n', 5));

            await Task.Delay(_applicationConfiguration.ErrorMessageSecondTime * 1000);
        }

        public Task HookUserActivity() => Task.Run(() =>
        {
            Hook.GlobalEvents().KeyPress += OnKeyboardActivityDetected;
            Hook.GlobalEvents().MouseWheel += OnMouseActivityDetected;
            Hook.GlobalEvents().MouseMove += OnMouseActivityDetected;
            Hook.GlobalEvents().MouseClick += OnMouseActivityDetected;

            Application.Run();
        });

        public Task UnhookUserActivity() => Task.Run(() =>
        {
            Hook.GlobalEvents().KeyPress -= OnKeyboardActivityDetected;
            Hook.GlobalEvents().MouseWheel -= OnMouseActivityDetected;
            Hook.GlobalEvents().MouseMove -= OnMouseActivityDetected;
            Hook.GlobalEvents().MouseClick -= OnMouseActivityDetected;

            Application.Run();
        });

        public void TurnOffComputer()
        {
            try
            {
                _logger.LogDebug("Turning off computer");

                ProcessStartInfo processStartInfo = new ProcessStartInfo("shutdown", "/s /t 0");
                processStartInfo.CreateNoWindow = true;
                processStartInfo.UseShellExecute = false;
                Process.Start(processStartInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occured during turning off computer");

                ApplicationShutdown();
            }
        }
    }
}
