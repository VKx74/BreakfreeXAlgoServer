using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Algoserver.API.Models.REST;
using Algoserver.API.Services;
using System;
using Algoserver.API.Services.CacheServices;
using System.Linq;

namespace Algoserver.API.Controllers
{
    public class AlgoController : AlgoControllerBase
    {
        private ScannerResultService _scannerResultService;
        private MesaPreloaderService _mesaPreloaderService;
        private AutoTradingPreloaderService _autoTradingPreloaderService;
        private AlgoService _algoService;
        private RTDService _rtdService;
        private ScannerService _scanerService;

        public AlgoController(AlgoService algoService, RTDService rtdService, ScannerService scanerService, ScannerResultService scannerResultService, MesaPreloaderService mesaPreloaderService, AutoTradingPreloaderService autoTradingPreloaderService)
        {
            _algoService = algoService;
            _rtdService = rtdService;
            _scanerService = scanerService;
            _scannerResultService = scannerResultService;
            _mesaPreloaderService = mesaPreloaderService;
            _autoTradingPreloaderService = autoTradingPreloaderService;
        }

        [Authorize(Policy = "free_user_restriction")]
        [HttpPost(Routes.CalculateV2)]
        [ProducesResponseType(typeof(Response<CalculationResponseV2>), 200)]
        public async Task<IActionResult> CalculateV2Async([FromBody] CalculationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _algoService.CalculateV2Async(request);
            return await ToEncryptedResponse(result, CancellationToken.None);
        }

        [Authorize(Policy = "free_user_restriction")]
        [HttpPost(Routes.CalculateV3)]
        [ProducesResponseType(typeof(Response<CalculationResponseV3>), 200)]
        public async Task<IActionResult> CalculateV3Async([FromBody] CalculationRequestV3 request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _algoService.CalculateV3Async(request);
            return await ToEncryptedResponse(result, CancellationToken.None);
        }

        [Authorize]
        [HttpPost(Routes.CalculatePositionSize)]
        [ProducesResponseType(typeof(Response<CalculatePositionSizeResponse>), 200)]
        public async Task<IActionResult> CalculatePositionSize([FromBody] CalculatePositionSizeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _algoService.CalculatePositionSize(request);
            return await ToEncryptedResponse(result, CancellationToken.None);
        }

        [Authorize]
        [HttpPost(Routes.CalculatePriceRatio)]
        [ProducesResponseType(typeof(Response<CalculatePriceRatioResponse>), 200)]
        public async Task<IActionResult> CalculatePriceRatio([FromBody] CalculatePriceRatioRequest request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _algoService.CalculatePriceRatio(request);
            return await ToEncryptedResponse(result, CancellationToken.None);
        }

        [Authorize]
        [HttpPost(Routes.CalculateMarketInfoV2)]
        [ProducesResponseType(typeof(Response<CalculationMarketInfoResponse>), 200)]
        public async Task<IActionResult> CalculateMarketInfoV2Async([FromBody] MarketInfoCalculationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _algoService.CalculateMarketInfoV2Async(request);
            return await ToEncryptedResponse(result, HttpContext.RequestAborted);
        }

        [Authorize]
        [HttpPost(Routes.CalculateCVar)]
        [ProducesResponseType(typeof(Response<CVarInfoResponse>), 200)]
        public async Task<IActionResult> CalculateCVarAsync([FromBody] Instrument Instrument)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _algoService.CalculateCVarAsync(Instrument);
            var res = new CVarInfoResponse
            {
                cvar = result
            };

            return await ToEncryptedResponse(res, HttpContext.RequestAborted);
        }

        [Authorize(Policy = "free_user_restriction")]
        [HttpPost(Routes.RTCalculation)]
        [ProducesResponseType(typeof(Response<RTDCalculationResponse>), 200)]
        public async Task<IActionResult> CalculateRTD([FromBody] RTDCalculationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _rtdService.CalculateMESARTD(request, HttpContext.RequestAborted);
            return await ToEncryptedResponse(result, HttpContext.RequestAborted);
        }

        // [Authorize(Policy="free_user_restriction")]
        [HttpGet(Routes.ScannerResults)]
        [ProducesResponseType(typeof(Response<ScannerResponse>), 200)]
        public async Task<IActionResult> ScannerResults([FromQuery] string segment = "")
        {
            var res = _scannerResultService.GetData();
            return await ToEncryptedResponse(res, HttpContext.RequestAborted);
        }

        [Authorize(Policy = "free_user_restriction")]
        [HttpGet(Routes.ScannerHistoryResults)]
        [ProducesResponseType(typeof(Response<ScannerHistoryResponse>), 200)]
        public async Task<IActionResult> ScannerHistoryResults([FromQuery] string segment = "")
        {
            var res = _scannerResultService.GetHistoryData();
            return await ToEncryptedResponse(res, HttpContext.RequestAborted);
        }

        [Authorize]
        [HttpPost(Routes.SonarHistoryCache)]
        [ProducesResponseType(typeof(Response<ScannerCacheItem>), 200)]
        public async Task<IActionResult> GetSonarHistoryCache([FromBody] HistoryDataRequest request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var res = _scannerResultService.GetSonarHistoryCache(request.Symbol, request.Exchange, request.Timeframe, request.Time);
            return await ToEncryptedResponse(res, HttpContext.RequestAborted);
        }

