using System.Threading.Tasks;

namespace SystemClockSetterNTP.Services
{
    public interface IApplicationService
    {
        void ApplicationShutdown();
        void ApplicationStartup();
        Task PrintErrorSettingUpSystemTimeAsync();
    }
}
