using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace SimpleGame
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Dictionary<int, Player> playersDict;
        private NetworkManager networkManager;
        private int playerId;
        private bool playerIdAssigned;
        private string messageDelimiter = "\n";

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            networkManager = new NetworkManager();
            networkManager.OnMessageReceived += HandleMessageReceived;
            playersDict = new Dictionary<int, Player>();
        }

        protected override void Initialize()
        {
            base.Initialize();
            StartMultiplayer();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
{
    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        Exit();

    try
    {
        if (playerIdAssigned)
        {
            if (playersDict.ContainsKey(playerId))
            {
                var player = playersDict[playerId];
                player.Update(gameTime);

                // Send positions of all body parts to the server
                var positions = player.ControlledCreature.GetAllPositions();
                var message = $"PlayerPositions:{playerId}";
                foreach (var position in positions)
                {
                    message += $":{position.X},{position.Y}";
                }
                message += messageDelimiter;
                _ = networkManager.SendData(message);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error in Update: {ex.Message}");
    }

    base.Update(gameTime);
}

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            try
            {
                foreach (var player in playersDict.Values)
                {
                    player.Draw(spriteBatch);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Draw: {ex.Message}");
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private async void StartMultiplayer()
        {
            int port = 12345;
            string ip = "192.168.1.153"; // Change to your server's IP address

            await networkManager.ConnectToServer(ip, port);
        }

        private void HandleMessageReceived(string message)
{
    try
    {
        var messages = message.Split(new[] { messageDelimiter }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var msg in messages)
        {
            if (msg.StartsWith("PlayerId:"))
            {
                playerId = int.Parse(msg.Substring("PlayerId:".Length));
                playerIdAssigned = true;

                Vector2 playerStartPosition = new Vector2(GraphicsDevice.Viewport.Width / 2 + (playerId * 20), GraphicsDevice.Viewport.Height / 2);
                var playerLizard = new Lizard(GraphicsDevice, playerStartPosition);
                var player = new Player(playerLizard);
                playersDict[playerId] = player;

                Console.WriteLine($"[Client] Assigned PlayerId: {playerId}, Start Position: {playerStartPosition}");
            }
            else if (msg.StartsWith("PlayerPositions:"))
            {
                var parts = msg.Substring("PlayerPositions:".Length).Split(':');
                var id = int.Parse(parts[0]);
                var positions = new List<Vector2>();

                for (int i = 1; i < parts.Length; i++)
                {
                    var positionParts = parts[i].Split(',');
                    var x = float.Parse(positionParts[0]);
                    var y = float.Parse(positionParts[1]);
                    positions.Add(new Vector2(x, y));
                }

                if (!playersDict.ContainsKey(id))
                {
                    var lizard = new Lizard(GraphicsDevice, positions[0]);
                    var player = new Player(lizard);
                    playersDict[id] = player;
                    Console.WriteLine($"[Client] Added new player with ID: {id} at starting position {positions[0]}");
                }
                else if (id != playerId)
            {
                playersDict[id].ControlledCreature.SetAllPositions(positions);
                Console.WriteLine($"[Client] Updated positions for player {id}: {string.Join(", ", positions)}");
            }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Client] Error in HandleMessageReceived: {ex.Message}");
    }
}

        protected override void UnloadContent()
        {
            networkManager.Disconnect();
            base.UnloadContent();
        }
    }
}