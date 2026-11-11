using UnityEngine;
using UnityEngine.AI;

public class NavigatorAgentFlee : MonoBehaviour
{
    [Header("Configuración de Huida")]
    public Transform objetivo;
    [SerializeField] private float distanciaDeHuida = 5f;

    private NavMeshAgent agente;
    private Vector3 ultimoPuntoHuida;
    private bool tienePuntoValido = false;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
    }

    public void EjecutarHuida()
    {
        if (objetivo != null && agente != null && agente.isActiveAndEnabled)
        {
            Vector3 direccionHuida = transform.position - objetivo.position;
            Vector3 puntoDeseado = transform.position + direccionHuida.normalized * distanciaDeHuida;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(puntoDeseado, out hit, distanciaDeHuida, NavMesh.AllAreas))
            {
                ultimoPuntoHuida = hit.position;
                tienePuntoValido = true;
                agente.SetDestination(hit.position);
            }
            else
            {
                // Requisito C.2: Si el punto de Flee no es válido, intentar hacia la dirección del jugador
                Vector3 puntoOpuesto = transform.position - direccionHuida.normalized * distanciaDeHuida;
                if (NavMesh.SamplePosition(puntoOpuesto, out hit, distanciaDeHuida, NavMesh.AllAreas))
                {
                    ultimoPuntoHuida = hit.position;
                    tienePuntoValido = true;
                    agente.SetDestination(hit.position);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (tienePuntoValido)
        {
            // Dibujar gizmo de la posición a la que huye (Triángulo/Esfera Magenta)
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(ultimoPuntoHuida, 0.4f);
            Gizmos.DrawLine(transform.position, ultimoPuntoHuida);
        }
    }
}