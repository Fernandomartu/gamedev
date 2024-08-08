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
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            Vector2 movement = Vector2.Zero;
            if (keyboardState.IsKeyDown(Keys.W))
                movement.Y -= 1;
            if (keyboardState.IsKeyDown(Keys.S))
                movement.Y += 1;
            if (keyboardState.IsKeyDown(Keys.A))
                movement.X -= 1;
            if (keyboardState.IsKeyDown(Keys.D))
                movement.X += 1;

            // If keyboard movement is detected
            if (movement != Vector2.Zero)
            {
                movement.Normalize();
                Vector2 targetPosition = ControlledCreature.HeadPosition + movement * ControlledCreature.CircleSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                ControlledCreature.Update(gameTime, targetPosition);
            }
            // If both mouse buttons are pressed
            else if (mouseState.LeftButton == ButtonState.Pressed && mouseState.RightButton == ButtonState.Pressed)
            {
                Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);
                Vector2 direction = mousePosition - ControlledCreature.HeadPosition;
                float distanceToMouse = direction.Length();
                float stopThreshold = 10f; // Define the distance at which the lizard should stop

                // If the lizard is further than the threshold distance from the mouse
                if (distanceToMouse > stopThreshold)
                {
                    direction.Normalize();
                    Vector2 targetPosition = ControlledCreature.HeadPosition + direction * ControlledCreature.CircleSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    ControlledCreature.Update(gameTime, targetPosition);
                }
            }
        }

        public List<Vector2> GetAllPositions()
        {
            return ControlledCreature.GetAllPositions();
        }

        public void ChangeCreature(Creature newCreature)
        {
            ControlledCreature = newCreature;
        }
    }
}