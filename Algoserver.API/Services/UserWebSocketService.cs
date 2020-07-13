using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Algoserver.API.Models.WebSocket;
using Algoserver.API.Services.Realtime;
using Algoserver.Client.Serialization;

namespace Algoserver.API.Services
{
    public class UserWebSocketService
    {
        private readonly ConcurrentDictionary<string, WebSocket> _webSocketsById;
        private readonly ILogger<UserWebSocketService> _logger;
        private readonly RealtimeService _realtimeService;

        public UserWebSocketService(ILogger<UserWebSocketService> logger, RealtimeService realtimeService)
        {
            _webSocketsById = new ConcurrentDictionary<string, WebSocket>();
            _logger = logger;
            _realtimeService = realtimeService;
        }

        public async Task OnConnectedAsync(WebSocket socket)
        {
            var socketId = Guid.NewGuid().ToString();

            if (_webSocketsById.Values.Any(p => p == socket))
                return; // already exists

            _webSocketsById.TryAdd(socketId, socket);
            var response = JsonConvert.SerializeObject(new { Key = socketId, IsSuccess = true });

            await SendJsonAsync(response, socket);
        }

        public void OnDisconnectedAsync(WebSocket socket)
        {
            var websocketById = _webSocketsById.FirstOrDefault(p => p.Value == socket);
            if (websocketById.Equals(default(KeyValuePair<string, WebSocket>)))
                return;

            _webSocketsById.TryRemove(websocketById.Key, out _);

            _realtimeService.RemoveSubscription(websocketById.Key);
        }

        public void ProcessMessage(WebSocket socket, string message)
        {
            var _webSocketById = _webSocketsById.FirstOrDefault(p => p.Value == socket);
            if (_webSocketById.Equals(default(KeyValuePair<string, WebSocket>)))
                return; // socket key is not valid

            if (!MessageSerializer.TryDeserializeObject<BaseMessage>(message, out BaseMessage baseMessage))
                return;

            if (baseMessage.MsgType == nameof(SubscribeMessage))
            {
                if (MessageSerializer.TryDeserializeObject<SubscribeMessage>(message, out var subscribeMessage))
                {
                    var userId = string.Empty;
                    //if (!string.IsNullOrWhiteSpace(subscribeMessage.AccessToken))
                    //{
                    //    var token = new JwtSecurityTokenHandler().ReadJwtToken(subscribeMessage.AccessToken);
                    //    userId = token.Claims.FirstOrDefault(p => p.Type.Equals("sub"))?.Value;
                    //}

                    var subscription = new UserWebsocket
                    {
                        SocketId = _webSocketById.Key,

                        UserId = userId,
                        Callback = async (response) =>
                        {
                            await SendMessageAsync(response, _webSocketById.Value);
                        }
                    };
                    
                    _realtimeService.ToggleSubscription(subscribeMessage.IsSubscribe, subscribeMessage.Product, subscribeMessage.Market ?? "",  subscription);
                }
                else
                {
                    _logger.LogError($"SubsribeMessage wasn't proceeded");
                }
            }
        }

        private async Task SendMessageAsync(BaseMessage message, WebSocket socket)
        {
            var json = MessageSerializer.SerializeObject(message);
            await SendJsonAsync(json, socket);
        }

        private async Task SendJsonAsync(string message, WebSocket socket)
        {
            try
            {
                if (socket == null) return;
                var bytes = Encoding.ASCII.GetBytes(message);
                await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (ArgumentException exception)
            {
                _logger.LogError($"Error during encoding message. {exception.Message}");
            }
            catch (Exception exception)
            {
                _logger.LogError($"Error during sending message. {exception.Message}");
            }
        }
    }
}