        [HttpGet(Routes.Version)]
        public ActionResult<IEnumerable<string>> Version()
        {
            return Ok(Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }

        [Obsolete]
        [Authorize(Policy = "free_user_restriction")]
        [HttpPost(Routes.Calculate)]
        [ProducesResponseType(typeof(Response<CalculationResponse>), 200)]
        public async Task<IActionResult> CalculateAsync([FromBody] CalculationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _algoService.CalculateAsync(request);

            var instrument = request.Instrument.Id
                .Replace("_", "")
                .Replace("/", "");

            return await ToEncryptedResponse(result, HttpContext.RequestAborted);
        }

        [Obsolete]
        [Authorize]
        [HttpPost(Routes.CalculateMarketInfo)]
        [ProducesResponseType(typeof(Response<CalculationMarketInfoResponse>), 200)]
        public async Task<IActionResult> CalculateMarketInfoAsync([FromBody] Instrument request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _algoService.CalculateMarketInfoAsync(request);
            return await ToEncryptedResponse(result, HttpContext.RequestAborted);
        }

        [Authorize]
        [HttpGet(Routes.Trends)]
        [ProducesResponseType(typeof(Response<MesaResponse>), 200)]
        public async Task<IActionResult> GetMesaAsync([FromQuery] string symbol, [FromQuery] string datafeed, [FromQuery] int granularity = -1)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var granularityList = new List<int>();
            if (granularity > 0)
            {
                granularityList.Add(granularity);
            }
            var result = await _algoService.GetMesaAsync(symbol, datafeed, granularityList);
            return await ToEncryptedResponse(result, HttpContext.RequestAborted);
        }

        [Authorize]
        [HttpGet(Routes.TrendsSummary)]
        [ProducesResponseType(typeof(Response<List<MesaSummaryResponse>>), 200)]
        public async Task<IActionResult> GetMesaSummaryAsync()
        {
            var res = getMesaSummaryResponse();
            return await ToEncryptedResponse(res, HttpContext.RequestAborted);
        }

        [Authorize]
        [HttpPost(Routes.NeuralAlgoGlobal)]
        [ProducesResponseType(typeof(Response<CalculationResponseV3>), 200)]
        public async Task<IActionResult> CalculateNeuralAlgoGlobalAsync([FromBody] CalculationRequestV3 request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _algoService.CalculateV3Async(request);
            return await ToResponse(result, HttpContext.RequestAborted);
        }

        [Authorize]
        [HttpGet(Routes.TrendsGlobal)]
        [ProducesResponseType(typeof(MesaResponse), 200)]
        public async Task<IActionResult> GetMesaGlobalAsync([FromQuery] string symbol, [FromQuery] string datafeed, [FromQuery] int granularity = -1)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var granularityList = new List<int>();
            if (granularity > 0)
            {
                granularityList.Add(granularity);
            }

            var result = await _algoService.GetMesaAsync(symbol, datafeed, granularityList);
            return await ToResponse(result, HttpContext.RequestAborted);
        }

        [Authorize]
        [HttpGet(Routes.TrendsGlobalSummary)]
        [ProducesResponseType(typeof(List<MesaSummaryResponse>), 200)]
        public async Task<IActionResult> GetMesaSummaryGlobalAsync()
        {
            var result = getMesaSummaryResponse();
            return await ToResponse(result, HttpContext.RequestAborted);
        }

        private List<MesaSummaryResponse> getMesaSummaryResponse()
        {
            var res = _mesaPreloaderService.GetMesaSummary();

            var result = new List<MesaSummaryResponse>();
            foreach (var r in res)
            {
                var autoTradingInfo = _autoTradingPreloaderService.GetAutoTradingSymbolInfoFromCache(r.Symbol, r.Datafeed);
                result.Add(new MesaSummaryResponse
                {
                    datafeed = r.Datafeed,
                    symbol = r.Symbol,
                    strength = r.Strength.ToDictionary((_) => _.Key, (_) => new MesaLevelResponse
                    {
                        f = _.Value.f,
                        s = _.Value.s,
                        t = _.Value.t,
                        v = _.Value.v
                    }),
                    trend_period_descriptions = r.TrendPeriodDescriptions.ToDictionary((_) => _.Key, (_) => new TrendPeriodDescriptionResponse
                    {
                        strength = _.Value.strength,
                        volatility = _.Value.volatility,
                        duration = _.Value.duration,
                        phase = _.Value.phase
                    }),
                    timeframe_strengths = r.TimeframeStrengths,
                    volatility = r.Volatility,
                    durations = r.Durations,
                    timeframe_phase = r.TimeframePhase,
                    total_strength = r.TotalStrength,
                    avg_strength = r.AvgStrength,
                    last_price = r.LastPrice,
                    price60 = r.Price60,
                    price300 = r.Price300,
                    price900 = r.Price900,
                    price3600 = r.Price3600,
                    price14400 = r.Price14400,
                    price86400 = r.Price86400,
                    timeframe_state = r.TimeframeState,
                    current_phase = r.CurrentPhase,
                    next_phase = r.NextPhase,
                    trading_state = autoTradingInfo != null ? autoTradingInfo.TradingState : 0
                });
            }
            return result;
        }
    }
}
