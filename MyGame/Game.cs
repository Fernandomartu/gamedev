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
    creatureOptions = new List<string> { "Lizard", "Frog", "Snake" };
    selectedOption = 0;
    messageHandler = new MessageHandler(this, playersDict, GraphicsDevice, creatureOptions, selectedOption);
    Console.WriteLine("[Client] Game initialized");
}

protected override void Initialize()
{
    base.Initialize();
    messageHandler = new MessageHandler(this, playersDict, GraphicsDevice, creatureOptions, selectedOption);
    Console.WriteLine("[Client] Game initialized and message handler set");
}

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            menuFont = Content.Load<SpriteFont>("MenuFont"); // Ensure this path matches your content structure
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

                    // Debug log
                    Console.WriteLine($"[Client] Player {playerId} positions sent to server: {string.Join(", ", positions)}");
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
                // Debug log
                Console.WriteLine($"[Client] Drawing player at position: {player.ControlledCreature.HeadPosition}");
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
    await networkManager.SendData($"CreatureType:{playerId}:{creatureType}{messageDelimiter}");
}
        private void HandleMessageReceived(string message)
        {

            Console.WriteLine("doing this");
            messageHandler.HandleMessageReceived(message);
        }

        protected override void UnloadContent()
        {
            networkManager.Disconnect();
            base.UnloadContent();
        }
    }
}