using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Algoserver.API.Models;
using Algoserver.API.Repositories;
using Microsoft.Extensions.Logging;

namespace Algoserver.API.Services
{
    public class StatisticsService
    {
        private readonly List<Statistic> _statisticsCache;
        private readonly ILogger<StatisticsService> _logger;
        private readonly StatisticsRepository _repo = new StatisticsRepository();
        private readonly Timer _dbBatchTimer = new Timer(1000 * 10);

        public StatisticsService(ILogger<StatisticsService> logger)
        {
            _logger = logger;
            _statisticsCache = new List<Statistic>();
            _dbBatchTimer.Elapsed += (sender, e) => DbSaveAndClearCache();
            _dbBatchTimer.Start();
        }

        public void AddToCache(Statistic item)
        {
            _statisticsCache.Add(item);
        }

        private void DbSaveAndClearCache()
        {
            try
            {
                if (_statisticsCache.Any())
                {
                    lock (_statisticsCache)
                    {
                        _repo.AddRange(_statisticsCache);
                        _statisticsCache.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during save statistics to db.", ex);
            }
        }
    }
}
