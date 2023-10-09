using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Algoserver.API.Models.REST;
using Algoserver.API.Services;
using System;
using Algoserver.API.Services.CacheServices;
using System.Text;
using Algoserver.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Threading;
using Algoserver.API.Models;
using System.Linq;

namespace Algoserver.API.Controllers
{
    [Serializable]
    public class AutoTraderStatistic
    {
        public Dictionary<string, DateTime> Accounts { get; set; }
        public Dictionary<string, int> Requests { get; set; }
        public Dictionary<string, int> Errors { get; set; }
        public string Id { get; set; }
        public DateTime Date { get; set; }
    }

    public static class AutoTraderStatisticService
    {
        private static string id = Guid.NewGuid().ToString();

        private static AutoTraderStatistic info;

        static AutoTraderStatisticService()
        {
            Init();
        }

        public static void Init()
        {
            info = new AutoTraderStatistic();
            info.Id = id;
            info.Date = DateTime.UtcNow;
            info.Accounts = new Dictionary<string, DateTime>();
            info.Requests = new Dictionary<string, int>();
            info.Errors = new Dictionary<string, int>();
        }

        public static void AddAccount(string account)
        {
            if (info.Accounts.ContainsKey(account))
            {
                info.Accounts[account] = DateTime.UtcNow;
            }
            else
            {
                info.Accounts.Add(account, DateTime.UtcNow);
            }
        }

        public static void AddRequest(string request)
        {
            if (!info.Requests.ContainsKey(request))
            {
                info.Requests.Add(request, 0);
            }
            info.Requests[request] = info.Requests[request] + 1;
        }

        public static void AddError(string error)
        {
            if (!info.Errors.ContainsKey(error))
            {
                info.Errors.Add(error, 0);
            }
            info.Errors[error] = info.Errors[error] + 1;
        }

        public static AutoTraderStatistic GetData()
        {
            return info;
        }
    }

    [Route("apex")]
    public class AutoTraderController : AlgoControllerBase
    {
        private AutoTradingAccountsService _autoTradingAccountsService;
        private readonly AutoTradingPreloaderService _autoTradingPreloaderService;
        private readonly AutoTradingRateLimitsService _autoTradingRateLimitsService;
        private readonly AutoTradingUserInfoService _autoTradingUserInfoService;

        public AutoTraderController(AutoTradingAccountsService autoTradingAccountsService, AutoTradingPreloaderService autoTradingPreloaderService, AutoTradingRateLimitsService autoTradingRateLimitsService, AutoTradingUserInfoService autoTradingUserInfoService)
        {
            _autoTradingAccountsService = autoTradingAccountsService;
            _autoTradingPreloaderService = autoTradingPreloaderService;
            _autoTradingRateLimitsService = autoTradingRateLimitsService;
            _autoTradingUserInfoService = autoTradingUserInfoService;
        }

        [Authorize]
        [HttpGet("info")]
        public async Task<IActionResult> GetInfoAsync()
        {
            return await ToResponse(AutoTraderStatisticService.GetData(), CancellationToken.None);
        }

        [Authorize]
        [HttpGet("config/{account}")]
        public async Task<IActionResult> GetUserInfoAsync([FromRoute] string account)
        {
            AutoTraderStatisticService.AddRequest("[GET]config/" + account);

            if (String.IsNullOrEmpty(account))
            {
                AutoTraderStatisticService.AddError("401");
                return Unauthorized("Invalid trading account");
            }

            AutoTraderStatisticService.AddAccount(account);

            if (!_autoTradingRateLimitsService.Validate(account))
            {
                AutoTraderStatisticService.AddError("429");
                return StatusCode(429);
            }

            if (!_autoTradingAccountsService.Validate(account))
            {
                AutoTraderStatisticService.AddError("401");
                return Unauthorized("Invalid trading account");
            }

            return await ToResponse(_autoTradingUserInfoService.GetUserInfo(account), CancellationToken.None);
        }

