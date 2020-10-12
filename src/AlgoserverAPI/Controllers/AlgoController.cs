using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using Algoserver.API.Exceptions;
using Algoserver.API.Models;
using Algoserver.API.Models.REST;
using Algoserver.API.Services;
using IdentityModel;
using RabbitMQ.Client.Impl;

namespace Algoserver.API.Controllers
{
    public class AlgoController : Controller
    {
        private ScannerCacheService _scannerCache;
        private AlgoService _algoService;
        private RTDService _rtdService;
        private ScannerService _scanerService;
        private StatisticsService _statisticsService;
        private ScannerHistoryService _scannerHistoryService;

        public AlgoController(AlgoService algoService, RTDService rtdService, ScannerService scanerService, StatisticsService statisticsService, ScannerCacheService scannerCache, ScannerHistoryService scannerHistoryService)
        {
            _algoService = algoService;
            _rtdService = rtdService;
            _scanerService = scanerService;
            _statisticsService = statisticsService;
            _scannerHistoryService = scannerHistoryService;
            _scannerCache = scannerCache;
        }

        [Authorize]
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
                .Replace("/","");
            _statisticsService.AddToCache(new Statistic
            {
                CreatedAt = DateTime.UtcNow,
                UserId = User.FindFirstValue(JwtClaimTypes.Subject),
                Email = User.FindFirstValue(JwtClaimTypes.Email),
                FirstName = User.FindFirstValue("first_name"),
                LastName = User.FindFirstValue("last_name"),
                Ip = HttpContext.Request.ClientIp(),
                AccountSize = request.InputAccountSize,
                Market = $"{instrument}-{request.Instrument.Exchange}",
                TimeFramePeriodicity = request.Timeframe.Periodicity,
                TimeFrameInterval = request.Timeframe.Interval,
                StopLossRatio = request.InputStoplossRatio,
                RiskOverride = request.InputRisk,
                SplitPositions = request.InputSplitPositions
            });

            return Json(result);
        }
        
        [Authorize]
        [HttpPost(Routes.Backtest)]
        [ProducesResponseType(typeof(Response<CalculationResponse>), 200)]
        public async Task<IActionResult> BacktestAsync([FromBody] BacktestRequest request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _algoService.BacktestAsync(request);

            return Json(result);
        } 
        
        [Authorize]
        [HttpPost(Routes.StrategyV2Backtest)]
        [ProducesResponseType(typeof(Response<CalculationResponse>), 200)]
        public async Task<IActionResult> Strategy2BacktestAsync([FromBody] Strategy2BacktestRequest request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _algoService.Strategy2BacktestAsync(request);

            return Json(result);
        } 
        
        [Authorize]
        [HttpPost(Routes.HitTestExtensions)]
        [ProducesResponseType(typeof(Response<CalculationResponse>), 200)]
        public async Task<IActionResult> HitTestExtensions([FromBody] HittestRequest request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _algoService.HitTestExtensionsAsync(request);

            return Json(result);
        }

        [Authorize]
        [HttpPost(Routes.RTCalculation)]
        [ProducesResponseType(typeof(Response<RTDCalculationResponse>), 200)]
        public async Task<IActionResult> CalculateRTD([FromBody] RTDCalculationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _rtdService.CalculateMESARTD(request);

            return Json(result);
        }

        [Authorize]
        [HttpPost(Routes.ScanInstrument)]
        [ProducesResponseType(typeof(Response<ScanInstrumentResponse>), 200)]
        public async Task<IActionResult> ScanInstrument([FromBody] ScanInstrumentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _scanerService.ScanInstrument(request);

            return Json(result);
        }

        [Authorize]
        [HttpPost(Routes.RefreshInstruments)]
        [ProducesResponseType(typeof(Response<object>), 200)]
        public async Task<IActionResult> LoadInstruments([FromBody] object request)
        {
            var res = await _scannerHistoryService.RefreshAll();
            return Json(new {
                res = res
            });
        }
        
        [Authorize]
        [HttpGet(Routes.ScannerResults)]
        [ProducesResponseType(typeof(Response<ScannerResponse>), 200)]
        public IActionResult ScannerResults([FromQuery] string segment = "")
        {
            var res = _scannerCache.GetData();
            return Json(new {
                res = res
            });
        }
    }
}
