using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using SystemClockSetterNTP.Models;
using SystemClockSetterNTP.Services;
using Timer = System.Timers.Timer;

namespace SystemClockSetterNTP
{
    public sealed class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IApplicationService _applicationService;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ApplicationConfiguration _applicationConfiguration;

        private readonly Timer _checkUserActivityForTimer = new Timer();

        public Worker(ILogger<Worker> logger, IApplicationService applicationService, 
            ApplicationConfiguration applicationConfiguration,IHostApplicationLifetime hostApplicationLifetime)
        {
            _logger = logger;
            _applicationService = applicationService;
            _applicationConfiguration = applicationConfiguration;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        private void StartTimers()
        {
            _logger.LogDebug($"Counting started, waiting {_applicationConfiguration.CheckUserActivityForMinuteTime} minute(s) before turning off computer");

            _checkUserActivityForTimer.Interval = (_applicationConfiguration.CheckUserActivityForMinuteTime * 60) * 1000;
            _checkUserActivityForTimer.Elapsed += CheckUserActivityForTimer_Elapsed;
            _checkUserActivityForTimer.Enabled = true;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Application startup");
            _logger.LogDebug($"Integration with user activity: {_applicationConfiguration.UserActivityIntegration}");
            _logger.LogDebug($"Count system time: {_applicationConfiguration.CountSystemRunningTime}");
            _logger.LogDebug($"Shutting down computer after time exceeded: {_applicationConfiguration.TurnOffComputerAfterTimeExceeded}");

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

            finally
            {
                _hostApplicationLifetime.StopApplication();
            }
        }

        private void CheckUserActivityForTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _logger.LogDebug("Time for checking user activity exceeded, no user activity detected");

            _applicationService.TurnOffComputer();
        }

        private void OnUserActivityDetected(object sender, EventArgs e)
        {
            try
            {
                _applicationService.UnhookUserActivity();

                Dispose();

                _logger.LogDebug("User activity detected");

                _checkUserActivityForTimer.Stop();
                _logger.LogDebug("Timers stopped");

                if (!_applicationConfiguration.CountSystemRunningTime)
                {
                    _applicationService.ApplicationShutdown();
                }
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
