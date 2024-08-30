using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.API.Services;

namespace Algoserver.Strategies.NLevelStrategy
{
    public abstract class NLevelStrategyBase
    {
        const int UPTREND = 1;
        const int DOWNTREND = -1;
        const int SIDEWAYS = 0;

        protected readonly bool ShowLogs = true;
        protected readonly NLevelStrategyInputContext context;

        protected NLevelStrategyBase(NLevelStrategyInputContext _context)
        {
            context = _context;
        }

        public abstract Task<NLevelStrategyResponse> Calculate();

        protected virtual async Task<NLevelStrategyResponse> CalculateInternal(NLevelStrategySettings settings)
        {
            var state = await GetState(settings);
            var sl = GetDefaultSL();
            var oppositeSl = GetDefaultOppositeSL();

            var result = new NLevelStrategyResponse
            {
                State = state,
                SL1M = sl,
                SL5M = sl,
                SL15M = sl,
                OppositeSL1M = oppositeSl,
                OppositeSL5M = oppositeSl,
                OppositeSL15M = oppositeSl,
                DDClosePositions = true,
                DDCloseInitialInterval = 30,
                DDCloseIncreasePeriod = 30,
                DDCloseIncreaseThreshold = 0.1m,
                MinStrength1M = 1,
                MinStrength5M = 1,
                MinStrength15M = 1,
                MinStrength1H = 1,
                MinStrength4H = 1,
                Skip1HourTrades = true,
                Skip4HourTrades = true
            };

            return result;
        }

        protected virtual async Task<uint> GetState(NLevelStrategySettings settings)
        {
            var symbol = context.mesaResponse.Symbol;

            // Capitulation logic
            if (IsAutoTradeCapitulationConfirmed())
            {
                WriteLog($"{symbol} AutoMode - capitulation filter");
                return 3; // Capitulation
            }

            var canAutoTrade = await IsAutoTradeModeEnabled(settings);
            if (canAutoTrade)
            {
                return 2; // Auto allowed
            }
            return 1; // HITL allowed
        }

