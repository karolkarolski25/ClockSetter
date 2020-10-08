using Microsoft.EntityFrameworkCore;

namespace SystemClockSetterNTP.Storage
{
    public interface IComputerDataContext
    {
        DbSet<ComputerData> ComputerDatas { get; set; }
    }
}
