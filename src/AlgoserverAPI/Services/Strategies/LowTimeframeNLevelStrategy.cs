using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.API.Models.REST;

namespace Algoserver.API.Services
{
    public class StrategyResponse
    {
        public uint State { get; set; }
        public uint StrategyType { get; set; }
        public decimal SL1M { get; set; }
        public decimal SL5M { get; set; }
        public decimal SL15M { get; set; }
        public decimal SL1H { get; set; }
        public decimal SL4H { get; set; }
        public decimal SL1D { get; set; }
        public decimal OppositeSL1M { get; set; }
        public decimal OppositeSL5M { get; set; }
        public decimal OppositeSL15M { get; set; }
        public decimal OppositeSL1H { get; set; }
        public decimal OppositeSL4H { get; set; }
        public decimal OppositeSL1D { get; set; }
    }

    public static class LowTimeframeNLevelStrategy
    {
        private static bool ShowLogs = true;

        private static void WriteLog(string str)
        {
            if (!ShowLogs)
            {
                return;
            }

            Console.WriteLine($"LowTimeframeNLevelStrategy >>> {str}");
        }

        public static bool IsAutoTradeModeEnabled(AutoTradingSymbolInfoResponse symbolInfo, MESADataSummary mesaResponse)
        {
            if (IsTooMatchVolatility(mesaResponse))
            {
                WriteLog($"{mesaResponse.Symbol} AutoMode - volatility filter");
                return false;
            }

            if (IsInOverheatZone(symbolInfo))
            {
                WriteLog($"{mesaResponse.Symbol} AutoMode - Overheat Zone filter");
                return false;
            }

            if (IsAutoTradeCapitulationConfirmed(symbolInfo))
            {
                WriteLog($"{mesaResponse.Symbol} AutoMode - capitulation filter");
                return false;
            }

            if (!mesaResponse.TimeframePhase.TryGetValue(TimeframeHelper.MIN1_GRANULARITY, out var m1Phase))
            {
                WriteLog($"{mesaResponse.Symbol} AutoMode - 1min phase not exists");
                return false;
            }

            if (!mesaResponse.TimeframePhase.TryGetValue(TimeframeHelper.MIN5_GRANULARITY, out var m5Phase))
            {
                WriteLog($"{mesaResponse.Symbol} AutoMode - 5min phase not exists");
                return false;
            }

            if (!mesaResponse.TimeframePhase.TryGetValue(TimeframeHelper.MIN15_GRANULARITY, out var m15Phase))
            {
                WriteLog($"{mesaResponse.Symbol} AutoMode - 15min phase not exists");
                return false;
            }

            if (m1Phase != PhaseState.Drive && m5Phase != PhaseState.Drive && m5Phase != PhaseState.CD && IsEnoughStrength(symbolInfo, 0, 1))
            {
                WriteLog($"{mesaResponse.Symbol} AutoMode - active");
                return true;
            }
            else
            {
                WriteLog($"{mesaResponse.Symbol} AutoMode - m1Phase/m5Phase group drive filter");
            }

            return false;
        }

        public static bool IsAutoTradeCapitulationConfirmed(AutoTradingSymbolInfoResponse symbolInfo)
        {
            if (symbolInfo.MidGroupPhase == PhaseState.CD && symbolInfo.LongGroupPhase != PhaseState.Drive)
            {
                return true;
            }

            return false;
        }

