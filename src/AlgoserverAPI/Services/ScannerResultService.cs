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

            return new ScannerHistoryResponse
            {
                items = res1
            };
        }
    }
}