using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace SimpleGame
{
   public class BodyPart
{
    public List<Vector2> Positions { get; set; }
    public List<Texture2D> Textures { get; set; }
    public List<int> Radii { get; set; }
    public string Name { get; set; }
    public BodyPart PreviousPart { get; set; }

    public BodyPart(List<Vector2> positions, List<Texture2D> textures, List<int> radii, string name)
    {
        Positions = positions;
        Textures = textures;
        Radii = radii;
        Name = name;
    }

    public void Draw(SpriteBatch spriteBatch, float rotationAngle = 0f, bool isFacingRight = true)
    {
        for (int i = 0; i < Positions.Count; i++)
        {
            var texture = Textures[i];
            var position = Positions[i];
            var radius = Radii[i];

            // Calculate the origin to center the texture
            var origin = new Vector2(texture.Width / 2f, texture.Height / 2f);

            // Scale the texture if necessary
            var scale = (float)radius * 2 / texture.Width;

            // Determine the sprite effects based on the facing direction
            SpriteEffects spriteEffects = isFacingRight ? SpriteEffects.FlipVertically : SpriteEffects.None;

            // Apply rotation and sprite effects for the head (first part)
            if (Name == "Head")
            {
                spriteBatch.Draw(texture, position, null, Color.White, rotationAngle, origin, scale, spriteEffects, 0f);
            }
            else
            {
                spriteBatch.Draw(texture, position, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }
    }
}
}