using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Twelvedata.Client.REST;

namespace Twelvedata.API.Services.Instruments
{
    public class InstrumentService
    {
        private ILogger<InstrumentService> _logger;
        private List<Instrument> _instruments = new List<Instrument>();
        private RestClient _restClient;
        private Timer _timer;
        private IEnumerable<string> _cryptoExchanges = new List<string>();

        private DateTime _lastRefreshTime = DateTime.MinValue;
        private ReaderWriterLockSlim _syncRoot = new ReaderWriterLockSlim();

        public InstrumentService(RestClient restClient, ILogger<InstrumentService> logger, IApplicationLifetime lifetime, IConfiguration configuration)
        {
            var exchanges = configuration["CryptoExchangesUsed"];
            if (!string.IsNullOrEmpty(exchanges)) {
                _cryptoExchanges = exchanges.Split(',');
            }

            _logger = logger;
            _restClient = restClient;

            var startRegistration = default(CancellationTokenRegistration);
            startRegistration = lifetime.ApplicationStarted.Register(async () =>
            {
                await StartAsync(lifetime.ApplicationStopping);
                startRegistration.Dispose();
            });
        }

        public Instrument GetInstrumentBySymbol(string symbol, string exchange = "")
        {
            _syncRoot.EnterReadLock();
            try
            {
                Instrument result = null;

                var instrument = _instruments.FirstOrDefault(i => i.Symbol.ToLowerInvariant() == symbol.ToLowerInvariant() 
                && (string.IsNullOrEmpty(exchange) 
                    || i.Exchange.ToLowerInvariant() == exchange.ToLowerInvariant()
                    || (i.AvailableExchanges != null && i.AvailableExchanges.Contains(exchange))
                    || i.Kind == InstrumentKind.Forex));

                if (instrument != null)
                {
                    result = new Instrument()
                    {
                        Type = instrument.Type,
                        Country = instrument.Country,
                        AvailableExchanges = instrument.AvailableExchanges,
                        CurrencyBase = instrument.CurrencyBase,
                        CurrencyQuote = instrument.CurrencyQuote,
                        Datafeed = instrument.Datafeed,
                        EscapedSymbol = instrument.EscapedSymbol,
                        Exchange = exchange,
                        Kind = instrument.Kind,
                        Name = instrument.Name,
                        Symbol = instrument.Symbol
                    };
                }

                return result;
            }
            finally
            {
                _syncRoot.ExitReadLock();
            }
        }
        public bool IsInitialized()
        {
            _syncRoot.EnterReadLock();
            try
            {
                return _instruments.Any();
            }
            finally
            {
                _syncRoot.ExitReadLock();
            }
        }

        public List<Instrument> GetInstruments(Func<Instrument, bool> predicate = null)
        {
            _syncRoot.EnterReadLock();
            try
            {
                if (predicate != null)
                    return _instruments.Where(predicate).ToList();

                return _instruments.ToList();
            }
            finally
            {
                _syncRoot.ExitReadLock();
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(TimerCallback, null, 0, 1800 * 1000);
            return Task.CompletedTask;
        }

        private string EscapeSymbol(string symbol) {
            return symbol.Replace("/", "").Replace("\\", "").Replace("_", "").Replace("^", "");
        }

        private async Task RefreshInstrumentsAsync()
        {
            _logger.LogInformation("InstrumentsManager is loading data ...");

            var allCryptoInstruments = (await _restClient.GetCryptoInstrumentsAsync()).Data.Select(c => new Instrument
            {
                Symbol = c.Symbol,
                EscapedSymbol = EscapeSymbol(c.Symbol),
                AvailableExchanges = c.AvailableExchanges,
                Kind = InstrumentKind.Crypto,
                CurrencyBase = c.CurrencyBase,
                CurrencyQuote = c.CurrencyQuote,
                Datafeed = Constants.Datafeed
            });

            var cryptoInstruments = new List<Instrument>();

            foreach (var instrument in allCryptoInstruments) {
                var intersection = instrument.AvailableExchanges.Intersect(_cryptoExchanges);
                if (intersection != null && intersection.Any()) {
                    instrument.AvailableExchanges = intersection.ToArray();
                    cryptoInstruments.Add(instrument);
                }
            }
            
            var forexInstruments = (await _restClient.GetForexInstrumentsAsync()).Data.Select(f => new Instrument 
            { 
                Symbol = f.Symbol,
                EscapedSymbol = EscapeSymbol(f.Symbol),
                Kind = InstrumentKind.Forex,
                CurrencyBase = f.CurrencyBase,
                CurrencyQuote = f.CurrencyQuote,
                Datafeed = Constants.Datafeed
            });

            var stocks = (await _restClient.GetStockInstrumentsAsync()).Data.Where(i => i.Exchange != "OTC").Select(s => new Instrument
            { 
                Symbol = s.Symbol,
                EscapedSymbol = EscapeSymbol(s.Symbol),
                Kind = InstrumentKind.Stock,
                Country = s.Country,
                Name = s.Name,
                Type = s.Type,
                Exchange = s.Exchange,
                Datafeed = Constants.Datafeed
            });

            var indices = (await _restClient.GetIndicesInstrumentsAsync()).Data.Select(i => new Instrument
            {
                Symbol = i.Symbol,
                CurrencyBase = i.Currency,
                Name = i.Name,
                Exchange = i.Exchange,
                Kind = InstrumentKind.Indices
            });

            var all = cryptoInstruments.Concat(forexInstruments).Concat(stocks).Concat(indices);
 
            _syncRoot.EnterWriteLock();
            try
            {
                _instruments = all.ToList();
            }
            finally
            {
                _syncRoot.ExitWriteLock();
            }

            _logger.LogInformation($"InstrumentsManager loaded {_instruments.Count()}");
        }

        private async void TimerCallback(object state)
        {
            if (DateTime.UtcNow.Subtract(_lastRefreshTime).TotalHours > 4)
            {
                await RefreshInstrumentsAsync();
            }
        }
    }
}
