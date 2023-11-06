using System;
using System.Collections.Generic;
using System.Linq;
using Algoserver.API.Helpers;
using Algoserver.API.Models.REST;
using Algoserver.API.Services.CacheServices;

namespace Algoserver.API.Services
{
    public static class ShortPeriodDriveStrategy
    {
        public static bool IsAutoTradeModeEnabled(AutoTradingSymbolInfoResponse symbolInfo, MESADataSummary mesaResponse)
        {
            if (IsTooMatchVolatility(mesaResponse, TimeframeHelper.MIN15_GRANULARITY))
            {
                return false;
            }

            if (IsInOverheatZone(symbolInfo))
            {
                return false;
            }

            if (IsEnoughStrength(symbolInfo, 25))
            {
                return false;
            }

            if (symbolInfo.MidGroupPhase != PhaseState.Drive)
            {
                return false;
            }

            if (!mesaResponse.TimeframePhase.TryGetValue(TimeframeHelper.MIN15_GRANULARITY, out var m15Phase))
            {
                return false;
            }

            if (!mesaResponse.TimeframePhase.TryGetValue(TimeframeHelper.MIN5_GRANULARITY, out var m5Phase))
            {
                return false;
            }

            if (m5Phase == PhaseState.CD)
            {
                return false;
            }

            if (m5Phase == PhaseState.Drive || m15Phase == PhaseState.Drive)
            {
                return true;
            }

            return false;
        } 
        
        public static bool IsHITLModeEnabled(AutoTradingSymbolInfoResponse symbolInfo, MESADataSummary mesaResponse)
        {
            if (IsTooMatchVolatility(mesaResponse, TimeframeHelper.HOURLY_GRANULARITY))
            {
                return false;
            }

            if (IsInOverheatZone(symbolInfo))
            {
                return false;
            }

            if (IsEnoughStrength(symbolInfo, 15))
            {
                return false;
            }

            if (symbolInfo.CurrentPhase == PhaseState.Drive || symbolInfo.NextPhase == PhaseState.Drive)
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

        public static bool IsTooMatchVolatility(MESADataSummary mesaResponse, int granularity)
        {
            if (mesaResponse.Volatility.TryGetValue(granularity, out var volatility))
            {
                var relativeVolatility = volatility - 100f;
                return relativeVolatility > 30;
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
            var maxShift1d = shift1d - (symbolInfo.HalfBand1D * 2);

            var shift4h = Math.Abs(n4h - e4h);
            var maxShift4h = shift4h - symbolInfo.HalfBand4H;

            if (symbolInfo.TrendDirection == 1)
            {
                // Uptrend
                if (currentPrice > n1d + maxShift1d)
                {
                    return true;
                }
                // if (currentPrice > n4h + maxShift4h)
                // {
                //     return true;
                // }
            }
            else if (symbolInfo.TrendDirection == -1)
            {
                // Downtrend
                if (currentPrice < n1d - maxShift1d)
                {
                    return true;
                }
                // if (currentPrice < n4h - maxShift4h)
                // {
                //     return true;
                // }
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
