using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;

namespace SimpleGame
{
    public abstract class Creature
{
    public List<BodyPart> bodyParts;
    protected float circleSpeed;

    protected float headRotationAngle; // Field to store the head rotation angle

    public Vector2 HeadPosition => bodyParts[0].Positions[0];
    public float CircleSpeed => circleSpeed;

    // Add this property to expose the head rotation angle
    public float HeadRotationAngle => headRotationAngle;

    public Creature(float circleSpeed)
    {
        this.circleSpeed = circleSpeed;
        bodyParts = new List<BodyPart>();
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

    protected void UpdateHeadRotation(Vector2 direction)
    {
        if (direction != Vector2.Zero)
        {
            headRotationAngle = (float)Math.Atan2(direction.Y, direction.X);
        }
    }
}
}