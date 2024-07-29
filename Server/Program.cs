using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Serilog;

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
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/server-log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Log.Information("Starting server...");
                int port = 12345;
                server = new UdpClient(port);
                Log.Information("Server started on port {Port}.", port);

                _ = Task.Run(() => BroadcastGameStateLoop());

                while (true)
                {
                    var result = await server.ReceiveAsync();
                    var clientEndpoint = result.RemoteEndPoint;
                    string message = Encoding.ASCII.GetString(result.Buffer);
                    Log.Information("Received message from {ClientEndpoint}: {Message}", clientEndpoint, message);

                    if (!clients.ContainsKey(clientEndpoint))
                    {
                        int newPlayerId = nextPlayerId++;
                        clients[clientEndpoint] = newPlayerId;
                        await SendDataToClient(clientEndpoint, $"PlayerId:{newPlayerId}{messageDelimiter}");
                        var initialPosition = new Vector2(400 + (newPlayerId * 20), 240);
                        playerPositions[newPlayerId] = new List<Vector2> { initialPosition };
                        Log.Information("Client connected with PlayerId: {PlayerId}", newPlayerId);

                        // Send current state to new client
                        await SendCurrentStateToClient(clientEndpoint);
                    }

                    var playerId = clients[clientEndpoint];
                    HandleMessage(message, playerId);
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "The server encountered a fatal error.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void HandleMessage(string message, int playerId)
        {
            try
            {
                var messages = message.Split(new[] { messageDelimiter }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var msg in messages)
                {
                    Log.Information("[Server] Received: {Message}", msg);
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
                        Log.Information("[Server] Updated positions for player {Id}: {Positions}", id, string.Join(", ", positions));
                    }
                    else if (msg.StartsWith("CreatureType:"))
                    {
                        var creatureType = msg.Substring("CreatureType:".Length);
                        playerCreatures[playerId] = creatureType;
                        BroadcastCreatureType(playerId, creatureType);
                        Log.Information("[Server] Player {PlayerId} selected creature: {CreatureType}", playerId, creatureType);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[Server] Error handling message from player {PlayerId}", playerId);
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
            Log.Information("Broadcasted game state: {GameStateMessage}", gameStateMessage);
        }

        private static async Task SendCurrentStateToClient(IPEndPoint clientEndpoint)
        {
            foreach (var player in playerPositions)
            {
                var positionsMessage = new StringBuilder($"PlayerPositions:{player.Key}");
                foreach (var position in player.Value)
                {
                    positionsMessage.Append($":{position.X},{position.Y}");
                }
                positionsMessage.Append(messageDelimiter);
                await SendDataToClient(clientEndpoint, positionsMessage.ToString());
            }

            foreach (var player in playerCreatures)
            {
                var creatureMessage = $"PlayerCreature:{player.Key}:{player.Value}{messageDelimiter}";
                await SendDataToClient(clientEndpoint, creatureMessage);
            }
        }

        private static async Task SendDataToClient(IPEndPoint clientEndpoint, string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            await server.SendAsync(data, data.Length, clientEndpoint);
            Log.Information("Sent to {ClientEndpoint}: {Message}", clientEndpoint, message);
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