using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Cache.Managers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BFT.AlgoService.Data.Models
{
    public class ApplicationDbContext : DbContext
    {
        private readonly CleanCacheManager _cleanCacheManager;

        public ApplicationDbContext(IServiceProvider serviceProvider)
            : base(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>())
        {
            _cleanCacheManager = serviceProvider.GetRequiredService<CleanCacheManager>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

        public DbSet<Data> Data { get; set; }

        public override int SaveChanges()
        {
            InvalidateCache().Wait();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            await InvalidateCache();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public async Task InvalidateCache()
        {
            var changedEntityTypes = this.ChangeTracker
                .Entries()
                .Where(x => x.State == EntityState.Added ||
                            x.State == EntityState.Modified ||
                            x.State == EntityState.Deleted)
                .Select(x => x.Entity.GetType())
                .Distinct()
                .ToList();

            if (!changedEntityTypes.Any()) return;

            await _cleanCacheManager.CleanCache(changedEntityTypes);
        }
    }
}
