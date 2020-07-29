using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Algoserver.API.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext()
        {
            return CreateDbContext(args: new []{"--verbose"});
        }

        public AppDbContext CreateDbContext(string[] args)
        {
            var connectionString = Program.Configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Could not find a connection string named 'DefaultConnection'.");
            }
            
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            optionsBuilder.UseMySql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
        
    }
}
