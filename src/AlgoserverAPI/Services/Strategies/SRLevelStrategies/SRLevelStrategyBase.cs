using System.Collections.Generic;
using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{

    public abstract class SRLevelStrategyBase : LevelStrategyBase
    {
        protected SRLevelStrategyBase(StrategyInputContext _context) : base(_context)
        {
            StrategyName = "SRLevelStrategy";
        }

        public abstract Task<SRLevelStrategyResponse> Calculate();

        protected virtual async Task<SRLevelStrategyResponse> CalculateInternal(SRLevelStrategySettings settings)
        {
            var state = await GetState(settings);
            var sl = GetDefaultSL();
            var oppositeSl = GetDefaultOppositeSL();

            var result = new SRLevelStrategyResponse
            {
                State = state,
                SL1M = sl,
                SL5M = sl,
                SL15M = sl,
                OppositeSL1M = oppositeSl,
                OppositeSL5M = oppositeSl,
                OppositeSL15M = oppositeSl,
                DDClosePositions = false,
                // DDCloseInitialInterval = 30,
                // DDCloseIncreasePeriod = 30,
                // DDCloseIncreaseThreshold = 0.1m,
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

        protected virtual async Task<uint> GetState(SRLevelStrategySettings settings)
        {
            var symbol = context.mesaResponse.Symbol;

            // Capitulation logic
            // if (IsAutoTradeCapitulationConfirmed())
            // {
            //     WriteLog($"{symbol} AutoMode - capitulation filter");
            //     return 3; // Capitulation
            // }

            var canAutoTrade = await IsAutoTradeModeEnabled(settings);
            if (canAutoTrade)
            {
                return 2; // Auto allowed
            }
            return 1; // HITL allowed
        }

        protected virtual async Task<bool> IsAutoTradeModeEnabled(SRLevelStrategySettings settings)
        {
            var symbol = context.mesaResponse.Symbol;

            if (settings.CheckTrendsStrength && !IsEnoughStrength(settings.LowGroupStrength, settings.HighGroupStrength))
            {
                WriteLog($"{symbol} AutoMode - trends strength is not enough");
                return false;
            }

            if (settings.UseCatReflex)
            {
                var result = await CheckReflexOscillator(settings.CatReflexGranularity, settings.CatReflexPeriodSuperSmoother, settings.CatReflexPeriodReflex, settings.CatReflexPeriodPostSmooth, settings.CatReflexMinLevel, settings.CatReflexMaxLevel, settings.CatReflexConfirmationPeriod, settings.CatReflexValidateZeroCrossover);
                if (!result)
                {
                    WriteLog($"{symbol} CatReflex - is not valid");
                    return false;
                }
            }

            if (settings.UseCatReflex2)
            {
                var result = await CheckReflexOscillator(settings.CatReflexGranularity2, settings.CatReflexPeriodSuperSmoother2, settings.CatReflexPeriodReflex2, settings.CatReflexPeriodPostSmooth2, settings.CatReflexMinLevel2, settings.CatReflexMaxLevel2, settings.CatReflexConfirmationPeriod2, settings.CatReflexValidateZeroCrossover2);
                if (!result)
                {
                    WriteLog($"{symbol} CatReflex2 - is not valid");
                    return false;
                }
            }

            if (settings.UseCatReflex3)
            {
                var result = await CheckReflexOscillator(settings.CatReflexGranularity3, settings.CatReflexPeriodSuperSmoother3, settings.CatReflexPeriodReflex3, settings.CatReflexPeriodPostSmooth3, settings.CatReflexMinLevel3, settings.CatReflexMaxLevel3, settings.CatReflexConfirmationPeriod3, settings.CatReflexValidateZeroCrossover3);
                if (!result)
                {
                    WriteLog($"{symbol} CatReflex2 - is not valid");
                    return false;
                }
            }

            return true;
        }

        protected bool IsAutoTradeCapitulationConfirmed()
        {
            if (context.symbolInfo.MidGroupPhase == PhaseState.CD && context.symbolInfo.LongGroupPhase != PhaseState.Drive)
            {
                return true;
            }

            return false;
        }

    }
}