using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Algoserver.API.Data;
using Algoserver.API.Models;

namespace Algoserver.API.Repositories
{
    public class StatisticsRepository
    {
        private readonly AppDbContextFactory _dbContextFactory;

        public StatisticsRepository()
        {
            _dbContextFactory = new AppDbContextFactory();
        }

        public void AddRange(List<Statistic> statistics)
        {
            if (statistics != null && statistics.Any())
            {
                using (var context = _dbContextFactory.CreateDbContext())
                {
                    context.Statistics.AddRange(statistics);
                    context.SaveChanges();
                }
            }
        }

        public async Task AddRangeAsync(List<Statistic> statistics)
        {
            if (statistics != null && statistics.Any())
            {
                using (var context = _dbContextFactory.CreateDbContext())
                {
                    context.Statistics.AddRange(statistics);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
