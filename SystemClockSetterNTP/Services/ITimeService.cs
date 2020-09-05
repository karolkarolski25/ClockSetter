using System.Threading.Tasks;

namespace SystemClockSetterNTP.Services
{
    public interface ITimeService
    {
        Task SetSystemClock(string networkTime);
        string GetNetworkTime();
        bool IsComputerTimeCorrect();
    }
}
