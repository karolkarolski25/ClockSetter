using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemClockSetterNTP.Storage.Models;

namespace SystemClockSetterNTP.Storage.Mappers
{
    public class ComputerDataMapper : IEntityTypeConfiguration<ComputerData>
    {
        public void Configure(EntityTypeBuilder<ComputerData> builder)
        {
            builder.HasIndex(p => p.Date)
                .IsUnique();

            builder.Property(p => p.Date)
                .IsRequired();
        }
    }
}
