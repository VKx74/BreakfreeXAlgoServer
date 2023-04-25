using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace Algoserver.API
{
    public class Program
    {
        public static string DbName { get; set; }
        public static IConfigurationRoot Configuration { get; private set; }
        private const string ASPNETCORE_ENVIRONMENT = "ASPNETCORE_ENVIRONMENT";

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            BuildConfiguration(args);

            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
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



            MySqlConnectionStringBuilder connBuilder = new MySqlConnectionStringBuilder
            {
                ConnectionString = Configuration.GetConnectionString("DefaultConnection")
            };
            DbName = connBuilder.Database;
        }
    }
}
