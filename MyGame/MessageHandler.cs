using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimpleGame;


public class MessageHandler
{
    private readonly Dictionary<int, Player> playersDict;
    private readonly GraphicsDevice graphicsDevice;
    private readonly List<string> creatureOptions;
    private readonly string messageDelimiter = "\n";
    private int selectedOption;

    public MessageHandler(Dictionary<int, Player> playersDict, GraphicsDevice graphicsDevice, List<string> creatureOptions, int selectedOption)
    {
        this.playersDict = playersDict;
        this.graphicsDevice = graphicsDevice;
        this.creatureOptions = creatureOptions;
        this.selectedOption = selectedOption;
    }

    public void HandleMessageReceived(string message)
    {
        try
        {
            var messages = message.Split(new[] { messageDelimiter }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var msg in messages)
            {
                var (messageType, content) = ParseMessage(msg);
                switch (messageType)
                {
                    case MessageType.PlayerId:
                        HandlePlayerId(content);
                        break;
                    case MessageType.PlayerPositions:
                        HandlePlayerPositions(content);
                        break;
                    case MessageType.PlayerCreature:
                        HandlePlayerCreature(content);
                        break;
                    default:
                        throw new Exception("Unknown message type");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Error in HandleMessageReceived: {ex.Message}");
        }
    }

    private (MessageType, string) ParseMessage(string message)
    {
        if (message.StartsWith("PlayerId:"))
        {
            return (MessageType.PlayerId, message.Substring("PlayerId:".Length));
        }
        else if (message.StartsWith("PlayerPositions:"))
        {
            return (MessageType.PlayerPositions, message.Substring("PlayerPositions:".Length));
        }
        else if (message.StartsWith("PlayerCreature:"))
        {
            return (MessageType.PlayerCreature, message.Substring("PlayerCreature:".Length));
        }
        else
        {
            throw new Exception("Unknown message type");
        }
    }

    private void HandlePlayerId(string content)
    {
        int playerId = int.Parse(content);
        Vector2 playerStartPosition = new Vector2(graphicsDevice.Viewport.Width / 2 + (playerId * 20), graphicsDevice.Viewport.Height / 2);

        var playerCreature = CreateCreature(creatureOptions[selectedOption], graphicsDevice, playerStartPosition);
        var player = new Player(playerCreature);
        playersDict[playerId] = player;

        Console.WriteLine($"[Client] Assigned PlayerId: {playerId}, Start Position: {playerStartPosition}");
    }

    private void HandlePlayerPositions(string content)
    {
        var parts = content.Split(':');
        int id = int.Parse(parts[0]);
        var positions = new List<Vector2>();

        for (int i = 1; i < parts.Length; i++)
        {
            var positionParts = parts[i].Split(',');
            float x = float.Parse(positionParts[0]);
            float y = float.Parse(positionParts[1]);
            positions.Add(new Vector2(x, y));
        }

        if (!playersDict.ContainsKey(id))
        {
            var playerCreature = CreateCreature("Lizard", graphicsDevice, positions[0]);
            var player = new Player(playerCreature);
            playersDict[id] = player;
            Console.WriteLine($"[Client] Added new player with ID: {id} at starting position {positions[0]}");
        }
        else
        {
            playersDict[id].ControlledCreature.SetAllPositions(positions);
            Console.WriteLine($"[Client] Updated positions for player {id}: {string.Join(", ", positions)}");
        }
    }

    private void HandlePlayerCreature(string content)
    {
        var parts = content.Split(':');
        int id = int.Parse(parts[0]);
        string creatureType = parts[1];

        if (!playersDict.ContainsKey(id))
        {
            Vector2 playerStartPosition = new Vector2(graphicsDevice.Viewport.Width / 2 + (id * 20), graphicsDevice.Viewport.Height / 2);
            var playerCreature = CreateCreature(creatureType, graphicsDevice, playerStartPosition);
            var player = new Player(playerCreature);
            playersDict[id] = player;
            Console.WriteLine($"[Client] Added new player with ID: {id}, Creature: {creatureType}");
        }
        else
        {
            var player = playersDict[id];
            var newCreature = CreateCreature(creatureType, graphicsDevice, player.ControlledCreature.HeadPosition);
            player.ChangeCreature(newCreature);
            Console.WriteLine($"[Client] Updated player {id} to Creature: {creatureType}");
        }
    }

    private Creature CreateCreature(string creatureType, GraphicsDevice graphicsDevice, Vector2 startPosition)
    {
        switch (creatureType)
        {
            case "Frog":
                return new Frog(graphicsDevice, startPosition);
            case "Snake":
                return new Snake(graphicsDevice, startPosition);
            case "Lizard":
            default:
                return new Lizard(graphicsDevice, startPosition);
        }
    }
}