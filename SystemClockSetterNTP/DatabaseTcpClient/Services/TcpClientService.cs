using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SystemClockSetterNTP.Models;
using SystemClockSetterNTP.Storage.Models;

namespace SystemClockSetterNTP.DatabaseTcpClient.Services
{
    public class TcpClientService : ITcpClientService
    {
        private readonly ILogger<TcpClientService> _logger;
        private readonly ServerConfiguration _serverConfiguration;

        private TcpClient tcpClient;

        public TcpClientService(ILogger<TcpClientService> logger, ServerConfiguration serverConfiguration)
        {
            _logger = logger;
            _serverConfiguration = serverConfiguration;

            tcpClient = new TcpClient();
        }

        public bool TryConnectWithServer()
        {
            try
            {
                tcpClient.Connect(new IPEndPoint(IPAddress.Parse(_serverConfiguration.ServerIp), _serverConfiguration.ServerPort));

                _logger.LogDebug("Succssfully connected with server");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting with server");

                return false;
            }
        }

        public async Task SendDataToServerAsync(List<ComputerData> dataToSend)
        {
            _logger.LogDebug("Sending data to server");

            var testBeginningDate = new DateTime(2000, 1, 1);

            var random = new Random();

            for (int i = 0; i < 100; i++)
            {
                testBeginningDate = testBeginningDate.AddDays(i);

                var testData = new ComputerData()
                {
                    Date = testBeginningDate.ToString("dd.MM.yyyy"),
                    Time = $"{random.Next(0, 23)}:{random.Next(0, 59)}:{random.Next(0, 59)}",
                    PowerOnCount = random.Next(1, 99),
                    GigabytesReceived = Math.Round(random.NextDouble() * (99.9999 - 0.0001) + 0.0001, 4),
                    GigabytesSent = Math.Round(random.NextDouble() * (99.9999 - 0.0001) + 0.0001, 4)
                };

                dataToSend.Add(testData);
            }

            foreach (var item in dataToSend)
            {
                byte[] message = new ASCIIEncoding().GetBytes($"{item.Date}|{item.Time}|{item.PowerOnCount}|{item.GigabytesReceived}|{item.GigabytesSent}");

                tcpClient.GetStream().Write(message, 0, message.Length);

                await Task.Delay(1);
            }

            byte[] computerName = new ASCIIEncoding().GetBytes(_serverConfiguration.ComputerName);
            tcpClient.GetStream().Write(computerName, 0, computerName.Length);
        }

        public void CloseConnection()
        {
            tcpClient.GetStream().Close();
            tcpClient.Close();
        }
    }
}
