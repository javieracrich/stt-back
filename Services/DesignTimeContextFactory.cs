using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Services
{
	public class DesignTimeContextFactory : IDesignTimeDbContextFactory<SttContext>
	{
		public SttContext CreateDbContext(string[] args)
		{
			var configuration = new ConfigurationBuilder()
		   .SetBasePath(Directory.GetCurrentDirectory())
		   .AddJsonFile("appsettings.json")
		   .Build();

			var builder = new DbContextOptionsBuilder<SttContext>();

			builder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

			return new SttContext(builder.Options);
		}
	}
}
