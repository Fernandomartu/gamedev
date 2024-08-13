using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace SimpleGame
{
  public class LizardArm
{
    public List<Vector2> Positions { get; set; }
    public List<Texture2D> Textures { get; set; }
    public List<int> Radii { get; set; }
    public string Name { get; set; }
    public LizardArm PreviousPart { get; set; }

    public LizardArm(List<Vector2> positions, List<Texture2D> textures, List<int> radii, string name)
    {
        Positions = positions;
        Textures = textures;
        Radii = radii;
        Name = name;
    }

   public void Draw()
{
 
}
}
}