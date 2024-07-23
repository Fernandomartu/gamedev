using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Text;

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
                        // Send player position to the server
                        var position = player.ControlledCreature.HeadPosition;
                        var message = $"PlayerPosition:{playerId}:{position.X},{position.Y}{messageDelimiter}";
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
            string ip = "127.0.0.1"; // Change to your server's IP address

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

                        Console.WriteLine($"Assigned PlayerId: {playerId}, Start Position: {playerStartPosition}");
                    }
                    else if (msg.StartsWith("PlayerPosition:"))
                    {
                        var positions = msg.Split(';', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var positionMessage in positions)
                        {
                            var parts = positionMessage.Substring("PlayerPosition:".Length).Split(':');
                            var id = int.Parse(parts[0]);
                            var positionParts = parts[1].Split(',');
                            var x = float.Parse(positionParts[0]);
                            var y = float.Parse(positionParts[1]);
                            Vector2 position = new Vector2(x, y);

                            if (!playersDict.ContainsKey(id))
                            {
                                var snake = new Snake(GraphicsDevice, position);
                                var player = new Player(snake);
                                playersDict[id] = player;
                                Console.WriteLine($"Added new player with ID: {id} at position {position}");
                            }
                            else
                            {
                                playersDict[id].ControlledCreature.SetPosition(position);
                                Console.WriteLine($"Updated position for player {id} to {position}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in HandleMessageReceived: {ex.Message}");
            }
        }

        protected override void UnloadContent()
        {
            networkManager.Disconnect();
            base.UnloadContent();
        }
    }
}