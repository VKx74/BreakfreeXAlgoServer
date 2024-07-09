using System.Collections.Generic;
using System.Linq;
using Algoserver.API.Helpers;

namespace Algoserver.API.Services.CacheServices
{
    public class MesaPreloaderService
    {
        private readonly List<MESADataSummary> mesaSummary = new List<MESADataSummary>();
        private readonly Dictionary<string, Dictionary<int, List<MESADataPoint>>> mesa = new Dictionary<string, Dictionary<int, List<MESADataPoint>>>();

        public void UpdateMesaSummary(List<MESADataSummary> data)
        {
            foreach (var ms in data)
            {
                if (ms.Symbol == "BTC_USD" && ms.Datafeed == "Oanda")
                {
                    ms.Datafeed = "Binance";
                    ms.Symbol = "BTCUSDT";
                }
                if (ms.Symbol == "ETH_USD" && ms.Datafeed == "Oanda")
                {
                    ms.Datafeed = "Binance";
                    ms.Symbol = "ETHUSDT";
                } 
                if (ms.Symbol == "SOL_USD" && ms.Datafeed == "Oanda")
                {
                    ms.Datafeed = "Binance";
                    ms.Symbol = "SOLUSDT";
                }
                if (ms.Symbol == "LTC_USD" && ms.Datafeed == "Oanda")
                {
                    ms.Datafeed = "Binance";
                    ms.Symbol = "LTCUSDT";
                }
            }
            lock (mesaSummary)
            {
                mesaSummary.Clear();
                mesaSummary.AddRange(data);
            }
        }

        public void UpdateMesa(Dictionary<string, Dictionary<int, List<MESADataPoint>>> data)
        {
            lock (mesa)
            {
                mesa.Clear();
                foreach (var d in data)
                {
                    mesa.Add(d.Key.ToLower(), d.Value);
                }
            }
        }

        public List<MESADataSummary> GetMesaSummary()
        {
            lock (mesaSummary)
            {
                return mesaSummary.ToList();
            }
        }

        public MESADataSummary GetMesaSummary(string symbol, string datafeed)
        {
            var normalizedSymbol = symbol.Replace("_", "").Replace("/", "").Replace("-", "");
            if (string.Equals(normalizedSymbol, "btcusdt", System.StringComparison.InvariantCultureIgnoreCase) || string.Equals(normalizedSymbol, "btcusd", System.StringComparison.InvariantCultureIgnoreCase))
            {
                datafeed = "Binance";
                symbol = "BTCUSDT";
            }
            else if (string.Equals(normalizedSymbol, "ethusdt", System.StringComparison.InvariantCultureIgnoreCase) || string.Equals(normalizedSymbol, "ethusd", System.StringComparison.InvariantCultureIgnoreCase))
            {
                datafeed = "Binance";
                symbol = "ETHUSDT";
            }

            lock (mesaSummary)
            {
                foreach (var m in mesaSummary)
                {
                    if (string.Equals(m.Datafeed, datafeed, System.StringComparison.InvariantCultureIgnoreCase) && string.Equals(m.Symbol, symbol, System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        return m;
                    }
                }
            }
            return null;
        }

        public Dictionary<int, List<MESADataPoint>> GetMesa(string symbol, string datafeed)
        {
            var normalizedSymbol = symbol.Replace("_", "").Replace("/", "").Replace("-", "");
            if (string.Equals(normalizedSymbol, "btcusdt", System.StringComparison.InvariantCultureIgnoreCase) || string.Equals(normalizedSymbol, "btcusd", System.StringComparison.InvariantCultureIgnoreCase))
            {
                datafeed = "Oanda";
                symbol = "BTC_USD";
            }
            else if (string.Equals(normalizedSymbol, "ethusdt", System.StringComparison.InvariantCultureIgnoreCase) || string.Equals(normalizedSymbol, "ethusd", System.StringComparison.InvariantCultureIgnoreCase))
            {
                datafeed = "Oanda";
                symbol = "ETH_USD";
            }

            var key = (datafeed + "_" + symbol).ToLower();
            lock (mesa)
            {
                if (mesa.TryGetValue(key, out var res))
                {
                    return res;
                }
            }
            return null;
        }
    }
}