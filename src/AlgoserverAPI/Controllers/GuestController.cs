using System;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Algoserver.API.Models.REST;
using Algoserver.API.Services;

namespace Algoserver.API.Controllers
{
    public class GuestController : AlgoControllerBase
    {
        private AlgoService _algoService;
        private RTDService _rtdService;

        public GuestController(AlgoService algoService, RTDService rtdService)
        {
            _algoService = algoService;
            _rtdService = rtdService;
        }

        [Authorize]
        [HttpPost(Routes.RTCalculationGuest)]
        [ProducesResponseType(typeof(Response<RTDCalculationResponse>), 200)]
        public async Task<IActionResult> CalculateRTDGuest([FromBody] RTDCalculationRequest request)
        {
            if (!ModelState.IsValid || request.Instrument == null || string.IsNullOrEmpty(request.Instrument.Id))
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            if (!string.Equals(request.Instrument.Id.Replace("_", ""), "eurusd", StringComparison.InvariantCultureIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            var result = await _rtdService.CalculateMESARTD(request, HttpContext.RequestAborted);
            return await ToEncryptedResponse(result, HttpContext.RequestAborted);
        }

        [Authorize]
        [HttpPost(Routes.CalculateV2Guest)]
        [ProducesResponseType(typeof(Response<CalculationResponseV2>), 200)]
        public async Task<IActionResult> CalculateV2GuestAsync([FromBody] CalculationRequest request)
        {
            if (!ModelState.IsValid || request.Instrument == null || string.IsNullOrEmpty(request.Instrument.Id))
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            if (!string.Equals(request.Instrument.Id.Replace("_", ""), "eurusd", StringComparison.InvariantCultureIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            var result = await _algoService.CalculateV2Async(request);
            return await ToEncryptedResponse(result, CancellationToken.None);
        }
    }
}
