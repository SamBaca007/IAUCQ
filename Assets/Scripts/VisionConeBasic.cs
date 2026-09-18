using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Código inspirado por: https://youtu.be/XrZg2rQtkDA
public class VisionConeBasic : MonoBehaviour
{
    [Range(0.0f, 360.0f)]
    public float VisionAngle = 90.0f;
    public float VisionDistance = 2.0f;

    private Vector3 PointForAngle(float angle)
    {
        // Información sobre Vector2 se consiguió de: https://docs.unity3d.com/es/530/ScriptReference/Vector2.html
        Vector2 ret = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad) * VisionDistance,
            0.0f,
            Mathf.Sin(angle * Mathf.Deg2Rad) * VisionDistance);

        return transform.TransformDirection(ret);
    }
    public void OnDrawGizmos()
    {
        if (VisionAngle <= 0.0f) return;
        float halfVisionAngle = VisionAngle * 0.5f;

        // Información sobre Vector3 se consiguió de: https://discussions.unity.com/t/vector3-explanations/841426
        Vector3 p1, p2;
        p1 = PointForAngle(halfVisionAngle);
        p2 = PointForAngle(-halfVisionAngle);

        Gizmos.DrawLine(transform.position, p1);
        Gizmos.DrawLine(transform.position, p2);
    }

}