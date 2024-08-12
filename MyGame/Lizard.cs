using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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
        private Vector2 legOneTarget;
        private Vector2 legOneFoot;
        private Vector2 legOneJoint;
        private float legMoveThreshold = 20f; 
        private Texture2D legTexture;
        private int legRadius = 10;
        private Texture2D headTexture;
        private float headRotationAngle;
        private bool isFacingRight;

        private Vector2 eyeballOffset = new Vector2(10, -13); // Example offset, adjust as needed
    private Texture2D eyeballTexture;

        public Lizard(GraphicsDevice graphicsDevice, ContentManager content, Vector2 startPosition)
            : base(200f)
        {
            allPositions = new List<Vector2>();
            allRadii = new List<int>();
            var LizardCol = Color.Cyan;

            // Load the head texture
            headTexture = content.Load<Texture2D>("head_50");
            eyeballTexture = content.Load<Texture2D>("LizardEye");

            // Initialize head with the texture
            List<Vector2> headPos = new List<Vector2> { startPosition };
            List<Texture2D> headTextures = new List<Texture2D> { headTexture };
            List<int> headRadii = new List<int> { 60 };
            var head = new BodyPart(headPos, headTextures, headRadii, "Head");
            bodyParts.Add(head);
            allPositions.AddRange(headPos);
            allRadii.AddRange(headRadii);

            // Initialize other parts of the lizard (body and tail) as before

            // Body initialization
            List<Vector2> bodyPositions = new List<Vector2>
            {
                startPosition - new Vector2(0, 25),
                startPosition - new Vector2(0, 45),
                startPosition - new Vector2(0, 70),
                startPosition - new Vector2(0, 95),
            };
            List<Texture2D> bodyTextures = new List<Texture2D>
            {
                CreateCircleTexture(graphicsDevice, 20, LizardCol),
                CreateCircleTexture(graphicsDevice, 25, LizardCol),
                CreateCircleTexture(graphicsDevice, 25, LizardCol),
                CreateCircleTexture(graphicsDevice, 25, LizardCol),
            };
            List<int> bodyRadii = new List<int> { 20, 25, 25, 25 };
            var body = new BodyPart(bodyPositions, bodyTextures, bodyRadii, "Body") { PreviousPart = head };
            bodyParts.Add(body);
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
            List<Texture2D> tailTextures = new List<Texture2D>
            {
                CreateCircleTexture(graphicsDevice, 20, LizardCol),
                CreateCircleTexture(graphicsDevice, 18, LizardCol),
                CreateCircleTexture(graphicsDevice, 16, LizardCol),
                CreateCircleTexture(graphicsDevice, 14, LizardCol),
                CreateCircleTexture(graphicsDevice, 12, LizardCol),
                CreateCircleTexture(graphicsDevice, 10, LizardCol),
                CreateCircleTexture(graphicsDevice, 8, LizardCol),
                CreateCircleTexture(graphicsDevice, 6, LizardCol),
                CreateCircleTexture(graphicsDevice, 4, LizardCol),
                CreateCircleTexture(graphicsDevice, 2, LizardCol),
            };
            List<int> tailRadii = new List<int> { 20, 18, 16, 14, 12, 10, 8, 6, 4, 2 };
            var tail = new BodyPart(tailPositions, tailTextures, tailRadii, "Tail") { PreviousPart = body };
            bodyParts.Add(tail);
            allPositions.AddRange(tailPositions);
            allRadii.AddRange(tailRadii);

            // Leg initialization
            float initialBodyAngle = (float)Math.Atan2(allPositions[0].Y - allPositions[1].Y, allPositions[0].X - allPositions[1].X);
            float legAngleOffset = MathHelper.PiOver4; 
            float legDistance = 60; 

            legOneTarget = allPositions[1] + new Vector2((float)Math.Cos(initialBodyAngle + legAngleOffset), (float)Math.Sin(initialBodyAngle + legAngleOffset)) * legDistance;
            legOneFoot = legOneTarget;
            legOneJoint = allPositions[1] + (legOneFoot - allPositions[1]) * 0.6f; 

            this.legTexture = CreateCircleTexture(graphicsDevice, legRadius, Color.Green);

            direction = Vector2.UnitX; 
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

            // Update the head rotation angle based on the direction
            UpdateHeadRotation(direction);

            // Determine if the lizard is facing right
            isFacingRight = direction.X > 0;

            // Update body positions
            UpdatePositions(MathHelper.ToRadians(40f)); // Adjust the angle as needed
            UpdateLegOne(); // Update leg position
        }

        private void UpdatePositions(float maxAngleChange)
        {
            float interpolationSpeed = 0.1f; 

            for (int i = 1; i < allPositions.Count; i++)
            {
                Vector2 direction;
                if (i == 1)
                {
                    // Update position for the first body part
                    float currentAngle = (float)Math.Atan2(allPositions[i].Y - allPositions[0].Y, allPositions[i].X - allPositions[0].X);
                    float desiredAngle = (float)Math.Atan2(this.direction.Y, this.direction.X) + MathHelper.Pi;
                    float newAngle = LerpAngle(currentAngle, desiredAngle, interpolationSpeed);
                    Vector2 desiredPosition = allPositions[0] + new Vector2((float)Math.Cos(newAngle), (float)Math.Sin(newAngle)) * allRadii[i - 1]/2;
                    allPositions[i] = desiredPosition;
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
                    allPositions[i] = allPositions[i - 1] - direction * allRadii[i - 1];
                }
            }

            int index = 0;
            foreach (var part in bodyParts)
            {
                for (int i = 0; i < part.Positions.Count; i++, index++)
                {
                    part.Positions[i] = allPositions[index];
                }
            }
        }

        private float LerpAngle(float from, float to, float t)
        {
            float difference = MathHelper.WrapAngle(to - from);
            if (difference < -MathHelper.Pi)
            {
                difference += MathHelper.TwoPi;
            }
            else if (difference > MathHelper.Pi)
            {
                difference -= MathHelper.TwoPi;
            }
            return from + difference * t;
        }

        private void UpdateLegOne()
        {
            Vector2 anchor = allPositions[1];
            Vector2 toTarget = legOneTarget - legOneFoot;
            if (toTarget.Length() > legMoveThreshold)
            {
                legOneFoot = legOneTarget;
            }
            float bodyAngle = (float)Math.Atan2(allPositions[0].Y - allPositions[1].Y, allPositions[0].X - allPositions[1].X);
            float legAngleOffset = MathHelper.PiOver4; 
            float legDistance = 50;
            legOneTarget = anchor + new Vector2((float)Math.Cos(bodyAngle + legAngleOffset), (float)Math.Sin(bodyAngle + legAngleOffset)) * legDistance;
            Vector2 toFoot = legOneFoot - anchor;
            float footAngle = (float)Math.Atan2(toFoot.Y, toFoot.X);
            float jointAngle = footAngle + MathHelper.PiOver4; 
            float jointDistance = 30f; 
            legOneJoint = anchor + new Vector2((float)Math.Cos(jointAngle), (float)Math.Sin(jointAngle)) * jointDistance;
            legOneFoot = Vector2.Lerp(legOneFoot, legOneTarget, 0.1f); 
        }

        public bool IsFacingRight => isFacingRight; // Property to expose the facing direction
         public Texture2D EyeballTexture => eyeballTexture;
    public Vector2 EyeballOffset => eyeballOffset;
    }
}