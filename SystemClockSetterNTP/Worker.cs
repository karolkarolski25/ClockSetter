using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using SystemClockSetterNTP.Models;
using SystemClockSetterNTP.Services;
using System.Threading.Tasks;
using System.Threading;
using Timer = System.Timers.Timer;
using System.Configuration;

namespace SystemClockSetterNTP
{
    public sealed class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IApplicationService _applicationService;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ApplicationConfiguration _applicationConfiguration;

        private readonly Timer _checkUserActivityForTimer = new Timer();

        public Worker(ILogger<Worker> logger, IApplicationService applicationService, ApplicationConfiguration applicationConfiguration,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            _logger = logger;
            _applicationService = applicationService;
            _applicationConfiguration = applicationConfiguration;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        private void StartTimers()
        {
            _logger.LogDebug($"Counting started, waiting {_applicationConfiguration.CheckUserActivityForMinuteTime} minutes before turning off computer");

            _checkUserActivityForTimer.Interval = (_applicationConfiguration.CheckUserActivityForMinuteTime * 60) * 1000;
            _checkUserActivityForTimer.Elapsed += CheckUserActivityForTimer_Elapsed;
            _checkUserActivityForTimer.Enabled = true;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Application startup");
                _logger.LogDebug($"Integration with user activity: {_applicationConfiguration.UserActivityIntegration}");

                try
                {
                    _applicationService.ApplicationStartup();

                    if (_applicationConfiguration.UserActivityIntegration)
                    {
                        _applicationService.UserActivityDetected += OnUserActivityDetected;

                        StartTimers();

                        await _applicationService.HookUserActivity();
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Exception occured during startup");

                    _applicationService.ApplicationShutdown();
                }
            }
            finally
            {
                _hostApplicationLifetime.StopApplication();
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
                _applicationService.UnhookUserActivity();
                Dispose();

                _logger.LogDebug("User activity detected, shutting down application requested");

                _checkUserActivityForTimer.Stop();
                _logger.LogDebug("Counting stopped");
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
