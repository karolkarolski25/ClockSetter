using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SystemClockSetterNTP.Services;
using SystemClockSetterNTP.Models;
using Microsoft.Extensions.Configuration;

namespace SystemClockSetterNTP
{
    class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Services.GetRequiredService<IApplicationService>().ApplicationStartup();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
                .ConfigureLogging((hostContext, loggingBuilder) =>
                {
                    loggingBuilder.AddFile(hostContext.Configuration.GetSection("Logging"));
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services
                        .AddSingleton(hostContext.Configuration.GetSection("NtpConfiguration").Get<NtpConfiguration>())
                        .AddSingleton(hostContext.Configuration.GetSection("WindowConfiguration").Get<WindowConfiguration>())
                        .AddSingleton(hostContext.Configuration.GetSection("DateAndTimeFormat").Get<DateAndTimeFormat>())
                        .AddSingleton(hostContext.Configuration.GetSection("ApplicationConfiguration").Get<ApplicationConfiguration>())
                        .AddSingleton<IApplicationService, ApplicationService>()
                        .AddSingleton<ITimeService, TimeService>()
                        .AddSingleton<IWindowService, WindowService>();
                });
    }
}
