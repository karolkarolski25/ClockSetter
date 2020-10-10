using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SystemClockSetterNTP.Storage.Models;

namespace SystemClockSetterNTP.Storage.Services
{
    public class StorageService : IStorageService
    {
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        private readonly IComputerDataContext _computerDataContext;
        private readonly ILogger<StorageService> _logger;

        public ComputerData ComputerData { get; set; }

        public StorageService(IComputerDataContext computerDataContext, ILogger<StorageService> logger)
        {
            _computerDataContext = computerDataContext;
            _logger = logger;

            ComputerData = new ComputerData();
        }

        public void UpdateData(ComputerData computerData)
        {
            ComputerData.Date ??= computerData.Date;
            ComputerData.Time ??= computerData.Time;
            ComputerData.PowerOnCount ??= computerData.PowerOnCount;
            ComputerData.GigabytesReceived ??= computerData.GigabytesReceived;
            ComputerData.GigabytesSent ??= computerData.GigabytesSent;
        }

        public async void EditData()
        {
            var itemToEdit = _computerDataContext.ComputerDatas.FirstOrDefault(d => d.Date == ComputerData.Date);

            if (itemToEdit != null)
            {
                _logger.LogDebug($"Computer data changed from | {itemToEdit.Date} | {itemToEdit.Time} | {itemToEdit.PowerOnCount} | {itemToEdit.GigabytesReceived} " +
                    $"| {itemToEdit.GigabytesSent} | to | {ComputerData.Date} | {ComputerData.Time} | {ComputerData.PowerOnCount} | {ComputerData.GigabytesReceived} " +
                    $"| {ComputerData.GigabytesSent} |");

                itemToEdit.Time = ComputerData.Time;
                itemToEdit.PowerOnCount = ComputerData.PowerOnCount;
                itemToEdit.GigabytesReceived = ComputerData.GigabytesReceived;
                itemToEdit.GigabytesSent = ComputerData.GigabytesSent;

                await SaveChangesAsync();
            }
            else
            {
                AddComputerDataAsync(ComputerData);
            }
        }

        public async void AddComputerDataAsync(ComputerData computerData)
        {
            _computerDataContext.ComputerDatas.Add(computerData);

            _logger.LogDebug($"Added new computer data | {computerData.Id} | {computerData.Date} | {computerData.Time} | {computerData.PowerOnCount} |" +
                $" {computerData.GigabytesReceived} | {computerData.GigabytesSent} |");

            await SaveChangesAsync();
        }

        public async void RemoveComputerData(ComputerData computerData)
        {
            _computerDataContext.ComputerDatas.Remove(computerData);

            _logger.LogDebug($"Computer entry | {computerData.Date} | {computerData.Time} | {computerData.PowerOnCount} | " +
                $"{computerData.GigabytesReceived} | {computerData.GigabytesSent} | removed");

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
                _logger.LogDebug("Data saved");

                await _computerDataContext.SaveChangesAsync();
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}
