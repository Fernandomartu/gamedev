using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public static class Program
    {
        private static TcpListener server;
        private static List<TcpClient> clients = new List<TcpClient>();
        private static byte[] buffer = new byte[1024];

        public static async Task Main(string[] args)
        {
            int port = 12345;
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            Console.WriteLine($"Server started on port {port}.");

            while (true)
            {
                var client = await server.AcceptTcpClientAsync();
                clients.Add(client);
                Console.WriteLine("Client connected.");
                _ = Task.Run(() => HandleClient(client));
            }
        }

        private static async Task HandleClient(TcpClient client)
        {
            var stream = client.GetStream();
            while (client.Connected)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received: {message}");
                    BroadcastMessage(message, client);
                }
            }
            clients.Remove(client);
            Console.WriteLine("Client disconnected.");
        }

        private static void BroadcastMessage(string message, TcpClient excludeClient)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            foreach (var client in clients)
            {
                if (client != excludeClient)
                {
                    var stream = client.GetStream();
                    stream.Write(data, 0, data.Length);
                }
            }
        }
    }
}