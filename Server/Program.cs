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
        private static Dictionary<int, List<Vector2>> playerPositions = new Dictionary<int, List<Vector2>>();
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
                var initialPosition = new Vector2(400 + (playerId * 20), 240);
                playerPositions[playerId] = new List<Vector2> { initialPosition }; // Initialize with the head position
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
                    Console.WriteLine($"[Server] Received: {message}");
                    if (message.StartsWith("PlayerPositions:"))
                    {
                        var parts = message.Substring("PlayerPositions:".Length).Split(':');
                        var id = int.Parse(parts[0]);
                        var positions = new List<Vector2>();

                        for (int i = 1; i < parts.Length; i++)
                        {
                            var positionParts = parts[i].Split(',');
                            float x = float.Parse(positionParts[0]);
                            float y = float.Parse(positionParts[1]);
                            positions.Add(new Vector2(x, y));
                        }

                        playerPositions[id] = positions; // Store the list of positions
                        Console.WriteLine($"[Server] Updated positions for player {id}: {string.Join(", ", positions)}");
                    }
                }

                // Clear the data buffer
                data.Clear();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Server] Error handling client {playerId}: {ex.Message}");
            break;
        }
    }
    clients.Remove(client);
    playerPositions.Remove(playerId);
    Console.WriteLine($"[Server] Client {playerId} disconnected.");
}

        private static async Task BroadcastGameStateLoop()
        {
            while (true)
            {
                BroadcastGameState();
                await Task.Delay(30); // Broadcast every 100ms
            }
        }

        private static void BroadcastGameState()
        {
            var gameStateMessage = new StringBuilder();
            foreach (var player in playerPositions)
            {
                gameStateMessage.Append($"PlayerPositions:{player.Key}");
                foreach (var position in player.Value)
                {
                    gameStateMessage.Append($":{position.X},{position.Y}");
                }
                gameStateMessage.Append(messageDelimiter);
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