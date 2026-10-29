using UnityEngine;
using UnityEngine.AI;
using System.Collections; // Necesario para las corrutinas

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(NavMeshAgent))] // Asegura que el enemigo tenga un NavMeshAgent
public class VisionCone : MonoBehaviour
{
    [Range(0.0f, 360.0f)]
    public float VisionAngle = 90.0f;
    public float VisionDistance = 5.0f;

    [Header("Configuración de Tiempos")]
    [Tooltip("Tiempo en segundos que el agente perseguirá al objetivo.")]
    public float PursuitTime = 3f;
    [Tooltip("Tiempo de espera antes de regresar a la posición inicial.")]
    public float TimeBeforeReturn = 1f;

    [Header("Configuración de Detección")]
    [Tooltip("Tag del objeto que el enemigo debe perseguir/destruir.")]
    public string TargetTag = "Player";

    [Header("Objetivo a perseguir")]
    public Transform target; // Arrastra aquí a tu Player o al cubo

    private SphereCollider _collider;
    private NavMeshAgent agent;
    private Vector3 initialPosition; // Guardará la posición de inicio (d.Extra)

    public bool TargetOnSight = false;
    private bool isChasing = false;
    private bool isReturning = false;

    void Start()
    {
        _collider = GetComponent<SphereCollider>();
        _collider.isTrigger = true;
        _collider.radius = VisionDistance;

        agent = GetComponent<NavMeshAgent>();
        initialPosition = transform.position; // Guardamos la posición inicial de patrullaje
    }

    void Update()
    {
        // Si el agente está regresando y ya llegó a su punto inicial, reseteamos su estado
        if (isReturning && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            isReturning = false;
            // Opcional: Aquí podrías hacer que vuelva a patrullar si tuviera waypoints
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Solo verificamos visión si NO está persiguiendo ni regresando
        if (!isChasing && !isReturning && (other.name == "Target" || other.CompareTag("Player")))
        {
            Vector3 vectorToTarget = other.transform.position - transform.position;
            float angleToTarget = Vector3.Angle(transform.forward, vectorToTarget);

            if (angleToTarget <= (VisionAngle * 0.5f))
            {
                // Verifica si hay línea de visión sin obstáculos (opcional pero recomendado)
                RaycastHit hit;
                if (Physics.Raycast(transform.position, vectorToTarget.normalized, out hit, VisionDistance))
                {
                    if (hit.collider.CompareTag("Player") || hit.collider.name == "Target")
                    {
                        StartCoroutine(ChaseRoutine());
                    }
                }
            }
        }
    }

    // Rutina que maneja los 3 segundos de persecución (d.2) y el regreso (d.Extra)
    private IEnumerator ChaseRoutine()
    {
        TargetOnSight = true;
        isChasing = true;
        float chaseTime = PursuitTime; // <-- Aquí usamos la variable

        while (chaseTime > 0)
        {
            agent.SetDestination(target.position);
            chaseTime -= Time.deltaTime;
            yield return null;
        }

        TargetOnSight = false;
        agent.SetDestination(transform.position);

        yield return new WaitForSeconds(TimeBeforeReturn); // <-- Aquí usamos la variable

        isChasing = false;
        isReturning = true;
        agent.SetDestination(initialPosition);
    }

    // Punto e: Si toca al agente, lo destruye
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.name == "Target")
        {
            // Destruye al objetivo
            Destroy(collision.gameObject);
        }
    }

    // Dibuja el cono en el editor
    private void OnDrawGizmos()
    {
        if (VisionAngle <= 0.0f) return;

        Gizmos.color = TargetOnSight ? Color.red : Color.green;
        float halfVisionAngle = VisionAngle * 0.5f;

        Vector3 p1 = PointForAngle(halfVisionAngle);
        Vector3 p2 = PointForAngle(-halfVisionAngle);

        Gizmos.DrawLine(transform.position, transform.position + p1);
        Gizmos.DrawLine(transform.position, transform.position + p2);
        Gizmos.DrawWireSphere(transform.position, VisionDistance);
    }

    private Vector3 PointForAngle(float angle)
    {
        angle += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad)) * VisionDistance;
    }
}