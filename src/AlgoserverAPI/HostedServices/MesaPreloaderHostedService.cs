using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.API.Services;
using Algoserver.API.Services.CacheServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Algoserver.API.HostedServices
{
    public class MesaPreloaderHostedService : BackgroundService
    {
        private int _prevMin = -1;
        private readonly ILogger<MesaPreloaderHostedService> _logger;
        private readonly ScannerHistoryService _scannerHistory;
        private readonly ScannerCacheService _scannerCache;
        private readonly MesaPreloaderService _mesaPreloaderService;
        private readonly AutoTradingPreloaderService _autoTradingPreloaderService;
        private Timer _timer;

        public MesaPreloaderHostedService(ILogger<MesaPreloaderHostedService> logger, MesaPreloaderService mesaPreloaderService, ScannerForexHistoryService scannerHistory, ScannerForexCacheService scannerCache, AutoTradingPreloaderService autoTradingPreloaderService)
        {
            _logger = logger;
            _scannerHistory = scannerHistory;
            _scannerCache = scannerCache;
            _mesaPreloaderService = mesaPreloaderService;
            _autoTradingPreloaderService = autoTradingPreloaderService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var currentMinute = DateTime.UtcNow.Minute;
                    if (currentMinute != _prevMin && DateTime.UtcNow.Second > 30)
                    {
                        _prevMin = currentMinute;
                        Console.WriteLine(">>> Mesa Preloader start");
                        var mesaSummary = _scannerCache.GetMesaSummary();
                        var instruments = _scannerHistory.getInstrumentsForLongHistory();
                        var minuteMesaCache = new Dictionary<string, Dictionary<int, List<MESADataPoint>>>();
                        foreach (var instrument in instruments)
                        {
                            try
                            {
                                var datafeed = instrument.Datafeed;
                                var symbol = instrument.Symbol;

                                var key = (datafeed + "_" + symbol).ToLower();
                                var mesa = _scannerCache.GetMinuteMesaCache(key);
                                if (mesa != null && mesa.Any())
                                {
                                    minuteMesaCache.Add(key, mesa);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }
                        _mesaPreloaderService.UpdateMesa(minuteMesaCache);
                        _mesaPreloaderService.UpdateMesaSummary(mesaSummary);

                        Console.WriteLine(">>> Mesa Preloader ends");

                        Console.WriteLine(">>> Auto Trading Info Preloader start");
                        await _autoTradingPreloaderService.LoadInstruments("Forex");
                        Console.WriteLine(">>> Auto Trading Info Preloader ends");
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