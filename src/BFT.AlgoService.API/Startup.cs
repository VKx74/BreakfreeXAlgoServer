using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.WebEncoders;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using BFT.AlgoService.Logic.Config;
using Microsoft.AspNetCore.Http.Features;
using Common.API.Filters;
using BFT.AlgoService.API.Extensions;
using BFT.AlgoService.API.Middlewares;
using Fintatech.TDS.ClientIdentity.External;
using Fintatech.TDS.HealthCheck;
using Fintatech.TDS.HealthCheck.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.HealthChecks;
using IdentityServer4.AccessTokenValidation;
using Newtonsoft.Json.Converters;

namespace BFT.AlgoService.API
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public static long MaxFileLength { get; set; } = 2048;

        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected string CorsOrigins => Program.Configuration.GetValue("CORS:Origins", "*");
        protected string CorsMethods => Program.Configuration.GetValue("CORS:Methods", "*");
        protected string CorsHeaders => Program.Configuration.GetValue("CORS:Headers", "*");
        protected string CorsExposeHeaders => Program.Configuration.GetValue("CORS:ExposeHeaders", "*");

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string[] corsOrigins = CorsOrigins.Replace(" ", "").Split(",");
            string[] corsMethods = CorsMethods.Replace(" ", "").Split(",");
            string[] corsHeaders = CorsHeaders.Replace(" ", "").Split(",");
            string[] corsExposeHeaders = CorsExposeHeaders.Replace(" ", "").Split(",");

            Console.WriteLine("Allowed CORS Origins: " + String.Join("; ", corsOrigins));
            Console.WriteLine("Allowed CORS Methods: " + String.Join("; ", corsMethods));
            Console.WriteLine("Allowed CORS Headers: " + String.Join("; ", corsHeaders));
            Console.WriteLine("Allowed CORS Expose Headers: " + String.Join("; ", corsExposeHeaders) + "\n");

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder =>
                    {
                        builder
                            .WithOrigins(corsOrigins)
                            .WithMethods(corsMethods)
                            .WithHeaders(corsHeaders)
                            .WithExposedHeaders(corsExposeHeaders)
                            .AllowCredentials()
                            .SetIsOriginAllowedToAllowWildcardSubdomains();
                    });
            });

            services.AddSingleton(Program.Configuration);

            var mySqlConnectionString = Program.Configuration["Db"] ?? Program.Configuration.GetConnectionString("DefaultConnection");

            Seed.AddDbContext(services, Program.Configuration);

            //add services
            Logic.Config.ServiceProviderExtensions.AddServices(services, Program.Configuration);

            services.AddHealthChecks(checks =>
            {
                var minutes = 1;
                if (int.TryParse(Program.Configuration["HealthCheck:Timeout"], out var minutesParsed))
                {
                    minutes = minutesParsed;
                }
                checks.AddSqlCheck("Storage_Db", mySqlConnectionString, TimeSpan.FromMinutes(minutes));

            });

            services.AddOptions();
            services.AddMemoryCache();
            services.Configure<WebEncoderOptions>(
                options => { options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All); });
            services.Configure<FormOptions>(x =>
            {
                x.MultipartBodyLengthLimit = int.MaxValue;
            });

            services
                .AddMvc(options =>
                {
                    options.Filters.Add(typeof(GlobalExceptionFilter));
                    options.UseCentralRoutePrefix(new RouteAttribute(Program.RoutePrefix + "api/v1"));
                })
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                    options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
                });

            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.DefaultApiVersion = new ApiVersion(1, 0); // specify the default api version
                o.AssumeDefaultVersionWhenUnspecified = true; // assume that the caller wants the default version if they don't specify
                o.ApiVersionReader = new MediaTypeApiVersionReader(); // read the version number from the accept header
            });

            services.AddSwaggerDocumentation();

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = Program.Configuration.GetValue<string>("Authority");
                    options.ApiName = Scopes.FILE_STORE;
                    options.RequireHttpsMetadata = false;
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                loggerFactory.AddConsole(Program.Configuration.GetSection("Logging"));
                loggerFactory.AddDebug();
            }

            app.UseAuthentication();

            MaxFileLength = Program.Configuration.GetValue("MaxFileLength", 2048);

            app.UseVersionMiddleware("/version");

            app.UseSwaggerDocumentation();

            app.UseCorsMiddleware(CorsOrigins, CorsHeaders, CorsMethods, CorsExposeHeaders);

            app.UseCors("CorsPolicy");

            var swaggerRoutePrefix = Program.RoutePrefix;
            var swaggerRoutePrefixForTemplate = swaggerRoutePrefix.StartsWith("/")
                ? swaggerRoutePrefix.RemoveFirst()
                : swaggerRoutePrefix;

            app.UseServiceHealthChecks(async () =>
            {
                var healthChecker = new HealthChecker();

                var mySqlConnenctionString = Program.Configuration["Db"] ?? Program.Configuration.GetConnectionString("DefaultConnection");
                await healthChecker.CheckMysqlAsync("StorageService-MySql", "ping failed", 
                    mySqlConnenctionString);

                return healthChecker;
            }, swaggerRoutePrefix);
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action?}/{id?}");
            });
        }
    }
}
