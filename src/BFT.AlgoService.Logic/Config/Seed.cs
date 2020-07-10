using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using BFT.AlgoService.Data.Models;

namespace BFT.AlgoService.Logic.Config
{
    public static class Seed
    {
        public static void Initialize(IServiceProvider serviceProvider, IConfigurationRoot configuration)
        {
            Seeder.Initialize(serviceProvider, configuration);
        }

        public static void AddDbContext(IServiceCollection services, IConfigurationRoot configuration)
        {
            Seeder.AddDbContext(services, configuration);
        }
    }
}
