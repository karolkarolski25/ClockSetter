using Microsoft.EntityFrameworkCore;
using SystemClockSetterNTP.Models;

namespace SystemClockSetterNTP.Storage
{
    public class ComputerDataContext : DbContext, IComputerDataContext
    {
        public DbSet<ComputerData> ComputerDatas { get; set; }

        private readonly ApplicationConfiguration _applicationConfiguration;

        public ComputerDataContext(ApplicationConfiguration applicationConfiguration, 
            DbContextOptions<ComputerDataContext> options) : base(options)
        {
            _applicationConfiguration = applicationConfiguration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_applicationConfiguration.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ComputerDataMapper());
        }
    }
}
