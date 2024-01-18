using System;
using Algoserver.API.Helpers;
using Algoserver.API.Models.REST;

namespace Algoserver.API.Services
{
    public static class TestStrategy
    {
        public static bool IsAutoTradeModeEnabled(AutoTradingSymbolInfoResponse symbolInfo, MESADataSummary mesaResponse)
        {
            if (IsInOverheatZone(symbolInfo))
            {
                return false;
            }

            if (symbolInfo.MidGroupPhase == PhaseState.CD)
            {
                return false;
            }

            if (symbolInfo.ShortGroupPhase != PhaseState.Drive)
            {
                return false;
            }

            if (!IsEnoughStrength(symbolInfo, 10, 5))
            {
                return false;
            }

            return true;
        }

        public static bool IsHITLModeEnabled(AutoTradingSymbolInfoResponse symbolInfo, MESADataSummary mesaResponse)
        {
            if (IsInOverheatZone(symbolInfo))
            {
                return false;
            }

            if (symbolInfo.MidGroupPhase == PhaseState.CD)
            {
                return false;
            }

            if (symbolInfo.ShortGroupPhase == PhaseState.CD)
            {
                return false;
            }

            if (!IsEnoughStrength(symbolInfo, 5, 5))
            {
                return false;
            }

            return true;
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

        public static bool IsInOverheatZone(AutoTradingSymbolInfoResponse symbolInfo)
        {
            var n4h = symbolInfo.TP4H;
            var e4h = symbolInfo.Entry4H;
            var currentPrice = symbolInfo.CurrentPrice;

            if (currentPrice <= 0)
            {
                return false;
            }

            var shift4h = Math.Abs(n4h - e4h);
            var maxShift4h = shift4h / 2;

            if (symbolInfo.TrendDirection == 1)
            {
                // Uptrend
                if (currentPrice > n4h + maxShift4h)
                {
                    return true;
                }
            }
            else if (symbolInfo.TrendDirection == -1)
            {
                // Downtrend
                if (currentPrice < n4h - maxShift4h)
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
                return 1; // HITL allowed
            }

            return 0; // Nothing
        }
    }
}
