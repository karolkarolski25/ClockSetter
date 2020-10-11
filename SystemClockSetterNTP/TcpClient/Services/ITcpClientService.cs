using System.Collections.Generic;
using SystemClockSetterNTP.Storage.Models;

namespace SystemClockSetterNTP.TcpClient.Services
{
    public interface ITcpClientService
    {
        void InitTcpClient();
        void SendDataToServer(List<ComputerData> dataToSend);
    }
}
