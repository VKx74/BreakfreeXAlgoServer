using System;
using System.Threading;
using System.Threading.Tasks;
using Algoserver.API.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Algoserver.API.HostedServices
{
    public class AutoTradingAccountsPreloaderHostedService : BackgroundService
    {
        private int _prevMinute = -1;
        private readonly ILogger<AutoTradingAccountsLoaderHostedService> _logger;
        private readonly AutoTradingAccountsService _autoTradingAccountsService;
        private Timer _timer;

        public AutoTradingAccountsPreloaderHostedService(ILogger<AutoTradingAccountsLoaderHostedService> logger, AutoTradingAccountsService autoTradingAccountsService)
        {
            _logger = logger;
            _autoTradingAccountsService = autoTradingAccountsService;
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
                        _autoTradingAccountsService.Update();
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