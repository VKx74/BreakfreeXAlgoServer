using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Common.Cache.Managers;

namespace BFT.AlgoService.Logic.Config
{
    public static class ServiceProviderExtensions
    {
        public static void AddServices(IServiceCollection services, IConfiguration configuration, bool useInMemoryDb = false)
        {
            services.AddTransient<Services.AlgoService>();
            services.AddTransient<CleanCacheManager>();
            services.AddMemoryCache();
            services.AddTransient<CacheManager<Data.Models.Data>>();
        }
    }
}
