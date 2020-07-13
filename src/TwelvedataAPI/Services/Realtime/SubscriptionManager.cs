using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Twelvedata.API.Models.WebSocket;
using Twelvedata.API.Services.Instruments;
using Twelvedata.API.Services.Realtime;
using Twelvedata.Client.WebSocket.Models.Events;

namespace Twelvedata.API.Services
{
    public class SubscriptionManager
    {
        public string GetKey(PriceEvent priceEvent) => $"{priceEvent.Symbol}:{priceEvent.Exchange}";
        public string GetNoExchangeSpecifiedSubscriptionKey(PriceEvent priceEvent) => $"{priceEvent.Symbol}";
        public string GetForexSpecifiedSubscriptionKey(PriceEvent priceEvent) => $"{priceEvent.Symbol}:Forex";
        
        private readonly Timer _statisticTimer = new Timer(1000 * 60);
        private ulong _ticksReceived = 0;
        private ulong _ticksSent = 0;

        private readonly ConcurrentDictionary<string, Subscribers> _subscribers = new ConcurrentDictionary<string, Subscribers>(StringComparer.OrdinalIgnoreCase);

        public readonly List<string> TwelvedataSubscribeSymbolsBuffer = new List<string>();
        public readonly List<string> TwelvedataUnsubscribeSymbolsBuffer = new List<string>();

        public SubscriptionManager()
        {
            _statisticTimer.Elapsed += OnShowStatistic;
            _statisticTimer.Start();
        }

        public long SubscriptionsCount()
        {
            lock (_subscribers)
            {
                return _subscribers.Sum(p => p.Value?.Count ?? 0);
            }
        }

        public List<string> GetSubscribersKeys()
        {
            lock (_subscribers)
            {
                return _subscribers.Keys.ToList();
            }
        }

        public Subscribers GetSubscribers(string key)
        {
            lock (_subscribers)
            {
                if (_subscribers.TryGetValue(key, out Subscribers subscribers))
                {
                    return subscribers;
                }
            }

            return new Subscribers();
        }
        
        public void AddSubscribtion(Instrument instrument, UserWebsocket client)
        {
            lock (_subscribers)
            {
                SubscribeTwelveData(instrument.Key);

                var subscription = new Subscription(client, instrument);
                var subscribers = _subscribers.GetOrAdd(instrument.Key, new Subscribers(instrument));
                subscribers.AddSubscription(subscription);
            }
        }
        
        public void RemoveSubscribtion(Instrument instrument, UserWebsocket client)
        {
            lock (_subscribers)
            {
                var subscription = new Subscription(client, instrument);
                var subscribers = _subscribers.GetOrAdd(instrument.Key, new Subscribers(instrument));
                subscribers.RemoveSubscription(subscription);
                
                UnsubscribeTwelveData(instrument.Key);
            }
        }

        private void SubscribeTwelveData(string key)
        {
            lock (TwelvedataSubscribeSymbolsBuffer)
            {
                // TODO: Need to improve subscribe logic
                // if(!_subscribers.ContainsKey(key))
                if(!TwelvedataSubscribeSymbolsBuffer.Contains(key))
                    TwelvedataSubscribeSymbolsBuffer.Add(key);
            }
        }

        private void UnsubscribeTwelveData(string key)
        {
            lock (TwelvedataUnsubscribeSymbolsBuffer)
            {
                if (!_subscribers.ContainsKey(key) && !TwelvedataSubscribeSymbolsBuffer.Contains(key))
                    TwelvedataUnsubscribeSymbolsBuffer.Add(key);
            }
        }

        public void RemoveSubscriptionBySocketId(string socketId)
        {
            List<Subscribers> values;
            lock (_subscribers)
            {
                values = _subscribers.Values.ToList();
            }

            foreach (var subscriptions in values)
            {
                lock (_subscribers)
                {
                    subscriptions.RemoveSubsriptionBySocketId(socketId);

                    if (!subscriptions.Any())
                    {
                        _subscribers.TryRemove(subscriptions.Instrument.Key, out _);
                        UnsubscribeTwelveData(subscriptions.Instrument.Key);
                    }
                }

            }
        }

        public void ProcessPriceEventMessage(PriceEvent priceEvent)
        {
            if (priceEvent == null) return;

            _ticksReceived++;

            var key = GetKey(priceEvent);
            var targetExchangeSubscribers = GetSubscribers(key);
            var noExchangeSpecifiedSubscribers = GetSubscribers(GetNoExchangeSpecifiedSubscriptionKey(priceEvent));
            var subscribers = targetExchangeSubscribers.ToList().Union(noExchangeSpecifiedSubscribers.ToList());
            var market = priceEvent.Exchange;

            // for forex
            if (!string.IsNullOrEmpty(priceEvent.Exchange)
                && string.Equals(priceEvent.Exchange?.ToUpperInvariant(), "PHYSICAL CURRENCY", StringComparison.InvariantCultureIgnoreCase) ||
                !string.IsNullOrEmpty(priceEvent.Type) 
                && string.Equals(priceEvent.Type?.ToUpperInvariant(), "PHYSICAL CURRENCY", StringComparison.InvariantCultureIgnoreCase))
            {
                var forexSubscribers = GetSubscribers(GetForexSpecifiedSubscriptionKey(priceEvent));
                subscribers = subscribers.Union(forexSubscribers.ToList());
                market = "Forex";
            }

            var tickerMessage = new TickerMessage
            {
                Price = priceEvent.Price,
                Product = priceEvent.Symbol,
                Market = market
                //Side = 

            };

            
            if (subscribers.Any())
            {
                Parallel.ForEach(subscribers, async subscription =>
                {
                    await subscription.Client.SendMessage(tickerMessage);
                    _ticksSent++;
                });
            }
            else
            {
                var subscriptionKey = $"{priceEvent.Symbol}:{market}";

                UnsubscribeTwelveData(subscriptionKey);
            }
        }

        public void ResetSubscriptions()
        {
            List<string> keys = GetSubscribersKeys();

            if (!keys.Any())
            {
                return;
            }

            Parallel.ForEach(keys, async key =>
            {
                SubscribeTwelveData(key);
            });
        }

        private void OnShowStatistic(object sender, ElapsedEventArgs e)
        {
            try
            {
                Console.WriteLine($">>> Ticks received (last minute): {_ticksReceived}");
                Console.WriteLine($">>> Ticks sent (last minute): {_ticksSent}");
                Console.WriteLine($">>> Subscribers length: {SubscriptionsCount()}");

                // reset counters
                _ticksReceived = 0;
                _ticksSent = 0;
            }
#pragma warning disable S2486 // Generic exceptions should not be ignored
            catch (Exception)
            {
            }
#pragma warning restore S2486 // Generic exceptions should not be ignored

        }
    }
}