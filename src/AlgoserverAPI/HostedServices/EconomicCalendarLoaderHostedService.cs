using System;
using System.Threading;
using System.Threading.Tasks;
using Algoserver.API.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Algoserver.API.HostedServices
{
    public class EconomicCalendarLoaderHostedService : BackgroundService
    {
        private int _prevHour = -1;
        private readonly ILogger<EconomicCalendarLoaderHostedService> _logger;
        private readonly EconomicCalendarService _economicCalendarService;
        private Timer _timer;

        public EconomicCalendarLoaderHostedService(ILogger<EconomicCalendarLoaderHostedService> logger, EconomicCalendarService economicCalendarService)
        {
            _logger = logger;
            _economicCalendarService = economicCalendarService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var currentHour = DateTime.UtcNow.Hour;
                var currentMinute = DateTime.UtcNow.Minute;
                try
                {
                    if (currentHour != _prevHour || currentMinute % 10 == 0)
                    {
                        await _economicCalendarService.LoadEconomicEvents();
                    } 
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                _prevHour = currentHour;

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken).ConfigureAwait(false);
            }
        }
    }
}