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
        private IServiceProvider _serviceProvider;
        private IMemoryCache _cache;
        public AlgoController(IServiceProvider serviceProvider, IMemoryCache cache)
        {
            _serviceProvider = serviceProvider;
            _cache = cache;
        }

        [Authorize]
        [HttpGet(Routes.Calculate)]
        [ProducesResponseType(typeof(Response<CalculationResponse>), 200)]
        public async Task<IActionResult> CalculateAsync([FromQuery] CalculationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var algoService = _serviceProvider.GetRequiredService<AlgoService>();
            var result = await algoService.CalculateAsync(request);

            return Json(result);
        }
    }
}