        [Authorize]
        [HttpPost("config/add-markets")]
        public async Task<IActionResult> UserInfoAddMarketsAsync([FromBody] UserInfoAddMarketsRequest request)
        {
            AutoTraderStatisticService.AddRequest("[POST]config/add-markets/" + request.Account);

            if (String.IsNullOrEmpty(request.Account))
            {
                AutoTraderStatisticService.AddError("401");
                return Unauthorized("Invalid trading account");
            }

            AutoTraderStatisticService.AddAccount(request.Account);

            if (!_autoTradingRateLimitsService.Validate(request.Account))
            {
                AutoTraderStatisticService.AddError("429");
                return StatusCode(429);
            }

            if (!_autoTradingAccountsService.Validate(request.Account))
            {
                AutoTraderStatisticService.AddError("401");
                return Unauthorized("Invalid trading account");
            }

            var markets = request.Markets.Select((_) => new UserDefinedMarketData
            {
                symbol = _.Symbol,
                minStrength = _.MinStrength,
                minStrength1H = _.MinStrength1H,
                minStrength4H = _.MinStrength4H,
                minStrength1D = _.MinStrength1D
            }).ToList();

            var maxAmount = _autoTradingAccountsService.GetMaxTradingInstrumentsCount(request.Account);
            var accountInfo = _autoTradingUserInfoService.GetUserInfo(request.Account);
            var existingCount = accountInfo != null && accountInfo.markets != null ? accountInfo.markets.Count : 0;

            if (existingCount >= maxAmount)
            {
                AutoTraderStatisticService.AddError("403");
                return StatusCode(403, "Maximum amount of tradable instruments used");
            }

            var result = _autoTradingUserInfoService.AddMarkets(request.Account, markets);

            return await ToResponse(result, CancellationToken.None);
        }

        [Authorize]
        [HttpPost("config/remove-markets")]
        public async Task<IActionResult> UserInfoRemoveMarketsAsync([FromBody] UserInfoRemoveMarketsRequest request)
        {
            AutoTraderStatisticService.AddRequest("[POST]config/remove-markets/" + request.Account);

            if (String.IsNullOrEmpty(request.Account))
            {
                AutoTraderStatisticService.AddError("401");
                return Unauthorized("Invalid trading account");
            }

            AutoTraderStatisticService.AddAccount(request.Account);

            if (!_autoTradingRateLimitsService.Validate(request.Account))
            {
                AutoTraderStatisticService.AddError("429");
                return StatusCode(429);
            }

            if (!_autoTradingAccountsService.Validate(request.Account))
            {
                AutoTraderStatisticService.AddError("401");
                return Unauthorized("Invalid trading account");
            }

            var result = _autoTradingUserInfoService.RemoveMarkets(request.Account, request.Markets);

            return await ToResponse(result, CancellationToken.None);
        }

        [Authorize]
        [HttpPost("config/add-disabled-markets")]
        public async Task<IActionResult> UserInfoAddDisabledMarketsAsync([FromBody] UserInfoAddDisabledMarketsRequest request)
        {
            AutoTraderStatisticService.AddRequest("[POST]config/add-disabled-markets/" + request.Account);

            if (String.IsNullOrEmpty(request.Account))
            {
                AutoTraderStatisticService.AddError("401");
                return Unauthorized("Invalid trading account");
            }

            AutoTraderStatisticService.AddAccount(request.Account);

            if (!_autoTradingRateLimitsService.Validate(request.Account))
            {
                AutoTraderStatisticService.AddError("429");
                return StatusCode(429);
            }

            if (!_autoTradingAccountsService.Validate(request.Account))
            {
                AutoTraderStatisticService.AddError("401");
                return Unauthorized("Invalid trading account");
            }

            var result = _autoTradingUserInfoService.AddDisabledMarkets(request.Account, request.Markets);

            return await ToResponse(result, CancellationToken.None);
        }

