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

namespace Algoserver.API.Controllers
{

    public class AlgoController : AlgoControllerBase
    {
        private ScannerResultService _scannerResultService;
        private AlgoService _algoService;
        private RTDService _rtdService;
        private ScannerService _scanerService;

        public AlgoController(AlgoService algoService, RTDService rtdService, ScannerService scanerService, ScannerResultService scannerResultService)
        {
            _algoService = algoService;
            _rtdService = rtdService;
            _scanerService = scanerService;
            _scannerResultService = scannerResultService;
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
            var res = new CVarInfoResponse {
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

        [Authorize(Policy="free_user_restriction")]
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
    }
}
