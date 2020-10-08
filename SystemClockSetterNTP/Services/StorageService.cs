using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SystemClockSetterNTP.Storage;

namespace SystemClockSetterNTP.Services
{
    public class StorageService : IStorageService
    {
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        private readonly IComputerDataContext _computerDataContext;
        private readonly ILogger<StorageService> _logger;

        public StorageService(IComputerDataContext computerDataContext, ILogger<StorageService> logger)
        {
            _computerDataContext = computerDataContext;
            _logger = logger;
        }

        public async void AddComputerDataAsync(ComputerData computerData)
        {
            _computerDataContext.ComputerDatas.Add(computerData);

            await SaveChangesAsync();
        }

        public async void EditComputerData(ComputerData computerData)
        {
            var itemToEdit = _computerDataContext.ComputerDatas.FirstOrDefault(d => d.Date == computerData.Date);

            if (itemToEdit != null)
            {
                itemToEdit.Time = computerData.Time;
                itemToEdit.PowerOnCount = computerData.PowerOnCount;
            }

            await SaveChangesAsync();
        }

        public async void RemoveComputerData(ComputerData computerData)
        {
            _computerDataContext.ComputerDatas.Remove(computerData);

            await SaveChangesAsync();
        }

        public async Task<List<ComputerData>> GetComputerDatasListAsync()
        {
            await _semaphoreSlim.WaitAsync();

            try
            {
                await _computerDataContext.ComputerDatas.LoadAsync();
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return _computerDataContext.ComputerDatas.Local.ToList();
        }

        public async Task MigrateAsync()
        {
            await _semaphoreSlim.WaitAsync();

            try
            {
                await _computerDataContext.Database.MigrateAsync();
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async Task SaveChangesAsync()
        {
            await _semaphoreSlim.WaitAsync();

            try
            {
                await _computerDataContext.SaveChangesAsync();
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}
