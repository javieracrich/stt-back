using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Services.Configurations
{
	public class SchoolConfiguration : IEntityTypeConfiguration<School>
	{
		public void Configure(EntityTypeBuilder<School> builder)
		{
            builder.HasIndex(u => u.Code).IsUnique();
            builder.Property(u => u.LogoUrl).IsUnicode(false).HasMaxLength(2000);
		}
	}
}
