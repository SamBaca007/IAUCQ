using UnityEngine;
// Obligatorio incluir esta librería para acceder a las clases de navegación
using UnityEngine.AI;

public class NavigatorAgent : MonoBehaviour
{
    [Tooltip("El objetivo hacia el cual se moverá el agente (ej. el Jugador).")]
    public Transform objetivo;

    private NavMeshAgent agente;

    void Start()
    {
        // Obtenemos la referencia al componente NavMeshAgent de este mismo objeto
        agente = GetComponent<NavMeshAgent>();

        if (agente == null)
        {
            Debug.LogError("Falta el componente NavMeshAgent en " + gameObject.name);
        }
    }

    void Update()
    {
        // Si hay un objetivo asignado, actualizamos el destino del agente constantemente
        if (objetivo != null)
        {
            agente.SetDestination(objetivo.position);
        }
    }
}