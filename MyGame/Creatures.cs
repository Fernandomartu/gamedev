using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SimpleGame
{
    public abstract class Creature
    {
        public List<BodyPart> bodyParts;
        protected float circleSpeed;

        public Vector2 HeadPosition => bodyParts[0].Positions[0];
        public float CircleSpeed => circleSpeed;

        public Creature(float circleSpeed)
        {
            this.circleSpeed = circleSpeed;
            bodyParts = new List<BodyPart>();
        }

        protected Texture2D CreateCircleTexture(GraphicsDevice device, int radius, Color color)
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

        public void SetAllPositions(List<Vector2> newPositions)
        {
            int index = 0;
            foreach (var part in bodyParts)
            {
                for (int i = 0; i < part.Positions.Count && index < newPositions.Count; i++, index++)
                {
                    part.Positions[i] = newPositions[index];
                }
            }
        }

        public List<Vector2> GetAllPositions()
        {
            var positions = new List<Vector2>();
            foreach (var part in bodyParts)
            {
                positions.AddRange(part.Positions);
            }
            return positions;
        }
    }

    public class BodyPart
    {
        public List<Vector2> Positions { get; private set; }
        public List<int> Radii { get; private set; }
        public List<Texture2D> Textures { get; private set; }

        public BodyPart(List<Vector2> positions, List<Texture2D> textures, List<int> radii)
        {
            Positions = positions;
            Textures = textures;
            Radii = radii;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < Positions.Count; i++)
            {
                spriteBatch.Draw(Textures[i], Positions[i], null, Color.White, 0f, new Vector2(Radii[i], Radii[i]), 1f, SpriteEffects.None, 0f);
            }
        }

        public void UpdatePositions(float maxAngleChange)
        {
            for (int i = 1; i < Positions.Count; i++)
            {
                var direction = Positions[i - 1] - Positions[i];
                float length = direction.Length();

                if (length < float.Epsilon)
                {
                    length = 1;
                }

                direction.Normalize();
                float angle = (float)Math.Atan2(direction.Y, direction.X);

                if (i > 1)
                {
                    float prevAngle = (float)Math.Atan2(Positions[i - 2].Y - Positions[i - 1].Y, Positions[i - 2].X - Positions[i - 1].X);
                    float angleDiff = MathHelper.WrapAngle(angle - prevAngle);

                    if (Math.Abs(angleDiff) > maxAngleChange)
                    {
                        angle = prevAngle + Math.Sign(angleDiff) * maxAngleChange;
                    }
                }

                direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                Positions[i] = Positions[i - 1] - direction * Radii[i];
            }
        }
    }

    public class Lizard : Creature
    {
        public Lizard(GraphicsDevice graphicsDevice, Vector2 startPosition)
            : base(200f)
        {
            // Body
            List<Vector2> bodyPositions = new List<Vector2>
            {
                startPosition,
                startPosition - new Vector2(0, 25),
                startPosition - new Vector2(0, 50)
            };
            List<Texture2D> bodyTextures = new List<Texture2D>
            {
                CreateCircleTexture(graphicsDevice, 25, Color.Green),
                CreateCircleTexture(graphicsDevice, 25, Color.Green),
                CreateCircleTexture(graphicsDevice, 25, Color.Green)
            };
            List<int> bodyRadii = new List<int> { 25, 25, 25 };
            bodyParts.Add(new BodyPart(bodyPositions, bodyTextures, bodyRadii));

            // Tail
            List<Vector2> tailPositions = new List<Vector2>
            {
                startPosition - new Vector2(0, 75),
                startPosition - new Vector2(0, 100),
                startPosition - new Vector2(0, 125),
                startPosition - new Vector2(0, 150),
                startPosition - new Vector2(0, 175)
            };
            List<Texture2D> tailTextures = new List<Texture2D>
            {
                CreateCircleTexture(graphicsDevice, 20, Color.Green),
                CreateCircleTexture(graphicsDevice, 18, Color.Green),
                CreateCircleTexture(graphicsDevice, 16, Color.Green),
                CreateCircleTexture(graphicsDevice, 14, Color.Green),
                CreateCircleTexture(graphicsDevice, 12, Color.Green)
            };
            List<int> tailRadii = new List<int> { 20, 18, 16, 14, 12 };
            bodyParts.Add(new BodyPart(tailPositions, tailTextures, tailRadii));
        }

        public override void Update(GameTime gameTime, Vector2 targetPosition)
        {
            if (float.IsNaN(targetPosition.X) || float.IsNaN(targetPosition.Y))
            {
                targetPosition = HeadPosition;
            }

            Vector2 direction = targetPosition - HeadPosition;
            direction.Normalize();

            bodyParts[0].Positions[0] += direction * CircleSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update body positions
            bodyParts[0].UpdatePositions(MathHelper.ToRadians(60f));

            // Update tail positions, starting from the last position of the body
            Vector2 lastBodySegment = bodyParts[0].Positions[^1];
            Vector2 secondLastBodySegment = bodyParts[0].Positions[^2];
            Vector2 bodyDirection = lastBodySegment - secondLastBodySegment;
            bodyDirection.Normalize();
            Vector2 tailStartPosition = lastBodySegment + bodyDirection * bodyParts[1].Radii[0];

            bodyParts[1].Positions[0] = tailStartPosition;
            for (int i = 1; i < bodyParts[1].Positions.Count; i++)
            {
                Vector2 prevPosition = bodyParts[1].Positions[i - 1];
                Vector2 currentPosition = bodyParts[1].Positions[i];
                Vector2 tailDirection = prevPosition - currentPosition;
                float tailLength = tailDirection.Length();

                if (tailLength < float.Epsilon)
                {
                    tailLength = 1;
                }

                tailDirection.Normalize();
                float tailAngle = (float)Math.Atan2(tailDirection.Y, tailDirection.X);

                if (i > 1)
                {
                    float prevTailAngle = (float)Math.Atan2(bodyParts[1].Positions[i - 2].Y - bodyParts[1].Positions[i - 1].Y, bodyParts[1].Positions[i - 2].X - bodyParts[1].Positions[i - 1].X);
                    float tailAngleDiff = MathHelper.WrapAngle(tailAngle - prevTailAngle);

                    if (Math.Abs(tailAngleDiff) > MathHelper.ToRadians(60f))
                    {
                        tailAngle = prevTailAngle + Math.Sign(tailAngleDiff) * MathHelper.ToRadians(60f);
                    }
                }

                tailDirection = new Vector2((float)Math.Cos(tailAngle), (float)Math.Sin(tailAngle));
                bodyParts[1].Positions[i] = prevPosition - tailDirection * bodyParts[1].Radii[i];
            }
        }
    }

    public class Snake : Creature
    {
        public Snake(GraphicsDevice graphicsDevice, Vector2 startPosition)
            : base(200f)
        {
            List<Vector2> positions = new List<Vector2>
            {
                startPosition,
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
                CreateCircleTexture(graphicsDevice, 25, Color.Red),
                CreateCircleTexture(graphicsDevice, 25, Color.Red)
            };
            List<int> radii = new List<int> { 25, 25, 25, 25, 25, 25 };
            bodyParts.Add(new BodyPart(positions, textures, radii));
        }

        public override void Update(GameTime gameTime, Vector2 targetPosition)
        {
            if (float.IsNaN(targetPosition.X) || float.IsNaN(targetPosition.Y))
            {
                targetPosition = HeadPosition;
            }

            Vector2 direction = targetPosition - HeadPosition;
            direction.Normalize();

            bodyParts[0].Positions[0] += direction * CircleSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            foreach (var part in bodyParts)
            {
                part.UpdatePositions(MathHelper.ToRadians(40f));
            }
        }
    }
}