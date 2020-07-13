using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Algoserver.API.Services.Instruments;
using Algoserver.Client.WebSocket;
using Algoserver.Client.WebSocket.Models.Actions;
using Algoserver.Client.WebSocket.Models.Events;

namespace Algoserver.API.Services.Realtime
{
    public class RealtimeService
    {
        private readonly InstrumentService _instrumentService;
        private readonly TwelveDataWebSocketService _twelveDataWebSocketService;
        private readonly SubscriptionManager _subscriptionManager;
        private readonly System.Timers.Timer _tvelwedtaResetSubscriptionsTimer = new System.Timers.Timer(2000);
        private ILogger<RealtimeService> _logger;

        public RealtimeService(ILogger<RealtimeService> logger, TwelveDataWebSocketService twelveDataWebSocketService, InstrumentService instrumentService, IApplicationLifetime lifetime)
        {
            _logger = logger;
            _instrumentService = instrumentService;
            _twelveDataWebSocketService = twelveDataWebSocketService;
            _subscriptionManager = new SubscriptionManager();
            
            _twelveDataWebSocketService.PriceReceived += OnTwelveDataWebSocketServicePriceReceived;
            _twelveDataWebSocketService.OnOpen += OnTwelveDataWebSocketOpened;

            var startRegistration = default(CancellationTokenRegistration);
            startRegistration = lifetime.ApplicationStarted.Register(async () =>
            {
                await StartAsync(lifetime.ApplicationStopping);
                startRegistration.Dispose();
            });
            
            _tvelwedtaResetSubscriptionsTimer.Elapsed += OnTvelwedataResetSubscriptions;
            _tvelwedtaResetSubscriptionsTimer.Start();
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _twelveDataWebSocketService.Open();
        }

        public void ToggleSubscription(bool isSubscribe, string symbol, string exchange, UserWebsocket client)
        {
            var instrument = _instrumentService.GetInstrumentBySymbol(symbol, exchange);
            if (instrument == null || instrument.Kind == InstrumentKind.Unknown)
            {
                _logger.LogInformation($"RealtimeManager can't handle client to {symbol}, instrument is missing OR it's kind is UNKNOWN");
                return;
            }

            if (isSubscribe)
            {
                _subscriptionManager.AddSubscribtion(instrument, client);
            }
            else
            {
                _subscriptionManager.RemoveSubscribtion(instrument, client);
            }
        }

        public void RemoveSubscription(string websocketById)
        {
            _subscriptionManager.RemoveSubscriptionBySocketId(websocketById);
        }

        private void OnTwelveDataWebSocketOpened(object sender, EventArgs e) {
            _subscriptionManager.ResetSubscriptions();
        }
        
        private void OnTwelveDataWebSocketServicePriceReceived(object sender, EventArgs<PriceEvent> e)
        {
            _subscriptionManager.ProcessPriceEventMessage(e.Value);
        }

        private void OnTvelwedataResetSubscriptions(object sender, ElapsedEventArgs e)
        {
            lock (_subscriptionManager.TwelvedataUnsubscribeSymbolsBuffer)
            {
                if (_subscriptionManager.TwelvedataUnsubscribeSymbolsBuffer.Any())
                {
                    var unsubscribeSymbols = string.Join(",", _subscriptionManager.TwelvedataUnsubscribeSymbolsBuffer.ToArray());
                    var unsubscribeAction = new UnsubscribeEvent(unsubscribeSymbols);
                    _twelveDataWebSocketService.SendAction(unsubscribeAction);
                    _subscriptionManager.TwelvedataUnsubscribeSymbolsBuffer.Clear();
                }
            }

            lock (_subscriptionManager.TwelvedataSubscribeSymbolsBuffer)
            {
                if (_subscriptionManager.TwelvedataSubscribeSymbolsBuffer.Any())
                {
                    var subscribeSymbols = string.Join(",", _subscriptionManager.TwelvedataSubscribeSymbolsBuffer.ToArray());
                    var subscribeAction = new SubscribeEvent(subscribeSymbols);
                    _twelveDataWebSocketService.SendAction(subscribeAction);
                    _subscriptionManager.TwelvedataSubscribeSymbolsBuffer.Clear();
                }
            }
        }
    }
}
