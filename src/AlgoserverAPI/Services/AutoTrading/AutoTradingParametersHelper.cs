using System.Collections.Generic;

namespace Algoserver.API.Services
{
    class AutoTradingParameter
    {
        public int strength1h { get; set; }
        public int strength4h { get; set; }
        public int strength1d { get; set; }
        public int aroonPeriod { get; set; }
        public int aroonCount { get; set; }
    }

    public static class AutoTradingParametersHelper
    {
        private static Dictionary<string, AutoTradingParameter> parameters = new Dictionary<string, AutoTradingParameter>
        {
            {"USDCHF", new AutoTradingParameter {strength1h = 55, strength4h = 55, strength1d = 55, aroonPeriod = 55, aroonCount = 55}},
            {"EURUSD", new AutoTradingParameter {strength1h = 33, strength4h = 66, strength1d = 66, aroonPeriod = 66, aroonCount = 66}},
            {"EURCHF", new AutoTradingParameter {strength1h = 44, strength4h = 44, strength1d = 44, aroonPeriod = 44, aroonCount = 44}},
            {"GBPUSD", new AutoTradingParameter {strength1h = 33, strength4h = 33, strength1d = 33, aroonPeriod = 33, aroonCount = 33}},
            {"BTCUSD", new AutoTradingParameter {strength1h = 33, strength4h = 55, strength1d = 55, aroonPeriod = 55, aroonCount = 55}},
            {"XAUUSD", new AutoTradingParameter {strength1h = 11, strength4h = 11, strength1d = 33, aroonPeriod = 33, aroonCount = 33}},
            {"NZDCAD", new AutoTradingParameter {strength1h = 33, strength4h = 33, strength1d = 33, aroonPeriod = 33, aroonCount = 33}},
            {"AUDNZD", new AutoTradingParameter {strength1h = 55, strength4h = 55, strength1d = 55, aroonPeriod = 55, aroonCount = 55}}
        };

        public static int GetStrength1H(string symbol)
        {
            symbol = NormalizeSymbol(symbol);
            if (parameters.TryGetValue(symbol, out var res))
            {
                return res.strength1h;
            }
            return 44;
        } 
        
        public static int GetStrength4H(string symbol)
        {
            symbol = NormalizeSymbol(symbol);
            if (parameters.TryGetValue(symbol, out var res))
            {
                return res.strength4h;
            }
            return 44;
        }

        public static int GetStrength1D(string symbol)
        {
            symbol = NormalizeSymbol(symbol);
            if (parameters.TryGetValue(symbol, out var res))
            {
                return res.strength1d;
            }
            return 44;
        }
        
        public static int GetAroonPeriod(string symbol)
        {
            symbol = NormalizeSymbol(symbol);
            if (parameters.TryGetValue(symbol, out var res))
            {
                return res.aroonPeriod;
            }
            return 44;
        }
        
        public static int GetAroonCount(string symbol)
        {
            symbol = NormalizeSymbol(symbol);
            if (parameters.TryGetValue(symbol, out var res))
            {
                return res.aroonCount;
            }
            return 44;
        }

        private static string NormalizeSymbol(string symbol)
        {
            return symbol.Replace("_", "").Replace("/", "").Replace("^", "").ToUpper();
        }
    }
}