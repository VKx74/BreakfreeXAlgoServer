using Fintatech.TDS.ClientIdentity.External;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using Algoserver.API.Conventions;
using Algoserver.API.Data;
using Algoserver.API.Services;
using Microsoft.AspNetCore.Http;
using Algoserver.Auth.Services;
using Algoserver.API.HostedServices;
using Algoserver.API.Services.CacheServices;
using Algoserver.API.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Algoserver.API
{
    public class Startup
    {
        public IConfiguration Configuration { get { return Program.Configuration; } }

        private string RoutePrefix => Configuration.GetValue<string>("RoutePrefix");

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore(options =>
            {
                //routing prefix stuff
                var routeAttribute = new RouteAttribute(RoutePrefix);
                options.Conventions.Insert(0, new RouteConvention(routeAttribute));
            })
            //.AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver())
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
            .AddApiExplorer()
            .AddJsonFormatters()
            .AddDataAnnotations()
            .AddAuthorization(options =>
            {
                options.AddPolicy("free_user_restriction", policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new GuestRightProtectionRequirement());
                });
            });

            var scanInstruments = Configuration.GetValue<bool>("ScanInstruments");

            //services.AddDistributedRedisCache(option =>
            //{
            //    var redisSettings = Configuration.GetSection("RedisSettings").Get<RedisSettings>();
            //    option.InstanceName = redisSettings.InstanceName;
            //    option.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions();
            //    option.ConfigurationOptions.AbortOnConnectFail = false;
            //    option.ConfigurationOptions.Ssl = redisSettings.UseSSL;
            //    option.ConfigurationOptions.Password = redisSettings.Password;
            //    option.ConfigurationOptions.DefaultDatabase = redisSettings.DefaultDatabase;
            //    option.ConfigurationOptions.EndPoints.Add(redisSettings.Host, redisSettings.Port);
            //});

            services.AddStackExchangeRedisCache(options =>
            {
                var redisSettings = Configuration.GetSection("RedisSettings").Get<RedisSettings>();
                options.InstanceName = redisSettings.InstanceName;
                options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions();
                options.ConfigurationOptions.AbortOnConnectFail = false;
                options.ConfigurationOptions.Ssl = redisSettings.UseSSL;
                options.ConfigurationOptions.Password = redisSettings.Password;
                options.ConfigurationOptions.DefaultDatabase = redisSettings.DefaultDatabase;
                options.ConfigurationOptions.EndPoints.Add(redisSettings.Host, redisSettings.Port);
            });

            // services.AddSingleton<ICacheService, MemoryCacheService>();
            services.AddSingleton<IAuthorizationHandler, GuestRightProtectionHandler>();
            services.AddSingleton<ICacheService, RedisCacheService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddLogging(opt => opt.AddConsole().AddDebug());
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<InstrumentService>();
            services.AddSingleton<AuthService>();
            services.AddSingleton<HistoryService>();
            services.AddSingleton<PriceRatioCalculationService>();
            services.AddSingleton<AlgoService>();
            services.AddSingleton<BacktestService>();
            services.AddSingleton<ScannerBacktestService>();
            services.AddSingleton<ScannerService>();
            services.AddSingleton<ScannerCryptoHistoryService>();
            services.AddSingleton<ScannerForexHistoryService>();
            services.AddSingleton<ScannerStockHistoryService>();
            services.AddSingleton<ScannerForexCacheService>();
            services.AddSingleton<ScannerStockCacheService>();
            services.AddSingleton<ScannerCryptoCacheService>();
            services.AddSingleton<ScannerResultService>();
            services.AddSingleton<RTDService>();
            services.AddSingleton<StatisticsService>();

            if (scanInstruments) {
                services.AddHostedService<StockHistoryLoaderHostedService>();
                services.AddHostedService<ForexHistoryLoaderHostedService>();
                services.AddHostedService<CryptoHistoryLoaderHostedService>();
            }

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                });
            });

            // Initialize DB context
            services.AddDbContext(Program.Configuration);
            services.AddOptions();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info { Title = "Algoserver API" });

                options.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Name = "Authorization",
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    In = "header",
                    Type = "apiKey"
                });

                var security = new Dictionary<string, IEnumerable<string>>
                {
                    {
                        "Bearer", new string[] { }
                    }
                };

                options.AddSecurityRequirement(security);
            });

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = Configuration.GetValue<string>("Authority");
                    options.ApiName = Scopes.ALGO;
                    options.RequireHttpsMetadata = false;
                    options.EnableCaching = true;
                });

            services.AddTokenProvider();

            services.AddMemoryCache();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, InstrumentService instrumentService)
        {
            instrumentService.Init();
            
            app.UseCors();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

#if !DEBUG
            // app.UseHttpsRedirection();
#endif
            var swaggerUIRoutePrefix = RoutePrefix + "/swagger";
            var swaggerEndpoint = $"/{RoutePrefix}/swagger/v1/swagger.json";
            app.UseSwagger(options => options.RouteTemplate = RoutePrefix + "/swagger/{documentName}/swagger.json");
            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = swaggerUIRoutePrefix;
                options.SwaggerEndpoint(swaggerEndpoint, "Algoserver API");
                options.DocExpansion(DocExpansion.None);
            });

            app.UseStaticFiles();
            app.UseWebSockets(new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = Configuration.GetValue<int>("WebSocketBufferSize", 4 * 1024)
            });

            app.UseMvc();

            Console.WriteLine($"Swagger UI: /{swaggerUIRoutePrefix}");
            Console.WriteLine($"Swagger Endpoint: {swaggerEndpoint}");
        }
    }
}
