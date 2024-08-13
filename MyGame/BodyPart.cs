using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Serilog;

namespace SimpleGame
{
    public class BodyPart
    {
        public List<Vector2> Positions { get; set; }
        public List<Texture2D> Textures { get; set; }
        public List<int> Radii { get; set; }
        public string Name { get; set; }
        public BodyPart PreviousPart { get; set; }

        // Modified constructor to accept an optional list of textures
        public BodyPart(List<Vector2> positions, List<int> radii, string name, GraphicsDevice graphicsDevice, Color color, List<Texture2D> customTextures = null)
        {
            Positions = positions;
            Radii = radii;
            Name = name;
            Textures = new List<Texture2D>();

            // Use custom textures if provided, otherwise create circle textures
            if (customTextures != null && customTextures.Count == positions.Count)
            {
                Textures = customTextures;
            }
            else
            {
                CreateCircleTextures(graphicsDevice, color);
            }
        }

        public void Draw(SpriteBatch spriteBatch, float rotationAngle = 0f, bool isFacingRight = true, Texture2D eyeballTexture = null, Vector2 eyeballOffset = default(Vector2))
        {
            for (int i = 0; i < Positions.Count; i++)
            {
                Log.Information("Drawing lizard at position: {Position}", Positions[i]);

                var texture = Textures[i];
                var position = Positions[i];
                var radius = Radii[i];

                // Calculate the origin to center the texture
                var origin = new Vector2(texture.Width / 2f, texture.Height / 2f);

                // Scale the texture if necessary
                var scale = (float)radius * 2 / texture.Width;

                // Determine the sprite effects based on the facing direction
                SpriteEffects spriteEffects = isFacingRight ? SpriteEffects.None : SpriteEffects.FlipVertically;

                // Apply rotation and sprite effects for the head (first part)
                if (Name == "Head")
                {
                    spriteBatch.Draw(texture, position, null, Color.White, rotationAngle, origin, scale, spriteEffects, 0f);

                    // Draw the eyeball
                    if (eyeballTexture != null)
                    {
                        // Flip the eyeball offset on the X-axis if the head is facing left
                        Vector2 adjustedEyeballOffset = isFacingRight ? eyeballOffset : new Vector2(eyeballOffset.X, -eyeballOffset.Y);

                        // Calculate the eyeball position based on the head's rotation
                        Vector2 rotatedEyeballOffset = Vector2.Transform(adjustedEyeballOffset, Matrix.CreateRotationZ(rotationAngle));
                        Vector2 eyeballPosition = position + rotatedEyeballOffset;

                        // Draw the eyeball texture
                        spriteBatch.Draw(eyeballTexture, eyeballPosition, null, Color.White, rotationAngle, new Vector2(eyeballTexture.Width / 2f, eyeballTexture.Height / 2f), scale, spriteEffects, 0f);
                    }
                }
                else
                {
                    spriteBatch.Draw(texture, position, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
                }
            }
        }

        protected void CreateCircleTextures(GraphicsDevice device, Color color)
        {
            Textures.Clear();
            foreach (var radius in Radii)
            {
                Textures.Add(CreateCircleTexture(device, radius, color));
            }
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
    }
}