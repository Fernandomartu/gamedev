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
        private static UdpClient server;
        private static Dictionary<IPEndPoint, int> clients = new Dictionary<IPEndPoint, int>();
        private static Dictionary<int, List<Vector2>> playerPositions = new Dictionary<int, List<Vector2>>();
        private static Dictionary<int, string> playerCreatures = new Dictionary<int, string>();
        private static int nextPlayerId = 1;
        private static string messageDelimiter = "\n";

        public static async Task Main(string[] args)
        {
            int port = 12345;
            server = new UdpClient(port);
            Console.WriteLine($"Server started on port {port}.");

            _ = Task.Run(() => BroadcastGameStateLoop());

            while (true)
            {
                var result = await server.ReceiveAsync();
                var clientEndpoint = result.RemoteEndPoint;
                string message = Encoding.ASCII.GetString(result.Buffer);
                Console.WriteLine($"Received message from {clientEndpoint}: {message}");

                if (!clients.ContainsKey(clientEndpoint))
                {
                    int newPlayerId = nextPlayerId++;
                    clients[clientEndpoint] = newPlayerId;
                    await SendDataToClient(clientEndpoint, $"PlayerId:{newPlayerId}{messageDelimiter}");
                    var initialPosition = new Vector2(400 + (newPlayerId * 20), 240);
                    playerPositions[newPlayerId] = new List<Vector2> { initialPosition };
                    Console.WriteLine($"Client connected with PlayerId: {newPlayerId}");
                }

                var playerId = clients[clientEndpoint];
                HandleMessage(message, playerId);
            }
        }

        private static void HandleMessage(string message, int playerId)
        {
            try
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

                        playerPositions[id] = positions;
                        Console.WriteLine($"[Server] Updated positions for player {id}: {string.Join(", ", positions)}");
                    }
                    else if (msg.StartsWith("CreatureType:"))
                    {
                        var creatureType = msg.Substring("CreatureType:".Length);
                        playerCreatures[playerId] = creatureType;
                        BroadcastCreatureType(playerId, creatureType);
                        Console.WriteLine($"[Server] Player {playerId} selected creature: {creatureType}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Server] Error handling message from player {playerId}: {ex.Message}");
            }
        }

        private static async Task BroadcastGameStateLoop()
        {
            while (true)
            {
                BroadcastGameState();
                await Task.Delay(40);
            }
        }

        private static void BroadcastCreatureType(int playerId, string creatureType)
        {
            var message = $"PlayerCreature:{playerId}:{creatureType}{messageDelimiter}";
            BroadcastMessage(message);
        }

        private static void BroadcastMessage(string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            foreach (var client in clients.Keys)
            {
                server.SendAsync(data, data.Length, client);
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

            byte[] data = Encoding.ASCII.GetBytes(gameStateMessage.ToString());
            foreach (var client in clients.Keys)
            {
                server.SendAsync(data, data.Length, client);
            }
            Console.WriteLine($"Broadcasted game state: {gameStateMessage}");
        }

        private static async Task SendDataToClient(IPEndPoint clientEndpoint, string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            await server.SendAsync(data, data.Length, clientEndpoint);
            Console.WriteLine($"Sent to {clientEndpoint}: {message}");
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