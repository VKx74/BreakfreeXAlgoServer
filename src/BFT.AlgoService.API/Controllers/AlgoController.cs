using System;
using System.Threading.Tasks;
using Common.API;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using BFT.AlgoService.Logic.Services;

namespace BFT.AlgoService.API.Controllers
{
    /// <summary>
    /// Working with file storage
    /// </summary>
    [ApiVersion("1")]
    [Produces("application/json")]
    [Route("Algo")]
    [EnableCors("CorsPolicy")]
    [Consumes("application/json", "multipart/form-data")]
    public class AlgoController : BaseController
    {
        private readonly Logic.Services.AlgoService _algoService;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="service"></param>
        /// <param name="config"></param>
        public AlgoController(Logic.Services.AlgoService service, IConfigurationRoot config) : base(config)
        {
            _algoService = service;
        }

        /// <summary>
        /// Test
        /// </summary>
        /// <returns></returns>
        [HttpGet("Hello")]
        [MapToApiVersion("1")]
        public async Task<IActionResult> SayHelloAsync()
        {
            return Ok(_algoService.SayHallo());
        }
    }
}
