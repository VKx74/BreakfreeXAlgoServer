using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using Common.API;
using Serilog;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace BFT.AlgoService.API
{
    public class Program
    {
        public static IConfigurationRoot Configuration { get; private set; }
        public static string RoutePrefix => Configuration.GetValue("SwaggerRoutePrefix", "/");

        // public static string ApiKey => Configuration.GetValue("ApiKey", "*");

        #region configuration param names
        private const string TLS = "Tls";
        private const string CERT_PATH = "CertPath";
        private const string CERT_SEC = "CertSec";
        private const string LOGFOLDER = "Log";
        private const string PORT = "Port";
        private const string TLS_PORT = "Tlsport";
        private const string ASPNETCORE_ENVIRONMENT = "ASPNETCORE_ENVIRONMENT";
        #endregion configuration param names

        public static void Main(string[] args)
        {
            var host = BuildWebHost(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    // TODO: Uncoment seed
                    // Seed.Initialize(services, Configuration);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Startup>>();
                    logger.LogError(ex, "An error occurred seeding the DB.");
                    throw ex;
                }
            }

            host.Run();
        }

        public static IWebHostBuilder BuildWebHost(string[] args)
        {
            AppVersion.OutToConsole();

            BuildConfiguration(args);
            LogConfiguration.InitLogger(Configuration, GetLogPath());
            var tlsEnabled = Configuration.GetValue<bool>(TLS);
            var certPathValue = Configuration.GetValue<string>(CERT_PATH);
            var certSecValue = Configuration.GetValue<string>(CERT_SEC);

            if (tlsEnabled && (string.IsNullOrEmpty(certPathValue) || !File.Exists(certPathValue)))
            {
                Console.WriteLine($"{ CERT_PATH } is not present in params or certificate file does not exist");
                Log.Information($"{ CERT_PATH } is not present in params or certificate file does not exist");
                tlsEnabled = false;
            }

            var host = new WebHostBuilder()
                .UseKestrel((options) =>
                {
                    options.Limits.MaxRequestBodySize = null;
                    options.Listen(IPAddress.Any, GetPort());
                    if (tlsEnabled)
                    {
                        options.Listen(IPAddress.Any, GetSSLPort(), listenOptions =>
                            listenOptions.UseHttps(certPathValue, certSecValue));
                    }
                })
                .UseHealthChecks("/hc")
                .UseContentRoot(Directory.GetCurrentDirectory())
                // .UseIISIntegration()
                .UseStartup<Startup>();
                // .UseApplicationInsights();

            return host;
        }

        private static string GetLogPath()
        {
            var fileName = "{Date}.algoservice.api.log";
            var logFolder = Configuration.GetValue<string>(LOGFOLDER);

            if (!string.IsNullOrEmpty(logFolder))
                return Path.Combine(logFolder, fileName);

            return Path.Combine(Directory.GetCurrentDirectory(), "logs", fileName);
        }

        private static void BuildConfiguration(string[] args)
        {
            var envName = Environment.GetEnvironmentVariable(ASPNETCORE_ENVIRONMENT);
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{envName}.json", optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args);
            Configuration = builder.Build();
        }

        private static int GetPort()
        {
            var port = Configuration.GetValue<int>(PORT);
            return port != 0 ? port : 5000;
        }

        private static int GetSSLPort()
        {
            var port = Configuration.GetValue<int>(TLS_PORT);
            return port != 0 ? port : 5443;
        }
    }
}
