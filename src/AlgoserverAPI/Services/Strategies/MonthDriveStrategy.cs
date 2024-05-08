using System;
using Algoserver.API.Helpers;
using Algoserver.API.Models.REST;

namespace Algoserver.API.Services
{
    public static class MonthDriveStrategy
    {
        private static bool ShowLogs = false;

        private static void WriteLog(string str)
        {
            if (!ShowLogs)
            {
                return;
            }

            Console.WriteLine($"MonthDriveStrategy >>> {str}");
        }

        public static bool IsAutoTradeModeEnabled(AutoTradingSymbolInfoResponse symbolInfo, MESADataSummary mesaResponse)
        {
            if (IsTooMatchVolatility(mesaResponse))
            {
                WriteLog($"{mesaResponse.Symbol} AutoMode - volatility filter");
                return false;
            }
            
            if (IsAutoTradeCapitulationConfirmed(symbolInfo))
            {
                WriteLog($"{mesaResponse.Symbol} AutoMode - capitulation filter");
                return false;
            }

            if (!mesaResponse.TimeframePhase.TryGetValue(TimeframeHelper.MIN15_GRANULARITY, out var m15Phase))
            {
                WriteLog($"{mesaResponse.Symbol} AutoMode - 15min phase not exists");
                return false;
            }

            if (!mesaResponse.TimeframePhase.TryGetValue(TimeframeHelper.MONTHLY_GRANULARITY, out var monthPhase))
            {
                WriteLog($"{mesaResponse.Symbol} AutoMode - month phase not exists");
                return false;
            }

            if (m15Phase == PhaseState.CD)
            {
                WriteLog($"{mesaResponse.Symbol} AutoMode - 15min CD filter");
                return false;
            }

            var minStrength = 15;

            if (monthPhase == PhaseState.Drive && symbolInfo.MidGroupPhase == PhaseState.Drive && IsEnoughStrength(symbolInfo, minStrength))
            {
                WriteLog($"{mesaResponse.Symbol} AutoMode - active");
                return true;
            }
            else
            {
                WriteLog($"{mesaResponse.Symbol} AutoMode - month/mid group drive filter");
            }

            if (IsInGoodZoneOn4H(symbolInfo) && IsEnoughStrength(symbolInfo, minStrength))
            {
                WriteLog($"{mesaResponse.Symbol} AutoMode - active is good on 4H zone");
                return true;
            }

            return false;
        }

        public static bool IsHITLModeEnabled(AutoTradingSymbolInfoResponse symbolInfo, MESADataSummary mesaResponse)
        {
            if (IsAutoTradeCapitulationConfirmed(symbolInfo))
            {
                WriteLog($"{mesaResponse.Symbol} HITL - capitulation filter");
                return false;
            }

            if (symbolInfo.LongGroupPhase == PhaseState.Drive && symbolInfo.MidGroupPhase == PhaseState.Drive && IsEnoughStrength(symbolInfo, 10))
            {
                WriteLog($"{mesaResponse.Symbol} HITL - active on stage 1");
                return true;
            }
            else
            {
                WriteLog($"{mesaResponse.Symbol} HITL - 1 filter");
            }

            if (symbolInfo.ShortGroupPhase == PhaseState.Drive && symbolInfo.LongGroupPhase == PhaseState.Drive && symbolInfo.MidGroupPhase == PhaseState.Drive && IsEnoughStrength(symbolInfo, 8))
            {
                WriteLog($"{mesaResponse.Symbol} HITL - active on stage 2");
                return true;
            }
            else
            {
                WriteLog($"{mesaResponse.Symbol} HITL - 2 filter");
            }

            if (symbolInfo.CurrentPhase == PhaseState.Drive && IsEnoughStrength(symbolInfo, 10))
            {
                WriteLog($"{mesaResponse.Symbol} HITL - active on stage 3");
                return true;
            }
            else
            {
                WriteLog($"{mesaResponse.Symbol} HITL - 3 filter");
            }

            if (!mesaResponse.TimeframePhase.TryGetValue(TimeframeHelper.MONTHLY_GRANULARITY, out var monthPhase))
            {
                WriteLog($"{mesaResponse.Symbol} HITL - month phase not exists");
                return false;
            }

            if (monthPhase == PhaseState.Drive && symbolInfo.MidGroupPhase == PhaseState.Drive && IsEnoughStrength(symbolInfo, 10))
            {
                WriteLog($"{mesaResponse.Symbol} HITL - active on stage 4");
                return true;
            }
            else
            {
                WriteLog($"{mesaResponse.Symbol} HITL - 4 filter");
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
            if (mesaResponse.Volatility.TryGetValue(TimeframeHelper.HOURLY_GRANULARITY, out var volatility1h) && mesaResponse.Volatility.TryGetValue(TimeframeHelper.HOURLY_GRANULARITY, out var volatility1d))
            {
                var relativeVolatility1h = volatility1h - 100f;
                var relativeVolatility1d = volatility1d - 100f;
                return relativeVolatility1h > 50 || relativeVolatility1d > 50;
            }

            return true;
        }

        public static bool IsInGoodZoneOn4H(AutoTradingSymbolInfoResponse symbolInfo)
        {
            var n4h = symbolInfo.TP4H;
            var currentPrice = symbolInfo.CurrentPrice;

            if (currentPrice <= 0)
            {
                return false;
            }

            if (symbolInfo.TrendDirection == 1)
            {
                // Uptrend
                if (currentPrice < n4h)
                {
                    return true;
                }
            }
            else if (symbolInfo.TrendDirection == -1)
            {
                // Downtrend
                if (currentPrice > n4h)
                {
                    return true;
                }
            }
            else
            {
                return true;
            }

            return false;
        }

        public static uint GetState(AutoTradingSymbolInfoResponse symbolInfo, MESADataSummary mesaResponse, string symbol)
        {
            // if (IsAutoTradeCapitulationConfirmed(symbolInfo))
            // {
            //     return 3; // Capitulation
            // }

            if (IsAutoTradeModeEnabled(symbolInfo, mesaResponse))
            {
                return 2; // Auto allowed
            }

            if (IsHITLModeEnabled(symbolInfo, mesaResponse))
            {
                return 2; // Auto allowed
                return 1; // HITL allowed
            }

            return 1; // HITL allowed
            return 0; // Nothing
        }
    }
}
