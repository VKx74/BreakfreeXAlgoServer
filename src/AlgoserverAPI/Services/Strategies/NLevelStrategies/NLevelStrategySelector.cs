using System.Threading.Tasks;

namespace Algoserver.Strategies.NLevelStrategy
{
    public static class NLevelStrategySelector
    {
        public static NLevelStrategyBase SelectStrategy(NLevelStrategyInputContext context)
        {
            if (context.symbol == "XAU_USD")
            {
                return new NLevelStrategy_XAUUSD(context);
            } 
            if (context.symbol.ToUpper().EndsWith("USD"))
            {
                return new NLevelStrategy_EURUSD(context);
            }
            if (context.symbol.ToUpper().EndsWith("CHF"))
            {
                return new NLevelStrategy_USDCHF(context);
            }
            return null;
        }

        public static async Task<NLevelStrategyResponse> Calculate(NLevelStrategyInputContext context)
        {
            var strategy = SelectStrategy(context);
            if (strategy == null)
            {
                return null;
            }

            var res = await strategy.Calculate();
            return res;
        }
    }
}