using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// esto forza a que cuando este Script esté asignado a un gameObject, se le añada automáticamente un rigidbody si 
// no lo tiene, y que no se le pueda quitar.
[RequireComponent(typeof(Rigidbody))]
public class PatrullaWaypoints : MonoBehaviour
{
    // Este es necesario porque el Pursuit y evade requieren saber la velocidad actual de su objetivo, y pues
    // el rigidbody tiene dicha información.
    private Rigidbody _rb;
    [SerializeField] private float maximaFuerza = 5.0f;

    [SerializeField] private float maximaVelocidad = 5.0f;

    [SerializeField] private float radioDeToleranciaAWaypoint = 2.0f;

    // Cuando el agente esté a esta distancia o menos del waypoint objetivo, va a comenzar a bajar su velocidad, 
    // para poder hacer un steering behavior de Arrive. Debe de ser más grande que el radio de tolerancia.
    [SerializeField] private float radioDeFrenado = 5.0f;

    private bool _estaDetenido = false;

    // los puntos que va a visitar y en qué orden los va a visitar. Cuando visite al último, se ciclará hacia el primero.
    [SerializeField] private List<Transform> waypoints = new List<Transform>();
    private Transform _waypointActual;
    private int _indiceWaypointActual = 0;

    [SerializeField] private float tiempoDeEsperaAntesDeNuevoWaypoint = 2.0f;
    private Coroutine _corrutinaEsperarAntesDeNuevoWaypoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
        {
            Debug.LogError("No tiene el rigidbody en este gameObject", gameObject);
        }

