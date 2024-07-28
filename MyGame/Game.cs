using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
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

        private enum GameState
        {
            Menu,
            Playing
        }

        private GameState gameState;
        private List<string> creatureOptions;
        private int selectedOption;
        private SpriteFont menuFont;

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            networkManager = new NetworkManager();
            networkManager.OnMessageReceived += HandleMessageReceived;
            playersDict = new Dictionary<int, Player>();

            gameState = GameState.Menu;
            creatureOptions = new List<string> { "Lizard", "Frog", "Snake" };
            selectedOption = 0;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            menuFont = Content.Load<SpriteFont>("MenuFont"); // Load a SpriteFont for drawing text
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var keyboardState = Keyboard.GetState();

            if (gameState == GameState.Menu)
            {
                if (keyboardState.IsKeyDown(Keys.Up))
                {
                    selectedOption = (selectedOption > 0) ? selectedOption - 1 : creatureOptions.Count - 1;
                }
                else if (keyboardState.IsKeyDown(Keys.Down))
                {
                    selectedOption = (selectedOption < creatureOptions.Count - 1) ? selectedOption + 1 : 0;
                }
                else if (keyboardState.IsKeyDown(Keys.Enter))
                {
                    string selectedCreature = creatureOptions[selectedOption];
                    StartMultiplayer(selectedCreature);
                    gameState = GameState.Playing;
                }
            }
            else if (gameState == GameState.Playing)
            {
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
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            try
            {
                if (gameState == GameState.Menu)
                {
                    DrawMenu();
                }
                else if (gameState == GameState.Playing)
                {
                    foreach (var player in playersDict.Values)
                    {
                        player.Draw(spriteBatch);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Draw: {ex.Message}");
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawMenu()
        {
            Vector2 position = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

            for (int i = 0; i < creatureOptions.Count; i++)
            {
                Color color = (i == selectedOption) ? Color.Yellow : Color.White;
                spriteBatch.DrawString(menuFont, creatureOptions[i], position, color);
                position.Y += menuFont.LineSpacing;
            }
        }

        private async void StartMultiplayer(string creatureType)
        {
            int port = 12345;
            string ip = "192.168.1.153"; // Change to your server's IP address

            await networkManager.ConnectToServer(ip, port);
            await networkManager.SendData($"CreatureType:{creatureType}{messageDelimiter}");
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

                var playerCreature = CreateCreature(creatureOptions[selectedOption], GraphicsDevice, playerStartPosition);
                var player = new Player(playerCreature);
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
                    var playerCreature = CreateCreature("Lizard", GraphicsDevice, positions[0]);
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
            else if (msg.StartsWith("PlayerCreature:"))
            {
                var parts = msg.Substring("PlayerCreature:".Length).Split(':');
                var id = int.Parse(parts[0]);
                var creatureType = parts[1];

                if (!playersDict.ContainsKey(id))
                {
                    var playerStartPosition = new Vector2(GraphicsDevice.Viewport.Width / 2 + (id * 20), GraphicsDevice.Viewport.Height / 2);
                    var playerCreature = CreateCreature(creatureType, GraphicsDevice, playerStartPosition);
                    var player = new Player(playerCreature);
                    playersDict[id] = player;
                    Console.WriteLine($"[Client] Added new player with ID: {id}, Creature: {creatureType}");
                }
                else
                {
                    var player = playersDict[id];
                    var newCreature = CreateCreature(creatureType, GraphicsDevice, player.ControlledCreature.HeadPosition);
                    player.ChangeCreature(newCreature);
                    Console.WriteLine($"[Client] Updated player {id} to Creature: {creatureType}");
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Client] Error in HandleMessageReceived: {ex.Message}");
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

        protected override void UnloadContent()
        {
            networkManager.Disconnect();
            base.UnloadContent();
        }
    }
}