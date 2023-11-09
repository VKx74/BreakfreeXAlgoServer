using System.IO;
using Microsoft.EntityFrameworkCore;
using Algoserver.API.Models;

namespace Algoserver.API.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<NAAccountBalances> NAAccountBalances { get; set; }
        public DbSet<NALogs> NALogs { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { }

        public void AddTriggers()
        {
            foreach (var file in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Data/Triggers"), "*.sql"))
            {
                base.Database.ExecuteSqlCommand(File.ReadAllText(file), new object[0]);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

    }
}
