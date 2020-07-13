using Microsoft.Extensions.Logging;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Timers;
using Twelvedata.Client.Serialization;
using Twelvedata.Client.WebSocket.Models.Actions;
using Twelvedata.Client.WebSocket.Models.Events;

namespace Twelvedata.Client.WebSocket
{
    public class TwelveDataWebSocketService
    {
        private readonly WebSocketItem _webSocketItem;
        public event EventHandler<EventArgs> OnOpen;
        private bool _stopped;
        private int maxTwelvedataSocketFailedCheckCount = 5;
        private int currentTwelvedataSocketFailedCheckCount = 0;
        private readonly Timer _checkSocketTimer = new Timer(1000 * 20); // Once a twenty seconds

        private readonly ILogger<TwelveDataWebSocketService> _logger;
        public TwelveDataWebSocketService(string apikey, string webSocketUri, ILogger<TwelveDataWebSocketService> logger)
        {
            _logger = logger;

            if (string.IsNullOrEmpty(apikey) || string.IsNullOrEmpty(webSocketUri))
                throw new Exception("Twelvedata's websocket bad URI or apikey");

            _webSocketItem = new WebSocketItem($"{webSocketUri}?apikey={apikey}");

            _webSocketItem.OnMessage += OnWebSocketMessageReceived;
            _webSocketItem.OnClose += OnWebSocketClosed;
            _webSocketItem.OnOpen += OnWebSocketOpened;
            _webSocketItem.OnError += OnWebSocketError;

            _checkSocketTimer.Elapsed += OnCheckSocketState;
            _checkSocketTimer.Start();
        }

        public event EventHandler<EventArgs<PriceEvent>> PriceReceived;

        private void OnCheckSocketState(object sender, ElapsedEventArgs e)
        {
            if (_webSocketItem.State != WebSocketState.Open)
            {
                if (maxTwelvedataSocketFailedCheckCount <= currentTwelvedataSocketFailedCheckCount)
                {
                    Console.WriteLine(">>> Twelvedata reconnecting...");
                    _webSocketItem.Reconnect();
                    currentTwelvedataSocketFailedCheckCount = 0;
                }
                else
                {
                    Console.WriteLine($">>> Twelvedata socket '{_webSocketItem.State}' it well be reconnected automatically after: {maxTwelvedataSocketFailedCheckCount - currentTwelvedataSocketFailedCheckCount}");
                    currentTwelvedataSocketFailedCheckCount++;
                }
            }
            else
            {
                currentTwelvedataSocketFailedCheckCount = 0;
            }
        }

        public void SendAction(TwelvedataAction action)
        {
            if (_webSocketItem.State == WebSocketState.Open)
            {
                var message = MessageSerializer.SerializeObject(action);
                _webSocketItem.Send(message);
                Console.WriteLine($">>> Twelvedata message sent: {message}");
            }
            else
            {
                _logger.LogError($"Send message to Twelvedata's Websocket failed, socket state is {_webSocketItem.State}");
            }
        }

        public void Open()
        {
            if (_webSocketItem.State != WebSocketState.Open)
            {
                _webSocketItem.Connect();
            }

            _stopped = false;
        }

        public void Stop()
        {
            _stopped = true;
            _webSocketItem.Close();
        }

        public void Dispose() => _webSocketItem.Dispose();

        private void OnWebSocketMessageReceived(object sender, EventArgs<string> e)
        {
            var twelvedataEvent = MessageSerializer.DeserializeObject<TwelvedataEvent>(e.Value);
            if (twelvedataEvent != null)
            {
                switch (twelvedataEvent.Event)
                {
                    case "price":
                        {
                            var priceEvent = MessageSerializer.DeserializeObject<PriceEvent>(e.Value);
                            PriceReceived?.Invoke(this, new EventArgs<PriceEvent>(priceEvent));
                        }
                        break;
                    case "subscribe-status":
                        {
                            var subscribeStatusEvent = MessageSerializer.DeserializeObject<SubscribeStatusEvent>(e.Value);
                            if (subscribeStatusEvent.Status == "ok")
                            {
                                _logger.LogInformation($"Twelvedata's Websocket subscribe successed");
                            }
                            if (subscribeStatusEvent.Status == "error")
                            {
                                _logger.LogInformation($"Twelvedata's websocket subscribe failed: {string.Join(Environment.NewLine, subscribeStatusEvent.Messages)}");
                            }
                            Console.WriteLine(">>> Subscription" + e.Value);
                        }
                    break; case "unsubscribe-status":
                        {
                            var subscribeStatusEvent = MessageSerializer.DeserializeObject<SubscribeStatusEvent>(e.Value);
                            if (subscribeStatusEvent.Status == "ok")
                            {
                                _logger.LogInformation($"Twelvedata's Websocket unsubscribe successed");
                            }
                            if (subscribeStatusEvent.Status == "error")
                            {
                                _logger.LogInformation($"Twelvedata's websocket unsubscribe failed: {string.Join(Environment.NewLine, subscribeStatusEvent.Messages)}");
                            }
                            Console.WriteLine(">>> Unsubscription" + e.Value);
                        }
                        break;
                }
            }
        }

        private void OnWebSocketClosed(object sender, EventArgs e)
        {
            _logger.LogInformation($"Twelvedata's Websocket closed, reconnecting...");
            if (!_stopped)
                Task.Delay(2000).ContinueWith(t => { _webSocketItem.Reconnect(); });
        }

        private void OnWebSocketOpened(object sender, EventArgs e)
        {
            _logger.LogInformation($"Twelvedata's Websocket opened");
            OnOpen?.Invoke(this, new EventArgs<EventArgs>(EventArgs.Empty));
        }

        private void OnWebSocketError(object sender, EventArgs<Exception> e)
        {
            _logger.LogError($"Twelvedata's Websocket error: {e.Value}");
        }
    }
}
