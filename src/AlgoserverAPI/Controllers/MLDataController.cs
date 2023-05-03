using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Algoserver.API.Models.REST;
using Algoserver.API.Services;

namespace Algoserver.API.Controllers
{
    public class MLDataController : AlgoControllerBase
    {
        private ScannerResultService _scannerResultService;
        private AlgoService _algoService;
        private RTDService _rtdService;
        private ScannerService _scanerService;

        public MLDataController(AlgoService algoService, RTDService rtdService, ScannerService scanerService, ScannerResultService scannerResultService)
        {
            _algoService = algoService;
            _rtdService = rtdService;
            _scanerService = scanerService;
            _scannerResultService = scannerResultService;
        }

        [HttpGet("mldata")]
        [ProducesResponseType(typeof(Response<MLDataResponse>), 200)]
        public async Task<IActionResult> CalculateSR([FromQuery] string symbol, [FromQuery] int granularity)
        {
            var result = await _algoService.CalculateSRAsync(symbol, granularity);
            return Ok(result);
        }
    }
}
