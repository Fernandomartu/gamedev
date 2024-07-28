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
        private static UdpClient udpServer;
        private static List<IPEndPoint> clients = new List<IPEndPoint>();
        private static Dictionary<int, List<Vector2>> playerPositions = new Dictionary<int, List<Vector2>>();
        private static Dictionary<int, string> playerCreatures = new Dictionary<int, string>();
        private static int nextPlayerId = 1;
        private static string messageDelimiter = "\n";

        public static async Task Main(string[] args)
        {
            int port = 12345;
            udpServer = new UdpClient(port);
            Console.WriteLine($"Server started on port {port}.");

            _ = Task.Run(() => ReceiveMessages());

            while (true)
            {
                var result = await udpServer.ReceiveAsync();
                var clientEndPoint = result.RemoteEndPoint;
                var message = Encoding.ASCII.GetString(result.Buffer);
                
                if (!clients.Contains(clientEndPoint))
                {
                    clients.Add(clientEndPoint);
                    Console.WriteLine("Client connected.");
                    int playerId = nextPlayerId++;
                    await SendDataToClient(clientEndPoint, $"PlayerId:{playerId}{messageDelimiter}");
                    var initialPosition = new Vector2(400 + (playerId * 20), 240);
                    playerPositions[playerId] = new List<Vector2> { initialPosition }; // Initialize with the head position
                }

                HandleMessage(clientEndPoint, message);
            }
        }

        private static async Task ReceiveMessages()
        {
            while (true)
            {
                var result = await udpServer.ReceiveAsync();
                var clientEndPoint = result.RemoteEndPoint;
                var message = Encoding.ASCII.GetString(result.Buffer);
                HandleMessage(clientEndPoint, message);
            }
        }

       private static void HandleMessage(IPEndPoint clientEndPoint, string message)
{
    var messages = message.Split(new[] { messageDelimiter }, StringSplitOptions.RemoveEmptyEntries);

    foreach (var msg in messages)
    {
        Console.WriteLine($"[Server] Received: {msg}");
        if (msg.StartsWith("PlayerPositions:"))
        {
            var parts = msg.Substring("PlayerPositions:".Length).Split(':');
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
        else if (msg.StartsWith("CreatureType:"))
        {
            var parts = msg.Substring("CreatureType:".Length).Split(':');
            var playerId = int.Parse(parts[0]);
            var creatureType = parts[1];
            playerCreatures[playerId] = creatureType;
            BroadcastCreatureType(playerId, creatureType);
            Console.WriteLine($"[Server] Player {playerId} selected creature: {creatureType}");
        }
    }
}
        private static async Task BroadcastGameStateLoop()
        {
            while (true)
            {
                BroadcastGameState();
                await Task.Delay(50); // Broadcast every 50ms
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

            BroadcastMessage(gameStateMessage.ToString());
        }

        private static void BroadcastCreatureType(int playerId, string creatureType)
        {
            var message = $"PlayerCreature:{playerId}:{creatureType}{messageDelimiter}";
            BroadcastMessage(message);
        }

        private static void BroadcastMessage(string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            foreach (var client in clients)
            {
                try
                {
                    udpServer.Send(data, data.Length, client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error broadcasting to client: {ex.Message}");
                }
            }
        }

        private static async Task SendDataToClient(IPEndPoint clientEndPoint, string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            await udpServer.SendAsync(data, data.Length, clientEndPoint);
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