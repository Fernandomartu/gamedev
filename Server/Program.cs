using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GameServer
{
    public static class Program
    {
        private static TcpListener server;
        private static List<TcpClient> clients = new List<TcpClient>();
        private static Dictionary<int, Vector2> playerPositions = new Dictionary<int, Vector2>();
        private static int nextPlayerId = 1;
        private static byte[] buffer = new byte[1024];
        private static string messageDelimiter = "\n";

        public static async Task Main(string[] args)
        {
            int port = 12345;
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            Console.WriteLine($"Server started on port {port}.");

            _ = Task.Run(() => BroadcastGameStateLoop());

            while (true)
            {
                var client = await server.AcceptTcpClientAsync();
                clients.Add(client);
                Console.WriteLine("Client connected.");
                int playerId = nextPlayerId++;
                await SendDataToClient(client, $"PlayerId:{playerId}{messageDelimiter}");
                playerPositions[playerId] = new Vector2(400 + (playerId * 20), 240);
                _ = Task.Run(() => HandleClient(client, playerId));
            }
        }

        private static async Task HandleClient(TcpClient client, int playerId)
        {
            var stream = client.GetStream();
            var data = new StringBuilder();

            while (client.Connected)
            {
                try
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        data.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                        var messages = data.ToString().Split(new[] { messageDelimiter }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (var message in messages)
                        {
                            Console.WriteLine($"Received: {message}");
                            if (message.StartsWith("PlayerPosition:"))
                            {
                                var parts = message.Substring("PlayerPosition:".Length).Split(':');
                                var id = int.Parse(parts[0]);
                                var positionParts = parts[1].Split(',');
                                var x = float.Parse(positionParts[0]);
                                var y = float.Parse(positionParts[1]);
                                Vector2 position = new Vector2(x, y);
                                playerPositions[id] = position;
                            }
                        }

                        // Clear the data buffer
                        data.Clear();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error handling client {playerId}: {ex.Message}");
                    break;
                }
            }
            clients.Remove(client);
            playerPositions.Remove(playerId);
            Console.WriteLine("Client disconnected.");
        }

        private static async Task BroadcastGameStateLoop()
        {
            while (true)
            {
                BroadcastGameState();
                await Task.Delay(100); // Broadcast every 100ms
            }
        }

        private static void BroadcastGameState()
        {
            var gameStateMessage = new StringBuilder();
            foreach (var player in playerPositions)
            {
                gameStateMessage.Append($"PlayerPosition:{player.Key}:{player.Value.X},{player.Value.Y}{messageDelimiter}");
            }

            foreach (var client in clients)
            {
                var stream = client.GetStream();
                byte[] data = Encoding.ASCII.GetBytes(gameStateMessage.ToString());
                try
                {
                    stream.Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error broadcasting to client: {ex.Message}");
                }
            }
        }

        private static async Task SendDataToClient(TcpClient client, string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            var stream = client.GetStream();
            await stream.WriteAsync(data, 0, data.Length);
        }
    }

    public struct Vector2
    {
        public float X;
        public float Y;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}