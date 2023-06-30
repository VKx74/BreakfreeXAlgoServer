using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Algoserver.API.Models.REST;

namespace Algoserver.API.Services
{
    public class ScannerResultService
    {
        private readonly ScannerForexCacheService _forex;
        private readonly ScannerStockCacheService _stock;
        private readonly ScannerCryptoCacheService _crypto;

        public ScannerResultService(ScannerForexCacheService forex, ScannerStockCacheService stock, ScannerCryptoCacheService crypto)
        {
            _forex = forex;
            _stock = stock;
            _crypto = crypto;
        }

        public ScannerResponse GetData()
        {
            var res1 = _forex.GetData();
            var res2 = _stock.GetData();
            var res3 = _crypto.GetData();
            res1.AddRange(res2);
            res1.AddRange(res3);

            return new ScannerResponse
            {
                items = res1
            };
        }
        
        public ScannerHistoryResponse GetHistoryData()
        {
            var res1 = _forex.GetHistoryData();
            var res2 = _stock.GetHistoryData();
            var res3 = _crypto.GetHistoryData();
            res1.AddRange(res2);
            res1.AddRange(res3);

            // todo move this calc on front
            res1.Sort((a, b) =>
            {
                var diff = a.time - b.time;
                if (diff == 0)
                {
                    return 0;
                }
                else if (diff > 0)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            });

            return new ScannerHistoryResponse
            {
                items = res1
            };
        }

        public ScannerCacheItem GetSonarHistoryCache(string symbol, string exchange, int timeframe, long time)
        {
            var res1 = _forex.GetSonarHistoryCache(symbol, exchange, timeframe, time);
            if (res1 != null)
            {
                return res1;
            }

            var res2 = _stock.GetSonarHistoryCache(symbol, exchange, timeframe, time);
            if (res2 != null)
            {
                return res2;
            }

            var res3 = _crypto.GetSonarHistoryCache(symbol, exchange, timeframe, time);
            if (res3 != null)
            {
                return res3;
            }

            return null;
        }
    }
}
