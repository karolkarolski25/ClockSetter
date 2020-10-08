using System;

namespace SystemClockSetterNTP.Storage
{
    public class ComputerData
    {
        public long Id { get; set; }
        public string Date { get; set; }
        public TimeSpan Time { get; set; }
        public int PowerOnCount { get; set; }
    }
}
