using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using SystemClockSetterNTP.Models;
using SystemClockSetterNTP.Services;

namespace SystemClockSetterNTP
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IApplicationService _applicationService;
        private readonly ApplicationConfiguration _applicationConfiguration;

        public Worker(ILogger<Worker> logger, IApplicationService applicationService, ApplicationConfiguration applicationConfiguration)
        {
            _logger = logger;
            _applicationService = applicationService;
            _applicationConfiguration = applicationConfiguration;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Application startup");

            try
            {
                _applicationService.UserActivityDetected += OnUserActivityDetected;

                await _applicationService.HookUserActivity();
                //_applicationService.ApplicationStartup();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception occured during startup");

                throw;
            }
        }

        private void OnUserActivityDetected(object sender, EventArgs e)
        {
            try
            {
                //_applicationService.ApplicationShutdown();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occured during application shutdown");
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}
