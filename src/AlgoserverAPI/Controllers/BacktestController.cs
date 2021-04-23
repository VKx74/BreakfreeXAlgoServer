using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Algoserver.API.Models.REST;
using Algoserver.API.Services;
using System.Collections.Generic;

namespace Algoserver.API.Controllers
{
    public class BacktestController : AlgoControllerBase
    {
        private AlgoService _algoService;

        public BacktestController(AlgoService algoService)
        {
            _algoService = algoService;
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

            return await ToEncryptedResponse(result, HttpContext.RequestAborted);
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

            return await ToEncryptedResponse(result, HttpContext.RequestAborted);
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

            return await ToEncryptedResponse(result, HttpContext.RequestAborted);
        }

        [Authorize]
        [HttpPost(Routes.BacktestExtensions)]
        [ProducesResponseType(typeof(Response<List<BacktestExtensionsResponse>>), 200)]
        public async Task<IActionResult> BacktestExtensions([FromBody] BacktestExtensions request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _algoService.BacktestExtensionsAsync(request);

            return Ok(result);
        }

    }
}
