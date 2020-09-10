using System;
using System.Threading.Tasks;

namespace SystemClockSetterNTP.Services
{
    public interface IApplicationService
    {
        void ApplicationShutdown();
        void ApplicationStartup();
        void TurnOffComputer();
        Task PrintErrorSettingUpSystemTimeAsync();
        Task HookUserActivity();
        void UnhookUserActivity();

        event EventHandler UserActivityDetected;
    }
}
