using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
}