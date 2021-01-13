using System;
using System.Security.Claims;
using System.Threading;
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
using Newtonsoft.Json;
using Algoserver.API.Helpers;
using System.Collections.Generic;
using System.Text;

namespace Algoserver.API.Controllers
{
    public class AlgoController : Controller
    {
        private ScannerResultService _scannerResultService;
        private AlgoService _algoService;
        private RTDService _rtdService;
        private ScannerService _scanerService;
        private StatisticsService _statisticsService;

        public AlgoController(AlgoService algoService, RTDService rtdService, ScannerService scanerService, StatisticsService statisticsService, ScannerResultService scannerResultService)
        {
            _algoService = algoService;
            _rtdService = rtdService;
            _scanerService = scanerService;
            _statisticsService = statisticsService;
            _scannerResultService = scannerResultService;
        }

        [Authorize]
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
        [HttpPost(Routes.CalculateMarketInfo)]
        [ProducesResponseType(typeof(Response<CalculationMarketInfoResponse>), 200)]
        public async Task<IActionResult> CalculateMarketInfoAsync([FromBody] Instrument request, CancellationToken token)
        {  
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _algoService.CalculateMarketInfoAsync(request);
            return await ToEncryptedResponse(result, token);
        }
            
        [Authorize]
        [HttpPost(Routes.Calculate)]
        [ProducesResponseType(typeof(Response<CalculationResponse>), 200)]
        public async Task<IActionResult> CalculateAsync([FromBody] CalculationRequest request, CancellationToken token)
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

            return await ToEncryptedResponse(result, token);
        }
        
        [Authorize]
        [HttpPost(Routes.Backtest)]
        [ProducesResponseType(typeof(Response<CalculationResponse>), 200)]
        public async Task<IActionResult> BacktestAsync([FromBody] BacktestRequest request, CancellationToken token)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _algoService.BacktestAsync(request);

            return await ToEncryptedResponse(result, token);
        } 
        
        [Authorize]
        [HttpPost(Routes.StrategyV2Backtest)]
        [ProducesResponseType(typeof(Response<CalculationResponse>), 200)]
        public async Task<IActionResult> Strategy2BacktestAsync([FromBody] Strategy2BacktestRequest request, CancellationToken token)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _algoService.Strategy2BacktestAsync(request);

            return await ToEncryptedResponse(result, token);
        } 
        
        [Authorize]
        [HttpPost(Routes.HitTestExtensions)]
        [ProducesResponseType(typeof(Response<CalculationResponse>), 200)]
        public async Task<IActionResult> HitTestExtensions([FromBody] HittestRequest request, CancellationToken token)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _algoService.HitTestExtensionsAsync(request);

            return await ToEncryptedResponse(result, token);
        }

        [Authorize]
        [HttpPost(Routes.RTCalculation)]
        [ProducesResponseType(typeof(Response<RTDCalculationResponse>), 200)]
        public async Task<IActionResult> CalculateRTD([FromBody] RTDCalculationRequest request, CancellationToken token)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _rtdService.CalculateMESARTD(request, token);
            return await ToEncryptedResponse(result, token);
        }
        
        [HttpPost("rtd_test")]
        [ProducesResponseType(typeof(Response<RTDCalculationResponse>), 200)]
        public async Task<IActionResult> CalculateTestRTD([FromBody] RTDCalculationRequest request)
        {
           if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            // var result = await _rtdService.CalculateMESARTD(request);
            var stringBuilder = new StringBuilder();
            var d = 0.0m;
            for (var i = 0; i < request.BarsCount; i++) {
                stringBuilder.Append(d);
                d += 0.00000001m;
            }
            return Ok(stringBuilder.ToString());
        }

        [Authorize]
        [HttpGet(Routes.ScannerResults)]
        [ProducesResponseType(typeof(Response<ScannerResponse>), 200)]
        public async Task<IActionResult> ScannerResults([FromQuery] string segment = "")
        {
            var res = _scannerResultService.GetData();
            return await ToEncryptedResponse(res, CancellationToken.None);
        } 
        
        [Authorize]
        [HttpGet(Routes.ScannerHistoryResults)]
        [ProducesResponseType(typeof(Response<ScannerHistoryResponse>), 200)]
        public async Task<IActionResult> ScannerHistoryResults([FromQuery] string segment = "")
        {
            var res = _scannerResultService.GetHistoryData();
            return await ToEncryptedResponse(res, CancellationToken.None);
        }

        [NonAction]
        public Task<JsonResult> ToEncryptedResponse(object data, CancellationToken token = default) {
            return Task.Run(() => {
                var res = JsonConvert.SerializeObject(data);
                var encryptedRes = EncryptionHelper.Encrypt(res);
                return Json(new EncryptedResponse {
                    data = encryptedRes
                });
            }, token);
        }
    }
}
