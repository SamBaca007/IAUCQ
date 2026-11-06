using System.Collections;
using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    [Header("Ruta del Obstáculo")]
    [Tooltip("Arrastra aquí los objetos vacíos que servirán como paradas.")]
    [SerializeField] private Transform[] wayPoints;

    [Header("Configuración de Movimiento")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float waitingTime = 2f;

    private int currentPointIndex = 0;
    private bool isWaiting = false;

    void Start()
    {
        if (wayPoints.Length > 0)
        {
            // Opcional: Iniciar directamente en el primer punto
            transform.position = wayPoints[0].position;
        }
        else
        {
            Debug.LogWarning("No hay puntos de ruta asignados en " + gameObject.name);
        }
    }

    void Update()
    {
        if (wayPoints.Length == 0 || isWaiting) return;

        Transform puntoDestino = wayPoints[currentPointIndex];

        // Mover el obstáculo hacia el destino
        transform.position = Vector3.MoveTowards(transform.position, puntoDestino.position, speed * Time.deltaTime);

        // Si llegamos al punto (con un pequeño margen de error para precisión de flotantes)
        if (Vector3.Distance(transform.position, puntoDestino.position) < 0.01f)
        {
            StartCoroutine(RutinaEspera());
        }
    }

    private IEnumerator RutinaEspera()
    {
        isWaiting = true;

        yield return new WaitForSeconds(waitingTime);

        // Pasar al siguiente punto, volviendo al inicio si llegamos al final
        currentPointIndex = (currentPointIndex + 1) % wayPoints.Length;

        isWaiting = false;
    }
}