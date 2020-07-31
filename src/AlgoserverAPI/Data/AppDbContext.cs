using System.IO;
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

        public void AddTriggers()
        {
            foreach (var file in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Data/Triggers"), "*.sql"))
            {
                base.Database.ExecuteSqlCommand(File.ReadAllText(file), new object[0]);
            }
        }

    }
}
