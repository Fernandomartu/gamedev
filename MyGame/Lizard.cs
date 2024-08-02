using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SimpleGame
{
    public class Lizard : Creature
    {
        private List<Vector2> allPositions;
        private List<int> allRadii;
        private Vector2 direction;

        public Lizard(GraphicsDevice graphicsDevice, Vector2 startPosition)
            : base(200f)
        {
            // Initialize allPositions and allRadii
            allPositions = new List<Vector2>();
            allRadii = new List<int>();
            var LizardCol = Color.Transparent;

            // Head
            List<Vector2> headPos = new List<Vector2> { startPosition };
            List<Texture2D> headTextures = new List<Texture2D> { CreateCircleTexture(graphicsDevice, 20, LizardCol) };
            List<int> headRadii = new List<int> { 20 };
            var head = new BodyPart(headPos, headTextures, headRadii, "Head");
            bodyParts.Add(head);
            allPositions.AddRange(headPos);
            allRadii.AddRange(headRadii);

            // Body
            List<Vector2> bodyPositions = new List<Vector2>
            {
                startPosition - new Vector2(0, 25),
                startPosition - new Vector2(0, 50)
            };
            List<Texture2D> bodyTextures = new List<Texture2D>
            {
                CreateCircleTexture(graphicsDevice, 25, LizardCol),
                CreateCircleTexture(graphicsDevice, 25, LizardCol)
            };
            List<int> bodyRadii = new List<int> { 25, 25 };
            var body = new BodyPart(bodyPositions, bodyTextures, bodyRadii, "Body") { PreviousPart = head };
            bodyParts.Add(body);
            allPositions.AddRange(bodyPositions);
            allRadii.AddRange(bodyRadii);

            // Tail
            List<Vector2> tailPositions = new List<Vector2>
            {
                startPosition - new Vector2(0, 75),
                startPosition - new Vector2(0, 100),
                startPosition - new Vector2(0, 125),
                startPosition - new Vector2(0, 150),
                startPosition - new Vector2(0, 175),
                startPosition - new Vector2(0, 200),
                startPosition - new Vector2(0, 225)
            };
            List<Texture2D> tailTextures = new List<Texture2D>
            {
                CreateCircleTexture(graphicsDevice, 20, LizardCol),
                CreateCircleTexture(graphicsDevice, 18, LizardCol),
                CreateCircleTexture(graphicsDevice, 16, LizardCol),
                CreateCircleTexture(graphicsDevice, 14, LizardCol),
                CreateCircleTexture(graphicsDevice, 12, LizardCol),
                CreateCircleTexture(graphicsDevice, 10, LizardCol),
                CreateCircleTexture(graphicsDevice, 8, LizardCol)
            };
            List<int> tailRadii = new List<int> { 20, 18, 16, 14, 12, 10, 8 };
            var tail = new BodyPart(tailPositions, tailTextures, tailRadii, "Tail") { PreviousPart = body };
            bodyParts.Add(tail);
            allPositions.AddRange(tailPositions);
            allRadii.AddRange(tailRadii);

            direction = Vector2.UnitX; // Initial direction
        }

        public override void Update(GameTime gameTime, Vector2 targetPosition)
        {
            if (float.IsNaN(targetPosition.X) || float.IsNaN(targetPosition.Y))
            {
                targetPosition = HeadPosition;
            }

            direction = targetPosition - HeadPosition;
            direction.Normalize();

            allPositions[0] += direction * CircleSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Debug: Print head position to check movement
            Console.WriteLine($"Head Position: {allPositions[0]}");

            UpdatePositions(MathHelper.ToRadians(70f)); // Adjust the angle as needed
        }

       private void UpdatePositions(float maxAngleChange)
{
    for (int i = 1; i < allPositions.Count; i++)
    {
        Vector2 direction;
        if (i == 1)
        {
            // Calculate the desired position of the first body part relative to the head's direction
            float headAngle = (float)Math.Atan2(this.direction.Y, this.direction.X) + MathHelper.Pi; // Directly behind the head
            Vector2 desiredPosition = allPositions[0] + new Vector2((float)Math.Cos(headAngle), (float)Math.Sin(headAngle)) * allRadii[i]/2;

            // Smoothly move towards the desired position
            allPositions[i] = Vector2.Lerp(allPositions[i], desiredPosition, 0.095f); // Adjust the interpolation factor as needed
        }
        else
        {
            direction = allPositions[i - 1] - allPositions[i];

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

public void DrawOutline(SpriteBatch spriteBatch)
{
    Vector2 headPosition = allPositions[0];
    int headRadius = allRadii[0];
    float angle = (float)Math.Atan2(direction.Y, direction.X);

    // Calculate head points
    Vector2 frontPoint = GetPointOnCircumference(headPosition, headRadius, angle);
    Vector2 leftPoint = GetPointOnCircumference(headPosition, headRadius, angle - MathHelper.PiOver2);
    Vector2 rightPoint = GetPointOnCircumference(headPosition, headRadius, angle + MathHelper.PiOver2);
    Vector2 leftMidPoint = GetPointOnCircumference(headPosition, headRadius, angle - MathHelper.PiOver4);
    Vector2 rightMidPoint = GetPointOnCircumference(headPosition, headRadius, angle + MathHelper.PiOver4);

    // Define the array for all points, adding additional points for a more rounded shape
    Vector2[] allPoints = new Vector2[(allPositions.Count - 1) * 2 + 7]; // Adjusted array size

    // Add head points
    allPoints[0] = frontPoint;
    allPoints[1] = leftMidPoint;
    allPoints[2] = leftPoint;

    // Calculate left points around each body part and add them to the array
    int index = 3;
    for (int i = 1; i < allPositions.Count; i++)
    {
        float partAngle = (float)Math.Atan2(allPositions[i - 1].Y - allPositions[i].Y, allPositions[i - 1].X - allPositions[i].X);
        allPoints[index++] = GetPointOnCircumference(allPositions[i], allRadii[i], partAngle - MathHelper.PiOver2);
    }

    // Add the tip of the tail with corrected angle
    Vector2 tailTipPosition = allPositions[allPositions.Count - 1];
    int tailTipRadius = allRadii[allRadii.Count - 1];
    float tailAngle = (float)Math.Atan2(allPositions[allPositions.Count - 2].Y - tailTipPosition.Y, allPositions[allPositions.Count - 2].X - tailTipPosition.X);
    Vector2 tailTip = GetPointOnCircumference(tailTipPosition, tailTipRadius, tailAngle + MathHelper.Pi);
    allPoints[index++] = tailTip;

    // Calculate right points around each body part and add them to the array
    for (int i = allPositions.Count - 1; i > 0; i--)
    {
        float partAngle = (float)Math.Atan2(allPositions[i - 1].Y - allPositions[i].Y, allPositions[i - 1].X - allPositions[i].X);
        allPoints[index++] = GetPointOnCircumference(allPositions[i], allRadii[i], partAngle + MathHelper.PiOver2);
    }

    // Add the right point of the head, right mid-point, and another front point to the end of the array
    allPoints[index++] = rightPoint;
    allPoints[index++] = rightMidPoint;
    allPoints[index] = frontPoint;

    Texture2D pointTexture = CreateCircleTexture(spriteBatch.GraphicsDevice, 2, Color.Red);

    // Draw all points
    foreach (var point in allPoints)
    {
        spriteBatch.Draw(pointTexture, point, Color.White);
    }

    // Draw lines between the points
    for (int i = 0; i < allPoints.Length - 1; i++)
    {
        if (allPoints[i] != Vector2.Zero && allPoints[i + 1] != Vector2.Zero) // Ensure no (0,0) points are drawn
        {
            DrawLine(spriteBatch, pointTexture, allPoints[i], allPoints[i + 1], Color.White, 1);
        }
    }

    // Connect the last point to the first point to close the polygon
    if (allPoints[allPoints.Length - 1] != Vector2.Zero && allPoints[0] != Vector2.Zero) // Ensure no (0,0) points are drawn
    {
        DrawLine(spriteBatch, pointTexture, allPoints[allPoints.Length - 1], allPoints[0], Color.White, 1);
    }
}

private Vector2 GetPointOnCircumference(Vector2 center, int radius, float angle)
{
    return new Vector2(
        center.X + radius * (float)Math.Cos(angle),
        center.Y + radius * (float)Math.Sin(angle)
    );
}

private void DrawLine(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 end, Color color, int thickness)
{
    Vector2 edge = end - start;
    float angle = (float)Math.Atan2(edge.Y, edge.X);

    spriteBatch.Draw(texture,
        new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), thickness),
        null,
        color,
        angle,
        new Vector2(0, 0.5f),
        SpriteEffects.None,
        0);
}
    }
}