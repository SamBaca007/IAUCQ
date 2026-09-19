using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Código inspirado por: https://youtu.be/XrZg2rQtkDA
// Creación de código asistida por Google Gemini. Conversación adjuntada con el repositorio.
public class VisionConeBasic : MonoBehaviour
{
    [Range(0.0f, 360.0f)]
    public float VisionAngle = 90.0f;
    public float VisionDistance = 2.0f;
    public Transform Target;
    public bool TargetOnSight = false;

    private Vector3 PointForAngle(float angle)
    {
        // Información sobre Vector2 se consiguió de: https://docs.unity3d.com/es/530/ScriptReference/Vector2.html
        Vector2 ret = new Vector3(
            Mathf.Sin(angle * Mathf.Deg2Rad) * VisionDistance,
            0.0f,
            Mathf.Cos(angle * Mathf.Deg2Rad) * VisionDistance);

        return transform.TransformDirection(ret);
    }
    public void OnDrawGizmos()
    {
        if (VisionAngle <= 0.0f) return;

        if (Target != null)
        {
            Vector3 vectorToTarget = Target.position - transform.position;
            bool onDistance = vectorToTarget.magnitude <= VisionDistance;

            float angleToTarget = Vector3.Angle(transform.forward, vectorToTarget);
            bool onAngle = angleToTarget <= (VisionAngle * 0.5f);

            if(onDistance && onAngle)
            {
                TargetOnSight = true;
            }
            else
            {
                TargetOnSight = false;
            }

            if(TargetOnSight)
            {
                Gizmos.color = Color.red;
                Debug.Log(message: "Objetivo detectado. Cono color rojo.");
            }
            else
            {
                Gizmos.color = Color.green;
                Debug.Log(message: "Objetivo fuera de alcance. Cono color verde.");
            }
        }

        float halfVisionAngle = VisionAngle * 0.5f;

        // Información sobre Vector3 se consiguió de: https://discussions.unity.com/t/vector3-explanations/841426
        Vector3 p1, p2;
        p1 = PointForAngle(halfVisionAngle);
        p2 = PointForAngle(-halfVisionAngle);

        Gizmos.DrawLine(transform.position, transform.position + p1);
        Gizmos.DrawLine(transform.position, transform.position + p2);
    }

}