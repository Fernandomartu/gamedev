using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SimpleGame
{
    public abstract class Creature
    {
        protected List<Vector2> circlePositions;
        protected Texture2D circleTexture;
        protected float circleSpeed;
        protected int circleRadius;

        public Vector2 HeadPosition => circlePositions[0];
        public float CircleSpeed => circleSpeed;  // Public getter for circleSpeed

        // Public getter for circleTexture
        public Texture2D CircleTexture => circleTexture;

        public Creature(GraphicsDevice graphicsDevice, int circleCount, int circleRadius, float circleSpeed, Vector2 startPosition, Color color)
        {
            this.circleRadius = circleRadius;
            this.circleSpeed = circleSpeed;
            circlePositions = new List<Vector2>();

            for (int i = 0; i < circleCount; i++)
            {
                circlePositions.Add(startPosition - new Vector2(0, i * circleRadius));
            }

            circleTexture = CreateCircleTexture(graphicsDevice, circleRadius, color);
        }

        public List<Vector2> GetAllPositions()
{
    return new List<Vector2>(circlePositions);
}

public void SetAllPositions(List<Vector2> newPositions)
{
    if (newPositions.Count == circlePositions.Count)
    {
        for (int i = 0; i < newPositions.Count; i++)
        {
            circlePositions[i] = newPositions[i];
        }
    }
}

        private Texture2D CreateCircleTexture(GraphicsDevice device, int radius, Color color)
        {
            int diameter = radius * 2;
            Texture2D texture = new Texture2D(device, diameter, diameter);
            Color[] data = new Color[diameter * diameter];

            for (int i = 0; i < data.Length; i++)
            {
                int x = (i % diameter) - radius;
                int y = (i / diameter) - radius;
                if (x * x + y * y <= radius * radius)
                    data[i] = color;
                else
                    data[i] = Color.Transparent;
            }

            texture.SetData(data);
            return texture;
        }

        public abstract void Update(GameTime gameTime, Vector2 targetPosition);

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var position in circlePositions)
            {
                spriteBatch.Draw(circleTexture, position, null, Color.White, 0f, new Vector2(circleRadius, circleRadius), 1f, SpriteEffects.None, 0f);
            }
        }

        public void SetPosition(Vector2 newPosition)
        {
            circlePositions[0] = newPosition;
            for (int i = 1; i < circlePositions.Count; i++)
            {
                circlePositions[i] = Vector2.Lerp(circlePositions[i], circlePositions[i - 1], 0.5f);
            }
        }

        protected void UpdatePositions(float maxAngleChange, GameTime gameTime)
        {
            for (int i = 1; i < circlePositions.Count; i++)
            {
                var direction = circlePositions[i - 1] - circlePositions[i];
                float length = direction.Length();

                if (length < float.Epsilon)
                {
                    length = 1;
                }

                direction.Normalize();
                float angle = (float)Math.Atan2(direction.Y, direction.X);

                if (i > 1)
                {
                    float prevAngle = (float)Math.Atan2(circlePositions[i - 2].Y - circlePositions[i - 1].Y, circlePositions[i - 2].X - circlePositions[i - 1].X);
                    float angleDiff = MathHelper.WrapAngle(angle - prevAngle);

                    if (Math.Abs(angleDiff) > maxAngleChange)
                    {
                        angle = prevAngle + Math.Sign(angleDiff) * maxAngleChange;
                    }
                }

                direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                circlePositions[i] = circlePositions[i - 1] - direction * circleRadius;
            }
        }
    }

    public class Lizard : Creature
    {
        public Lizard(GraphicsDevice graphicsDevice, Vector2 startPosition)
            : base(graphicsDevice, 3, 25, 200f, startPosition, Color.Green)
        {
        }

        public override void Update(GameTime gameTime, Vector2 targetPosition)
        {
            if (float.IsNaN(targetPosition.X) || float.IsNaN(targetPosition.Y))
            {
                targetPosition = HeadPosition;
            }

            Vector2 direction = targetPosition - circlePositions[0];
            direction.Normalize();

            circlePositions[0] += direction * circleSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            UpdatePositions(MathHelper.ToRadians(60f), gameTime);
        }
    }

    public class Snake : Creature
    {
        public Snake(GraphicsDevice graphicsDevice, Vector2 startPosition)
            : base(graphicsDevice, 6, 25, 200f, startPosition, Color.Red)
        {
        }

        public override void Update(GameTime gameTime, Vector2 targetPosition)
        {
            if (float.IsNaN(targetPosition.X) || float.IsNaN(targetPosition.Y))
            {
                targetPosition = HeadPosition;
            }

            Vector2 direction = targetPosition - circlePositions[0];
            direction.Normalize();

            circlePositions[0] += direction * circleSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            UpdatePositions(MathHelper.ToRadians(40f), gameTime);
        }
    }

public class Frog : Creature
    {
        public Frog(GraphicsDevice graphicsDevice, Vector2 startPosition)
            : base(graphicsDevice, 4, 25, 180f, startPosition, Color.Blue)
        {
        }

        public override void Update(GameTime gameTime, Vector2 targetPosition)
        {
            if (float.IsNaN(targetPosition.X) || float.IsNaN(targetPosition.Y))
            {
                targetPosition = HeadPosition;
            }

            Vector2 direction = targetPosition - circlePositions[0];
            direction.Normalize();

            circlePositions[0] += direction * circleSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            UpdatePositions(MathHelper.ToRadians(50f), gameTime);
        }
    }
}


