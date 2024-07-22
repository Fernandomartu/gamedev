using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleGame
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private List<Player> players;
        private Snake enemySnake;
        private NetworkManager networkManager;

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            networkManager = new NetworkManager();
        }

        protected override void Initialize()
        {
            players = new List<Player>();

            Vector2 playerStartPosition = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            var playerLizard = new Lizard(GraphicsDevice, playerStartPosition);
            players.Add(new Player(playerLizard));

            Vector2 enemyStartPosition = playerStartPosition + new Vector2(100, 0);
            enemySnake = new Snake(GraphicsDevice, enemyStartPosition);

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

            foreach (var player in players)
            {
                player.Update(gameTime);
            }

            // Update the snake to follow the player's head position
            enemySnake.Update(gameTime, players[0].ControlledCreature.HeadPosition);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            foreach (var player in players)
            {
                player.Draw(spriteBatch);
            }

            // Draw the enemy snake
            enemySnake.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private async void StartMultiplayer()
        {
            bool isServer = false; // Change this based on your needs
            int port = 12345;
            string ip = "127.0.0.1"; // Change to your server's IP address

            if (isServer)
            {
                await networkManager.StartServer(port);
            }
            else
            {
                await networkManager.ConnectToServer(ip, port);
            }
        }

        protected override void UnloadContent()
        {
            networkManager.Disconnect();
            base.UnloadContent();
        }
    }
}