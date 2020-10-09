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

        private readonly IStopwatchService _stopwatchService;

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
            ApplicationConfiguration applicationConfiguration, IStopwatchService stopwatchService)
        {
            _logger = logger;
            _ntpConfiguration = ntpConfiguration;
            _windowConfiguration = windowConfiguration;
            _dateAndTimeFormat = dateAndTimeFormat;
            _applicationConfiguration = applicationConfiguration;
            _stopwatchService = stopwatchService;
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

            await Task.Run(() =>
            {
                _stopwatchService.StartTimer();
                _stopwatchService.RunTimer();
            });
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

            DateTime systemTime = times[0];
            DateTime networkTime = times[1];

            if (systemTime == networkTime)
            {
                _logger.LogDebug($"System time ({systemTime}) and network time ({networkTime}) are the same");
                return true;
            }

            else if (networkTime == default)
            {
                throw new ArgumentNullException($"Invalid time format: {networkTime}");
            }

            else
            {
                try
                {
                    int secondsDifferential = Math.Abs(Convert.ToInt32((systemTime - networkTime).TotalSeconds));

                    if (secondsDifferential <= _applicationConfiguration.MaximumSystemAndNetworkTimeSecondDifferential)
                    {
                        _logger.LogDebug($"System time ({systemTime}) and network time ({networkTime}) are the same, seconds differential is {secondsDifferential}");
                        return true;
                    }

                    else
                    {
                        _logger.LogDebug($"System time ({systemTime}) and network time ({networkTime}) are different");
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        private DateTime[] PerformTimeTasks()
        {
            List<Task<DateTime>> tasks = new List<Task<DateTime>>
            {
                new Task<DateTime>(() => Convert.ToDateTime(DateTime.Now.ToString($"{_dateAndTimeFormat.DateFormat} {_dateAndTimeFormat.TimeFormat}"))),
                new Task<DateTime>(() =>
                {
                    var networkTime = GetNetworkTime();

                    if (networkTime == null)
                    {
                        return default;
                    }

                    return Convert.ToDateTime(networkTime);
                })
            };

            Parallel.ForEach(tasks, (t) => { t.Start(); });

            Task.WaitAll(tasks.ToArray());

            return tasks.Select(tr => tr.Result).ToArray();
        }
    }
}
