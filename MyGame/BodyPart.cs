using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace SimpleGame
{
    public class BodyPart
    {
        public List<Vector2> Positions { get; private set; }
        public List<int> Radii { get; private set; }
        public List<Texture2D> Textures { get; private set; }
        public BodyPart PreviousPart { get; set; }
        public string Name { get; private set; }

        public BodyPart(List<Vector2> positions, List<Texture2D> textures, List<int> radii, string name)
        {
            Positions = positions;
            Textures = textures;
            Radii = radii;
            Name = name;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < Positions.Count; i++)
            {
                spriteBatch.Draw(Textures[i], Positions[i], null, Color.White, 0f, new Vector2(Radii[i], Radii[i]), 1f, SpriteEffects.None, 0f);
            }
        }
    }
}