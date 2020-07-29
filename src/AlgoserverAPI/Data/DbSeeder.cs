using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Algoserver.API.Data
{
    public static class DbSeeder
    {
        public static void InitializeDbContext(IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>();
            using (var context = new AppDbContext(options))
            {
                // context.Database.EnsureCreated();
                context.Database.Migrate();
                context.SaveChanges();
            }
        }

        public static void AddDbContext(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddEntityFrameworkMySql().AddDbContext<AppDbContext>(options =>
            {
                options.UseMySql(configuration.GetConnectionString("DefaultConnection"));
            });
            InitializeDbContext(services.BuildServiceProvider());
        }
    }
}
