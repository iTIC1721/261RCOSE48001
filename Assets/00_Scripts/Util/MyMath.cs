using UnityEngine;

public static class MyMath
{
    public static Vector2 Bezier(Vector2 P0, Vector2 P1, Vector2 P2, Vector2 P3, float t)
    {
        Vector2 M0 = Vector2.Lerp(P0, P1, t);
        Vector2 M1 = Vector2.Lerp(P1, P2, t);
        Vector2 M2 = Vector2.Lerp(P2, P3, t);

        Vector2 B0 = Vector2.Lerp(M0, M1, t);
        Vector2 B1 = Vector2.Lerp(M1, M2, t);

        return Vector2.Lerp(B0, B1, t);
    }

    // Ease In (점점 빨라짐)
    public static float EaseIn(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t;
    }

    // Ease Out (점점 느려짐)
    public static float EaseOut(float t)
    {
        t = Mathf.Clamp01(t);
        return 1f - (1f - t) * (1f - t);
    }

    // Ease In-Out (처음/끝 느리고 중간 빠름)
    public static float EaseInOut(float t)
    {
        t = Mathf.Clamp01(t);
        if (t < 0.5f)
        {
            return 2f * t * t;
        }
        else
        {
            return 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        }
    }
}
