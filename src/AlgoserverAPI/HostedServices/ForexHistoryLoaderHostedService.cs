using System;
using System.Threading;
using System.Threading.Tasks;
using Algoserver.API.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Algoserver.API.HostedServices
{
    public class ForexHistoryLoaderHostedService : BackgroundService
    {
        private int _prevDay = -1;
        private int _prevHour = -1;
        private int _prevMin = -1;
        private readonly ILogger<ForexHistoryLoaderHostedService> _logger;
        private readonly ScannerHistoryService _scannerHistory;
        private readonly ScannerCacheService _scannerCache;
        private Timer _timer;

        public ForexHistoryLoaderHostedService(ILogger<ForexHistoryLoaderHostedService> logger, ScannerForexHistoryService scannerHistory, ScannerForexCacheService scannerCache)
        {
            _logger = logger;
            _scannerHistory = scannerHistory;
            _scannerCache = scannerCache;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var currentDay = DateTime.UtcNow.Day;
                    var currentHour = DateTime.UtcNow.Hour;
                    var currentMinute = DateTime.UtcNow.Minute;
                    var scanRequired = false;

                    if (currentDay != _prevDay)
                    {
                        var result = await _scannerHistory.Refresh1MinLongHistory();
                        _scannerCache.RefreshLongMinuteHistoryTime = result;
                    }

                    if (currentHour != _prevHour)
                    {
                        var result = await _scannerHistory.RefreshAll();
                        _scannerCache.RefreshAllMarketsTime = result;
                        scanRequired = true;
                    }
                    else if (currentMinute != _prevMin)
                    {
                        var result = await _scannerHistory.Refresh();
                        _scannerCache.RefreshMarketsTime = result;
                        scanRequired = true;
                    }

                    _prevDay = currentDay;
                    _prevHour = currentHour;
                    _prevMin = currentMinute;

                    if (scanRequired)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken).ConfigureAwait(false);
                        Console.WriteLine(">>> Forex ScanMarkets start");
                        _scannerCache.CalculateMinuteMesa();
                        await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken).ConfigureAwait(false);
                        _scannerCache.ScanMarkets();
                        Console.WriteLine(">>> Forex ScanMarkets ends");
                        await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).ConfigureAwait(false);
            }
        }
    }
}