using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace SimpleGame
{
    public class Player
    {
        public Creature ControlledCreature { get; private set; }

        public Player(Creature creature)
        {
            ControlledCreature = creature;
        }

        public void Update(GameTime gameTime)
        {
            KeyboardState state = Keyboard.GetState();

            Vector2 movement = Vector2.Zero;
            if (state.IsKeyDown(Keys.W))
                movement.Y -= 1;
            if (state.IsKeyDown(Keys.S))
                movement.Y += 1;
            if (state.IsKeyDown(Keys.A))
                movement.X -= 1;
            if (state.IsKeyDown(Keys.D))
                movement.X += 1;

            if (movement != Vector2.Zero)
            {
                movement.Normalize();
                Vector2 targetPosition = ControlledCreature.HeadPosition + movement * ControlledCreature.CircleSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                ControlledCreature.Update(gameTime, targetPosition);
                
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            ControlledCreature.Draw(spriteBatch);
        }

        public List<Vector2> GetAllPositions()
{
    return ControlledCreature.GetAllPositions();
}
    }
}