using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Algoserver.API.Models.REST;
using Algoserver.API.Services;
using System;
using Algoserver.API.Models.EconomicCalendar;

namespace Algoserver.API.Controllers
{

    public class EconomicCalendarController : AlgoControllerBase
    {
        private EconomicCalendarService _economicCalendar;

        public EconomicCalendarController(EconomicCalendarService economicCalendar)
        {
            _economicCalendar = economicCalendar;
        }

        [Authorize]
        [HttpGet("economic-calendar")]
        [ProducesResponseType(typeof(Response<List<EconomicEvent>>), 200)]
        public async Task<IActionResult> GetEconomicCalendar()
        {
            var res = await _economicCalendar.GetEconomicEvents();
            return await ToEncryptedResponse(res, HttpContext.RequestAborted);
        }
    }
}
