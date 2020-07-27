using System;
using System.Collections.Generic;
using System.Timers;
using Algoserver.API.Data;
using Algoserver.API.Models;
using Microsoft.Extensions.Logging;

namespace Algoserver.API.Services
{
    public class StatisticsService
    {
        private readonly List<Statistic> _statisticsCache;
        private readonly ILogger<StatisticsService> _logger;
        private readonly AppDbContext _dbContext;
        private readonly Timer _dbBatchTimer = new Timer(1000);

        public StatisticsService(ILogger<StatisticsService> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;

            _dbBatchTimer.Elapsed += SaveToDb;
            _dbBatchTimer.Start();
        }

        public void AddToStatisticsCache(Statistic item)
        {
            _statisticsCache.Add(item);
        }

        public void SaveToDb(object sender, ElapsedEventArgs e)
        {
            try
            {
                lock (_statisticsCache)
                {
                    _dbContext.Statistics.AddRange(_statisticsCache);
                    _statisticsCache.Clear();
                }

                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during save statistics to db.", ex);
            }
        }
    }
}