        [Authorize]
        [HttpPost("config/remove-disabled-markets")]
        public async Task<IActionResult> UserInfoRemoveDisabledMarketsAsync([FromBody] UserInfoRemoveDisabledMarketsRequest request)
        {
            AutoTraderStatisticService.AddRequest("[POST]config/remove-disabled-markets/" + request.Account);

            if (String.IsNullOrEmpty(request.Account))
            {
                AutoTraderStatisticService.AddError("401");
                return Unauthorized("Invalid trading account");
            }

            AutoTraderStatisticService.AddAccount(request.Account);

            if (!_autoTradingRateLimitsService.Validate(request.Account))
            {
                AutoTraderStatisticService.AddError("429");
                return StatusCode(429);
            }

            if (!_autoTradingAccountsService.Validate(request.Account))
            {
                AutoTraderStatisticService.AddError("401");
                return Unauthorized("Invalid trading account");
            }

            var result = _autoTradingUserInfoService.RemoveDisabledMarkets(request.Account, request.Markets);

            return await ToResponse(result, CancellationToken.None);
        }

        [Authorize]
        [HttpPost("config/change-use-manual-trading")]
        public async Task<IActionResult> UserInfoChangeUseManualTradingAsync([FromBody] UserInfoChangeUseManualTradingRequest request)
        {
            AutoTraderStatisticService.AddRequest("[POST]config/change-use-manual-trading/" + request.Account);

            if (String.IsNullOrEmpty(request.Account))
            {
                AutoTraderStatisticService.AddError("401");
                return Unauthorized("Invalid trading account");
            }

            AutoTraderStatisticService.AddAccount(request.Account);

            if (!_autoTradingRateLimitsService.Validate(request.Account))
            {
                AutoTraderStatisticService.AddError("429");
                return StatusCode(429);
            }

            if (!_autoTradingAccountsService.Validate(request.Account))
            {
                AutoTraderStatisticService.AddError("401");
                return Unauthorized("Invalid trading account");
            }

            var result = _autoTradingUserInfoService.UpdateUseManualTrading(request.Account, request.UseManualTrading);

            return await ToResponse(result, CancellationToken.None);
        }

        [Authorize]
        [HttpPost("config/change-bot-state")]
        public async Task<IActionResult> UserInfoChangeBotStateAsync([FromBody] UserInfoChangeBotStateRequest request)
        {
            AutoTraderStatisticService.AddRequest("[POST]config/change-bot-state/" + request.Account);

            if (String.IsNullOrEmpty(request.Account))
            {
                AutoTraderStatisticService.AddError("401");
                return Unauthorized("Invalid trading account");
            }

            AutoTraderStatisticService.AddAccount(request.Account);

            if (!_autoTradingRateLimitsService.Validate(request.Account))
            {
                AutoTraderStatisticService.AddError("429");
                return StatusCode(429);
            }

            if (!_autoTradingAccountsService.Validate(request.Account))
            {
                AutoTraderStatisticService.AddError("401");
                return Unauthorized("Invalid trading account");
            }

            var result = _autoTradingUserInfoService.UpdateBotState(request.Account, request.SwitchedOff);

            return await ToResponse(result, CancellationToken.None);
        }

        [Authorize]
        [HttpPost("config/change-market-risk")]
        public async Task<IActionResult> ChangeMarketRiskAsync([FromBody] UserInfoChangeMarketRiskRequest request)
        {
            AutoTraderStatisticService.AddRequest("[POST]config/change-market-risk/" + request.Account);

            if (String.IsNullOrEmpty(request.Account))
            {
                AutoTraderStatisticService.AddError("401");
                return Unauthorized("Invalid trading account");
            }

            AutoTraderStatisticService.AddAccount(request.Account);

            if (!_autoTradingRateLimitsService.Validate(request.Account))
            {
                AutoTraderStatisticService.AddError("429");
                return StatusCode(429);
            }

            if (!_autoTradingAccountsService.Validate(request.Account))
            {
                AutoTraderStatisticService.AddError("401");
                return Unauthorized("Invalid trading account");
            }

            var result = _autoTradingUserInfoService.ChangeMarketRisk(request.Account, request.Market, request.Risk);

            return await ToResponse(result, CancellationToken.None);
        }

