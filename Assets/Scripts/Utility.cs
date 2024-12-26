using UnityEngine;

public static class Utility
{
    public static Vector3 UnitCirclePosition()
    {
        float circleAngle = Random.Range(0.0f, 360f);
        float x = Mathf.Cos(circleAngle);
        float z = Mathf.Sin(circleAngle);
        const float radius = 5f;
        Vector3 unitCirclePosition = new Vector3(x, 0, z) * radius;
        return unitCirclePosition;
    }
}