        // Cuando inicias, pones como Waypoint actual al primero de la lista
        _indiceWaypointActual = 0;
        _waypointActual = waypoints[_indiceWaypointActual];
    }

    // Se llama una vez por frame y determina el valor de Time.deltaTime.
    void Update()
    {

    }

    // Es lo "mismo" que el Update, PERO este a fuerzas se manda a llamar cada cierto tiempo FIJO, independientemente
    // del framerate.
    // Por defecto se ejecuta 50 veces por segundo (cada 0.02 milisegundos),
    // lo pueden cambiar en Project Settings->Time->Fixed Timestep, pero es rara la vez que necesiten cambiarlo,
    // aumentarlo la cantidad de veces es pesado porque se aumenta el cuántas veces se checan las físicas de TODA la escena.
    // Este FixedUpdate es el que siempre se recomienda para el manejo de movimiento de Rigidbodies (gameObjects que contengan rigidbodies)
    // y (creo) que character controllers, siempre y cuando haya elementos con simulación física o detección de colisión física.
    // este tiene Time.fixedDeltaTime, que es el que debe de usarse aquí en lugar del Time.deltaTime normalito.
    private void FixedUpdate()
    {
        // Por ejemplo, addForces, cambios a la velocity de un rigidbody, o a la rotación de un objeto que depende de rigidbody, 
        // entre otras. Como regla general, casi siempre que tenga que ver con actualizar un rigidbody de forma "constante",
        // va a ser en este fixedUpdate.


        // COMPORTAMIENTO DEL AGENTE:
        // ¿cómo sabemos cuál es el waypoint actualmente activo?
        // Pues inicias por el primero... :D
        // ya que llegas al primero, te vas luego al segundo... :D 
        // ¿cómo sabes que ya llegaste al primero?
        // decimos que "llegamos" cuando estamos lo suficientemente cerca de nuestra posición objetivo
        // y ese "cerca" se mide con distancia entre mi posición y la posición del objetivo.
        float distanciaAlWaypoint = Vector3.Distance(transform.position, _waypointActual.position);

        // si dicha distancia es Menor que lo que yo considero Cerca
        if (distanciaAlWaypoint <= radioDeToleranciaAWaypoint)
        {
            // como estaba antes sería solo mandar a llamar AsignarNuevoWaypoint();
            Debug.Log("Llegué al radio de tolerancia a waypoint. Comenzando corrutina para esperar a poner nuevo waypoint");

            // La nueva versión, que es la que espera tiempo, sería mandando la corrutina
            if (_corrutinaEsperarAntesDeNuevoWaypoint == null)
                _corrutinaEsperarAntesDeNuevoWaypoint = StartCoroutine(EsperarAntesDeNuevoWaypoint());
        }
        // y si no, pues nada más me sigo yendo al que ya estaba yendo.


        Vector3 steeringForce = Vector3.zero;
        // Si el agente No está detenido, entonces es cuando puede moverse
        if (!_estaDetenido)
        {
            // Si el agente está dentro del radio de frenado, es decir: si la distancia entre la posición del agente y
            // su posición objetivo es menor que el radio de frenado, entonces se empieza a frenar.
            float distanciaAlObjetivo = Vector3.Distance(transform.position, _waypointActual.position);
            if (distanciaAlObjetivo < radioDeFrenado)
            {
                // entonces empezamos a frenar hasta la velocidad interpolada.
                // Normalizar nuestra distancia respecto al radio de frenado
                float t = distanciaAlObjetivo / radioDeFrenado; // esto es nuestra 't' ya en el rango de 0 a 1. // NOTA, TIENE ALGO MAL.
                float interpolacionLineal = maximaVelocidad + (1.0f - t) * (0.0f - maximaVelocidad);



                // _rb.linearVelocity = Vector3.ClampMagnitude(_rb.linearVelocity, interpolacionLineal);
                // Este agente hace Seek hacia la posición del Waypoint actualmente activo PERO usando el valor de la
                // interpolación lineal en vez de la maximaVelocidad.
                steeringForce = SteeringBehaviors.Seek(_waypointActual.position, transform.position,
                    interpolacionLineal, _rb.linearVelocity, maximaFuerza);



                // usamos Acceleration para no tomar en cuenta la Masa del rigidbody (al menos por ahora).
                _rb.AddForce(steeringForce, ForceMode.Acceleration);
                Debug.Log($"Dentro del radio de frenado. frenando. Debe ir al {t * 100.0f} porciento de velocidad, " +
                          $"que sería {interpolacionLineal}. La steering force es: {steeringForce} y la linear velocity " +
                          $"es: {_rb.linearVelocity}");

                _rb.linearVelocity = Vector3.ClampMagnitude(_rb.linearVelocity, interpolacionLineal);
                Debug.Log("después del clamp magnitude, la linear velocity es: " + _rb.linearVelocity + " y su" +
                          "magnitud es: " + _rb.linearVelocity.magnitude);
            }
            else
            {
                // Este agente hace Seek hacia la posición del Waypoint actualmente activo.
                steeringForce = SteeringBehaviors.Seek(_waypointActual.position, transform.position,
                    maximaVelocidad, _rb.linearVelocity, maximaFuerza);
                // usamos Acceleration para no tomar en cuenta la Masa del rigidbody (al menos por ahora).
                _rb.AddForce(steeringForce, ForceMode.Acceleration);
            }

        }

    }

    private void AsignarNuevoWaypoint()
    {
        // entonces ya llegué, y ahora voy a ir hacia el siguiente waypoint
        // es decir, mi waypoint Objetivo ahora es el siguiente del actual.
        _indiceWaypointActual += 1;
        // si el índice es mayor o igual que el total de waypoints, es decir, ya no hay un waypoint siguiente al cual ir,
        if (_indiceWaypointActual >= waypoints.Count)
        {
            // entonces lo regresamos al primero
            _indiceWaypointActual = 0;
        }

        _waypointActual = waypoints[_indiceWaypointActual];
    }

    private IEnumerator EsperarAntesDeNuevoWaypoint()
    {
        _estaDetenido = true;
        // Detiene todo el movimiento del rigidbody para detener a este agente,
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        yield return new WaitForSeconds(tiempoDeEsperaAntesDeNuevoWaypoint);

        Debug.Log("terminó la espera, asignando nuevo waypoint");

        AsignarNuevoWaypoint();

        _estaDetenido = false;

        // cuando se acaba la corrutina, lo reseteamos a null.
        _corrutinaEsperarAntesDeNuevoWaypoint = null;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;


        // radio de empezar a frenar. 
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(waypoints[_indiceWaypointActual].position, radioDeFrenado);

        // Que nos dibuje el radio de detenimiento del waypoint actual
        // queremos que nos lo dibuje en la posición del waypoint, y con el tamaño del radio de tolerancia
        Gizmos.color = Color.red; // Rojo como en un semáforo, que significa detenerte completamente.
        Gizmos.DrawWireSphere(waypoints[_indiceWaypointActual].position, radioDeToleranciaAWaypoint);

    }
}