        [Authorize]
        [HttpPost("config/change-account-risk")]
        public async Task<IActionResult> ChangeAccountRiskAsync([FromBody] UserInfoChangeAccountRiskRequest request)
        {
            AutoTraderStatisticService.AddRequest("[POST]config/change-account-risk/" + request.Account);

            if (String.IsNullOrEmpty(request.Account))
            {
                AutoTraderStatisticService.AddError("401");
                return Unauthorized("Invalid trading account");
            }

            AutoTraderStatisticService.AddAccount(request.Account);

            if (!_autoTradingRateLimitsService.Validate(request.Account))
            {
                AutoTraderStatisticService.AddError("429");
                return StatusCode(429);
            }

            if (!_autoTradingAccountsService.Validate(request.Account))
            {
                AutoTraderStatisticService.AddError("401");
                return Unauthorized("Invalid trading account");
            }

            var result = _autoTradingUserInfoService.ChangeAccountRisk(request.Account, request.Risk);

            return await ToResponse(result, CancellationToken.None);
        }

        [Authorize]
        [HttpPost("config/change-default-market-risk")]
        public async Task<IActionResult> ChangeDefaultMarketRiskAsync([FromBody] UserInfoChangeDefaultMarketRiskRequest request)
        {
            AutoTraderStatisticService.AddRequest("[POST]config/change-default-market-risk/" + request.Account);

            if (String.IsNullOrEmpty(request.Account))
            {
                AutoTraderStatisticService.AddError("401");
                return Unauthorized("Invalid trading account");
            }

            AutoTraderStatisticService.AddAccount(request.Account);

            if (!_autoTradingRateLimitsService.Validate(request.Account))
            {
                AutoTraderStatisticService.AddError("429");
                return StatusCode(429);
            }

            if (!_autoTradingAccountsService.Validate(request.Account))
            {
                AutoTraderStatisticService.AddError("401");
                return Unauthorized("Invalid trading account");
            }

            var result = _autoTradingUserInfoService.ChangeDefaultMarketRisk(request.Account, request.Risk);

            return await ToResponse(result, CancellationToken.None);
        }

