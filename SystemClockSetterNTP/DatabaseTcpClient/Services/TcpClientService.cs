using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
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

        public void SendDataToServer(List<ComputerData> dataToSend)
        {
            _logger.LogDebug("Sending data to server");

            foreach (var item in dataToSend)
            {
                byte[] message = new ASCIIEncoding().GetBytes($"{item.Date}|{item.Time}|{item.PowerOnCount}|{item.GigabytesReceived}|{item.GigabytesSent}");

                tcpClient.GetStream().Write(message, 0, message.Length);
            }
        }

        public void CloseConnection()
        {
            tcpClient.GetStream().Close();
            tcpClient.Close();
        }
    }
}
