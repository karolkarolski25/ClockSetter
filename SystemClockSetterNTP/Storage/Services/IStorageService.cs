using System.Collections.Generic;
using System.Threading.Tasks;
using SystemClockSetterNTP.Storage;
using SystemClockSetterNTP.Storage.Models;

namespace SystemClockSetterNTP.Storage.Services
{
    public interface IStorageService
    {
        Task MigrateAsync();
        Task SaveChangesAsync();
        Task<List<ComputerData>> GetComputerDatasListAsync();
        void AddComputerDataAsync(ComputerData computerData);
        void RemoveComputerData(ComputerData computerData);
        void UpdateData(ComputerData computerData);
        void EditData();

        ComputerData ComputerData { get; set; }
    }
}