        [HttpPost(Routes.ApexStream)]
        [ProducesResponseType(typeof(Response<string>), 200)]
        public async Task<IActionResult> GetAutoTradeInfoAsync([FromBody] AutoTradingSymbolInfoRequest request)
        {
            AutoTraderStatisticService.AddRequest(Routes.ApexStream);

            if (!ModelState.IsValid)
            {
                AutoTraderStatisticService.AddError("500");
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            if (String.IsNullOrEmpty(request.Account))
            {
                AutoTraderStatisticService.AddError("401");
                return Unauthorized("Invalid trading account");
            }

            AutoTraderStatisticService.AddAccount(request.Account);

            if (!_autoTradingRateLimitsService.Validate(request.Account))
            {
                AutoTraderStatisticService.AddError("429");
                return StatusCode(429);
            }

            if (!_autoTradingAccountsService.Validate(request.Account))
            {
                AutoTraderStatisticService.AddError("401");
                return Unauthorized("Invalid trading account");
            }

            return await CalculateSymbolInfoAsync(request);
        }

        [HttpPost(Routes.ApexMarkets)]
        [ProducesResponseType(typeof(Response<string>), 200)]
        public async Task<IActionResult> GetAutoTradeInstrumentsAsync([FromBody] AutoTradeInstrumentsRequest request)
        {
            AutoTraderStatisticService.AddRequest(Routes.ApexMarkets);

            if (!ModelState.IsValid)
            {
                AutoTraderStatisticService.AddError("500");
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            if (String.IsNullOrEmpty(request.Account))
            {
                AutoTraderStatisticService.AddError("401");
                return Unauthorized("Invalid trading account");
            }

            AutoTraderStatisticService.AddAccount(request.Account);

            if (!_autoTradingRateLimitsService.Validate(request.Account))
            {
                AutoTraderStatisticService.AddError("429");
                return StatusCode(429);
            }

            if (!_autoTradingAccountsService.Validate(request.Account))
            {
                AutoTraderStatisticService.AddError("401");
                return Unauthorized("Invalid trading account");
            }

            var result = string.Empty;

            if (request.Version == "2.0")
            {
                result = await GetAutoTradeInstrumentsAsyncV2(request.Account);
            }
            else
            {
                result = await GetAutoTradeInstrumentsAsyncV1(request.Account);
            }

            return Ok(result);
        }

        [HttpPost(Routes.MarketsConfig)]
        [ProducesResponseType(typeof(Response<string>), 200)]
        public async Task<IActionResult> GetAutoTradeMarketsConfigAsync([FromBody] AutoTradeMarketsConfigRequest request)
        {
            AutoTraderStatisticService.AddRequest(Routes.ApexMarkets);

            if (!ModelState.IsValid)
            {
                AutoTraderStatisticService.AddError("500");
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid input parameters");
            }

            if (String.IsNullOrEmpty(request.Account))
            {
                AutoTraderStatisticService.AddError("401");
                return Unauthorized("Invalid trading account");
            }

            AutoTraderStatisticService.AddAccount(request.Account);

            if (!_autoTradingRateLimitsService.Validate(request.Account))
            {
                AutoTraderStatisticService.AddError("429");
                return StatusCode(429);
            }

            if (!_autoTradingAccountsService.Validate(request.Account))
            {
                AutoTraderStatisticService.AddError("401");
                return Unauthorized("Invalid trading account");
            }

            var result = new List<AutoTradingInstrumentConfigResponse>();
            var dedications = await _autoTradingPreloaderService.GetAutoTradingInstrumentsDedication(request.Account);
            var instruments = dedications.Instruments;

            foreach (var item in instruments)
            {
                var instrumentRisk = GetInstrumentRisk(dedications, item.Symbol);
                result.Add(new AutoTradingInstrumentConfigResponse
                {
                    IsTradable = true,
                    MaxRisks = instrumentRisk,
                    Risks = (double)item.Risk,
                    Symbol = item.Symbol,
                    IsDisabled = item.IsDisabled
                });
            }

            var allMarkets = _autoTradingPreloaderService.GetAutoTradeAllInstruments();
            foreach (var symbol in allMarkets)
            {
                if (!instruments.Any((_) => string.Equals(_.Symbol, symbol, StringComparison.InvariantCultureIgnoreCase)))
                {
                    var instrumentRisk = GetInstrumentRisk(dedications, symbol);
                    result.Add(new AutoTradingInstrumentConfigResponse
                    {
                        IsTradable = false,
                        MaxRisks = instrumentRisk,
                        Risks = 0,
                        Symbol = symbol,
                        IsDisabled = dedications.DisabledInstruments.Any((_) => string.Equals(InstrumentsHelper.NormalizedInstrumentWithCrypto(_), InstrumentsHelper.NormalizedInstrumentWithCrypto(symbol), StringComparison.InvariantCultureIgnoreCase))
                    });
                }
            }

            return Ok(result);
        }

        [Obsolete]
        [NonAction]
        private async Task<string> GetAutoTradeInstrumentsAsyncV1(string account)
        {
            var items = await _autoTradingPreloaderService.GetAutoTradeInstruments(account);
            var stringResult = new StringBuilder();
            foreach (var item in items)
            {
                stringResult.AppendLine($"{item.Symbol}={Math.Round(item.Risk, 2)}");
            }

            return stringResult.ToString();
        }

        [NonAction]
        private async Task<string> GetAutoTradeInstrumentsAsyncV2(string account)
        {
            var dedications = await _autoTradingPreloaderService.GetAutoTradingInstrumentsDedication(account);
            var stringResult = new StringBuilder();
            var instruments = dedications.Instruments;

            foreach (var item in instruments)
            {
                var instrumentRisk = GetInstrumentRisk(dedications, item.Symbol);
                if (!item.IsDisabled)
                {
                    stringResult.AppendLine($"{item.Symbol}={Math.Round(item.Risk, 2)};{instrumentRisk}");
                }
                else
                {
                    stringResult.AppendLine($"{item.Symbol}=0;{instrumentRisk}");
                }
            }

            var allMarkets = _autoTradingPreloaderService.GetAutoTradeAllInstruments();
            foreach (var symbol in allMarkets)
            {
                if (!instruments.Any((_) => string.Equals(_.Symbol, symbol, StringComparison.InvariantCultureIgnoreCase)))
                {
                    var instrumentRisk = GetInstrumentRisk(dedications, symbol);
                    stringResult.AppendLine($"{symbol}=0;{instrumentRisk}");
                }
            }

            stringResult.AppendLine($"accountRisk={dedications.AccountRisk}");
            stringResult.AppendLine($"useManualTrading={dedications.UseManualTrading}");

            return stringResult.ToString();
        }

        [NonAction]
        private int GetInstrumentRisk(AutoTradingInstrumentsDedicationResponse dedications, string instrument)
        {
            instrument = InstrumentsHelper.NormalizedInstrumentWithCrypto(instrument);
            if (dedications.Risks.ContainsKey(instrument))
            {
                return dedications.Risks[instrument];
            }

            return dedications.DefaultMarketRisk;
        }

        private async Task<IActionResult> CalculateSymbolInfoAsync(AutoTradingSymbolInfoRequest request)
        {
            var mappedSymbol = SymbolMapper(request.Instrument.Id);
            if (string.IsNullOrEmpty(mappedSymbol))
            {
                return BadRequest("Invalid instrument");
            }

            var result = await _autoTradingPreloaderService.GetAutoTradingSymbolInfo(mappedSymbol, request.Instrument.Datafeed, request.Instrument.Exchange, request.Instrument.Type);
            return Ok(BuildBotStringResponse(result));
        }
        private string BuildBotStringResponse(AutoTradingSymbolInfoResponse result)
        {
            var stringResult = new StringBuilder();
            stringResult.AppendLine($"strengthTotal={Math.Round(result.TotalStrength * 100, 2)}");
            stringResult.AppendLine($"generalStopLoss={Math.Round(result.SL, 5)}");
            stringResult.AppendLine($"trendDirection={result.TrendDirection}");

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
            stringResult.AppendLine($"strength1month={Math.Round(result.Strength1Month * 100, 2)}");
            stringResult.AppendLine($"strength1y={Math.Round(result.Strength1Y * 100, 2)}");
            stringResult.AppendLine($"strength10y={Math.Round(result.Strength10Y * 100, 2)}");

            stringResult.AppendLine($"currentPhase={result.CurrentPhase}");
            stringResult.AppendLine($"nextPhase={result.NextPhase}");
            stringResult.AppendLine($"shortGroupPhase={result.ShortGroupPhase}");
            stringResult.AppendLine($"midGroupPhase={result.MidGroupPhase}");
            stringResult.AppendLine($"longGroupPhase={result.LongGroupPhase}");
            stringResult.AppendLine($"shortGroupStrength={Math.Round(result.ShortGroupStrength * 100, 2)}");
            stringResult.AppendLine($"midGroupStrength={Math.Round(result.MidGroupStrength * 100, 2)}");
            stringResult.AppendLine($"longGroupStrength={Math.Round(result.LongGroupStrength * 100, 2)}");

            stringResult.AppendLine($"tradingState={result.TradingState}");
            stringResult.AppendLine($"tt={result.Time}");

            return stringResult.ToString();
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
                {"BTC_USDT", "BTCUSDT"},
                {"ETHUSDT", "ETHUSDT"},
                {"ETH_USDT", "ETHUSDT"},
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
