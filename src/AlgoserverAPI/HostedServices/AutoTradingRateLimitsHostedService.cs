using System;
using System.Threading;
using System.Threading.Tasks;
using Algoserver.API.Controllers;
using Algoserver.API.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Algoserver.API.HostedServices
{
    public class AutoTradingRateLimitsHostedService : BackgroundService
    {
        private int _prevHour = -1;
        private int _prevDay = -1;
        private readonly ILogger<AutoTradingRateLimitsHostedService> _logger;
        private readonly AutoTradingRateLimitsService _autoTradingRateLimitsService;

        public AutoTradingRateLimitsHostedService(ILogger<AutoTradingRateLimitsHostedService> logger, AutoTradingRateLimitsService autoTradingRateLimitsService)
        {
            _logger = logger;
            _autoTradingRateLimitsService = autoTradingRateLimitsService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var currentHour = DateTime.UtcNow.Hour;
                var currentDay = DateTime.UtcNow.Day;

                try
                {
                    if (currentHour != _prevHour)
                    {
                        _autoTradingRateLimitsService.Clear();
                    }
                    else
                    {
                        _autoTradingRateLimitsService.Decrease();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                
                try
                {
                    if (currentDay != _prevDay)
                    {
                        AutoTraderStatisticService.Init();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                _prevHour = currentHour;
                _prevDay = currentDay;

                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken).ConfigureAwait(false);
            }
        }
    }
}