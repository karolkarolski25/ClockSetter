using System.Collections.Generic;
using System.Threading.Tasks;
using SystemClockSetterNTP.Storage;

namespace SystemClockSetterNTP.Storage.Services
{
    public interface IStorageService
    {
        Task MigrateAsync();
        Task SaveChangesAsync();
        Task<List<ComputerData>> GetComputerDatasListAsync();
        void AddComputerDataAsync(ComputerData computerData);
        void RemoveComputerData(ComputerData computerData);
        void EditComputerData(ComputerData computerData);
    }
}
