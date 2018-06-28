using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            RunAsync().Wait();
        }

        public static async Task RunAsync()
        {
            using (var webSocket = new ClientWebSocket())
            {
                await webSocket.ConnectAsync(new Uri("ws://localhost:8280/ws"), CancellationToken.None);
                var bytes = Encoding.UTF8.GetBytes("Hello from the client!");
                await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

                bytes = new byte[1024];
                var result = await webSocket.ReceiveAsync(bytes, CancellationToken.None);
                Console.WriteLine(Encoding.UTF8.GetString(new ArraySegment<byte>(bytes, 0, result.Count).ToArray()));
            }
        }
    }
}
