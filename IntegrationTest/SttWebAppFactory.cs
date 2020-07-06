using Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Services;
using System;

namespace IntegrationTest
{

	//https://fullstackmark.com/post/20/painless-integration-testing-with-aspnet-core-web-api
	public class SttWebAppFactory<TStartup> : WebApplicationFactory<Startup>
	{
		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.ConfigureServices(services =>
			{
				// Create a new service provider.
				var serviceProvider = new ServiceCollection()
						.AddEntityFrameworkSqlite()
						.BuildServiceProvider();

				// Add a database context (AppDbContext) using an in-memory database for testing.
				services.AddDbContext<SttContext>(options =>
					{
						options.UseSqlite("DataSource=:memory:");
						options.UseInternalServiceProvider(serviceProvider);
					});

				// Build the service provider.
				var sp = services.BuildServiceProvider();

				// Create a scope to obtain a reference to the database contexts
				using (var scope = sp.CreateScope())
				{
					var scopedServices = scope.ServiceProvider;
					var appDb = scopedServices.GetRequiredService<SttContext>();

					var logger = scopedServices.GetRequiredService<ILogger<SttWebAppFactory<TStartup>>>();

					// Ensure the database is created.
					appDb.Database.EnsureCreated();

					try
					{
						// Seed the database with some specific test data.
						SeedData.PopulateTestData(appDb);
					}
					catch (Exception ex)
					{
						logger.LogError(ex, "An error occurred seeding the " +
											"database with test messages. Error: {ex.Message}");
					}
				}
			});
		}
	}
}
