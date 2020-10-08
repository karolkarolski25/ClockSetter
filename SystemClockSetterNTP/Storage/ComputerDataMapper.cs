using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SystemClockSetterNTP.Storage
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
