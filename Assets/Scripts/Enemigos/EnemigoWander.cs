using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemigoWander : EnemigoBase
{
    [SerializeField] private float distanciaDelPuntoDeWander = 5.0f;
    private Vector3 _posicionObjetivo = Vector3.zero;

    [SerializeField] private float tiempoAntesDeObtenerNuevaPosicionObjetivo = 3.0f;
    private Coroutine _corrutinaObtenerNuevaPosicionObjetivo;

    [SerializeField] private LayerMask capasQueChocanContraRaycast;

    // que nos quedemos a este porcentaje de distancia antes de chocar contra el objeto contra el que chocó el raycast.
    [SerializeField] private float porcentajeDeSeparacionDelRaycast = 0.8f;

    // si el agente ya está a esta distancia o menos del objetivo, se detiene (o genera una nueva posición objetivo, como prefieran) 
    [SerializeField] private float radioDeToleraciaALaPosicionObjetivo = 1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    new void Start()
    {
        base.Start();

        _corrutinaObtenerNuevaPosicionObjetivo = StartCoroutine(ObtenerNuevaPosicionObjetivo());
    }

    // Update is called once per frame
    new void Update()
    {
        // Deambular.

        /* 1) la más sencilla es decir: ahora voy para allá ... cada frame
        // Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0.0f, Random.Range(-1f, 1f));
        // if (randomDirection.sqrMagnitude < float.Epsilon) // float.Epsilon es un valor muy muy chiquito, y sirve para esto.
        // {
        //     return;
        // }
        
        // ahora tu objetivo está en dicha dirección y cierta distancia, házle seek hacia dicha dirección
        // la posición objetivo es: desde donde estoy yo, movernos hacia la dirección aleatoria con una magnitud de distanciaDelPuntoDeWander
        // _posicionObjetivo = transform.position + randomDirection.normalized * distanciaDelPuntoDeWander;
        */

        // Versión 2, en vez de hacer nueva posición cada frame, puedes hacerlo cada X tiempo,
        // y por ejemplo con una corrutina queda muy sencillo.

        // si el agente ya está a esta distancia o menos del objetivo, se detiene (o genera una nueva posición objetivo, como prefieran) 
        if (Vector3.Distance(transform.position, _posicionObjetivo) <= radioDeToleraciaALaPosicionObjetivo)
        {
            // Ahorita vamos a hacer que se detenga
            // por ahora le podemos decir que simplemente se salga del update para que no se mueva. 

            // Aquí el problema que el waypoint se cambia cada X tiempo por la corrutina,
            // independientemente de si cambiaste el objetivo porque ya habías llegado
            // CalcularPosicionObjetivo();

            // una mejor solución es detener la corrutina y volverla a iniciar, así detienes el temporizador, 
            // y además te da una nueva posición.
            if (_corrutinaObtenerNuevaPosicionObjetivo != null)
            {
                StopCoroutine(_corrutinaObtenerNuevaPosicionObjetivo); // la detenemos
                // y la volvemos a iniciar.
                _corrutinaObtenerNuevaPosicionObjetivo = StartCoroutine(ObtenerNuevaPosicionObjetivo());
            }
        }

        Vector3 steeringForce = Seek();

        ActualizarAceleracionVelocidadYPosicion(steeringForce);
    }

    private void CalcularPosicionObjetivo()
    {
        // obtén una nueva dirección y con ella la nueva posición
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0.0f, Random.Range(-1f, 1f));
        if (randomDirection.sqrMagnitude < float.Epsilon) // float.Epsilon es un valor muy muy chiquito, y sirve para esto.
        {
            return; // esperarse un frame
        }

        _posicionObjetivo = transform.position + randomDirection.normalized * distanciaDelPuntoDeWander;

        // Podemos mejorar nuestro wander haciendo que no obtenga posiciones fuera de los límites del escenario,
        // (o también dentro de algún obstáculo). 
        // Lo primero es determinar si la línea recta desde el agente hacia dicho punto objetivo choca contra algún
        // obstáculo o pared.
        // La función Raycast hace justo eso, nos dice qué objetos (con colliders) están atravesados por una línea
        // que va del punto A al punto B. A sería la posición de nuestro agente, y B sería la posición objetivo.
        Vector3 puntaMenosCola = _posicionObjetivo - transform.position;
        if (puntaMenosCola.magnitude < float.Epsilon)
        {
            return;
        }

        Vector3 direccionPuntaMenosCola = puntaMenosCola.normalized; // es decir, la dirección desde mí hacia el objetivo

        RaycastHit hitInfo;
        // si el Physics.Raycast regresa verdadero, es porque el rayo sí chocó contra algo
        // (en alguna de las capas especificadas). Y el hitInfo es la estructura que nos dice contra quién y en dónde chocó.
        if (Physics.Raycast(transform.position, direccionPuntaMenosCola, out hitInfo, distanciaDelPuntoDeWander,
                capasQueChocanContraRaycast))
        {
            // si llegamos aquí es porque sí chocó el rayo, vamos a checar contra quién y dónde.
            // Otra mejor es que nuestra nueva posición objetivo no esté justo en el punto de contacto, sino
            // que esté un poco antes, para no tener que llegar hasta chocar.
            _posicionObjetivo = transform.position + direccionPuntaMenosCola * (hitInfo.distance * porcentajeDeSeparacionDelRaycast);
        }
    }

    // Versión 2 del wander: hacerlo cada cierto tiempo.
    private IEnumerator ObtenerNuevaPosicionObjetivo()
    {
        // con el mismo método que ya habíamos hecho de sacar una posición aleatoria, pero ahora cada X tiempo.
        while (true)
        {
            CalcularPosicionObjetivo();

            // Le decimos que se espere X tiempo antes de obtener otra nueva posición.
            yield return new WaitForSeconds(tiempoAntesDeObtenerNuevaPosicionObjetivo);
        }
    }


    new void OnDrawGizmos()
    {
        // esta variable nos dice si el juego se está jugando o no, independientemente de si es en editor o en build.
        if (!Application.isPlaying)
            return; // si no está en play, que no haga nada.

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, _posicionObjetivo);
        Gizmos.DrawSphere(_posicionObjetivo, 0.1f);
    }
}