using Microsoft.AspNetCore.Http;
using System;
using System.Fabric;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace WebService
{
    public class WorkerMiddleware 
    {
        private readonly StatelessServiceContext _serviceContext;
        private readonly RequestDelegate _next;

        public WorkerMiddleware(StatelessServiceContext serviceContext, RequestDelegate next)
        {
            _serviceContext = serviceContext;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/ws", out var remainingPath))
            {
                await _next.Invoke(context);
                return;
            }

            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                return;
            }

            ServiceEventSource.Current.ServiceMessage(_serviceContext, $"Remaining path is {remainingPath}");

            var id = remainingPath.ToString().TrimStart('/');
            if (id.Contains('/'))
            {
                context.Response.StatusCode = 400;
                return;
            }

            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await Echo(_serviceContext, context, webSocket);
        }


        private async Task Echo(StatelessServiceContext serviceContext, HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), context.RequestAborted);
            while (!result.CloseStatus.HasValue)
            {
                var bytes = new ArraySegment<byte>(buffer, 0, result.Count).ToArray();
                var @string = Encoding.UTF8.GetString(bytes);

                ServiceEventSource.Current.ServiceMessage(serviceContext, @string);

                @string = @string.Replace("client", "server");
                await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(@string)), result.MessageType, result.EndOfMessage, context.RequestAborted);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), context.RequestAborted);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, context.RequestAborted);
        }
    }
}
