using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SimpleGame
{
    public class LizardSpine
    {
        public List<BodyPart> SpineParts { get; set; }
        public List<Vector2> allPositions;
        public List<int> allRadii;

        private Texture2D headTexture;

        public LizardSpine(GraphicsDevice graphicsDevice, ContentManager content, Vector2 startPosition)
        {
            allPositions = new List<Vector2>();
            allRadii = new List<int>();

            

            var spineCol = Color.Cyan;

            // Initialize SpineParts list
            SpineParts = new List<BodyPart>();


List<Texture2D> customTextures = new List<Texture2D>
{
    content.Load<Texture2D>("head_50")
};
            // Head initialization
            List<Vector2> headPos = new List<Vector2> { startPosition };
            List<int> headRadii = new List<int> { 60 };
            var head = new BodyPart(headPos, headRadii, "Head", graphicsDevice, spineCol, customTextures);
            SpineParts.Add(head);
            
            allPositions.AddRange(headPos);
            allRadii.AddRange(headRadii);

            // Body initialization
            List<Vector2> bodyPositions = new List<Vector2>
            {
                startPosition - new Vector2(0, 25),
                startPosition - new Vector2(0, 45),
                startPosition - new Vector2(0, 70),
                startPosition - new Vector2(0, 95),
            };
            List<int> bodyRadii = new List<int> { 20, 25, 25, 25 };
            var body = new BodyPart(bodyPositions, bodyRadii, "Body", graphicsDevice, spineCol) { PreviousPart = head };
            SpineParts.Add(body);
            allPositions.AddRange(bodyPositions);
            allRadii.AddRange(bodyRadii);

            // Tail initialization
            List<Vector2> tailPositions = new List<Vector2>
            {
                startPosition - new Vector2(0, 75),
                startPosition - new Vector2(0, 100),
                startPosition - new Vector2(0, 125),
                startPosition - new Vector2(0, 150),
                startPosition - new Vector2(0, 175),
                startPosition - new Vector2(0, 200),
                startPosition - new Vector2(0, 225),
                startPosition - new Vector2(0, 230),
                startPosition - new Vector2(0, 240),
                startPosition - new Vector2(0, 250),
            };
            List<int> tailRadii = new List<int> { 20, 18, 16, 14, 12, 10, 8, 6, 4, 2 };
            var tail = new BodyPart(tailPositions, tailRadii, "Tail", graphicsDevice, spineCol) { PreviousPart = body };
            SpineParts.Add(tail);
            allPositions.AddRange(tailPositions);
            allRadii.AddRange(tailRadii);
        }

  



        
}
}