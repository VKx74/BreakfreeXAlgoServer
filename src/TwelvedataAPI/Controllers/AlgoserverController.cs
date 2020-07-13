using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Algoserver.API.Models.REST;
using Algoserver.API.Services.History;
using Algoserver.API.Services.Instruments;
using Algoserver.API.Helpers;

namespace Algoserver.API.Controllers
{
    public class TwelvedataController : Controller
    {
        private IServiceProvider _serviceProvider;
        private IMemoryCache _cache;
        public TwelvedataController(IServiceProvider serviceProvider, IMemoryCache cache)
        {
            _serviceProvider = serviceProvider;
            _cache = cache;
        }

        [HttpGet(Routes.History)]
        [ProducesResponseType(typeof(Response<HistoryResponse>), 200)]
        public async Task<IActionResult> GetHistoryAsync([FromQuery] HistoryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var historyManager = _serviceProvider.GetRequiredService<HistoryService>();
            var result = await historyManager.GetHistoryAsync(request);

            return Json(result);
        }


        [HttpGet(Routes.Instruments)]
        [ProducesResponseType(typeof(Response<List<InstrumentResponse>>), 200)]
        public IActionResult GetInstruments([FromQuery] InstrumentRequest request)
        {
            // var hash = request.Hash();
            // try {
            //     if (_cache.TryGetValue(hash, out JsonResult cachedResponse)) {
            //         return cachedResponse;
            //     }
            // } catch(Exception e) {
            //     Console.WriteLine("Failed to get cached response");
            //     Console.WriteLine(e.Message);
            // }

            var instumentManager = _serviceProvider.GetRequiredService<InstrumentService>();
            // var isInitialized = instumentManager.IsInitialized();
            var instruments = instumentManager.GetInstruments();

            Expression<Func<Instrument, bool>> search = i => string.IsNullOrEmpty(request.Search) || 
                i.Symbol.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                i.EscapedSymbol.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                (request.Search.Length > 2 && (i.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase) || i.CurrencyBase.Contains(request.Search, StringComparison.OrdinalIgnoreCase) || i.CurrencyQuote.Contains(request.Search, StringComparison.OrdinalIgnoreCase)));

            var response = instruments.GetCountResponse(request, search, null, i => i.ToInstrumentResponse());
            var json = Json(response);

            // try {
            //     _cache.Set(hash, json, TimeSpan.FromDays(3));
            // } catch(Exception e) {
            //     Console.WriteLine("Failed to set cached response");
            //     Console.WriteLine(e.Message);
            // }

            return json;
        }

        [HttpGet(Routes.InstrumentsExtended)]
        [ProducesResponseType(typeof(Response<List<InstrumentExtendedResponse>>), 200)]
        public IActionResult GetInstrumentsExtended(InstrumentRequest request)
        {
            var hash = request.Hash();
            try {
                if (_cache.TryGetValue(hash, out JsonResult cachedResponse)) {
                    return cachedResponse;
                }
            } catch(Exception e) {
                Console.WriteLine("Failed to get cached response");
                Console.WriteLine(e.Message);
            }

            var instumentManager = _serviceProvider.GetRequiredService<InstrumentService>();

            var isInitialized = instumentManager.IsInitialized();
            var instruments = instumentManager.GetInstruments(i =>
               (string.IsNullOrEmpty(request.Search) || i.Symbol.Contains(request.Search, StringComparison.OrdinalIgnoreCase) || i.EscapedSymbol.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
               (request.Search.Length > 2 && (i.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase) || i.CurrencyBase.Contains(request.Search, StringComparison.OrdinalIgnoreCase) || i.CurrencyQuote.Contains(request.Search, StringComparison.OrdinalIgnoreCase)))) //search
            && (request.Exchange == null || ((i.Exchange?.Contains(request.Exchange, StringComparison.OrdinalIgnoreCase) ?? false) || (i.AvailableExchanges?.Contains(request.Exchange, StringComparer.OrdinalIgnoreCase) ?? false))) //look for Exhcanges or Available Exchanges
            && (request.Kind == null || i.Kind.ToString().Contains(request.Kind, StringComparison.OrdinalIgnoreCase))); //look for kind

            var response = instruments.GetCountResponse(request, null, null, i => i.ToInstrumentExtendedResponse());
            var json = Json(response);

            try {
                if (isInitialized) {
                    _cache.Set(hash, json, TimeSpan.FromDays(1));
                }
            } catch(Exception e) {
                Console.WriteLine("Failed to set cached response");
                Console.WriteLine(e.Message);
            }

            return json;
        }
    }
}