        protected virtual async Task<bool> IsAutoTradeModeEnabled(NLevelStrategySettings settings)
        {
            var symbol = context.mesaResponse.Symbol;

            // Volatility logic
            if (!IsVolatilityCorrect(settings))
            {
                return false;
            }

            // Zone logic
            if (settings.UseOverheatZone1DFilter && IsInOverheatOn1DZone(settings.OverheatZone1DThreshold))
            {
                WriteLog($"{symbol} AutoMode - Overheat 1D Zone filter");
                return false;
            }

            if (settings.UseOverheatZone4HFilter && IsInOverheatOn4HZone(settings.OverheatZone4HThreshold))
            {
                WriteLog($"{symbol} AutoMode - Overheat 4H Zone filter");
                return false;
            }

            if (settings.UseOverheatZone1HFilter && IsInOverheatOn1HZone(settings.OverheatZone1HThreshold))
            {
                WriteLog($"{symbol} AutoMode - Overheat 1H Zone filter");
                return false;
            }

            if (settings.CheckTrends && !IsTrendCorrect(settings.TrendFilters))
            {
                WriteLog($"{symbol} AutoMode - trends state is not valid");
                return false;
            }

            if (settings.CheckTrendsStrength && !IsEnoughStrength(settings.LowGroupStrength, settings.HighGroupStrength))
            {
                WriteLog($"{symbol} AutoMode - trends strength is not enough");
                return false;
            }

            if (settings.CheckStrengthIncreasing && !IsStrengthIncreasing(settings.CheckStrengthReducePeriod, settings.CheckStrengthResetPeriod, settings.CheckStrengthReduceGranularity, settings.CheckStrengthResetGranularity))
            {
                WriteLog($"{symbol} AutoMode - strength decrease detected");
                return false;
            }

            if (settings.CheckPeaks && IsPeakDetected(settings.PeakDetectionGranularity, settings.PeakDetectionPeriod, settings.PeakDetectionThreshold))
            {
                WriteLog($"{symbol} AutoMode - strength decrease detected");
                return false;
            }

            if (settings.CheckRSI)
            {
                var rsiValidation = await RSICheck(settings.RSIPeriod, settings.RSIMax, settings.RSIMin);
                if (!rsiValidation)
                {
                    WriteLog($"{symbol} AutoMode - trends state is not valid");
                    return false;
                }
            }

            if (settings.CheckStochastic)
            {
                var result = await CheckStochastic(settings.StochasticGranularity, settings.StochasticPeriodK, settings.StochasticPeriodD, settings.StochasticSmooth, settings.StochasticThreshold);
                if (!result)
                {
                    WriteLog($"{symbol} Stochastic - is not valid");
                    return false;
                }
            }

            if (settings.UseCatReflex)
            {
                var result = await CheckReflexOscillator(settings.CatReflexGranularity, settings.CatReflexPeriodSuperSmoother, settings.CatReflexPeriodReflex, settings.CatReflexPeriodPostSmooth, settings.CatReflexMinLevel, settings.CatReflexMaxLevel, settings.CatReflexConfirmationPeriod);
                if (!result)
                {
                    WriteLog($"{symbol} CatReflex - is not valid");
                    return false;
                }
            }

            if (settings.UseCatReflex2)
            {
                var result = await CheckReflexOscillator(settings.CatReflexGranularity2, settings.CatReflexPeriodSuperSmoother2, settings.CatReflexPeriodReflex2, settings.CatReflexPeriodPostSmooth2, settings.CatReflexMinLevel2, settings.CatReflexMaxLevel2, settings.CatReflexConfirmationPeriod2);
                if (!result)
                {
                    WriteLog($"{symbol} CatReflex2 - is not valid");
                    return false;
                }
            }

            if (settings.UseCatReflex3)
            {
                var result = await CheckReflexOscillator(settings.CatReflexGranularity3, settings.CatReflexPeriodSuperSmoother3, settings.CatReflexPeriodReflex3, settings.CatReflexPeriodPostSmooth3, settings.CatReflexMinLevel3, settings.CatReflexMaxLevel3, settings.CatReflexConfirmationPeriod3);
                if (!result)
                {
                    WriteLog($"{symbol} CatReflex2 - is not valid");
                    return false;
                }
            }

            return true;
        }

