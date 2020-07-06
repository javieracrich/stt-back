using Api;
using Api.Auth;
using AutoMapper;
using Common;
using Domain;
using FluentValidation.AspNetCore;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Services;
using Services.Config;
using Services.Cosmos;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Text;

namespace Api
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services)
        {
            /*
				Singleton:  which creates a single instance throughout the application. It creates the instance for the first time and reuses the same object in the all calls.
				Scoped:     lifetime services are created once per request within the scope. It is equivalent to Singleton in the current scope. eg. in MVC it creates 1 instance per each http request but uses the same instance in the other calls within the same web request.
				Transient:  lifetime services are created each time they are requested. This lifetime works best for lightweight, stateless services.
			*/

            services.AddScoped<IGenericService, GenericService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IImageService, ImageService>();

            services.AddScoped<IBusGpsLogService, BusGpsLogService>();
            services.AddScoped<IDeviceEventService, DeviceEventService>();
            services.AddScoped<ILocationLogService, LocationLogService>();
            services.AddScoped<IRFIDService, RFIDService>();

            services.AddScoped<IDateTimeService, DateTimeService>();
            services.AddScoped<ICardService, CardService>();
            services.AddScoped<IPushNotificationService, PushNotificationService>();
            services.AddScoped<ILocationService, LocationService>();
            services.AddScoped<IPushLogService, PushLogService>();
            services.AddScoped<IPrincipalProvider, PrincipalProvider>();

            // https://davidpine.net/blog/principal-architecture-changes/
            services.AddHttpContextAccessor();
            services.AddTransient<IPrincipal>(provider => provider.GetService<IHttpContextAccessor>().HttpContext.User);
            // https://docs.microsoft.com/en-us/azure/azure-monitor/app/api-filtering-sampling#add-properties-itelemetryinitializer
            services.AddSingleton<ITelemetryInitializer, SttTelemetryInitializer>();

            return services;
        }

        public static IServiceCollection AddEFCore(this IServiceCollection services, IConfiguration configuration, IHostingEnvironment env)
        {
            var cnn = configuration.GetConnectionString("DefaultConnection");

            //https://rehansaeed.com/optimally-configuring-entity-framework-core/

            services.AddDbContext<SttContext>(o => o
            .UseLazyLoadingProxies()
            .UseSqlServer(cnn,
                b =>
                {
                    b.MigrationsAssembly(typeof(SttContext).Assembly.GetName().Name);
                    b.EnableRetryOnFailure();
                })
                 .ConfigureWarnings(x =>
                 {
                     x.Throw(RelationalEventId.QueryClientEvaluationWarning);
                     x.Ignore(CoreEventId.DetachedLazyLoadingWarning);
                 }
            )
                 .EnableSensitiveDataLogging(env.IsDevelopment()));
            //	 .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            return services;
        }

        public static IServiceCollection AddToggleOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<ToggleOptions>(configuration.GetSection("Toggle"));
            services.Configure<CosmosOptions>(configuration.GetSection("CosmosDB"));
            services.Configure<GeneralOptions>(configuration.GetSection("General"));
            // Explicitly register the settings object by delegating to the IOptions object
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<ToggleOptions>>().Value);
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<CosmosOptions>>().Value);
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<GeneralOptions>>().Value);

            return services;
        }

        public static IServiceCollection AddMvcConfiguration(this IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                //options.Filters.Add(typeof(SetThreadCurrentPrincipalFilterAttribute));
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
            .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>())
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                options.SerializerSettings.Formatting = Formatting.None;
            });

            return services;
        }

        public static IServiceCollection AddAuthorization(this IServiceCollection services)
        {
            // https://docs.microsoft.com/en-us/aspnet/core/security/authorization/claims?view=aspnetcore-2.1
            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthPolicy.Reader, p => p.Requirements.Add(new ApiAccessRequirement(ApiAccess.Reader)));
                options.AddPolicy(AuthPolicy.Contributor, p => p.Requirements.Add(new ApiAccessRequirement(ApiAccess.Contributor)));
                options.AddPolicy(AuthPolicy.Test, p => p.Requirements.Add(new ApiAccessRequirement(ApiAccess.Test)));
            });

            services.AddSingleton<IAuthorizationHandler, ApiAccessHandler>();

            return services;
        }

        public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
        {

            // https://fullstackmark.com/post/13/jwt-authentication-with-aspnet-core-2-web-api-angular-5-net-core-identity-and-facebook-login

            services.AddSingleton<IJwtFactory, JwtFactory>();
            // jwt wire up
            // Get options from app settings
            var jwtAppSettingOptions = configuration.GetSection(nameof(JwtIssuerOptions));

            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtAppSettingOptions[nameof(JwtIssuerOptions.SecretKey)]));

            // Configure JwtIssuerOptions
            services.Configure<JwtIssuerOptions>(options =>
                {
                    options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                    options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
                    options.SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
                });

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                RequireExpirationTime = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(configureOptions =>
            {
                configureOptions.ClaimsIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                configureOptions.TokenValidationParameters = tokenValidationParameters;
                configureOptions.SaveToken = true;
            });

            return services;

        }

        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "School Time Tracker API",
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                c.OperationFilter<SecurityRequirementsOperationFilter>();

                c.OperationFilter<SetRightContentTypes>();
                c.OrderActionsBy(x => x.HttpMethod);
                //     c.TagActionsBy(x=>x.ActionDescriptor.RouteValues["controller"]);

                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });

                //c.DescribeAllEnumsAsStrings();
            });
            return services;
        }

        public static IServiceCollection AddAutoMapper(this IServiceCollection services)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutomapperProfile());
                cfg.AllowNullCollections = true;
            });

            config.AssertConfigurationIsValid();
            services.AddSingleton(x => config.CreateMapper());

            return services;
        }

        public static IServiceCollection AddIdentity(this IServiceCollection services)
        {
            // add identity
            var builder = services.AddIdentityCore<User>(o =>
            {
                // configure identity options
                //todo: improve for production scenario
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 6;
            });

            builder = new IdentityBuilder(builder.UserType, typeof(UserRole), builder.Services);

            builder.AddUserManager<SttUserManager>()
                .AddEntityFrameworkStores<SttContext>()
                .AddDefaultTokenProviders();

            return services;
        }
    }
}