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
        private IMemoryCache _cache;
        private AlgoService _algoService;
        private StatisticsService _statisticsService;

        public AlgoController(AlgoService algoService, StatisticsService statisticsService, IMemoryCache cache)
        {
            _algoService = algoService;
            _statisticsService = statisticsService;
            _cache = cache;
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
    }
}
