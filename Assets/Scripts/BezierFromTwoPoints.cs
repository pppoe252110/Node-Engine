using Radishmouse;
using Unity.Mathematics;
using UnityEngine;

public static class BezierFromTwoPoints
{
    public static Vector2[] GetPoints(Vector2 a, Vector2 b, float h, int pointsCount)
    {
        var points = new Vector2[pointsCount];
        for (int i = 0; i < pointsCount; i++)
        {
            float t = ((float)i) / (pointsCount - 1);
            var aDir = (b - a) * h;
            aDir.y = 0;
            var bDir = (a - b) * h;
            bDir.y = 0;
            points[i] = Bezier(a, a + aDir, b + bDir, b, t);
        }
        return points;
    }

    static Vector2 Bezier(Vector2 a, Vector2 b, float t)
    {
        return Vector2.Lerp(a, b, t);
    }

    static Vector2 Bezier(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        return Vector2.Lerp(Bezier(a, b, t), Bezier(b, c, t), t);
    }

    static Vector2 Bezier(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
    {
        return Vector2.Lerp(Bezier(a, b, c, t), Bezier(b, c, d, t), t);
    }
}
