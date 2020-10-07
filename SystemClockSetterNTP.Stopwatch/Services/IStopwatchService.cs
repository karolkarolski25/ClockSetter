using System.Threading.Tasks;

namespace SystemClockSetterNTP.StopwatchLibrary.Services
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
