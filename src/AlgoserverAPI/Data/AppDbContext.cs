using Microsoft.EntityFrameworkCore;
using Algoserver.API.Models;

namespace Algoserver.API.Data
{
    public class AppDbContext: DbContext
    {
        public DbSet<Statistic> Statistics { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {}
    }
}
