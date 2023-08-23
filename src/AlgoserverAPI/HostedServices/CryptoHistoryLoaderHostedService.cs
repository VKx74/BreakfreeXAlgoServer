using System;
using System.Threading;
using System.Threading.Tasks;
using Algoserver.API.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Algoserver.API.HostedServices
{
    public class CryptoHistoryLoaderHostedService : BackgroundService
    {
        private int _prevDay = -1;
        private int _prevHour = -1;
        private int _prevMin = -1;
        private readonly ILogger<CryptoHistoryLoaderHostedService> _logger;
        private readonly ScannerHistoryService _scannerHistory;
        private readonly ScannerCacheService _scannerCache;
        private Timer _timer;

        public CryptoHistoryLoaderHostedService(ILogger<CryptoHistoryLoaderHostedService> logger, ScannerCryptoHistoryService scannerHistory, ScannerCryptoCacheService scannerCache)
        {
            _logger = logger;
            _scannerHistory = scannerHistory;
            _scannerCache = scannerCache;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var currentDay = DateTime.UtcNow.Day;
                var currentHour = DateTime.UtcNow.Hour;
                var currentMinute = DateTime.UtcNow.Minute;
                try
                {
                    var scanRequired = false;

                    // if (currentDay != _prevDay)
                    // {
                    //     var result = await _scannerHistory.Refresh1MinLongHistory();
                    //     _scannerCache.RefreshLongMinuteHistoryTime = result;
                    // }

                    if (currentHour != _prevHour)
                    {
                        var result = await _scannerHistory.RefreshAll();
                        _scannerCache.RefreshAllMarketsTime = result;
                        await Task.Delay(TimeSpan.FromSeconds(40), stoppingToken).ConfigureAwait(false);
                        // scanRequired = true;
                    }
                    else if (currentMinute != _prevMin && currentMinute % 2 == 1)
                    {
                        var result = await _scannerHistory.Refresh();
                        _scannerCache.RefreshMarketsTime = result;
                        scanRequired = true;
                    }

                    if (scanRequired)
                    {
                        // Console.WriteLine(">>> Crypto ScanMarkets start");
                        // _scannerCache.ScanMarkets();
                        // _scannerCache.CalculateMinuteMesa();
                        // Console.WriteLine(">>> Crypto ScanMarkets ends");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                _prevDay = currentDay;
                _prevHour = currentHour;
                _prevMin = currentMinute;

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).ConfigureAwait(false);
            }
        }
    }
}