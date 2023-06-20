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
            // HttpClient tokenClient = new HttpClient();
            // var tokenResponse = await tokenClient.GetAsync("https://prod-stockserver.smarttrader.com:8091/GetAuthToken?key=ce99d5346dcbaade596123ac91d46b2c");
            // if (!tokenResponse.IsSuccessStatusCode)
            // {
            //     Console.WriteLine(tokenResponse.StatusCode);
            //     Console.WriteLine(">>> Error: Failed to load economical calendar token");
            //     return;
            // }
            // var token = await tokenResponse.Content.ReadAsStringAsync();
            // token = token.Trim('"');
            var token = "bearer pH7gaT7d2sRCd7EDGt4vJfnpxL0CqfIBmh-3J6SZo3hCv7qCecfCE_z85Lud9WKSzVWwuYUlTgLYR0YGoCkwyEiaPlZyPYrj2g13CDo66c_AxLOqKgg2-D8QubL63B49M5LiqDUDALnk3EhtXsfYszqFrLeqnTb-wKJuTgSTkhYv3YU2DYFzbUPxImhDv_FBd1ZgIN0UUmA2EaVFxiB3tMaIWjQTk9DOP-qruQaxgIPp62VOl7E64DtUVTyMnSasCI4l8EP6tHu_IAiBo-8FmWiAGhgmSNylphro9R9oeU1L9paJpyEgwnyxSutcHViCDC6zwEiIE6rhEdGlaGix2GL36qfCQETJxbY_-2PjAQXBk0Tmb4re0YVjstev9AmbeUxw05fE69-jyAne5GjBlGrNeqYyIBhtmcS1zP3KjhFk56vdIHRjJuoZOV8f0PetqCPxzUAuK6Ktu7DmQQ-1KD5m3akshkM6DmY8oz8XIMx_3y_SU-7lfsCEEMhFTZOSkpKTd7xiXWiDXOJDy8edJsVHe7f0sco07vQttGyGA0eLNT5Xuh83sQCNUNpCo6bEtxqL0A701MUz_lLCSIjTfUJVo2IE-DSjPyOiujNdP52jValF9pcyf45POcUnE66izO1Pwxnh6o-Oir3Wj9RQCTtly6Q9VMI1gxGQZET0nz2xJSjloAuEwiBWSLF-cCaeFKTE5ds5mGhPxTyCz2w9Y9293Ws4qwwDDO_cTOgHYDUjjyO958qCsDV9tWdvbL7YPNzNF3DJUL3EFPlKlJ7J5xc0Z6cyPfOYiaTGPnZ0zWAxSvuflwRGviCd7uu8y4Bi_UzgDecY-umrlrHaFdhCbVC9HFDJayLfvFoIYhzXbPRV_PmLMIEmEyw4n5twQK7UNMs5RXZ2Ie-I_9GLH6lxElg7odYa3Hk4sAjzBmo6SjRnQAVaQW498VaKAlV1o8n3uRMduqDgNnvNiuASRrJrHPeMnLU";

            // load data
            HttpClient dataClient = new HttpClient();
            var dataResponse = await dataClient.GetAsync("https://calendar.fxstreet.com/v4.1/eventdate?authorization=" + token);
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