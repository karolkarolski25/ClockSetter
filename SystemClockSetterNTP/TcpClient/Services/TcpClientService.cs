using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using SystemClockSetterNTP.Models;
using SystemClockSetterNTP.Storage.Models;

namespace SystemClockSetterNTP.TcpClient.Services
{
    public class TcpClientService : ITcpClientService
    {
        private readonly ILogger<TcpClientService> _logger;
        private readonly ApplicationConfiguration _applicationConfiguration;

        public TcpClientService(ILogger<TcpClientService> logger, ApplicationConfiguration applicationConfiguration)
        {
            _logger = logger;
            _applicationConfiguration = applicationConfiguration;
        }

        public void InitTcpClient()
        {

        }

        public void ConnectWithServer()
        {

        }

        public void SendDataToServer(List<ComputerData> dataToSend)
        {
            _logger.LogDebug("Sending data to server");
        }
    }
}
