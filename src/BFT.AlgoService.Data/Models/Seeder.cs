using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace BFT.AlgoService.Data.Models
{
    public static class Seeder
    {
        public static void Initialize(IServiceProvider serviceProvider, IConfigurationRoot configuration)
        {
            using (var context = new ApplicationDbContext(serviceProvider))
            {
                context.Database.EnsureCreated();
                context.Database.Migrate();

                if (context.Data.Any())
                    return;

                // TODO: Add default data here

                context.SaveChanges();
            }
        }

        public static void AddDbContext(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddEntityFrameworkMySql().AddDbContext<ApplicationDbContext>(options =>
            {
                var connection = configuration["db"] ?? configuration.GetConnectionString("DefaultConnection");
                options.UseMySql(connection);
            });
        }
    }
}