using Algoserver.API.Helpers;
using Algoserver.API.Models.REST;

namespace Algoserver.API.Services
{
    public static class MonthDriveStrategy
    {
        public static bool IsAutoTradeModeEnabled(AutoTradingSymbolInfoResponse symbolInfo, MESADataSummary mesaResponse)
        {
            if (IsTooMatchVolatility(mesaResponse) || IsAutoTradeCapitulationConfirmed(symbolInfo))
            {
                return false;
            }

            if (!mesaResponse.TimeframePhase.TryGetValue(TimeframeHelper.MIN15_GRANULARITY, out var m15Phase))
            {
                return false;
            }

            if (!mesaResponse.TimeframePhase.TryGetValue(TimeframeHelper.MONTHLY_GRANULARITY, out var monthPhase))
            {
                return false;
            }

            if (m15Phase == PhaseState.CD)
            {
                return false;
            }

            var minStrength = 15;

            if (monthPhase == PhaseState.Drive && symbolInfo.MidGroupPhase == PhaseState.Drive && IsEnoughStrength(symbolInfo, minStrength))
            {
                return true;
            }

            if (IsInGoodZoneOn4H(symbolInfo) && IsEnoughStrength(symbolInfo, minStrength))
            {
                return true;
            }

            return false;

        }

        public static bool IsHITLModeEnabled(AutoTradingSymbolInfoResponse symbolInfo, MESADataSummary mesaResponse)
        {
            if (IsAutoTradeCapitulationConfirmed(symbolInfo))
            {
                return false;
            }

            if (symbolInfo.LongGroupPhase == PhaseState.Drive && symbolInfo.MidGroupPhase == PhaseState.Drive && IsEnoughStrength(symbolInfo, 10))
            {
                return true;
            }

            if (symbolInfo.ShortGroupPhase == PhaseState.Drive && symbolInfo.LongGroupPhase == PhaseState.Drive && symbolInfo.MidGroupPhase == PhaseState.Drive && IsEnoughStrength(symbolInfo, 8))
            {
                return true;
            }

            if (symbolInfo.CurrentPhase == PhaseState.Drive && IsEnoughStrength(symbolInfo, 10))
            {
                return true;
            }

            if (!mesaResponse.TimeframePhase.TryGetValue(TimeframeHelper.MONTHLY_GRANULARITY, out var monthPhase))
            {
                return false;
            }

            if (monthPhase == PhaseState.Drive && symbolInfo.MidGroupPhase == PhaseState.Drive && IsEnoughStrength(symbolInfo, 10))
            {
                return true;
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
            if (IsAutoTradeCapitulationConfirmed(symbolInfo))
            {
                return 3; // Capitulation
            }

            if (IsAutoTradeModeEnabled(symbolInfo, mesaResponse))
            {
                return 2; // Auto allowed
            }

            if (IsHITLModeEnabled(symbolInfo, mesaResponse))
            {
                return 2; // Auto allowed
                return 1; // HITL allowed
            }

            return 0; // Nothing
        }
    }
}
