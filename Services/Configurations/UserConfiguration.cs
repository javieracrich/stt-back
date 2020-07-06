using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Services.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {

            builder.HasMany<User>("Students");

            builder.HasIndex(u => u.Code).IsUnique();
            builder.Property(u => u.PasswordHash).HasMaxLength(8000);
            builder.Property(u => u.SecurityStamp).HasMaxLength(8000);
            builder.Property(u => u.ConcurrencyStamp).HasMaxLength(8000);
            builder.Property(u => u.PhotoUrl).IsUnicode(false).HasMaxLength(2000);
            builder.Property(u => u.StudentId).IsUnicode(false).HasMaxLength(500);
        }
    }
}
