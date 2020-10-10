namespace SystemClockSetterNTP.Storage.Models
{
    public class ComputerData
    {
        public long Id { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public int? PowerOnCount { get; set; }
        public double? GigabytesReceived { get; set; }
        public double? GigabytesSent { get; set; }
    }
}
