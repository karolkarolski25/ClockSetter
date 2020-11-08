using System.Collections.Generic;
using System.Threading.Tasks;
using SystemClockSetterNTP.Storage.Models;

namespace SystemClockSetterNTP.DatabaseTcpClient.Services
{
    public interface ITcpClientService
    {
        void CloseConnection();
        Task SendDataToServerAsync(List<ComputerData> dataToSend);
        bool TryConnectWithServer();
    }
}
