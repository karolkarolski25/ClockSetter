using System;
using System.Threading.Tasks;

namespace SystemClockSetterNTP.Services
{
    public interface ITimeService
    {
        Task SetSystemClock();
        string GetNetworkTime();
        bool IsComputerTimeCorrect();
    }
}
