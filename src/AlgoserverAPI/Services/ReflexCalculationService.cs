using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.API.Models.REST;
using Microsoft.Extensions.Logging;

namespace Algoserver.API.Services
{
    public class ReflexCalculationService
    {

        private readonly ILogger<AlgoService> _logger;
        private readonly HistoryService _historyService;

        public ReflexCalculationService(ILogger<AlgoService> logger, HistoryService historyService)
        {
            _logger = logger;
            _historyService = historyService;
        }

        internal Task<ReflexCalculationResponse> CalculateReflex(ReflexCalculationRequest req, CancellationToken token)
        {
            return Task.Run(() =>
            {
                return calculateReflex(req);
            }, token);
        }

        private async Task<ReflexCalculationResponse> calculateReflex(ReflexCalculationRequest req)
        {
            var Exchange = req.Instrument.Exchange.ToLowerInvariant();
            var Datafeed = req.Instrument.Datafeed.ToLowerInvariant();
            var Type = req.Instrument.Type.ToLowerInvariant();
            var Symbol = req.Instrument.Id;
            var granularity = AlgoHelper.ConvertTimeframeToGranularity(req.Timeframe.Interval, req.Timeframe.Periodicity);

            var additionalBarsCount = 3000;
            var priceData = await _historyService.GetHistoryWithCount(Symbol, granularity, Datafeed, Exchange, req.BarsCount + additionalBarsCount);

            if (priceData == null)
            {
                return null;
            }

            var calculation_input = priceData.Bars.Select(_ => _.Close).Reverse().ToArray();
            var reflex = req.Version == "mql" ?
                TechCalculations.ReflexOscillatorMQL(calculation_input, req.SuperSmooth, req.Period, req.PostSmooth) :
                TechCalculations.ReflexOscillatorTradingView(calculation_input, req.SuperSmooth, req.Period, req.PostSmooth);

            if (reflex.Length != calculation_input.Length)
            {
                return null;
            }

            var dates = priceData.Bars.Select(_ => _.Timestamp).ToArray();
            if (dates.Length > req.BarsCount + 1)
            {
                dates = dates.TakeLast(req.BarsCount + 1).ToArray();
            }

            reflex = reflex.Reverse().ToArray();
            if (reflex.Length > req.BarsCount + 1)
            {
                reflex = reflex.TakeLast(req.BarsCount + 1).ToArray();
            }
            return new ReflexCalculationResponse
            {
                dates = dates,
                values = reflex,
                id = req.Id
            };

        }
    }
}
