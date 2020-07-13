using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Algoserver.Client.WebSocket
{
    public class WebSocketItem : IDisposable
    {
        private ClientWebSocket _socket;
        private readonly Uri _socketUri;

        private readonly int _webSocketPingTimeout = 5 * 1000;
        private DateTime _lastPingTime = DateTime.MinValue;
        //private bool _isHealthy;
        private bool _isReconnecting;

        public event EventHandler<EventArgs> OnOpen;
        public event EventHandler<EventArgs<string>> OnMessage;
        public event EventHandler<EventArgs<string>> OnClose;
        public event EventHandler<EventArgs<Exception>> OnError;
        private bool _disposed;

        private Timer _timer;

        public WebSocketState State => _socket.State;

        public WebSocketItem(string uri)
        {
            _socket = new ClientWebSocket();
            _socketUri = new Uri(uri);
        }

        ~WebSocketItem()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Send(string message)
        {
            if (State != WebSocketState.Open)
            {
                if (!_isReconnecting) Reconnect();
            }
            else
            {
                var messageBuffer = Encoding.UTF8.GetBytes(message);

                _socket.SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Text, true,
                    CancellationToken.None);
            }
        }
        public void Connect()
        {
            ConnectAsync();
        }

        public async void Close(string message = "")
        {
            await _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty,
                    CancellationToken.None);

            OnClose?.Invoke(this, new EventArgs<string>(message));
        }

        private async void ConnectAsync()
        {
            try
            {
                Console.WriteLine(">>> Socket URL: " + _socketUri);
                await _socket.ConnectAsync(_socketUri, CancellationToken.None);
                //_isHealthy = true;
                _isReconnecting = false;
                OnOpen?.Invoke(this, new EventArgs<EventArgs>(EventArgs.Empty));
                StartListen();
                //_timer = new Timer(Ping, null, TimeSpan.FromMilliseconds(_webSocketPingTimeout), TimeSpan.FromMilliseconds(_webSocketPingTimeout));
            }
            catch (Exception exception)
            {
                OnError?.Invoke(this, new EventArgs<Exception>(exception));
                await Task.Delay(10000).ContinueWith(t => { Reconnect(); });
            }
        }

        private async void StartListen()
        {
            var buffer = new byte[1024];

            try
            {
                while (_socket != null && State == WebSocketState.Open)
                {
                    var stringResult = new StringBuilder();
                    WebSocketReceiveResult result;

                    do
                    {
                        result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                        if (result.MessageType == WebSocketMessageType.Close && State == WebSocketState.Open)
                        {
                            Close(result.CloseStatusDescription);
                        }
                        else
                        {
                            var str = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            stringResult.Append(str);
                        }
                    }
                    while (!result.EndOfMessage);

                    var res = stringResult.ToString();

                    if (string.IsNullOrEmpty(res)) continue;

                    //if (res.ToLower() == "pong")
                    //{
                    //    _isHealthy = true;
                    //}
                    //else
                    OnMessage?.Invoke(this, new EventArgs<string>(res));
                }
            }
            catch (Exception exception)
            {
                OnError?.Invoke(this, new EventArgs<Exception>(exception));
                if (!_isReconnecting) Reconnect();
            }
        }

        public void Reconnect()
        {
            _isReconnecting = true;
            //_isHealthy = false;

            try
            {
                _socket.Abort();
                _socket = new ClientWebSocket();
                ConnectAsync();
            }
            catch (Exception exception)
            {
                OnClose?.Invoke(this, new EventArgs<string>(exception.Message));
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _timer.Dispose();
                _socket?.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                _socket?.Dispose();
            }

            _disposed = true;
        }

        //private void Ping(object state)
        //{
        //    if ((DateTime.Now - _lastPingTime).TotalMilliseconds > _webSocketPingTimeout)
        //    {
        //        if (/*!_isHealthy && */!_isReconnecting)
        //        {
        //            Reconnect();
        //        }
        //        else
        //        {
        //            Send("ping");
        //            _lastPingTime = DateTime.Now;
        //        }
        //    }
        //}
    }
}