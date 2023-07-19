using System;
using System.Threading;
using System.Threading.Tasks;
using Algoserver.API.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Algoserver.API.HostedServices
{
    public class AutoTradingAccountsLoaderHostedService : BackgroundService
    {
        private int _prevMinute = -1;
        private readonly ILogger<AutoTradingAccountsLoaderHostedService> _logger;
        private readonly AutoTradingAccountsLoadingService _autoTradingAccountsLoadingService;
        private Timer _timer;

        public AutoTradingAccountsLoaderHostedService(ILogger<AutoTradingAccountsLoaderHostedService> logger, AutoTradingAccountsLoadingService autoTradingAccountsLoadingService)
        {
            _logger = logger;
            _autoTradingAccountsLoadingService = autoTradingAccountsLoadingService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var currentMinute = DateTime.UtcNow.Minute;
                try
                {
                    if (currentMinute != _prevMinute)
                    {
                        await _autoTradingAccountsLoadingService.Update();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                _prevMinute = currentMinute;

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken).ConfigureAwait(false);
            }
        }
    }
}