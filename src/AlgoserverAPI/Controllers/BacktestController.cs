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
        private BacktestService _backtestService;
        private ScannerBacktestService _scannerBacktestService;

        public BacktestController(BacktestService backtestService, ScannerBacktestService scannerBacktestService)
        {
            _backtestService = backtestService;
            _scannerBacktestService = scannerBacktestService;
        }

        [Authorize]
        [HttpPost(Routes.Backtest)]
        [ProducesResponseType(typeof(Response<BacktestResponse>), 200)]
        public async Task<IActionResult> BacktestAsync([FromBody] BacktestRequest request)
        {
            // return Ok();
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _backtestService.BacktestAsync(request);

            return await ToEncryptedResponse(result, HttpContext.RequestAborted);
        }

        [Authorize]
        [HttpPost(Routes.StrategyV2Backtest)]
        [ProducesResponseType(typeof(Response<Strategy2BacktestResponse>), 200)]
        public async Task<IActionResult> Strategy2BacktestAsync([FromBody] Strategy2BacktestRequest request)
        {
            // return Ok();
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _backtestService.Strategy2BacktestAsync(request);

            return await ToEncryptedResponse(result, HttpContext.RequestAborted);
        }

        [Authorize]
        [HttpPost(Routes.HitTestExtensions)]
        [ProducesResponseType(typeof(Response<ExtHitTestResponse>), 200)]
        public async Task<IActionResult> HitTestExtensions([FromBody] HittestRequest request)
        {
            // return Ok();
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _backtestService.HitTestExtensionsAsync(request);

            return await ToEncryptedResponse(result, HttpContext.RequestAborted);
        }

        [Authorize]
        [HttpPost(Routes.BacktestExtensions)]
        [ProducesResponseType(typeof(Response<List<ExtensionBacktestResponse>>), 200)]
        public async Task<IActionResult> BacktestExtensions([FromBody] ExtensionsBacktestRequest request)
        {
            // return Ok();
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _backtestService.BacktestExtensionsAsync(request);

            return await ToEncryptedResponse(result, HttpContext.RequestAborted);
        }

        [Authorize]
        [HttpPost(Routes.BacktestScanner)]
        [ProducesResponseType(typeof(Response<List<ScannerBacktestResponse>>), 200)]
        public async Task<IActionResult> BacktestScanner([FromBody] ScannerBacktestRequest request)
        {
            // return Ok();
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _scannerBacktestService.BacktestAsync(request);

            return await ToEncryptedResponse(result, HttpContext.RequestAborted);
        }

    }
}
