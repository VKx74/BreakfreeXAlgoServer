using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Algoserver.API.Services.CacheServices;
using Algoserver.Auth.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Algoserver.API.Services
{
    [Serializable]
    class UserAutoTradingAccountCacheItem
    {
        public string AccountId { get; set; }
        public List<string> Subscriptions { get; set; }
    }

    class UserAutoTradingAccountResponse
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string UserId { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; }
        public List<string> Subscriptions { get; set; }
        public DateTime Time { get; set; }
    }

    public class AutoTradingAccountsLoadingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _serverUrl;
        private ICacheService _cache;
        private readonly AuthService _auth;
        private string _cachePrefix = "user_auto_trading_";
        private string _cacheKey = "accounts";
        public AutoTradingAccountsLoadingService(ICacheService cache, IConfiguration configuration, AuthService auth)
        {
            _cache = cache;
            _auth = auth;
            _serverUrl = configuration["Authority"];
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task Update()
        {
            try
            {
                var uri = $"{_serverUrl}/AutoTradingAccount/get_valid_trading_accounts";
                var token = await _auth.GetToken();
                var request = new HttpRequestMessage(HttpMethod.Get, uri)
                {
                    Headers = {
                { HttpRequestHeader.Authorization.ToString(), token }
            }
                };
                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error occured while sending http request to {request.RequestUri}, response string: {content}");
                    response.EnsureSuccessStatusCode();
                }

                var accounts = JsonConvert.DeserializeObject<List<UserAutoTradingAccountResponse>>(content);
                var accountsList = accounts.Select((_) => new UserAutoTradingAccountCacheItem {
                    AccountId = _.AccountId,
                    Subscriptions = _.Subscriptions
                }).ToList();
                Console.WriteLine(">>> Loaded auto trading account from identity: " + accountsList.Count);
                _cache.Set(_cachePrefix, _cacheKey, accountsList, TimeSpan.FromDays(1));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine(">>> Error: Failed to load user_auto_trading_accounts from identity");
            }
        }
    }
}