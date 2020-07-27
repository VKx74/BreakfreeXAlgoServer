using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Algoserver.API.Data
{
    public static class DbSeeder
    {
        public static void InitializeDbContext()
        {
            using (var context = new AppDbContext())
            {
                context.Database.EnsureCreated();
                context.Database.Migrate();
                context.SaveChanges();
            }
        }

        public static void AddDbContext(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddEntityFrameworkMySql().AddDbContext<AppDbContext>(options =>
            {
                var connection = configuration["db"] ?? configuration.GetConnectionString("DefaultConnection");
                options.UseMySql(connection);
            });
            InitializeDbContext();
        }
    }
}
