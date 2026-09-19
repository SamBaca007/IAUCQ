using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class VisionCone : MonoBehaviour
{
    [Range(0.0f, 360.0f)]
    public float VisionAngle = 90.0f;
    public float VisionDistance = 5.0f;

    [Header("Configuración de Movimiento")]
    public float maxSpeed = 3.0f;

    private SphereCollider _collider;
    public bool TargetOnSight = false;

    void Start()
    {
        _collider = GetComponent<SphereCollider>();
        _collider.isTrigger = true;
        _collider.radius = VisionDistance;
    }

    void Update()
    {
        TargetOnSight = false;
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.name == "Target")
        {
            Vector3 vectorToTarget = other.transform.position - transform.position;

            float angleToTarget = Vector3.Angle(transform.forward, vectorToTarget);

            if (angleToTarget <= (VisionAngle * 0.5f))
            {
                TargetOnSight = true;
                Vector3 puntaMenosColaNormalizado = vectorToTarget.normalized;
                transform.position += puntaMenosColaNormalizado * (Time.deltaTime * maxSpeed);
                transform.forward = puntaMenosColaNormalizado;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (VisionAngle <= 0.0f) return;

        if (TargetOnSight)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.green;
        }

        float halfVisionAngle = VisionAngle * 0.5f;

        Vector3 p1 = PointForAngle(halfVisionAngle);
        Vector3 p2 = PointForAngle(-halfVisionAngle);

        Gizmos.DrawLine(transform.position, transform.position + p1);
        Gizmos.DrawLine(transform.position, transform.position + p2);

        Gizmos.DrawWireSphere(transform.position, VisionDistance);
    }

    private Vector3 PointForAngle(float angle)
    {
        // Información sobre eulerAngles se consiguió de: https://docs.unity3d.com/es/530/ScriptReference/Transform-eulerAngles.html
        angle += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad)) * VisionDistance;
    }
}