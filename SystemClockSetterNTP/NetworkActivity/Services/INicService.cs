namespace SystemClockSetterNTP.NetworkActivity.Services
{
    public interface INicService
    {
        double? GigabytesSent { get; set; }
        double? GigabytesReceived { get; set; }

        void InitializeNICs();
        void StartNicsMonitoring();
        void StopNicsMonitoring();
        void UpdateNicData();
    }
}
