using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SimpleGame
{
    public class Snake : Creature
    {
        private List<Vector2> allPositions;
        private List<int> allRadii;

        public Snake(GraphicsDevice graphicsDevice, Vector2 startPosition)
            : base(200f)
        {
            // Initialize allPositions and allRadii
            allPositions = new List<Vector2>();
            allRadii = new List<int>();

            // Head
            List<Vector2> headPos = new List<Vector2> { startPosition };
            List<Texture2D> headTextures = new List<Texture2D> { CreateCircleTexture(graphicsDevice, 20, Color.Blue) };
            List<int> headRadii = new List<int> { 20 };
            var head = new BodyPart(headPos, headTextures, headRadii, "Head");
            bodyParts.Add(head);
            allPositions.AddRange(headPos);
            allRadii.AddRange(headRadii);

            // Body
            List<Vector2> positions = new List<Vector2>
            {
                startPosition - new Vector2(0, 25),
                startPosition - new Vector2(0, 50),
                startPosition - new Vector2(0, 75),
                startPosition - new Vector2(0, 100),
                startPosition - new Vector2(0, 125)
            };
            List<Texture2D> textures = new List<Texture2D>
            {
                CreateCircleTexture(graphicsDevice, 25, Color.Red),
                CreateCircleTexture(graphicsDevice, 25, Color.Red),
                CreateCircleTexture(graphicsDevice, 25, Color.Red),
                CreateCircleTexture(graphicsDevice, 25, Color.Red),
                CreateCircleTexture(graphicsDevice, 25, Color.Red)
            };
            List<int> radii = new List<int> { 25, 25, 25, 25, 25 };
            var body = new BodyPart(positions, textures, radii, "Body") { PreviousPart = head };
            bodyParts.Add(body);
            allPositions.AddRange(positions);
            allRadii.AddRange(radii);
        }

        public override void Update(GameTime gameTime, Vector2 targetPosition)
        {
            if (float.IsNaN(targetPosition.X) || float.IsNaN(targetPosition.Y))
            {
                targetPosition = HeadPosition;
            }

            Vector2 direction = targetPosition - HeadPosition;
            direction.Normalize();

            allPositions[0] += direction * CircleSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Debug: Print head position to check movement
            Console.WriteLine($"Head Position: {allPositions[0]}");

            UpdatePositions(MathHelper.ToRadians(30f)); // Adjust the angle as needed
        }

        private void UpdatePositions(float maxAngleChange)
        {
            for (int i = 1; i < allPositions.Count; i++)
            {
                Vector2 direction;
                if (i == 1)
                {
                    direction = allPositions[0] - allPositions[i];
                }
                else
                {
                    direction = allPositions[i - 1] - allPositions[i];
                }

                float length = direction.Length();
                if (length < float.Epsilon)
                {
                    length = 1;
                }

                direction.Normalize();
                float angle = (float)Math.Atan2(direction.Y, direction.X);

                if (i > 1)
                {
                    float prevAngle = (float)Math.Atan2(allPositions[i - 2].Y - allPositions[i - 1].Y, allPositions[i - 2].X - allPositions[i - 1].X);
                    float angleDiff = MathHelper.WrapAngle(angle - prevAngle);

                    if (Math.Abs(angleDiff) > maxAngleChange)
                    {
                        angle = prevAngle + Math.Sign(angleDiff) * maxAngleChange;
                    }
                }

                direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                allPositions[i] = allPositions[i - 1] - direction * allRadii[i];
            }

            // Update bodyParts with new positions
            int index = 0;
            foreach (var part in bodyParts)
            {
                for (int i = 0; i < part.Positions.Count; i++, index++)
                {
                    part.Positions[i] = allPositions[index];
                }
            }
        }
    }
}