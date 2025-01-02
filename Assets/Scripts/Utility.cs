using UnityEngine;

public static class Utility
{
    public static Vector3 RandomOnCircle(float radius = 1f)
    {
        float circleAngle = Random.Range(0.0f, 360f);
        float x = Mathf.Cos(circleAngle);
        float z = Mathf.Sin(circleAngle);
        Vector3 unitCirclePosition = new Vector3(x, 0, z) * radius;
        return unitCirclePosition;
    }

    public static Vector3 RandomInCircle(float radius = 1f)
    {
        return RandomOnCircle(Random.Range(0f, radius));
    }
}