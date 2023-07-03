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
                    mesa.Add(d.Key, d.Value);
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