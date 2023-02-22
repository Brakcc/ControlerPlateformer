using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Physics
{
    public class RayRange
    {
        public readonly Vector2 Start, End, Dir;

        public RayRange(Vector2 a, Vector2 b, Vector2 dir)
        {
            Start = a;
            End = b;
            Dir = dir;
        }

        public RayRange(Vector2 centre, float size)
        {
            Start = (centre - Vector2.left * (size/2));
            End = (centre + Vector2.right * (size/2));
            Dir = Vector2.Perpendicular(End - Start).normalized;
        }

        public IEnumerable<Vector2> EvaluateRayPositions(int rayCount)
        {
            for (int i =0; i < rayCount; i++)
            {
                float t = (float)i / (rayCount - 1);
                yield return Vector2.Lerp(Start, End, t);
            }
        }
    }
}