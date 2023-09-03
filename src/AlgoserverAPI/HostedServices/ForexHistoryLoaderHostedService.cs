using System;
using System.Threading;
using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.API.Services.CacheServices;
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
        private readonly MesaPreloaderService _mesaPreloaderService;
        private readonly AutoTradingPrecalculationService _autoTradingPrecalculationService;
        private Timer _timer;

        public ForexHistoryLoaderHostedService(ILogger<ForexHistoryLoaderHostedService> logger, ScannerForexHistoryService scannerHistory, ScannerForexCacheService scannerCache, MesaPreloaderService mesaPreloaderService, AutoTradingPrecalculationService autoTradingPrecalculationService)
        {
            _logger = logger;
            _scannerHistory = scannerHistory;
            _scannerCache = scannerCache;
            _mesaPreloaderService = mesaPreloaderService;
            _autoTradingPrecalculationService = autoTradingPrecalculationService;
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

                    if (currentDay != _prevDay)
                    {
                        var result = await _scannerHistory.Refresh1MinLongHistory();
                        _scannerCache.RefreshLongMinuteHistoryTime = result;
                    }

                    if (currentHour != _prevHour)
                    {
                        var result = await _scannerHistory.RefreshAll();
                        _scannerCache.RefreshAllMarketsTime = result;
                        await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken).ConfigureAwait(false);
                        // scanRequired = true;
                    }
                    else if (currentMinute != _prevMin)
                    {
                        var result = await _scannerHistory.Refresh();
                        _scannerCache.RefreshMarketsTime = result;
                        scanRequired = true;
                    }

                    if (scanRequired)
                    {
                        // await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken).ConfigureAwait(false);
                        Console.WriteLine(">>> Forex Calculate Minute Mesa start");
                        var mesaInfo = _scannerCache.CalculateMinuteMesa();
                        if (mesaInfo != null)
                        {
                            _mesaPreloaderService.UpdateMesa(mesaInfo.MesaDataPoints);
                            _mesaPreloaderService.UpdateMesaSummary(mesaInfo.MesaSummary);
                        }
                        Console.WriteLine(">>> Forex Calculate Minute Mesa ends");
                        Console.WriteLine(">>> Forex Auto Trading Precalculation start");
                        var instruments = _scannerHistory.getInstrumentsForLongHistory();
                        await _autoTradingPrecalculationService.CalculateInstruments(instruments, "Forex");
                        Console.WriteLine(">>> Forex Auto Trading Precalculation ends");
                        await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken).ConfigureAwait(false);
                        // Console.WriteLine(">>> Forex ScanMarkets start");
                        // _scannerCache.ScanMarkets();
                        // Console.WriteLine(">>> Forex ScanMarkets ends");
                        await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken).ConfigureAwait(false);
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