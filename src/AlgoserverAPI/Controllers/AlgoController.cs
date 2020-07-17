using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Algoserver.API.Models.REST;
using Algoserver.API.Services;

namespace Algoserver.API.Controllers
{
    public class AlgoController : Controller
    {
        private IMemoryCache _cache;
        private AlgoService _algoService;
        public AlgoController(AlgoService algoService, IMemoryCache cache)
        {
            _algoService = algoService;
            _cache = cache;
        }

        // [Authorize]
        [HttpPost(Routes.Calculate)]
        [ProducesResponseType(typeof(Response<CalculationResponse>), 200)]
        public async Task<IActionResult> CalculateAsync([FromBody] CalculationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var result = await _algoService.CalculateAsync(request);

            return Json(result);
        }
    }
}
