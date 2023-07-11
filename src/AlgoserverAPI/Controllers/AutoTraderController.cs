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
using Algoserver.API.Services.CacheServices;
using System.Linq;
using System.Text;
using Algoserver.API.Helpers;

namespace Algoserver.API.Controllers
{

    public class AutoTraderController : AlgoControllerBase
    {
        private ScannerResultService _scannerResultService;
        private MesaPreloaderService _mesaPreloaderService;
        private AlgoService _algoService;
        private ScannerService _scanerService;

        public AutoTraderController(AlgoService algoService, ScannerService scanerService, ScannerResultService scannerResultService, MesaPreloaderService mesaPreloaderService)
        {
            _algoService = algoService;
            _scanerService = scanerService;
            _scannerResultService = scannerResultService;
            _mesaPreloaderService = mesaPreloaderService;
        }

        [HttpPost(Routes.SymbolInfo)]
        [ProducesResponseType(typeof(Response<string>), 200)]
        public async Task<IActionResult> GetSymbolInfoAsync([FromBody] AutoTradingSymbolInfoRequest request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            var mappedSymbol = SymbolMapper(request.Instrument.Id);
            if (string.IsNullOrEmpty(mappedSymbol))
            {
                return BadRequest("Invalid instrument");
            }

            var result = await _algoService.CalculateV3LevelsAsync(mappedSymbol, request.Instrument.Datafeed, request.Instrument.Exchange, request.Instrument.Type);
            var trendDirection = 0;
            if ((decimal)Math.Abs(result.TotalStrength) * 100 >= request.MinStrength)
            {
                if (result.TotalStrength > 0)
                {
                    if ((result.Strength4H * 100 >= request.MinStrength4h) && (result.Strength1D * 100 >= request.MinStrength1d))
                    {
                        trendDirection = 1;
                    }
                }
                else if (result.TotalStrength < 0)
                {
                    if ((result.Strength4H * 100 <= request.MinStrength4h * -1) && (result.Strength1D * 100 <= request.MinStrength1d * -1))
                    {
                        trendDirection = -1;
                    }
                }
            }

            var stringResult = new StringBuilder();
            stringResult.AppendLine($"strengthTotal={Math.Round(result.TotalStrength * 100, 2)}");
            stringResult.AppendLine($"generalStopLoss={Math.Round(result.SL, 5)}");
            stringResult.AppendLine($"trendDirection={trendDirection}");

            stringResult.AppendLine($"hbh1m={Math.Round(result.HalfBand1M, 5)}");
            stringResult.AppendLine($"hbh5m={Math.Round(result.HalfBand5M, 5)}");
            stringResult.AppendLine($"hbh15m={Math.Round(result.HalfBand15M, 5)}");
            stringResult.AppendLine($"hbh1h={Math.Round(result.HalfBand1H, 5)}");
            stringResult.AppendLine($"hbh4h={Math.Round(result.HalfBand4H, 5)}");
            stringResult.AppendLine($"hbh1d={Math.Round(result.HalfBand1D, 5)}");

            stringResult.AppendLine($"n1m={Math.Round(result.TP1M, 5)}");
            stringResult.AppendLine($"n5m={Math.Round(result.TP5M, 5)}");
            stringResult.AppendLine($"n15m={Math.Round(result.TP15M, 5)}");
            stringResult.AppendLine($"n1h={Math.Round(result.TP1H, 5)}");
            stringResult.AppendLine($"n4h={Math.Round(result.TP4H, 5)}");
            stringResult.AppendLine($"n1d={Math.Round(result.TP1D, 5)}");

            stringResult.AppendLine($"1m={Math.Round(result.Entry1M, 5)}");
            stringResult.AppendLine($"5m={Math.Round(result.Entry5M, 5)}");
            stringResult.AppendLine($"15m={Math.Round(result.Entry15M, 5)}");
            stringResult.AppendLine($"1h={Math.Round(result.Entry1H, 5)}");
            stringResult.AppendLine($"4h={Math.Round(result.Entry4H, 5)}");
            stringResult.AppendLine($"1d={Math.Round(result.Entry1D, 5)}");

            stringResult.AppendLine($"strength1m={Math.Round(result.Strength1M * 100, 2)}");
            stringResult.AppendLine($"strength5m={Math.Round(result.Strength5M * 100, 2)}");
            stringResult.AppendLine($"strength15m={Math.Round(result.Strength15M * 100, 2)}");
            stringResult.AppendLine($"strength1h={Math.Round(result.Strength1H * 100, 2)}");
            stringResult.AppendLine($"strength4h={Math.Round(result.Strength4H * 100, 2)}");
            stringResult.AppendLine($"strength1d={Math.Round(result.Strength1D * 100, 2)}");

            return Ok(stringResult.ToString());
        }

        private string SymbolMapper(string symbol)
        {
            var mappedInstruments = new Dictionary<string, string> {
                {"BTCUSD", "BTCUSDT"},
                {"ETHUSD", "ETHUSDT"},
                {"BTC/USD", "BTCUSDT"},
                {"ETH/USD", "ETHUSDT"},
                {"BTC_USD", "BTCUSDT"},
                {"ETH_USD", "ETHUSDT"},
                {"BTCUSDT", "BTCUSDT"},
                {"ETHUSDT", "ETHUSDT"},
                {"US30", "US30_USD"},
                {"US100", "NAS100_USD"},
                {"NAS100", "NAS100_USD"},
                {"US500", "SPX500_USD"},
                {"SPX500", "SPX500_USD"},
                {"XBRUSD", "BCO_USD"},
                {"XTIUSD", "WTICO_USD"},
            };

            if (mappedInstruments.ContainsKey(symbol))
            {
                return mappedInstruments[symbol];
            }

            symbol = symbol.Replace("_", "").Replace("-", "").Replace("/", "").Replace("^", "");
            
            var allowedForex = InstrumentsHelper.ForexInstrumentList;
            foreach (var instrument in allowedForex)
            {
                var normalizedInstrument = instrument.Replace("_", "").Replace("-", "").Replace("/", "").Replace("^", "");
                if (string.Equals(symbol, normalizedInstrument, StringComparison.InvariantCultureIgnoreCase))
                {
                    return instrument;
                }
            }

            return string.Empty;
        }
    }
}
