using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Services.Configurations
{
    public class PushLogConfiguration : IEntityTypeConfiguration<PushLog>
    {
        public void Configure(EntityTypeBuilder<PushLog> builder)
        {
            builder.HasIndex(u => new { u.DeviceCode, u.CardCode, u.PushType, u.SchoolCode }).IsUnique();
            builder.Property(u => u.DeviceCode).IsRequired(true);
            builder.Property(u => u.CardCode).IsRequired(true);
        }
    }
}
