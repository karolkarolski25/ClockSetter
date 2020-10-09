using Microsoft.Extensions.Logging;
using System;
using System.Net;
using SystemClockSetterNTP.Models;

namespace SystemClockSetterNTP.Services
{
    public class ServerService : IServerService
    {
        private readonly ILogger<ServerService> _logger;
        private readonly IStorageService _storageService;

        private readonly ServerConfiguration _serverConfiguration;

        public ServerService(IStorageService storageService, ILogger<ServerService> logger,
            ServerConfiguration serverConfiguration)
        {
            _storageService = storageService;
            _logger = logger;
            _serverConfiguration = serverConfiguration;

            _logger.LogDebug($"Server ip address: {_serverConfiguration.ServerIp}");
        }

        public void SendDataToServer()
        {
            var collectionToSend = _storageService.GetComputerDatasListAsync().Result;

            IPAddress serverAddress;

            if (IPAddress.TryParse(_serverConfiguration.ServerIp, out serverAddress))
            {

            }
            else
            {
                throw new ArgumentException("Invalid server ip address");
            }
        }
    }
}
