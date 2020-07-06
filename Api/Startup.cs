using System.Collections.Generic;
using Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Services;
using Swashbuckle.AspNetCore.SwaggerUI;
using static Common.Constants;

namespace Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        private IConfiguration Configuration { get; }
        private IHostingEnvironment Environment { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddCustomServices()
                .AddEFCore(Configuration, Environment)
                .AddAutoMapper()
                .AddToggleOptions(Configuration)
                .AddMemoryCache()
                .AddMvcConfiguration()
                .AddAuthentication(Configuration)
                .AddAuthorization()
                .AddSwagger()
                .AddIdentity()
                .AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();

            var allowedOrigins = new List<string>
            {
                "http://localhost:4200",
                "https://localhost:4200",
                "http://localhost:4300",
                "https://localhost:4300",
                "http://127.0.0.1:8080",
                "https://sttadminpanel.z20.web.core.windows.net",
                "https://sttschoolpanel.z20.web.core.windows.net"
            };

            app.UseSwagger();
            app.UseCors(x => x.AllowAnyHeader()
                              .AllowAnyMethod()
                              .WithExposedHeaders(TelemetryProperties.OperationId)
                              .WithOrigins(allowedOrigins.ToArray())
                              .AllowCredentials());

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "STT API");
                c.DocExpansion(DocExpansion.None);
                c.DocumentTitle = "STT API";
                //disables the try it out button
                //c.SupportedSubmitMethods(new SubmitMethod[0]);
            });

            app.UseMiddleware<SetOperationIdInHeaderMiddleware>();
            app.UseMvc();

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<SttContext>();
                context.Database.Migrate();
            }
        }
    }
}
