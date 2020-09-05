using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using SystemClockSetterNTP.Models;
using SystemClockSetterNTP.Services;
using System.Threading.Tasks;
using System.Threading;
using Timer = System.Timers.Timer;

namespace SystemClockSetterNTP
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IApplicationService _applicationService;
        private readonly ApplicationConfiguration _applicationConfiguration;

        private Timer _checkUserActivityForTimer = new Timer();

        public Worker(ILogger<Worker> logger, IApplicationService applicationService, ApplicationConfiguration applicationConfiguration)
        {
            _logger = logger;
            _applicationService = applicationService;
            _applicationConfiguration = applicationConfiguration;
        }

        private void StartTimers()
        {
            _checkUserActivityForTimer.Interval = (_applicationConfiguration.CheckUserActivityForMinuteTime * 60) * 1000;
            _checkUserActivityForTimer.Elapsed += CheckUserActivityForTimer_Elapsed;
            _checkUserActivityForTimer.Enabled = true;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {      
            _logger.LogDebug("Application startup");
            _logger.LogDebug($"Integration with user activity: {_applicationConfiguration.UserActivityIntegration}");

            try
            {
                _applicationService.ApplicationStartup();

                if (_applicationConfiguration.UserActivityIntegration)
                {
                    _applicationService.UserActivityDetected += OnUserActivityDetected;

                    await _applicationService.HookUserActivity();

                    StartTimers();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception occured during startup");

                _applicationService.ApplicationShutdown();
            }
        }

        private void CheckUserActivityForTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _logger.LogDebug("Time for checking user activity exceeded");

            _applicationService.TurnOffComputer();
        }

        private void OnUserActivityDetected(object sender, EventArgs e)
        {
            try
            {
                _logger.LogDebug("User activity detected, shutting down application");

                _checkUserActivityForTimer.Stop();

                Dispose();

                _applicationService.ApplicationShutdown();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occured during application shutdown");
            }
        }

        public override void Dispose()
        {
            _checkUserActivityForTimer.Dispose();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}
