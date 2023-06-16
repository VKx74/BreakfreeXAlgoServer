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
        private int _prevDay = -1;
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
                try
                {
                    var currentDay = DateTime.UtcNow.Day;

                    if (currentDay != _prevDay)
                    {
                        await _economicCalendarService.LoadEconomicEvents();
                    } 

                    _prevDay = currentDay;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken).ConfigureAwait(false);
            }
        }
    }
}