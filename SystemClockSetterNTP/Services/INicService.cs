namespace SystemClockSetterNTP.Services
{
    public interface INicService
    {
        double GigabytesSent { get; set; }
        double GigabytesReceived { get; set; }

        void InitializeNICs();
        void RunMonitoringNics();
    }
}