        protected decimal GetSL(int granularity, int trendDirection)
        {
            var data = context.levelsResponse;
            if (data.TryGetValue(granularity, out var item))
            {
                var lastSar = item.sar.LastOrDefault();
                if (lastSar == null)
                {
                    return 0;
                }

                var halsBand = GetHalfBand(granularity, trendDirection);
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

        protected decimal GetHalfBand(int granularity, int trendDirection)
        {
            var data = context.levelsResponse;
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

        protected async Task<bool> RSICheck(int period, int rsivmax, int rsivmin)
        {

            var history = await context.historyService.GetHistory(context.symbol, TimeframeHelper.DAILY_GRANULARITY, context.datafeed, context.exchange, context.type, 0, period * 2);
            if (history.Bars.Count < period)
            {
                return false;
            }
            var rsiValue = RSIIndicator.CalculateLastRSI(history.Bars.Select(_ => _.Close).ToArray(), period);

            if (rsiValue > rsivmax || rsiValue < rsivmin)
            {
                return false;  // Filter out the top and bottom RSI values
            }

            return true;
        }


        protected bool IsStrengthIncreasing(int decreasePeriod, int resetPeriod, int decreaseGranularity, int resetGranularity)
        {
            var symbolInfo = context.symbolInfo;
            var mesa_additional = context.mesaAdditional;

            // use 1min not compressed data
            if (!mesa_additional.mesa.TryGetValue(decreaseGranularity, out var decreaseData))
            {
                return false;
            }
            // use 1min not compressed data
            if (!mesa_additional.mesa.TryGetValue(resetGranularity, out var resetData))
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

        protected bool IsInOverheatOn1DZone(decimal skipZoneThreshold)
        {
            return IsInOverheatZone(skipZoneThreshold, context.symbolInfo.TP1D, context.symbolInfo.Entry1D, context.symbolInfo.HalfBand1D);
        }

        protected bool IsInOverheatOn4HZone(decimal skipZoneThreshold)
        {
            return IsInOverheatZone(skipZoneThreshold, context.symbolInfo.TP4H, context.symbolInfo.Entry4H, context.symbolInfo.HalfBand4H);
        }

        protected bool IsInOverheatOn1HZone(decimal skipZoneThreshold)
        {
            return IsInOverheatZone(skipZoneThreshold, context.symbolInfo.TP1H, context.symbolInfo.Entry1H, context.symbolInfo.HalfBand1H);
        }

        protected bool IsInOverheatZone(decimal skipZoneThreshold, decimal nLevel, decimal eLevel, decimal halfBand)
        {
            var symbolInfo = context.symbolInfo;
            // var n1d = symbolInfo.TP1D;
            // var e1d = symbolInfo.Entry1D;
            // var halfBand = symbolInfo.HalfBand1D;
            var currentPrice = symbolInfo.CurrentPrice;

            if (currentPrice <= 0)
            {
                return false;
            }

            var shift1d = Math.Abs(nLevel - eLevel);
            var maxShift1d = shift1d - (halfBand * 2m / 100m * skipZoneThreshold);

            if (symbolInfo.TrendDirection == 1)
            {
                // Uptrend
                if (currentPrice > nLevel + maxShift1d)
                {
                    return true;
                }
            }
            else if (symbolInfo.TrendDirection == -1)
            {
                // Downtrend
                if (currentPrice < nLevel - maxShift1d)
                {
                    return true;
                }
            }

            return false;
        }

        protected bool IsTooMatchVolatility(int granularity, int min, int max)
        {
            if (context.mesaResponse.Volatility.TryGetValue(granularity, out var volatility15min))
            {
                var relativeVolatility15min = volatility15min - 100f;
                return relativeVolatility15min > max || relativeVolatility15min < min;
            }

            return false;
        }

        protected bool IsAutoTradeCapitulationConfirmed()
        {
            if (context.symbolInfo.MidGroupPhase == PhaseState.CD && context.symbolInfo.LongGroupPhase != PhaseState.Drive)
            {
                return true;
            }

            return false;
        }

        protected bool IsEnoughStrength(decimal lowStrength, decimal highStrength)
        {
            var symbolInfo = context.symbolInfo;
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

        protected bool IsPeakDetected(int granularity, int peakDetectionPeriod, int peakDetectionThreshold)
        {
            if (!context.mesaAdditional.mesa.TryGetValue(granularity, out var mesa))
            {
                return false;
            }

            if (!mesa.Any() || mesa.Count < peakDetectionPeriod)
            {
                return false;
            }

            var peaks = new List<float>();
            for (var i = 0; i < mesa.Count; i++)
            {
                var value = mesa[i].f - mesa[i].s;

                if (!peaks.Any())
                {
                    peaks.Add(value);
                    continue;
                }

                var lastValue = peaks.LastOrDefault();

                if (lastValue > 0 && value > 0 && value > lastValue)
                {
                    peaks[peaks.Count - 1] = value;
                }

                if (lastValue < 0 && value < 0 && value < lastValue)
                {
                    peaks[peaks.Count - 1] = value;
                }

                if (lastValue > 0 && value < 0)
                {
                    peaks.Add(value);
                }

                if (lastValue < 0 && value > 0)
                {
                    peaks.Add(value);
                }
            }

            var lastMesa = mesa.LastOrDefault();
            var currentStrength = lastMesa.f - lastMesa.s;

            if (peaks.Count <= peakDetectionPeriod)
            {
                return false;
            }

            float sum = 0;
            float min = float.MaxValue;
            float max = float.MinValue;
            // Do not count last peak, it still not formed to the end
            for (int i = peaks.Count - peakDetectionPeriod - 1; i < peaks.Count - 1; i++)
            {
                sum += peaks[i];

                if (peaks[i] > max)
                {
                    max = peaks[i];
                }
                if (peaks[i] < min)
                {
                    min = peaks[i];
                }
            }

            var avg = sum / peakDetectionPeriod;
            if (peakDetectionPeriod >= 5)
            {
                // filtering minimal and maximal value from range
                sum -= min;
                sum -= max;
                avg = sum / (peakDetectionPeriod - 2);
            }

            var currentStrengthPercentage = currentStrength / avg * 100;
            var res = currentStrengthPercentage >= peakDetectionThreshold;
            return res;
        }

        protected virtual bool IsVolatilityCorrect(NLevelStrategySettings settings)
        {
            var symbol = context.mesaResponse.Symbol;

            // Volatility logic
            if (settings.UseVolatilityFilter && IsTooMatchVolatility(settings.VolatilityGranularity, settings.VolatilityMin, settings.VolatilityMax))
            {
                WriteLog($"{symbol} AutoMode - volatility filter");
                return false;
            }

            // Volatility logic
            if (settings.UseVolatilityFilter2 && IsTooMatchVolatility(settings.VolatilityGranularity2, settings.VolatilityMin2, settings.VolatilityMax2))
            {
                WriteLog($"{symbol} AutoMode - volatility filter");
                return false;
            }
            return true;
        }

        protected virtual bool IsTrendCorrect(TrendFiltersSettings settings)
        {
            var si = context.symbolInfo;
            var dir = si.TrendDirection;

            var strengthConditionFilter1m = (si.Strength1M > 0 && si.Strength5M < 0) ||  // Prevent trading if 1m is positive and 5m is negative
                             (si.Strength1M < 0 && si.Strength5M > 0);     // Prevent trading if 1m is negative and 5m is positive

            var strengthConditionFilter5m = (si.Strength5M > 0 && si.Strength15M < 0) ||  // Prevent trading if 5m is positive and 15m is negative
                             (si.Strength5M < 0 && si.Strength15M > 0);     // Prevent trading if 5m is negative and 15m is positive

            var strengthConditionFilter15m = (si.Strength15M > 0 && si.Strength1H < 0) ||  // Prevent trading if 15m is positive and 1h is negative
                             (si.Strength15M < 0 && si.Strength1H > 0);     // Prevent trading if 15m is negative and 1h is positive

            var strengthConditionFilter1h = (si.Strength1H > 0 && si.Strength4H < 0) ||  // Prevent trading if 1h is positive and 4h is negative
                             (si.Strength1H < 0 && si.Strength4H > 0);     // Prevent trading if 1h is negative and 4h is positive

            if (settings.strengthConditionFilter1m && !strengthConditionFilter1m)
            {
                return false;
            }
            if (settings.strengthConditionFilter5m && !strengthConditionFilter5m)
            {
                return false;
            }
            if (settings.strengthConditionFilter15m && !strengthConditionFilter15m)
            {
                return false;
            }
            if (settings.strengthConditionFilter1h && !strengthConditionFilter1h)
            {
                return false;
            }

            if (settings.trendfilter1x)
            {
                if (!(dir == DOWNTREND && si.Strength5M <= si.Strength1M) &&
                    !(dir == UPTREND && si.Strength5M >= si.Strength1M))
                {
                    return false;
                }
            }

            if (settings.trendfilter2x)
            {
                if (!(dir == DOWNTREND && si.Strength5M <= si.Strength1M && si.Strength15M <= si.Strength5M) &&
                    !(dir == UPTREND && si.Strength5M >= si.Strength1M && si.Strength15M >= si.Strength5M))
                {
                    return false;
                }
            }


            if (settings.trendfilter3x)
            {
                if (!(dir == DOWNTREND && si.Strength5M <= si.Strength1M && si.Strength15M <= si.Strength5M && si.Strength1H <= si.Strength15M) &&
                    !(dir == UPTREND && si.Strength5M >= si.Strength1M && si.Strength15M >= si.Strength5M && si.Strength1H >= si.Strength15M))
                {
                    return false;
                }
            }

            return true;
        }

        protected async Task<bool> CheckStochastic(int granularity, int periodK, int periodD, int smooth, int threshold)
        {
            var barsCount = Math.Max(Math.Max(periodK, periodD), 100) * 3;
            var history = await context.historyService.GetHistory(context.symbol, granularity, context.datafeed, context.exchange, context.type, 0, barsCount);
            var high = history.Bars.Select((_) => _.High).ToArray();
            var low = history.Bars.Select((_) => _.Low).ToArray();
            var close = history.Bars.Select((_) => _.Close).ToArray();

            var stoch = TechCalculations.Stochastic(high, low, close, periodK, periodD, smooth);
            var isValid = ValidateStochastic(stoch[0], stoch[1], threshold);
            return isValid;
        }

        protected async Task<bool> CheckReflexOscillator(int granularity, double periodSuperSmoother, int reflexPeriod, double periodPostSmooth, double min, double max, int confirmationPeriod)
        {
            var barsCount = 500;

            var history = await context.historyService.GetHistory(context.symbol, granularity, context.datafeed, context.exchange, context.type, 0, barsCount);
            var close = history.Bars.Select((_) => _.Close).TakeLast(barsCount).Reverse().ToArray();

            var reflexValues = TechCalculations.ReflexOscillatorMQL(close, periodSuperSmoother, reflexPeriod, periodPostSmooth);

            var result = true;

            if (!CheckReflexIndicatorConditions(reflexValues, min, max, confirmationPeriod))
            {
                WriteLog($"{context.symbol}_{granularity}({reflexPeriod}, {periodSuperSmoother}, {periodPostSmooth}) => base conditions not correct");
                result = false;
            }

            if (result)
            {
                if (!CheckReflexIndicatorResetCondition(reflexValues, min, max, confirmationPeriod))
                {
                    WriteLog($"{context.symbol}_{granularity}({reflexPeriod}, {periodSuperSmoother}, {periodPostSmooth}) => reset conditions not correct");
                    result = false;
                }
            }

            double currentReflexValue = reflexValues[0];
            double previouse1ReflexValue = reflexValues[1];
            double previouse2ReflexValue = reflexValues[2];

            WriteLog($"{context.symbol}_{granularity}({reflexPeriod}, {periodSuperSmoother}, {periodPostSmooth}) => {currentReflexValue}, {previouse1ReflexValue}, {previouse2ReflexValue}");

            return result;
        }

        protected bool CheckReflexIndicatorResetCondition(double[] data, double min, double max, int confirmationPeriod)
        {
            double firstValue = data[0];
            int dataCount = data.Length;
            int end = dataCount - confirmationPeriod - 1;
            bool stopTradingConditionDetected = false;
            for (int i = 1; i < end; i++)
            {
                double currentValue = data[i];
                // reverse detected
                if ((firstValue > 0 && currentValue < 0) || (firstValue < 0 && currentValue > 0))
                {
                    return true;
                }

                double[] source = data.Skip(i).ToArray();
                bool canTrade = CheckReflexIndicatorConditions(source, min, max, confirmationPeriod);
                if (!canTrade && !stopTradingConditionDetected)
                {
                    stopTradingConditionDetected = true;
                }

                if (canTrade && stopTradingConditionDetected)
                {
                    return false;
                }
            }
            return true;
        }

        protected bool CheckReflexIndicatorConditions(double[] data, double min, double max, int confirmationPeriod)
        {
            var si = context.symbolInfo;
            var dir = si.TrendDirection;

            if (dir == SIDEWAYS)
            {
                return false;
            }

            double currentReflexValue = data[0];
            if (dir == UPTREND)
            {
                if (!(currentReflexValue > min && currentReflexValue < max))
                {
                    return false;
                }
            }
            if (dir == DOWNTREND)
            {
                if (!(currentReflexValue < min * -1 && currentReflexValue > max * -1))
                {
                    return false;
                }
            }

            if (!ConfirmReflexIndicatorDirection(data, confirmationPeriod, dir))
            {
                return false;
            }

            return true;
        }

        protected bool ConfirmReflexIndicatorDirection(double[] data, int period, int dir)
        {
            int arraySize = data.Length;
            int end = Math.Min(arraySize - 1, period);

            for (int i = 0; i < end; i++)
            {
                double current = data[i];
                double previouse = data[i + 1];

                if (dir == UPTREND && current < previouse)
                {
                    return false;
                }
                if (dir == DOWNTREND && current > previouse)
                {
                    return false;
                }
            }

            return true;
        }

        protected bool ValidateStochastic(decimal[] stochasticBuffer, decimal[] signalBuffer, int stochRSIThreshold)
        {
            var si = context.symbolInfo;
            var dir = si.TrendDirection;
            var isUpTrend = dir == UPTREND;

            int count = stochasticBuffer.Length;
            var lastRSIMain = stochasticBuffer[count - 2];
            var previouseRSIMain = stochasticBuffer[count - 3];

            var lastRSISignal = signalBuffer[count - 2];

            var result = true;
            var upLevel = 100 - stochRSIThreshold;
            var downLevel = stochRSIThreshold;
            var isInsideThreshold = lastRSIMain > downLevel && lastRSIMain < upLevel;
            var stochRSICheckTrendDirection = false;
            var stochRSICheckLevelCrossed = true;

            if (isUpTrend)
            {
                bool stochRSITrendDirectionFilter = false;
                if (stochRSICheckTrendDirection)
                {
                    stochRSITrendDirectionFilter = previouseRSIMain > lastRSIMain;
                }
                // Check direction
                if (stochRSITrendDirectionFilter || lastRSIMain <= lastRSISignal || !isInsideThreshold)
                {
                    result = false;
                }
                else
                {
                    if (stochRSICheckLevelCrossed)
                    {
                        result = false;
                        // check is previouse rsi crossover was in right up/down wave
                        for (int i = count - 2; i >= 0; i--)
                        {
                            var value = stochasticBuffer[i];
                            if (value >= upLevel)
                            {
                                break;
                            }
                            if (value <= downLevel)
                            {
                                result = true;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                bool stochRSITrendDirectionFilter = false;
                if (stochRSICheckTrendDirection)
                {
                    stochRSITrendDirectionFilter = previouseRSIMain < lastRSIMain;
                }
                // Check direction
                if (stochRSITrendDirectionFilter || lastRSIMain >= lastRSISignal || !isInsideThreshold)
                {
                    result = false;
                }
                else
                {
                    if (stochRSICheckLevelCrossed)
                    {
                        result = false;
                        // check is previouse rsi crossover was in right up/down wave
                        for (int i = count - 2; i >= 0; i--)
                        {
                            var value = stochasticBuffer[i];
                            if (value >= upLevel)
                            {
                                result = true;
                                break;
                            }
                            if (value <= downLevel)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return result;
        }

        protected decimal GetDefaultSL()
        {
            var hourSL = GetSL(TimeframeHelper.HOURLY_GRANULARITY, context.symbolInfo.TrendDirection);
            return hourSL;
        }

        protected decimal GetDefaultOppositeSL()
        {
            var oppositeHourSL = GetSL(TimeframeHelper.HOURLY_GRANULARITY, context.symbolInfo.TrendDirection > 0 ? -1 : 1);
            return oppositeHourSL;
        }

        protected void WriteLog(string str)
        {
            if (!ShowLogs)
            {
                return;
            }

            Console.WriteLine($"LowTimeframeNLevelStrategy >>> {str}");
        }

    }
}