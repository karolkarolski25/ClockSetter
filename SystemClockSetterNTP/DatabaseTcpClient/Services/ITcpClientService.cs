using System.Collections.Generic;
using SystemClockSetterNTP.Storage.Models;

namespace SystemClockSetterNTP.DatabaseTcpClient.Services
{
    public interface ITcpClientService
    {
        void CloseConnection();
        void SendDataToServer(List<ComputerData> dataToSend);
        bool TryConnectWithServer();
    }
}
