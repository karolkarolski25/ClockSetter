using System;
using System.Threading.Tasks;

namespace SystemClockSetterNTP.Services
{
    public interface IStopwatchService
    {
        void ReadTimeFromDataBase();
        void SaveTime();
        void StartTimer();
        void StopTimer();
        Task RunTimer();
    }
}
