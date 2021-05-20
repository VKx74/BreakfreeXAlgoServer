using Algoserver.API.Models.REST;

namespace Algoserver.API.Helpers
{
    public static class DataMappingHelper 
    {
        public static CalculationResponse ToResponse(Levels levels, SupportAndResistanceResult sar, TradeEntryResult trade)
        {
            var algoInfo = new AlgoInfo
            {
                Macrotrend = trade.algo_Info.macrotrend,
                NCurrencySymbol = trade.algo_Info.n_currencySymbol,
                Objective = trade.algo_Info.objective,
                Pas = trade.algo_Info.pas,
                Positionsize = trade.algo_Info.positionsize,
                Status = trade.algo_Info.status,
                Suggestedrisk = trade.algo_Info.suggestedrisk
            };

            var tradeResponse = new StrategyModeV1
            {
                AlgoTP2 = trade.algo_TP2,
                AlgoTP1High = trade.algo_TP1_high,
                AlgoTP1Low = trade.algo_TP1_low,
                AlgoEntryHigh = trade.algo_Entry_high,
                AlgoEntryLow = trade.algo_Entry_low,
                AlgoEntry = trade.algo_Entry,
                AlgoStop = trade.algo_Stop,
                AlgoRisk = trade.algo_Risk,
                AlgoInfo = algoInfo
            };

            return new CalculationResponse
            {
                levels = ToLevels(levels, sar),
                trade = tradeResponse
            };
        }

        public static CalculationLevels ToLevels(Levels levels, SupportAndResistanceResult sar)
        {
            return new CalculationLevels
            {
                EE = levels.Level128.EightEight,
                EE1 = levels.Level32.EightEight,
                EE2 = levels.Level16.EightEight,
                EE3 = levels.Level8.EightEight,
                FE = levels.Level128.FourEight,
                FE1 = levels.Level32.FourEight,
                FE2 = levels.Level16.FourEight,
                FE3 = levels.Level8.FourEight,
                ZE = levels.Level128.ZeroEight,
                ZE1 = levels.Level32.ZeroEight,
                ZE2 = levels.Level16.ZeroEight,
                ZE3 = levels.Level8.ZeroEight,
                VR100 = sar.ValidRes100,
                VR75A = sar.ValidRes75a,
                VR75B = sar.ValidRes75b,
                VN100 = sar.ValidNeu100,
                VN75A = sar.ValidNeu75a,
                VN75B = sar.ValidNeu75b,
                VS100 = sar.ValidSup100,
                VS75A = sar.ValidSup75a,
                VS75B = sar.ValidSup75b,
                VSCS = sar.Validscs,
                VSCS2 = sar.Validscs2,
                VEXTTP = sar.Validexttp,
                VEXTTP2 = sar.Validexttp2,
                M18 = sar.Minus18,
                M28 = sar.Minus28,
                P18 = sar.Plus18,
                P28 = sar.Plus28,
            };
        }
    
        public static Strategy2BacktestData ToStrategy2BacktestData(TradeEntryV2CalculationData calculationData, TradeEntryV2Result tradeSR, TradeEntryV2Result tradeEx1)
        {
            var top_ext2 = calculationData.sar.Plus28;
            var top_ext1 = calculationData.sar.Plus18;
            var resistance = calculationData.levels.Level128.EightEight;
            var natural = calculationData.levels.Level128.FourEight;
            var support = calculationData.levels.Level128.ZeroEight;
            var bottom_ext1 = calculationData.sar.Minus18;
            var bottom_ext2 = calculationData.sar.Minus28;

            return new Strategy2BacktestData
            {
                top_ex2 = top_ext2,
                top_ex1 = top_ext1,
                r = resistance,
                s = support,
                n = natural,
                bottom_ex1 = bottom_ext1,
                bottom_ex2 = bottom_ext2,
                trend = calculationData.trend,
                trade_sr = tradeSR,
                trade_ex1 = tradeEx1
            };
        }
    
        public static CalculationResponse ToResponse(Levels levels, SupportAndResistanceResult sar)
        {
            return new CalculationResponse
            {
                levels = ToLevels(levels, sar)
            };
        }

        public static CalculationResponseV2 ToResponseV2(Levels levels, SupportAndResistanceResult sar, ScanResponse scanRes, decimal size)
        {
            return new CalculationResponseV2
            {
                levels = DataMappingHelper.ToLevels(levels, sar),
                trade = ToStrategyModeV2(scanRes),
                size = size
            };
        }

        public static StrategyModeV2 ToStrategyModeV2(ScanResponse scanRes)
        {
            if (scanRes == null)
            {
                return null;
            }

            return new StrategyModeV2
            {
                trend = scanRes.trend,
                type = scanRes.type,
                tp = scanRes.tp,
                tte = scanRes.tte,
                entry = scanRes.entry,
                stop = scanRes.stop,
                risk = scanRes.risk,
                sl_ratio = scanRes.sl_ratio,
                entry_h = scanRes.entry_h,
                entry_l = scanRes.entry_l,
                take_profit = scanRes.take_profit,
                take_profit_h = scanRes.take_profit_h,
                take_profit_l = scanRes.take_profit_l
            };
        }
    }
}
