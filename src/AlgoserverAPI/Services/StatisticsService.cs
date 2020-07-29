using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Algoserver.API.Data.Repositories;
using Algoserver.API.Models;
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
            _dbBatchTimer.Elapsed += async (sender, e) => await DbSaveAndClearCacheAsync();
            _dbBatchTimer.Start();
        }

        public void AddToCache(Statistic item)
        {
            _statisticsCache.Add(item);
        }

        private async Task DbSaveAndClearCacheAsync()
        {
            try
            {
                if (_statisticsCache.Any())
                {
                    var buffer = _statisticsCache.ToArray();
                    _statisticsCache.Clear();
                    await _repo.AddRangeAsync(buffer);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during save statistics to db. {ex.Message}");
            }
        }
    }
}
