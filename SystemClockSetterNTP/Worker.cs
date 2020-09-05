using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using SystemClockSetterNTP.Models;
using SystemClockSetterNTP.Services;

namespace SystemClockSetterNTP
{
    public class Worker : IHostedService, IDisposable
    {
        private readonly ILogger<Worker> _logger;
        private readonly IApplicationService _applicationService;
        private readonly ApplicationConfiguration _applicationConfiguration;

        private Timer _timer;

        public Worker(ILogger<Worker> logger, IApplicationService applicationService, ApplicationConfiguration applicationConfiguration)
        {
            _logger = logger;
            _applicationService = applicationService;
            _applicationConfiguration = applicationConfiguration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Application startup");

            try
            {
                _applicationService.UserActivityDetected += OnUserActivityDetected;

                await _applicationService.HookUserActivity();
                _applicationService.ApplicationStartup();

                _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception occured during startup");

                throw;
            }
        }

        private void DoWork(object state)
        {
            throw new NotImplementedException();
        }

        private void OnUserActivityDetected(object sender, EventArgs e)
        {
            try
            {
                _timer?.Change(Timeout.Infinite, 0);

                //_applicationService.ApplicationShutdown();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occured during application shutdown");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Time exceeded");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
