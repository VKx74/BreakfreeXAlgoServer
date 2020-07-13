using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Twelvedata.API.Services;

namespace Twelvedata.API.Middlewares
{
    public class WebSocketMiddleware
    {
        private readonly int _bufferSize;

        private readonly RequestDelegate _next;
        private readonly UserWebSocketService _wsService;
        private readonly ILogger<WebSocketMiddleware> _logger;

        public WebSocketMiddleware(RequestDelegate next, UserWebSocketService notificationSender, IConfiguration configuration, ILogger<WebSocketMiddleware> logger)
        {
            _next = next;
            _wsService = notificationSender;
            _bufferSize = configuration.GetValue("WebSocketBufferSize", 4 * 1024);
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            var socket = await AcceptWebSocketAsync(context);
            if (socket == null)
            {
                await _next.Invoke(context);
                return;
            }
            var cancellationToken = context.RequestAborted;

            await _wsService.OnConnectedAsync(socket);

            await Receive(socket, cancellationToken);

            if (!context.Response.HasStarted)
                await _next.Invoke(context);
            else
                _wsService.OnDisconnectedAsync(socket);
        }

        private async Task Receive(WebSocket socket, CancellationToken cancellationToken)
        {
            try
            {
                while (socket.State == WebSocketState.Open)
                {

                    cancellationToken.ThrowIfCancellationRequested();
                    var buffer = new byte[_bufferSize];
                    await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    var message = Encoding.ASCII.GetString(buffer);
                    _wsService.ProcessMessage(socket, message);
                }
            }
            catch (OperationCanceledException exception)
            {
                // _logger.LogError($"Operation was cancelled: {exception.Message}");
            }
            catch (ObjectDisposedException exception)
            {
                _logger.LogError($"Exception during socket receiving: {exception.Message}");
            }
            catch (ArgumentException exception)
            {
                _logger.LogError($"Exception during decoding buffer: {exception.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Socket Exception, socket aborted: {ex.Message}");
                socket.Abort();
            }
        }

        private Task<WebSocket> AcceptWebSocketAsync(HttpContext context)
        {
            return context.WebSockets.AcceptWebSocketAsync();
        }
    }
}
