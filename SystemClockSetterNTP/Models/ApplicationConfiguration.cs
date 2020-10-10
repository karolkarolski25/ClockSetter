namespace SystemClockSetterNTP.Models
{
    public class ApplicationConfiguration
    {
        public int ErrorMessageSecondTime { get; set; }
        public int CheckUserActivityForMinuteTime { get; set; }
        public int StartupDelayInSeconds { get; set; }
        public bool UserActivityIntegration { get; set; }
        public bool TurnOffComputerAfterTimeExceeded { get; set; }
        public bool CountSystemRunningTime { get; set; }
        public bool CountNetworkActivity { get; set; }
        public int MaximumSystemAndNetworkTimeSecondDifferential { get; set; }
        public string ConnectionString { get; set; }
    }
}
