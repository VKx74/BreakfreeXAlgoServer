using System;
using System.Collections.Generic;
using System.Linq;
using Algoserver.API.Helpers;
using Algoserver.API.Models.REST;
using Algoserver.API.Services.CacheServices;

namespace Algoserver.API.Services
{
    [Serializable]
    public class DriveCapitulationStrategyState
    {
        public uint State { get; set; } // 1 - capitulated, 2 - drive
    }

    public static class DriveCapitulationStrategy
    {
        public static bool IsHITLModeEnabled(AutoTradingSymbolInfoResponse symbolInfo)
        {
            var strength1month = symbolInfo.Strength1Month * 100;

            if (symbolInfo.TrendDirection == 1)
            {
                // Uptrend
                if (strength1month < 42)
                {
                    return false;
                }
            }
            else if (symbolInfo.TrendDirection == -1)
            {
                // Downtrend
                if (strength1month > -42)
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

        public static bool IsAutoTradeModeEnabled(AutoTradingSymbolInfoResponse symbolInfo, MESADataSummary mesaResponse, string symbol, ICacheService cacheService)
        {
            var state = getState(symbol, cacheService);
            if (symbolInfo.CurrentPhase == PhaseState.Drive && (IsEnoughStrength(symbolInfo, 20, 20) || IsEnoughStrength(symbolInfo, 15, 25) || IsEnoughStrength(symbolInfo, 25, 15)))
            {
                if (state.State != 2)
                {
                    setState(symbol, new DriveCapitulationStrategyState { State = 2 }, cacheService);
                }
                // return true;
            }

            if (state.State != 2)
            {
                return false;
            }

            if (mesaResponse.TimeframePhase.TryGetValue(TimeframeHelper.MIN15_GRANULARITY, out var m15Phase))
            {
                if (m15Phase != PhaseState.Drive)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            if (symbolInfo.MidGroupPhase != PhaseState.Drive)
            {
                return false;
            }

            return true;
        }

        public static bool IsAutoTradeCapitulationConfirmed(AutoTradingSymbolInfoResponse symbolInfo, string symbol, ICacheService cacheService)
        {
            if (symbolInfo.MidGroupPhase == PhaseState.CD && symbolInfo.LongGroupPhase != PhaseState.Drive)
            {
                setState(symbol, new DriveCapitulationStrategyState { State = 1 }, cacheService);
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
        
        public static bool IsTooMatchVolatility(MESADataSummary mesaResponse, int granularity)
        {
            if (mesaResponse.Volatility.TryGetValue(granularity, out var volatility))
            {
                var relativeVolatility = volatility - 100f;
                return relativeVolatility > 0;
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

        public static uint GetState(AutoTradingSymbolInfoResponse symbolInfo, MESADataSummary mesaResponse, string symbol, ICacheService cacheService)
        {
            if (IsAutoTradeCapitulationConfirmed(symbolInfo, symbol, cacheService))
            {
                return 3; // Capitulation
            }

            // Validating support/resistance and min strength across all phases
            if (!IsInOverheatZone(symbolInfo))
            {
                if ((IsEnoughStrength(symbolInfo, 10, 10) || IsEnoughStrength(symbolInfo, 5, 15)) && IsAutoTradeModeEnabled(symbolInfo, mesaResponse, symbol, cacheService))
                {
                    if (IsTooMatchVolatility(mesaResponse, TimeframeHelper.MIN15_GRANULARITY))
                    {
                        return 1; // HITL allowed
                    }
                    return 2; // Auto trading allowed
                }

                if (IsEnoughStrength(symbolInfo, 10, 10) || IsEnoughStrength(symbolInfo, 5, 15))
                {
                    if (IsTooMatchVolatility(mesaResponse, TimeframeHelper.DAILY_GRANULARITY))
                    {
                        return 0; // Nothing
                    }
                    return 1; // HITL allowed
                }
            }

            return 0; // Nothing
        }

        private static DriveCapitulationStrategyState getState(string symbol, ICacheService cacheService)
        {
            try
            {
                if (cacheService.TryGetValue<DriveCapitulationStrategyState>("DriveCapitulationStrategyState_", symbol, out var driveCapitulationStrategyState))
                {
                    return driveCapitulationStrategyState;
                }

                // var initialSymbols = new List<string>() {
                //     "EUR_CHF",
                //     "EUR_NZD",
                //     "USD_CHF",
                //     "EUR_CAD"
                // };

                // if (initialSymbols.Any((_) => string.Equals(_, symbol, StringComparison.InvariantCultureIgnoreCase)))
                // {
                //     var presetState = new DriveCapitulationStrategyState 
                //     {
                //         State = 2
                //     };
                //     setState(symbol, presetState, cacheService);
                //     return presetState;
                // }

                return new DriveCapitulationStrategyState();
            }
            catch (Exception ex)
            {
                Console.WriteLine(">>> FAILED to get DriveCapitulationStrategyState, " + symbol);
                return new DriveCapitulationStrategyState();
            }
        }


        private static void setState(string symbol, DriveCapitulationStrategyState state, ICacheService cacheService)
        {
            try
            {
                cacheService.Set("DriveCapitulationStrategyState_", symbol, state, TimeSpan.FromDays(3));
            }
            catch (Exception ex)
            {
                Console.WriteLine(">>> FAILED to set DriveCapitulationStrategyState, " + symbol);
            }
        }

    }
}
