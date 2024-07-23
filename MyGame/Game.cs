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
        private Dictionary<int, Snake> enemySnakes;
        private NetworkManager networkManager;
        private int playerId;

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            networkManager = new NetworkManager();
            networkManager.OnMessageReceived += HandleMessageReceived;
            enemySnakes = new Dictionary<int, Snake>();
        }

        protected override void Initialize()
        {
            players = new List<Player>();

            Vector2 playerStartPosition = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            var playerLizard = new Lizard(GraphicsDevice, playerStartPosition);
            var player = new Player(playerLizard);
            players.Add(player);

            playerId = players.Count;

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
                // Send player position to the other instances
                var position = player.ControlledCreature.HeadPosition;
                var message = $"PlayerPosition:{playerId}:{position.X},{position.Y}";
                _ = networkManager.SendData(message);
            }

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

            // Draw enemy snakes
            foreach (var enemySnake in enemySnakes.Values)
            {
                enemySnake.Draw(spriteBatch);
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
            if (message.StartsWith("PlayerPosition:"))
            {
                var parts = message.Substring("PlayerPosition:".Length).Split(':');
                var id = int.Parse(parts[0]);
                var positionParts = parts[1].Split(',');
                var x = float.Parse(positionParts[0]);
                var y = float.Parse(positionParts[1]);
                Vector2 position = new Vector2(x, y);

                if (!enemySnakes.ContainsKey(id))
                {
                    enemySnakes[id] = new Snake(GraphicsDevice, position);
                }
                else
                {
                    enemySnakes[id].SetPosition(position);
                }
            }
        }

        protected override void UnloadContent()
        {
            networkManager.Disconnect();
            base.UnloadContent();
        }
    }
}