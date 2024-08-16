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
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

namespace Algoserver.API.Controllers
{
    [Serializable]
    public class AutoTraderStatistic
    {
        public ConcurrentDictionary<string, DateTime> Accounts { get; set; }
        public ConcurrentDictionary<string, int> Requests { get; set; }
        public ConcurrentDictionary<string, int> Errors { get; set; }
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
            info.Accounts = new ConcurrentDictionary<string, DateTime>();
            info.Requests = new ConcurrentDictionary<string, int>();
            info.Errors = new ConcurrentDictionary<string, int>();
        }

        public static void AddAccount(string account)
        {
            if (info.Accounts.ContainsKey(account))
            {
                info.Accounts[account] = DateTime.UtcNow;
            }
            else
            {
                info.Accounts.TryAdd(account, DateTime.UtcNow);
            }
        }

        public static void AddRequest(string request)
        {
            if (!info.Requests.ContainsKey(request))
            {
                info.Requests.TryAdd(request, 0);
            }
            info.Requests[request] = info.Requests[request] + 1;
        }

        public static void AddError(string error)
        {
            if (!info.Errors.ContainsKey(error))
            {
                info.Errors.TryAdd(error, 0);
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
        private readonly StatisticsService _statisticsService;

        public AutoTraderController(AutoTradingAccountsService autoTradingAccountsService, AutoTradingPreloaderService autoTradingPreloaderService, AutoTradingRateLimitsService autoTradingRateLimitsService, AutoTradingUserInfoService autoTradingUserInfoService, StatisticsService statisticsService)
        {
            _autoTradingAccountsService = autoTradingAccountsService;
            _autoTradingPreloaderService = autoTradingPreloaderService;
            _autoTradingRateLimitsService = autoTradingRateLimitsService;
            _autoTradingUserInfoService = autoTradingUserInfoService;
            _statisticsService = statisticsService;
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

            var infoResponse = _autoTradingUserInfoService.GetUserInfo(account);
            var maxAmount = _autoTradingAccountsService.GetMaxTradingInstrumentsCount(account);
            infoResponse.maxInstrumentCount = maxAmount;

            return await ToResponse(infoResponse, CancellationToken.None);
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
                tradingDirection = (TradingDirection)_.TradingDirection,
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
        [HttpPost("config/update-markets")]
        public async Task<IActionResult> UserInfoUpdateMarketsAsync([FromBody] UserInfoAddMarketsRequest request)
        {
            AutoTraderStatisticService.AddRequest("[POST]config/update-markets/" + request.Account);

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
                tradingDirection = (TradingDirection)_.TradingDirection,
                minStrength = _.MinStrength,
                minStrength1H = _.MinStrength1H,
                minStrength4H = _.MinStrength4H,
                minStrength1D = _.MinStrength1D
            }).ToList();

            var result = _autoTradingUserInfoService.UpdateMarkets(request.Account, markets);

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
        [HttpPost("config/reset-bot-settings")]
        public async Task<IActionResult> ResetBotSettingsAsync([FromBody] ResetBotSettingsRequest request)
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

            var result = _autoTradingUserInfoService.ResetSettings(request.Account);
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
        [HttpPost("config/change-group-risk")]
        public async Task<IActionResult> ChangeGroupRiskAsync([FromBody] UserInfoChangeGroupRiskRequest request)
        {
            AutoTraderStatisticService.AddRequest("[POST]config/change-group-risk/" + request.Account);

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

            var result = _autoTradingUserInfoService.ChangeGroupRisk(request.Account, request.Group, request.Risk);

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
        public async Task<IActionResult> ChangeDefaultMarketRiskAsync([FromBody] UserInfoChangeDefaultRiskRequest request)
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

        [Authorize]
        [HttpPost("config/change-default-group-risk")]
        public async Task<IActionResult> ChangeDefaultGroupRiskAsync([FromBody] UserInfoChangeDefaultRiskRequest request)
        {
            AutoTraderStatisticService.AddRequest("[POST]config/change-default-group-risk/" + request.Account);

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

            var result = _autoTradingUserInfoService.ChangeDefaultGroupRisk(request.Account, request.Risk);

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

            try
            {
                var logs = new List<NALogs>();
                if (!string.IsNullOrEmpty(request.Logs))
                {
                    var logsMessages = request.Logs.Split("|", StringSplitOptions.RemoveEmptyEntries);
                    var logsItems = logsMessages.Select((_) => new NALogs
                    {
                        Account = request.Account,
                        Data = _,
                        Date = DateTime.UtcNow,
                        Type = 0
                    });

                    logs.AddRange(logsItems);
                }
                if (!string.IsNullOrEmpty(request.Errors))
                {
                    var errorsMessages = request.Errors.Split("|", StringSplitOptions.RemoveEmptyEntries);
                    var errorsItems = errorsMessages.Select((_) => new NALogs
                    {
                        Account = request.Account,
                        Data = _,
                        Date = DateTime.UtcNow,
                        Type = 1
                    });

                    logs.AddRange(errorsItems);
                }
                logs.Add(new NALogs
                {
                    Account = request.Account,
                    Data = string.IsNullOrEmpty(request.Naversion) ? string.Empty : request.Naversion,
                    Date = DateTime.UtcNow,
                    Type = 2
                });

                _statisticsService.AddLogsToCache(logs);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            try
            {
                if (!string.IsNullOrEmpty(request.Mode) && !string.IsNullOrEmpty(request.Balance) && !string.IsNullOrEmpty(request.PNL))
                {
                    _statisticsService.AddAccountToCache(new NAAccountBalances
                    {
                        Account = request.Account,
                        AccountType = int.Parse(request.Mode),
                        Balance = double.Parse(request.Balance),
                        Pnl = double.Parse(request.PNL),
                        Currency = request.Currency,
                        Date = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            if (!validateNAVersion(request.Naversion))
            {
                return StatusCode(403, "Old NA Version. Pleas update you NA client.");
            }

            var result = string.Empty;

            if (request.Version == "3.0")
            {
                result = await GetAutoTradeInstrumentsAsyncV3_0(request.Account);
            }
            else if (request.Version == "2.1")
            {
                result = await GetAutoTradeInstrumentsAsyncV2_1(request.Account);
            }
            else if (request.Version == "2.0")
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
                    IsDisabled = item.IsDisabled,
                    StrategyType = item.StrategyType
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

        [Authorize]
        [HttpGet("runtime/logs")]
        public async Task<IActionResult> GetRuntimeLogsAsync([FromQuery] string account)
        {
            AutoTraderStatisticService.AddRequest("[GET]runtime/logs/" + account);

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

            var result = await _statisticsService.GetLogs(account);

            return await ToResponse(result, CancellationToken.None);
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
                if (!item.IsDisabled && item.StrategyType != 2)
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
        private async Task<string> GetAutoTradeInstrumentsAsyncV2_1(string account)
        {
            return await GetAutoTradeInstrumentsAsyncV2_1And3_0(account, 2);
        }
        
        [NonAction]
        private async Task<string> GetAutoTradeInstrumentsAsyncV3_0(string account)
        {
            return await GetAutoTradeInstrumentsAsyncV2_1And3_0(account, -1);
        }
        
        [NonAction]
        private async Task<string> GetAutoTradeInstrumentsAsyncV2_1And3_0(string account, int strategyFilter)
        {
            var dedications = await _autoTradingPreloaderService.GetAutoTradingInstrumentsDedication(account);
            var stringResult = new StringBuilder();
            var instruments = dedications.Instruments;

            foreach (var item in instruments)
            {
                var instrumentRisk = GetInstrumentRisk(dedications, item.Symbol);
                if (!item.IsDisabled && item.StrategyType != strategyFilter)
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
            stringResult.AppendLine($"botShutDown={dedications.BotShutDown}");
            stringResult.AppendLine($"disabled={String.Join(",", dedications.DisabledInstruments)}");

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
            var userSettings = _autoTradingUserInfoService.GetUserInfo(request.Account);
            return Ok(BuildBotStringResponse(result, mappedSymbol, userSettings));
        }
        private string BuildBotStringResponse(AutoTradingSymbolInfoResponse result, string symbol, UserInfoData userSettings)
        {
            var normalizedMarket = InstrumentsHelper.NormalizeInstrument(symbol);
            var existingMarket = userSettings.markets.FirstOrDefault((_) => string.Equals(InstrumentsHelper.NormalizeInstrument(_.symbol), normalizedMarket, StringComparison.InvariantCultureIgnoreCase));
            var useOpposite = false;

            if (existingMarket != null && existingMarket.tradingDirection != TradingDirection.Auto)
            {
                if (result.TradingState > 0 && existingMarket.tradingDirection == TradingDirection.Short)
                {
                    useOpposite = true;
                }
                if (result.TradingState < 0 && existingMarket.tradingDirection == TradingDirection.Long)
                {
                    useOpposite = true;
                }
            }

            var trendDirection = useOpposite ? result.OppositeTrendDirection : result.TrendDirection;
            var stringResult = new StringBuilder();
            stringResult.AppendLine($"strengthTotal={Math.Round(result.TotalStrength * 100, 2)}");
            stringResult.AppendLine($"generalStopLoss={Math.Round(useOpposite ? result.OppositeSL : result.SL, 5)}");
            stringResult.AppendLine($"trendDirection={trendDirection}");
            stringResult.AppendLine($"strategyType={result.StrategyType}");

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

            stringResult.AppendLine($"1m={Math.Round(useOpposite ? result.OppositeEntry1M : result.Entry1M, 5)}");
            stringResult.AppendLine($"5m={Math.Round(useOpposite ? result.OppositeEntry5M : result.Entry5M, 5)}");
            stringResult.AppendLine($"15m={Math.Round(useOpposite ? result.OppositeEntry15M : result.Entry15M, 5)}");
            stringResult.AppendLine($"1h={Math.Round(useOpposite ? result.OppositeEntry1H : result.Entry1H, 5)}");
            stringResult.AppendLine($"4h={Math.Round(useOpposite ? result.OppositeEntry4H : result.Entry4H, 5)}");
            stringResult.AppendLine($"1d={Math.Round(useOpposite ? result.OppositeEntry1D : result.Entry1D, 5)}");

            stringResult.AppendLine($"strength1m={Math.Round(result.Strength1M * 100, 2)}");
            stringResult.AppendLine($"strength5m={Math.Round(result.Strength5M * 100, 2)}");
            stringResult.AppendLine($"strength15m={Math.Round(result.Strength15M * 100, 2)}");
            stringResult.AppendLine($"strength1h={Math.Round(result.Strength1H * 100, 2)}");
            stringResult.AppendLine($"strength4h={Math.Round(result.Strength4H * 100, 2)}");
            stringResult.AppendLine($"strength1d={Math.Round(result.Strength1D * 100, 2)}");
            stringResult.AppendLine($"strength1month={Math.Round(result.Strength1Month * 100, 2)}");
            stringResult.AppendLine($"strength1y={Math.Round(result.Strength1Y * 100, 2)}");
            stringResult.AppendLine($"strength10y={Math.Round(result.Strength10Y * 100, 2)}");

            stringResult.AppendLine($"ph1m={result.Phase1M}");
            stringResult.AppendLine($"ph5m={result.Phase5M}");
            stringResult.AppendLine($"ph15m={result.Phase15M}");
            stringResult.AppendLine($"ph1h={result.Phase1H}");
            stringResult.AppendLine($"ph4h={result.Phase4H}");
            stringResult.AppendLine($"ph1d={result.Phase1D}");
            // stringResult.AppendLine($"ph1month={result.Phase1Month}");
            // stringResult.AppendLine($"ph1y={result.Phase1Y}");
            // stringResult.AppendLine($"ph10y={result.Phase10Y}");

            stringResult.AppendLine($"st1m={result.State1M}");
            stringResult.AppendLine($"st5m={result.State5M}");
            stringResult.AppendLine($"st15m={result.State15M}");
            stringResult.AppendLine($"st1h={result.State1H}");
            stringResult.AppendLine($"st4h={result.State4H}");
            stringResult.AppendLine($"st1d={result.State1D}");
            // stringResult.AppendLine($"st1month={result.State1Month}");
            // stringResult.AppendLine($"st1y={result.State1Y}");
            // stringResult.AppendLine($"st10y={result.State10Y}");

            stringResult.AppendLine($"sl1m={Math.Round(useOpposite ? result.OppositeSL1M : result.SL1M, 5)}");
            stringResult.AppendLine($"sl5m={Math.Round(useOpposite ? result.OppositeSL5M : result.SL5M, 5)}");
            stringResult.AppendLine($"sl15m={Math.Round(useOpposite ? result.OppositeSL15M : result.SL15M, 5)}");
            stringResult.AppendLine($"sl1h={Math.Round(useOpposite ? result.OppositeSL1H : result.SL1H, 5)}");
            stringResult.AppendLine($"sl4h={Math.Round(useOpposite ? result.OppositeSL4H : result.SL4H, 5)}");

            stringResult.AppendLine($"vol1m={result.Volatility1M}");
            stringResult.AppendLine($"vol15m={result.Volatility15M}");
            stringResult.AppendLine($"vol1h={result.Volatility1H}");
            stringResult.AppendLine($"vol1d={result.Volatility1D}");

            stringResult.AppendLine($"currentPhase={result.CurrentPhase}");
            stringResult.AppendLine($"nextPhase={result.NextPhase}");
            stringResult.AppendLine($"shortGroupPhase={result.ShortGroupPhase}");
            stringResult.AppendLine($"midGroupPhase={result.MidGroupPhase}");
            stringResult.AppendLine($"longGroupPhase={result.LongGroupPhase}");
            stringResult.AppendLine($"shortGroupStrength={Math.Round(result.ShortGroupStrength * 100, 2)}");
            stringResult.AppendLine($"midGroupStrength={Math.Round(result.MidGroupStrength * 100, 2)}");
            stringResult.AppendLine($"longGroupStrength={Math.Round(result.LongGroupStrength * 100, 2)}");

            stringResult.AppendLine($"skipTrade1m={(result.Skip1MinTrades ? 1 : 0)}");
            stringResult.AppendLine($"skipTrade5m={(result.Skip5MinTrades ? 1 : 0)}");
            stringResult.AppendLine($"skipTrade15m={(result.Skip15MinTrades ? 1 : 0)}");
            stringResult.AppendLine($"skipTrade1h={(result.Skip1HourTrades ? 1 : 0)}");
            stringResult.AppendLine($"skipTrade4h={(result.Skip4HourTrades ? 1 : 0)}");

            stringResult.AppendLine($"minStrength1m={Math.Round(result.MinStrength1M, 0)}");
            stringResult.AppendLine($"minStrength5m={Math.Round(result.MinStrength5M, 0)}");
            stringResult.AppendLine($"minStrength15m={Math.Round(result.MinStrength15M, 0)}");
            stringResult.AppendLine($"minStrength1h={Math.Round(result.MinStrength1H, 0)}");
            stringResult.AppendLine($"minStrength4h={Math.Round(result.MinStrength4H, 0)}");

            stringResult.AppendLine($"ddClosePositions={(result.DDClosePositions ? 1 : 0)}");
            stringResult.AppendLine($"ddCloseInitialInterval={result.DDCloseInitialInterval}");
            stringResult.AppendLine($"ddCloseIncreasePeriod={result.DDCloseIncreasePeriod}");
            stringResult.AppendLine($"ddCloseIncreaseThreshold={Math.Round(result.DDCloseIncreaseThreshold, 2)}");

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

                {"SOLUSD", "SOLUSDT"},
                {"LTCUSD", "LTCUSDT"},
                {"SOL/USD", "SOLUSDT"},
                {"LTC/USD", "LTCUSDT"},
                {"SOL_USD", "SOLUSDT"},
                {"LTC_USD", "LTCUSDT"},
                {"SOLUSDT", "SOLUSDT"},
                {"SOL_USDT", "SOLUSDT"},
                {"LTCUSDT", "LTCUSDT"},
                {"LTC_USDT", "LTCUSDT"},

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

        private bool validateNAVersion(string naversion)
        {
            // "AK-2.17.4a";
            // "AK-2.17.3";
            if (string.IsNullOrEmpty(naversion))
            {
                return false;
            }

            var versions = naversion.Split(".");
            if (versions.Length != 3)
            {
                return false;
            }

            var majorVersionString = Regex.Replace(versions[0], @"[^\d]", String.Empty);
            var minorVersionString = Regex.Replace(versions[1], @"[^\d]", String.Empty);
            var buildVersionString = Regex.Replace(versions[2], @"[^\d]", String.Empty);

            if (int.TryParse(majorVersionString, out var major))
            {
                if (major < 2)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            // if (int.TryParse(minorVersionString, out var minor))
            // {
            //     if (minor < 17)
            //     {
            //         return false;
            //     }
            // }
            // else
            // {
            //     return false;
            // }

            // if (int.TryParse(buildVersionString, out var build))
            // {
            //     if (build < 3)
            //     {
            //         return false;
            //     }
            // }
            // else
            // {
            //     return false;
            // }

            return true;
        }
    }
}