        public static bool IsEnoughStrength(AutoTradingSymbolInfoResponse symbolInfo, decimal lowStrength, decimal highStrength)
        {
            var shortGroupStrength = symbolInfo.ShortGroupStrength * 100;
            var midGroupStrength = symbolInfo.MidGroupStrength * 100;
            var longGroupStrength = symbolInfo.LongGroupStrength * 100;

            if (symbolInfo.TrendDirection == 1)
            {
                // Uptrend
                if (longGroupStrength < highStrength || midGroupStrength < highStrength || shortGroupStrength < lowStrength)
                {
                    return false;
                }
            }
            else if (symbolInfo.TrendDirection == -1)
            {
                // Downtrend
                if (longGroupStrength > highStrength * -1 || midGroupStrength > highStrength * -1 || shortGroupStrength > lowStrength * -1)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public static bool IsEnoughStrength(AutoTradingSymbolInfoResponse symbolInfo, decimal highStrength)
        {
            var midGroupStrength = symbolInfo.MidGroupStrength * 100;
            var longGroupStrength = symbolInfo.LongGroupStrength * 100;

            if (symbolInfo.TrendDirection == 1)
            {
                // Uptrend
                if (longGroupStrength < highStrength || midGroupStrength < highStrength)
                {
                    return false;
                }
            }
            else if (symbolInfo.TrendDirection == -1)
            {
                // Downtrend
                if (longGroupStrength > highStrength * -1 || midGroupStrength > highStrength * -1)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public static bool IsTooMatchVolatility(MESADataSummary mesaResponse)
        {
            if (mesaResponse.Volatility.TryGetValue(TimeframeHelper.MIN15_GRANULARITY, out var volatility15min))
            {
                var relativeVolatility15min = volatility15min - 100f;
                return relativeVolatility15min > 70 || relativeVolatility15min < -83;
            }

            return false;
        }

        public static bool IsInOverheatZone(AutoTradingSymbolInfoResponse symbolInfo)
        {
            var n1d = symbolInfo.TP1D;
            var e1d = symbolInfo.Entry1D;
            var currentPrice = symbolInfo.CurrentPrice;

            if (currentPrice <= 0)
            {
                return false;
            }

            var skipZoneThreshold = 5m;
            var shift1d = Math.Abs(n1d - e1d);
            var maxShift1d = shift1d - (symbolInfo.HalfBand1D * 2m / 100m * skipZoneThreshold);

            if (symbolInfo.TrendDirection == 1)
            {
                // Uptrend
                if (currentPrice > n1d + maxShift1d)
                {
                    return true;
                }
            }
            else if (symbolInfo.TrendDirection == -1)
            {
                // Downtrend
                if (currentPrice < n1d - maxShift1d)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsStrengthIncreasing(AutoTradingSymbolInfoResponse symbolInfo, MesaResponse mesa_additional)
        {
            var decreasePeriod = 4;
            var resetPeriod = 6;

            // use 1min not compressed data
            if (!mesa_additional.mesa.TryGetValue(TimeframeHelper.MIN1_GRANULARITY * -1, out var decreaseData))
            {
                return false;
            }
            // use 1min not compressed data
            if (!mesa_additional.mesa.TryGetValue(TimeframeHelper.MIN1_GRANULARITY * -1, out var resetData))
            {
                return false;
            }

            long lastDecreaseTime = 0;
            var startIndex = decreaseData.Count - 1000;
            if (startIndex < decreasePeriod)
            {
                startIndex = decreasePeriod;
            }

            for (var i = startIndex; i < decreaseData.Count; i++)
            {
                var p = decreaseData[i - decreasePeriod];
                var c = decreaseData[i];
                var pV = p.f - p.s;
                var cV = c.f - c.s;
                if (symbolInfo.TrendDirection == 1)
                {
                    if (cV < pV)
                    {
                        var isValidChain = true;
                        for (var j = i - decreasePeriod; j < i; j++)
                        {
                            var current = decreaseData[j];
                            var next = decreaseData[j + 1];
                            var currentValue = current.f - current.s;
                            var nextValue = next.f - next.s;

                            if (nextValue >= currentValue)
                            {
                                isValidChain = false;
                                break;
                            }
                        }

                        if (isValidChain)
                        {
                            lastDecreaseTime = c.t;
                        }
                    }
                }
                else if (symbolInfo.TrendDirection == -1)
                {
                    if (cV > pV)
                    {
                        var isValidChain = true;
                        for (var j = i - decreasePeriod; j < i; j++)
                        {
                            var current = decreaseData[j];
                            var next = decreaseData[j + 1];
                            var currentValue = current.f - current.s;
                            var nextValue = next.f - next.s;

                            if (nextValue <= currentValue)
                            {
                                isValidChain = false;
                                break;
                            }
                        }

                        if (isValidChain)
                        {
                            lastDecreaseTime = c.t;
                        }
                    }
                }
            }

            long lastResetTime = 0;
            startIndex = resetData.Count - 1000;
            if (startIndex < resetPeriod)
            {
                startIndex = resetPeriod;
            }
            for (var i = startIndex; i < resetData.Count; i++)
            {
                var p = resetData[i - resetPeriod];
                var c = resetData[i];
                var pV = p.f - p.s;
                var cV = c.f - c.s;
                if (symbolInfo.TrendDirection == 1)
                {
                    if (cV > pV)
                    {
                        var isValidChain = true;
                        for (var j = i - resetPeriod; j < i; j++)
                        {
                            var current = resetData[j];
                            var next = resetData[j + 1];
                            var currentValue = current.f - current.s;
                            var nextValue = next.f - next.s;

                            if (nextValue <= currentValue)
                            {
                                isValidChain = false;
                                break;
                            }
                        }

                        if (isValidChain)
                        {
                            lastResetTime = c.t;
                        }
                    }
                }
                else if (symbolInfo.TrendDirection == -1)
                {
                    if (cV < pV)
                    {
                        var isValidChain = true;
                        for (var j = i - resetPeriod; j < i; j++)
                        {
                            var current = resetData[j];
                            var next = resetData[j + 1];
                            var currentValue = current.f - current.s;
                            var nextValue = next.f - next.s;

                            if (nextValue >= currentValue)
                            {
                                isValidChain = false;
                                break;
                            }
                        }

                        if (isValidChain)
                        {
                            lastResetTime = c.t;
                        }
                    }
                }
            }

            if (lastResetTime == 0 || lastDecreaseTime == 0)
            {
                return true;
            }

            return lastResetTime > lastDecreaseTime;
        }

        public static async Task<bool> RSICheck(string symbol, string datafeed, string exchange, string type, HistoryService historyService)
        {
            var rsivmax = 57;
            var rsivmin = 7;
            var xp = 91;

            var history = await historyService.GetHistory(symbol, TimeframeHelper.DAILY_GRANULARITY, datafeed, exchange, type, 0, xp * 2);
            if (history.Bars.Count < xp)
            {
                return false;
            }
            var rsiValue = RSIIndicator.CalculateLastRSI(history.Bars.Select(_ => _.Close).ToArray(), xp);

            if (rsiValue > rsivmax || rsiValue < rsivmin)
            {
                return false;  // Filter out the top and bottom RSI values
            }

            return true;
        }

        public static decimal GetSL(int granularity, Dictionary<int, LevelsV3Response> data, int trendDirection)
        {
            if (data.TryGetValue(granularity, out var item))
            {
                var lastSar = item.sar.LastOrDefault();
                if (lastSar == null)
                {
                    return 0;
                }

                var halsBand = GetHalfBand(granularity, data, trendDirection);
                if (trendDirection > 0)
                {
                    return lastSar.s - halsBand;
                }
                if (trendDirection < 0)
                {
                    return lastSar.r + halsBand;
                }
            }
            return 0;
        }
        public static decimal GetHalfBand(int granularity, Dictionary<int, LevelsV3Response> data, int trendDirection)
        {
            if (data.TryGetValue(granularity, out var item))
            {
                var lastSar = item.sar.LastOrDefault();
                if (lastSar == null)
                {
                    return 0;
                }

                var rH = (lastSar.r_p28 - lastSar.r) / 6;
                var sH = (lastSar.s - lastSar.s_m28) / 6;

                if (trendDirection > 0)
                {
                    return sH;
                }
                if (trendDirection < 0)
                {
                    return rH;
                }
            }
            return 0;
        }
        
        public static async Task<uint> GetState(AutoTradingSymbolInfoResponse symbolInfo, MESADataSummary mesaResponse, MesaResponse mesa_additional, Dictionary<int, LevelsV3Response> levelsResponse, string symbol, string datafeed, string exchange, string type, HistoryService historyService)
        {
            if (IsAutoTradeCapitulationConfirmed(symbolInfo))
            {
                return 3; // Capitulation
            }

            if (!IsStrengthIncreasing(symbolInfo, mesa_additional))
            {
                WriteLog($"{mesaResponse.Symbol} IsStrengthIncreasing - FALSE");
                return 1; // HITL allowed
            }

            if (IsAutoTradeModeEnabled(symbolInfo, mesaResponse))
            {
                var isRSICorrect = await RSICheck(symbol, datafeed, exchange, type, historyService);
                if (isRSICorrect)
                {
                    return 2; // Auto allowed
                }
                else
                {
                    WriteLog($"{mesaResponse.Symbol} isRSICorrect - FALSE");
                }
            }

            return 1; // HITL allowed
        }

        public static async Task<StrategyResponse> Calculate(AutoTradingSymbolInfoResponse symbolInfo, MESADataSummary mesaResponse, MesaResponse mesa_additional, Dictionary<int, LevelsV3Response> levelsResponse, string symbol, string datafeed, string exchange, string type, HistoryService historyService)
        {
            var state = await GetState(symbolInfo, mesaResponse, mesa_additional, levelsResponse, symbol.ToUpper(), datafeed, exchange, type, historyService);
            var hourSL = GetSL(TimeframeHelper.HOURLY_GRANULARITY, levelsResponse, symbolInfo.TrendDirection);
            var oppositeHourSL = GetSL(TimeframeHelper.HOURLY_GRANULARITY, levelsResponse, symbolInfo.TrendDirection > 0 ? -1 : 1);

            var result = new StrategyResponse
            {
                State = state,
                StrategyType = 2,
                SL1M = hourSL,
                SL5M = hourSL,
                SL15M = hourSL,
                OppositeSL1M = oppositeHourSL,
                OppositeSL5M = oppositeHourSL,
                OppositeSL15M = oppositeHourSL,
            };
            return result;
        }
    }
}
