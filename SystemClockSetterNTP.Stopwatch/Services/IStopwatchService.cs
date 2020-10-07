using System.Threading.Tasks;

namespace SystemClockSetterNTP.StopwatchLibrary.Services
{
    public interface IStopwatchService
    {
        void ReadTimeAndDateFromDataBase();
        void SaveTime();
        void StartTimer();
        void StopTimer();
        Task RunTimer();
    }
}
