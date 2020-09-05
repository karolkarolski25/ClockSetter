namespace SystemClockSetterNTP.Models
{
    public class WindowConfiguration
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public int FailureBeepFrequency { get; set; }
        public int SuccessBeepFrequency { get; set; }
        public int SuccessBeepDuration { get; set; }
        public int FailureBeepDuration { get; set; }
        public int DelayBetweenBeep { get; set; }
        public int BeepCount { get; set; }
        public string Title { get; set; }
        public bool Beep { get; set; }
    }
}
