using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
//using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Logging;
using System;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                CreateWebHostBuilder(args)
                    .Build()
                    .Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>

        // https://docs.microsoft.com/en-us/aspnet/core/migration/21-to-22?view=aspnetcore-2.2&tabs=visual-studio

        WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config
                    .SetBasePath(context.HostingEnvironment.ContentRootPath)
                    .AddJsonFile("appsettings.json", true, true)
                    .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true)
                    .AddEnvironmentVariables();

                //    var configRoot = config.Build();
                //    var keyVaultEndpoint = configRoot["AzureKeyVaultEndpoint"];
                //    if (string.IsNullOrWhiteSpace(keyVaultEndpoint))
                //        throw new Exception("azure key vault endpoint is null");
                //    var azureServiceTokenProvider = new AzureServiceTokenProvider();
                //    var callback = new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback);
                //    var keyVaultClient = new KeyVaultClient(callback);
                //    config.AddAzureKeyVault(keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager());

            })
                .UseApplicationInsights()
                .UseStartup<Startup>()
                .ConfigureKestrel((context, options) => { options.AddServerHeader = false; });
    }
}
