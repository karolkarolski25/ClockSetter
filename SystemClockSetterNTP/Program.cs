using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SystemClockSetterNTP.Models;
using SystemClockSetterNTP.Services;
using SystemClockSetterNTP.Storage;

namespace SystemClockSetterNTP
{
    class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
                .ConfigureLogging((hostContext, loggingBuilder) =>
                {
                    loggingBuilder.AddFile(hostContext.Configuration.GetSection("Logging"));
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services
                        .AddHostedService<Worker>()
                        .AddSingleton(hostContext.Configuration.GetSection("NtpConfiguration").Get<NtpConfiguration>())
                        .AddSingleton(hostContext.Configuration.GetSection("WindowConfiguration").Get<WindowConfiguration>())
                        .AddSingleton(hostContext.Configuration.GetSection("DateAndTimeFormat").Get<DateAndTimeFormat>())
                        .AddSingleton(hostContext.Configuration.GetSection("ApplicationConfiguration").Get<ApplicationConfiguration>())
                        .AddSingleton<IApplicationService, ApplicationService>()
                        .AddSingleton<ITimeService, TimeService>()
                        .AddSingleton<IStopwatchService, StopwatchService>()
                        .AddSingleton<IStorageService, StorageService>()
                        .AddSingleton<IWindowService, WindowService>()
                        .AddDbContext<ComputerDataContext>();
                });
    }
}
