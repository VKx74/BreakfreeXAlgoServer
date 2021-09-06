using Algoserver.API.Models.Algo;

namespace Algoserver.API.Models.REST
{
    public class ScanResponse
    {
        public decimal entry { get; set; }
        public decimal stop { get; set; }
        public int tte { get; set; }
        public TradeProbability tp { get; set; }
        public TradeType type { get; set; }
        public Trend trend { get; set; }
        public decimal entry_h { get; set; }
        public decimal entry_l { get; set; }
        public decimal take_profit { get; set; }
        public decimal take_profit_h { get; set; }
        public decimal take_profit_l { get; set; }
        public decimal risk { get; set; }
        public decimal sl_ratio { get; set; }
        public long time { get; set; }

        public static bool IsEquals(ScanResponse a, ScanResponse b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            return a.entry == b.entry && a.stop == b.stop;
        }
    }
}
