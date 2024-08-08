using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using Serilog;

namespace SimpleGame
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        public float scale = 0.44444f;

        RenderTarget2D renderTarget;
        private Dictionary<int, Player> playersDict;
        private NetworkManager networkManager;
        public int playerId;  // Make these fields public so MessageHandler can access them
        public bool playerIdAssigned;
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
        private MessageHandler messageHandler;

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            networkManager = new NetworkManager();
            networkManager.OnMessageReceived += HandleMessageReceived;
            playersDict = new Dictionary<int, Player>();

            gameState = GameState.Menu;
            creatureOptions = new List<string> { "Lizard", "Snake" };
            selectedOption = 0;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            renderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080);

            menuFont = Content.Load<SpriteFont>("MenuFont"); // Ensure this path matches your content structure
            
            Log.Information("Content loaded.");

            
        }

        protected override void Initialize()
        {
            base.Initialize();
            messageHandler = new MessageHandler(this, playersDict, GraphicsDevice, creatureOptions, selectedOption);
            Log.Information("Game initialized.");

            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            graphics.ApplyChanges();
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
                            var positions = player.GetAllPositions();
                            var message = $"PlayerPositions:{playerId}";
                            foreach (var position in positions)
                            {
                                message += $":{position.X},{position.Y}";
                            }
                            message += messageDelimiter;
                            _ = networkManager.SendData(message);
                            Log.Information("Sent player positions to server: {Message}", message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error in Update");
                }
            }

            base.Update(gameTime);
        }

      protected override void Draw(GameTime gameTime)
{
    scale = 1F/ (1089f/ graphics.GraphicsDevice.Viewport.Height);

    GraphicsDevice.SetRenderTarget(renderTarget);

    GraphicsDevice.Clear(Color.CornflowerBlue);

    BasicEffect basicEffect = new BasicEffect(GraphicsDevice)
    {
        VertexColorEnabled = true,
        Projection = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1)
    };

    spriteBatch.Begin();
    try
    {
        foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            if (gameState == GameState.Menu)
            {
                DrawMenu();
            }
            else if (gameState == GameState.Playing)
            {
                DrawCreatures(spriteBatch, playersDict.Values);
            }
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error in Draw");
    }
    spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);

    GraphicsDevice.Clear(Color.CornflowerBlue);

    spriteBatch.Begin();
    spriteBatch.Draw(renderTarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
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

    private void DrawCreatures(SpriteBatch spriteBatch, IEnumerable<Player> players)
{
    foreach (var player in players)
    {
        var creature = player.ControlledCreature;
        for (int i = 0; i < creature.bodyParts.Count; i++)
        {
            var part = creature.bodyParts[i];
            if (i == 0 && part.Name == "Head") // Assuming the first body part is the head
            {
                part.Draw(spriteBatch, creature.HeadRotationAngle, ((Lizard)creature).IsFacingRight);
            }
            else
            {
                part.Draw(spriteBatch);
            }
        }
    }
}

        private async void StartMultiplayer(string creatureType)
        {
            int port = 12345;
            string ip = "192.168.1.153"; // Change to your server's IP address

            Log.Information("Starting multiplayer...");
            await networkManager.StartClient(ip, port);
            Log.Information("Sending creature type to server...");
            await networkManager.SendData($"CreatureType:{creatureType}{messageDelimiter}");
        }

        private void HandleMessageReceived(string message)
        {
            messageHandler.HandleMessageReceived(message);
        }

        protected override void UnloadContent()
        {
            networkManager.Disconnect();
            base.UnloadContent();
            Log.Information("Content unloaded.");
        }
    }
}