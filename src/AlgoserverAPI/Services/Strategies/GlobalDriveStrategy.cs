using System;
using Algoserver.API.Models.REST;

namespace Algoserver.API.Services
{
    public static class GlobalDriveStrategy
    {
        public static bool IsHITLModeEnabled(AutoTradingSymbolInfoResponse symbolInfo)
        {
            var strength1month = symbolInfo.Strength1Month * 100;

            if (symbolInfo.TrendDirection == 1)
            {
                // Uptrend
                if (strength1month < 21)
                {
                    return false;
                }
            }
            else if (symbolInfo.TrendDirection == -1)
            {
                // Downtrend
                if (strength1month > -21)
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

        public static bool IsAutoTradeModeEnabled(AutoTradingSymbolInfoResponse symbolInfo)
        {
            if (symbolInfo.CurrentPhase != PhaseState.Drive)
            {
                return false;
            }

            // if (symbolInfo.CurrentPhase != PhaseState.Drive && symbolInfo.NextPhase != PhaseState.Drive)
            // {
            //     return false;
            // }

            var strength1month = symbolInfo.Strength1Month * 100;
            var midGroupStrength = symbolInfo.MidGroupStrength * 100;
            var longGroupStrength = symbolInfo.LongGroupStrength * 100;

            if (symbolInfo.TrendDirection == 1)
            {
                // Uptrend
                if (longGroupStrength < 10 || midGroupStrength < 10)
                {
                    return false;
                }
            }
            else if (symbolInfo.TrendDirection == -1)
            {
                // Downtrend
                if (longGroupStrength > -10 || midGroupStrength > -10)
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

        public static bool IsAutoTradeCapitulationConfirmed(AutoTradingSymbolInfoResponse symbolInfo)
        {
            if (symbolInfo.ShortGroupPhase != PhaseState.CD)
            {
                return false;
            }

            if (symbolInfo.MidGroupPhase == PhaseState.Drive || symbolInfo.MidGroupPhase == PhaseState.TailTransition)
            {
                return false;
            }

            if (symbolInfo.LongGroupPhase == PhaseState.Drive)
            {
                return false;
            }

            var strength1h = symbolInfo.Strength1H * 100;

            if (symbolInfo.TrendDirection == 1)
            {
                // Uptrend
                if (strength1h > 16)
                {
                    return false;
                }
            }
            else if (symbolInfo.TrendDirection == -1)
            {
                // Downtrend
                if (strength1h < -16)
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

        public static bool IsHITLCapitulationConfirmed(AutoTradingSymbolInfoResponse symbolInfo)
        {
            if (symbolInfo.MidGroupPhase != PhaseState.CD)
            {
                return false;
            }

            if (symbolInfo.LongGroupPhase != PhaseState.Tail && symbolInfo.LongGroupPhase != PhaseState.DriveTransition)
            {
                return false;
            }

            var strength1h = symbolInfo.Strength1H * 100;

            if (symbolInfo.TrendDirection == 1)
            {
                // Uptrend
                if (strength1h > 1)
                {
                    return false;
                }
            }
            else if (symbolInfo.TrendDirection == -1)
            {
                // Downtrend
                if (strength1h < -1)
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
            var n1d = symbolInfo.TP1D;
            var e1d = symbolInfo.Entry1D;
            var n4h = symbolInfo.TP4H;
            var e4h = symbolInfo.Entry4H;
            var currentPrice = symbolInfo.CurrentPrice;

            if (currentPrice <= 0)
            {
                return false;
            }

            var shift1d = Math.Abs(n1d - e1d);
            var maxShift1d = shift1d - symbolInfo.HalfBand1D;

            var shift4h = Math.Abs(n4h - e4h);
            var maxShift4h = shift4h - symbolInfo.HalfBand4H;

            if (symbolInfo.TrendDirection == 1)
            {
                // Uptrend
                if (currentPrice > n1d + maxShift1d)
                {
                    return true;
                }
                if (currentPrice > n4h + maxShift4h)
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

        public static uint GetState(AutoTradingSymbolInfoResponse symbolInfo)
        {
            if (IsAutoTradeCapitulationConfirmed(symbolInfo))
            {
                return 3;
            }
            else if (!IsInOverheatZone(symbolInfo))
            {
                if (IsAutoTradeModeEnabled(symbolInfo))
                {
                    return 2;
                }
                else if (IsHITLModeEnabled(symbolInfo))
                {
                    return 1;
                }
            }
            
            return 0;
        }

    }
}
