using System.Threading.Tasks;

namespace SystemClockSetterNTP.SystemStopwatch.Services
{
    public interface IStopwatchService
    {
        void ReadStopwatchDataFromDatabase();
        void UpdateStopwatchData();
        void StartTimer();
        void StopTimer();
        Task RunTimer();
    }
}
