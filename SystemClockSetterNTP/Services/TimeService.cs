using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SystemClockSetterNTP.Models;

namespace SystemClockSetterNTP.Services
{
    public class TimeService : ITimeService
    {
        private readonly ILogger<TimeService> _logger;
        private readonly NtpConfiguration _ntpConfiguration;
        private readonly WindowConfiguration _windowConfiguration;
        private readonly DateAndTimeFormat _dateAndTimeFormat;
        private readonly ApplicationConfiguration _applicationConfiguration;

        [DllImport("kernel32.dll")]
        static extern bool SetSystemTime(ref SYSTEMTIME time);

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEMTIME
        {
            public ushort Year;
            public ushort Month;
            public ushort DayOfWeek;
            public ushort Day;
            public ushort Hour;
            public ushort Minute;
            public ushort Second;
            public ushort Milliseconds;

            public SYSTEMTIME(DateTime dt)
            {
                Year = (ushort)dt.Year;
                Month = (ushort)dt.Month;
                DayOfWeek = (ushort)dt.DayOfWeek;
                Day = (ushort)dt.Day;
                Hour = (ushort)dt.Hour;
                Minute = (ushort)dt.Minute;
                Second = (ushort)dt.Second;
                Milliseconds = (ushort)dt.Millisecond;
            }
        }

        public TimeService(ILogger<TimeService> logger, NtpConfiguration ntpConfiguration,
            WindowConfiguration windowConfiguration, DateAndTimeFormat dateAndTimeFormat, 
            ApplicationConfiguration applicationConfiguration)
        {
            _logger = logger;
            _ntpConfiguration = ntpConfiguration;
            _windowConfiguration = windowConfiguration;
            _dateAndTimeFormat = dateAndTimeFormat;
            _applicationConfiguration = applicationConfiguration;
        }

        public async Task SetSystemClock()
        {
            _logger.LogDebug("Trying to set system time");

            string networkTime = await Task.Run(() => GetNetworkTime());

            if (networkTime == null)
            {
                throw new ArgumentNullException($"Incorrect time format: {networkTime}");
            }

            SYSTEMTIME systime = new SYSTEMTIME(Convert.ToDateTime(networkTime).ToUniversalTime());

            SetSystemTime(ref systime);

            if (_windowConfiguration.Beep)
            {
                for (int i = 0; i < _windowConfiguration.BeepCount; i++)
                {
                    Console.Beep(_windowConfiguration.SuccessBeepFrequency, _windowConfiguration.SuccessBeepDuration);

                    await Task.Delay(_windowConfiguration.DelayBetweenBeep);
                }
            }

            _logger.LogDebug($"System time set to: {networkTime}");
        }

        public string GetNetworkTime()
        {
            try
            {
                _logger.LogDebug("Trying to fetch network time");

                string ntpServer = _ntpConfiguration.NtpIP;
                var ntpData = new byte[48];
                ntpData[0] = 0x1B;
                var addresses = Dns.GetHostEntry(ntpServer).AddressList;
                var ipEndPoint = new IPEndPoint(addresses[0], 123);
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.Connect(ipEndPoint);
                socket.Send(ntpData);
                socket.Receive(ntpData);
                socket.Close();
                ulong intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | (ulong)ntpData[43];
                ulong fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | (ulong)ntpData[47];
                var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
                var networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);

                string fetchedNetworkTime = networkDateTime.ToLocalTime().ToString($"{_dateAndTimeFormat.DateFormat} {_dateAndTimeFormat.TimeFormat}");

                _logger.LogDebug($"Fetched time successful: {fetchedNetworkTime}");
                return fetchedNetworkTime;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching network time");

                return null;
            }
        }

        public bool IsComputerTimeCorrect()
        {
            _logger.LogDebug("Comparing system time and network time");

            var times = PerformTimeTasks();

            string systemTime = times[0];
            string networkTime = times[1];

            if (systemTime == networkTime)
            {
                _logger.LogDebug($"System time ({systemTime}) and network time ({networkTime}) are the same");
                return true;
            }

            else if (networkTime == null)
            {
                throw new ArgumentNullException($"Invalid time format: {networkTime}");
            }

            else
            {
                if (CompareTimeExpectSeconds(systemTime, networkTime) && GetSecondsDifferential(systemTime, networkTime) <= 
                    _applicationConfiguration.MaximumSystemAndNetworkTimeSecondDifferential) 
                {
                    _logger.LogDebug($"System time ({systemTime}) and network time ({networkTime}) are the same");
                    return true;
                }

                else
                {
                    _logger.LogDebug($"System time ({systemTime}) and network time ({networkTime}) are different");
                    return false;
                }
            }
        }

        private string[] PerformTimeTasks()
        {
            List<Task<string>> tasks = new List<Task<string>>();

            tasks.Add(new Task<string>(() => { return DateTime.Now.ToString($"{_dateAndTimeFormat.DateFormat} {_dateAndTimeFormat.TimeFormat}"); }));
            tasks.Add(new Task<string>(() => { return GetNetworkTime(); }));

            Parallel.ForEach(tasks, (t) => { t.Start(); });

            Task.WaitAll(tasks.ToArray());

            return tasks.Select(tr => tr.Result).ToArray();
        }

        private bool CompareTimeExpectSeconds(string systemTime, string networkTime)
        {
            try
            {
                string systemDate = systemTime.Substring(0, systemTime.Length - 3);
                string networkDate = networkTime.Substring(0, networkTime.Length - 3);

                return systemDate == networkDate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occured during compare system and network date expect seconds");
                return false;
            }
        }

        private int GetSecondsDifferential(string systemTime, string networkTime)
        {
            try
            {
                int systemSeconds = Convert.ToInt32(systemTime.Substring(systemTime.Length - 2));
                int networkSeconds = Convert.ToInt32(networkTime.Substring(networkTime.Length - 2));

                return Math.Abs(systemSeconds - networkSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occured during getting system and network seconds differential");
                return -1;
            }
        }
    }
}
