using UnityEngine;
using UnityEngine.AI;

public class NavigatorAgentFlee : MonoBehaviour
{
    [Header("Configuración de Huida")]
    public Transform objetivo;
    [SerializeField] private float distanciaDeHuida = 5f;

    private NavMeshAgent agente;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
    }

    // Cambiamos Update por un método público que será controlado externamente
    public void EjecutarHuida()
    {
        if (objetivo != null && agente != null && agente.isActiveAndEnabled)
        {
            Vector3 direccionHuida = transform.position - objetivo.position;
            Vector3 puntoDeseado = transform.position + direccionHuida.normalized * distanciaDeHuida;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(puntoDeseado, out hit, distanciaDeHuida, NavMesh.AllAreas))
            {
                agente.SetDestination(hit.position);
            }
        }
    }
}