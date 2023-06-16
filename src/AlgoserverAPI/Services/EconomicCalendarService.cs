using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Algoserver.API.Models.EconomicCalendar;
using Algoserver.API.Services.CacheServices;
using Newtonsoft.Json;

namespace Algoserver.API.Services
{
    public class EconomicCalendarService
    {
        private ICacheService _cache;
        private string _cachePrefix = "economical_";
        private string _calendarCacheKey = "calendar";

        public EconomicCalendarService(ICacheService cache)
        {
            _cache = cache;
        }

        public async Task<List<EconomicEvent>> GetEconomicEvents()
        {
            try
            {
                var cachedResponse = await _cache.TryGetValueAsync<List<EconomicEvent>>(_cachePrefix, _calendarCacheKey);
                if (cachedResponse != null)
                {
                   return cachedResponse;
                }
                return new List<EconomicEvent>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine(">>> Error: Failed to load economical calendar data from cache");
                throw new ApplicationException(e.Message);
            }
        }

        public async Task LoadEconomicEvents()
        {
            // load token
            HttpClient tokenClient = new HttpClient();
            var tokenResponse = await tokenClient.GetAsync("https://prod-stockserver.smarttrader.com:8091/GetAuthToken?key=ce99d5346dcbaade596123ac91d46b2c");
            if (!tokenResponse.IsSuccessStatusCode)
            {
                Console.WriteLine(tokenResponse.StatusCode);
                Console.WriteLine(">>> Error: Failed to load economical calendar token");
                return;
            }
            var token = await tokenResponse.Content.ReadAsStringAsync();
            token = token.Trim('"');

            // load data
            HttpClient dataClient = new HttpClient();
            var dataResponse = await tokenClient.GetAsync("https://calendar.fxstreet.com/v4.1/eventdate?authorization=" + token);
            if (!dataResponse.IsSuccessStatusCode)
            {
                Console.WriteLine(dataResponse.StatusCode);
                Console.WriteLine(">>> Error: Failed to load economical calendar data");
                return;
            }
            var data = await dataResponse.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<List<EconomicEvent>>(data);

            if (response != null && response.Any())
            {
                try
                {
                    var cachedResponse = await _cache.SetAsync(_cachePrefix, _calendarCacheKey, response, TimeSpan.FromDays(7));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Console.WriteLine(">>> Error: Failed to save economical calendar data");
                }
            }
        }
    }
}