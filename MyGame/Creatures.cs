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

       protected void UpdatePositions(float maxAngleChange, GameTime gameTime)
{
    for (int i = 1; i < circlePositions.Count; i++)
    {
        var direction = circlePositions[i - 1] - circlePositions[i];
        float length = direction.Length();

        // Log direction and length
        Console.WriteLine($"Circle {i} direction before normalization: {direction}, length: {length}");

        // Prevent division by zero
        if (length < float.Epsilon)
        {
            length = 1;
        }

        direction.Normalize();

        // Ensure normalization does not produce NaN
        if (float.IsNaN(direction.X) || float.IsNaN(direction.Y))
        {
            Console.WriteLine($"NaN detected in direction normalization for circle {i}");
            direction = Vector2.UnitX; // Default direction to prevent NaN
        }

        float angle = (float)Math.Atan2(direction.Y, direction.X);

        if (float.IsNaN(angle))
        {
            Console.WriteLine($"NaN detected in angle calculation for circle {i}");
            angle = 0f; // Default angle to prevent NaN
        }

        if (i > 1)
        {
            float prevAngle = (float)Math.Atan2(circlePositions[i - 2].Y - circlePositions[i - 1].Y, circlePositions[i - 2].X - circlePositions[i - 1].X);

            if (float.IsNaN(prevAngle))
            {
                Console.WriteLine($"NaN detected in prevAngle calculation for circle {i}");
                prevAngle = 0f; // Default previous angle to prevent NaN
            }

            float angleDiff = MathHelper.WrapAngle(angle - prevAngle);

            if (Math.Abs(angleDiff) > maxAngleChange)
            {
                angle = prevAngle + Math.Sign(angleDiff) * maxAngleChange;
            }
        }

        direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

        // Ensure new direction does not produce NaN
        if (float.IsNaN(direction.X) || float.IsNaN(direction.Y))
        {
            Console.WriteLine($"NaN detected in new direction calculation for circle {i}");
            direction = Vector2.UnitX; // Default direction to prevent NaN
        }

        circlePositions[i] = circlePositions[i - 1] - direction * circleRadius;

        // Ensure new position does not produce NaN
        if (float.IsNaN(circlePositions[i].X) || float.IsNaN(circlePositions[i].Y))
        {
            Console.WriteLine($"NaN detected in new position calculation for circle {i}");
            circlePositions[i] = circlePositions[i - 1]; // Default position to prevent NaN
        }

        // Log positions for debugging
        Console.WriteLine($"Circle {i}: {circlePositions[i]}");
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
    // Log targetPosition
    Console.WriteLine($"Update targetPosition: {targetPosition}");

    if (float.IsNaN(targetPosition.X) || float.IsNaN(targetPosition.Y))
    {
        Console.WriteLine("NaN detected in targetPosition");
        targetPosition = HeadPosition; // Default target position to prevent NaN
    }

    Vector2 direction = targetPosition - circlePositions[0];
    direction.Normalize();

    // Log direction
    Console.WriteLine($"Head direction before normalization: {direction}, length: {direction.Length()}");

    if (float.IsNaN(direction.X) || float.IsNaN(direction.Y))
    {
        Console.WriteLine("NaN detected in direction normalization");
        direction = Vector2.UnitX; // Default direction to prevent NaN
    }

    circlePositions[0] += direction * circleSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

    // Log new head position
    Console.WriteLine($"New head position: {circlePositions[0]}");

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
    // Log targetPosition
    Console.WriteLine($"Update targetPosition: {targetPosition}");

    if (float.IsNaN(targetPosition.X) || float.IsNaN(targetPosition.Y))
    {
        Console.WriteLine("NaN detected in targetPosition");
        targetPosition = HeadPosition; // Default target position to prevent NaN
    }

    Vector2 direction = targetPosition - circlePositions[0];
    direction.Normalize();

    // Log direction
    Console.WriteLine($"Head direction before normalization: {direction}, length: {direction.Length()}");

    if (float.IsNaN(direction.X) || float.IsNaN(direction.Y))
    {
        Console.WriteLine("NaN detected in direction normalization");
        direction = Vector2.UnitX; // Default direction to prevent NaN
    }

    circlePositions[0] += direction * circleSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

    // Log new head position
    Console.WriteLine($"New head position: {circlePositions[0]}");

    UpdatePositions(MathHelper.ToRadians(40f), gameTime);
}
    }
}