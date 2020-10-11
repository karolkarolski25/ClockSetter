using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading;
using System.Threading.Tasks;
using SystemClockSetterNTP.Storage.Models;

namespace SystemClockSetterNTP.Storage
{
    public interface IComputerDataContext
    {
        DbSet<ComputerData> ComputerDatas { get; set; }
        DatabaseFacade Database { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
