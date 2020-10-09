using System.Threading.Tasks;

namespace SystemClockSetterNTP.Services
{
    public interface IStopwatchService
    {
        void ReadTimeAndDateFromDataBase();
        void SaveOrEditTime();
        void StartTimer();
        void StopTimer();
        Task RunTimer();
    }
}
