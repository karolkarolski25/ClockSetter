using Gma.System.MouseKeyHook;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using SystemClockSetterNTP.Models;

namespace SystemClockSetterNTP.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly ILogger<ApplicationService> _logger;
        private readonly ITimeService _timeService;
        private readonly IWindowService _windowService;
        private readonly IStopwatchService _stopwatchService;
        private readonly IStorageService _storageService;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        private IKeyboardMouseEvents _keyboardMouseEvents;

        private readonly DateAndTimeFormat _dateAndTimeFormat;
        private readonly WindowConfiguration _windowConfiguration;
        private readonly ApplicationConfiguration _applicationConfiguration;

        public event EventHandler UserActivityDetected;

        public ApplicationService(ILogger<ApplicationService> logger, ITimeService timeService,
            IWindowService windowService, DateAndTimeFormat dateAndTimeFormat, WindowConfiguration windowConfiguration,
            ApplicationConfiguration applicationConfiguration, IStopwatchService stopwatchService, IHostApplicationLifetime hostApplicationLifetime,
            IStorageService storageService)
        {
            _logger = logger;
            _timeService = timeService;
            _windowService = windowService;
            _dateAndTimeFormat = dateAndTimeFormat;
            _windowConfiguration = windowConfiguration;
            _applicationConfiguration = applicationConfiguration;
            _stopwatchService = stopwatchService;
            _hostApplicationLifetime = hostApplicationLifetime;
            _storageService = storageService;

            _hostApplicationLifetime.ApplicationStopping.Register(() =>
            {
                _stopwatchService.StopTimer();
                _logger.LogDebug("Shutting down application");
            });
        }

        public void ApplicationShutdown()
        {
            Application.Exit();
        }

        public void ApplicationStartup()
        {
            Task.Run(() => _storageService.MigrateAsync());

            Task stopwatchTask = new Task(() =>
            {
                _stopwatchService.StartTimer();
                _stopwatchService.RunTimer();
            });

            stopwatchTask.Start();

            if (_windowConfiguration.ChangeWindowDimensions)
            {
                _windowService.WindowServiceStartup();
            }

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
                    else
                    {
                        _logger.LogDebug("Monitoring user activity started");
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
                    else
                    {
                        _logger.LogDebug("Monitoring user activity started");
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
                else
                {
                    _logger.LogDebug("Monitoring user activity started");
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
            else
            {
                _logger.LogDebug("Startup delay has not been given, passing");
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

        public Task HookUserActivity() => Task.Factory.StartNew(() =>
        {
            _keyboardMouseEvents = Hook.GlobalEvents();

            _keyboardMouseEvents.KeyPress += OnKeyboardActivityDetected;
            _keyboardMouseEvents.MouseWheel += OnMouseActivityDetected;
            _keyboardMouseEvents.MouseMove += OnMouseActivityDetected;
            _keyboardMouseEvents.MouseClick += OnMouseActivityDetected;


            Application.Run();

        });

        public void UnhookUserActivity()
        {
            if (_keyboardMouseEvents == null) return;

            _keyboardMouseEvents.KeyPress -= OnKeyboardActivityDetected;
            _keyboardMouseEvents.MouseWheel -= OnMouseActivityDetected;
            _keyboardMouseEvents.MouseMove -= OnMouseActivityDetected;
            _keyboardMouseEvents.MouseClick -= OnMouseActivityDetected;

            _keyboardMouseEvents.Dispose();

            _keyboardMouseEvents = null;
        }

        public void TurnOffComputer()
        {
            if (_applicationConfiguration.TurnOffComputerAfterTimeExceeded)
            {
                try
                {
                    _logger.LogDebug("Turning off computer");

                    ProcessStartInfo processStartInfo = new ProcessStartInfo("shutdown", "/s /t 0")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };

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
}
