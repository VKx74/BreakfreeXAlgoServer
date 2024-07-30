using System;
using Algoserver.API.Helpers;
using Algoserver.API.Models.REST;

namespace Algoserver.API.Services
{
    public static class LowTimeframeNLevelStrategy
    {
        private static bool ShowLogs = false;

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

            if (m1Phase == PhaseState.Drive && m5Phase == PhaseState.Drive && IsEnoughStrength(symbolInfo, 0, 11))
            {
                WriteLog($"{mesaResponse.Symbol} AutoMode - active");
                return true;
            }
            else
            {
                WriteLog($"{mesaResponse.Symbol} AutoMode - month/mid group drive filter");
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
                return relativeVolatility15min > 70;
            }

            return true;
        }

        public static bool IsStrengthIncreasing(AutoTradingSymbolInfoResponse symbolInfo, MesaResponse mesa_additional)
        {
            var decreasePeriod = 22;
            var resetPeriod = 369;
            //   if (symbolInfo.TrendDirection == 1)
            // {
            //     // Uptrend

            if (!mesa_additional.mesa.TryGetValue(TimeframeHelper.MIN1_GRANULARITY, out var decreaseData))
            {
                return true;
            }
            if (!mesa_additional.mesa.TryGetValue(TimeframeHelper.MIN5_GRANULARITY, out var resetData))
            {
                return true;
            }

            long lastDecreaseTime = 0;            
            for (var i = decreasePeriod; i < decreaseData.Count; i++)
            {
                var p = decreaseData[i - decreasePeriod];
                var c = decreaseData[i];
                var pV = p.f - p.s;
                var cV = c.f - c.s;
                if (symbolInfo.TrendDirection == 1)
                {
                    if (cV < pV)
                    {
                        lastDecreaseTime = c.t;
                    }
                }
                else if (symbolInfo.TrendDirection == -1)
                {
                    if (cV > pV)
                    {
                        lastDecreaseTime = c.t;
                    }
                }
            }
            
            long lastResetTime = 0;            
            for (var i = resetPeriod; i < resetData.Count; i++)
            {
                var p = resetData[i - resetPeriod];
                var c = resetData[i];
                var pV = p.f - p.s;
                var cV = c.f - c.s;
                if (symbolInfo.TrendDirection == 1)
                {
                    if (cV > pV)
                    {
                        lastResetTime = c.t;
                    }
                }
                else if (symbolInfo.TrendDirection == -1)
                {
                    if (cV < pV)
                    {
                        lastResetTime = c.t;
                    }
                }
            }

            if (lastResetTime == 0 || lastDecreaseTime == 0)
            {
                return true;
            }

            return lastResetTime > lastDecreaseTime;
        }

        public static uint GetState(AutoTradingSymbolInfoResponse symbolInfo, MESADataSummary mesaResponse, MesaResponse mesa_additional, string symbol)
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
                return 2; // Auto allowed
            }

            return 1; // HITL allowed
        }
    }
}
