﻿using System;
using System.Threading.Tasks;
using SystemClockSetterNTP.Models;
using Microsoft.Extensions.Logging;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using System.Diagnostics;

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

        public event EventHandler UserActivityDetected;

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
            Environment.Exit(1);
        }

        public void ApplicationStartup()
        {
            _windowService.WindowServiceStartup();

            if (_timeService.IsComputerTimeCorrect())
            {
                _logger.LogDebug("Time is correnct, no need to set it up once again");
            }
            else
            {
                _logger.LogDebug($"Selected date format: {_dateAndTimeFormat.DateFormat}, time format: {_dateAndTimeFormat.TimeFormat}");

                try
                {
                    _timeService.SetSystemClock().GetAwaiter().GetResult();
                }

                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error setting system time, time hasn't been set");

                    if (_windowConfiguration.Beep)
                    {
                        Console.Beep(_windowConfiguration.FailureBeepFrequency, _windowConfiguration.FailureBeepDuration);
                    }

                    PrintErrorSettingUpSystemTimeAsync().GetAwaiter().GetResult();
                }
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
            }
        }
    }
}
