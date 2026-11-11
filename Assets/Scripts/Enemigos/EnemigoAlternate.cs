using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemigoAlternante : EnemigoBase
{
    [Header("Referencias Principales")]
    [SerializeField] private Transform jugadorTransform;
    [SerializeField] private Renderer rendererEnemigo; // Para cambiar el color visual al cansarse

    [Header("Configuración de Radios")]
    [SerializeField] private float radioDeteccion = 12f; // Radio amplio para perseguir sin visión
    [SerializeField] private float radioHuida = 5f;      // Radio corto para entrar en pánico/Flee

    [Header("Configuración de Tiempos y Estado")]
    [SerializeField] private float FleeTime = 3.0f;
    [SerializeField] private float RestingTime = 2.0f;
    [SerializeField] private float aimWarningTime = 0.75f;
    private bool _isResting = false;

    [Header("Configuración de Disparo y Capas")]
    [SerializeField] private LayerMask capasBloqueanVision;
    public GameObject enemyBulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 15f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Colores de Estado (Visual Feedback)")]
    [SerializeField] private Color colorActivo = Color.red;
    [SerializeField] private Color colorCansado = Color.gray;

    private NavMeshAgent navAgent;
    private NavigatorAgentFlee motorHuida;
    private bool tieneLineaDeVision = false;

    protected override void Start()
    {
        base.Start();
        navAgent = GetComponent<NavMeshAgent>();
        motorHuida = GetComponent<NavigatorAgentFlee>();

        if (rendererEnemigo == null)
            rendererEnemigo = GetComponent<Renderer>();

        // Si no se asignó en el Inspector, intentamos encontrar al jugador por su Tag
        if (jugadorTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) jugadorTransform = p.transform;
        }

        StartCoroutine(RutinaHuirYDescansar());
    }

    protected override void Update()
    {
        if (jugadorTransform == null) return;

        float distanciaAlJugador = Vector3.Distance(transform.position, jugadorTransform.position);

        // Comprobar Línea de Visión Directa con Raycast
        ComprobarLineaDeVision(distanciaAlJugador);

        // Feedback Visual de Estado
        if (rendererEnemigo != null)
        {
            rendererEnemigo.material.color = _isResting ? colorCansado : colorActivo;
        }

        if (!_isResting)
        {
            // --- ESTADO ACTIVO ---
            if (distanciaAlJugador <= radioHuida)
            {
                // Si el jugador está MUY cerca, huir usando Flee
                if (navAgent != null) navAgent.isStopped = false;
                if (motorHuida != null) motorHuida.EjecutarHuida();
            }
            else if (distanciaAlJugador <= radioDeteccion)
            {
                if (tieneLineaDeVision)
                {
                    // Si TIENE línea de visión directa: Dejar de moverse y apuntar
                    if (navAgent != null) navAgent.isStopped = true;
                    ApuntarAlJugador();
                }
                else
                {
                    // Si NO tiene línea de visión directa: Ir a la posición del jugador para buscarlo
                    if (navAgent != null)
                    {
                        navAgent.isStopped = false;
                        navAgent.SetDestination(jugadorTransform.position);
                    }
                }
            }
            else
            {
                // Fuera de rango de detección: Se detiene completamente
                if (navAgent != null) navAgent.isStopped = true;
            }
        }
        else
        {
            // --- ESTADO CANSADO ---
            if (navAgent != null) navAgent.isStopped = true;

            // NUEVO: Solo voltea a apuntar si REALMENTE tiene línea de visión directa
            if (tieneLineaDeVision)
            {
                ApuntarAlJugador();
            }
        }
    }

    private void ComprobarLineaDeVision(float distanciaAlJugador)
    {
        // Por defecto asumimos que no lo ve
        tieneLineaDeVision = false;

        // Solo intentamos "ver" al jugador si ya entró al área amarilla (radioDeteccion)
        if (distanciaAlJugador <= radioDeteccion)
        {
            Vector3 direccion = (jugadorTransform.position - transform.position).normalized;

            // Lanzamos el Raycast limitado por el radioDeteccion
            if (Physics.Raycast(transform.position, direccion, out RaycastHit hit, radioDeteccion))
            {
                if (hit.transform == jugadorTransform)
                {
                    // El rayo tocó al jugador: Visión limpia (Línea Verde)
                    tieneLineaDeVision = true;
                    Debug.DrawLine(transform.position, hit.point, Color.green);
                }
                else
                {
                    // El rayo chocó con una pared u obstáculo (Línea Roja)
                    Debug.DrawLine(transform.position, hit.point, Color.red);
                }
            }
        }
    }

    private void ApuntarAlJugador()
    {
        Vector3 direccionAlJugador = jugadorTransform.position - transform.position;
        direccionAlJugador.y = 0f;
        if (direccionAlJugador != Vector3.zero)
        {
            Quaternion rotacionDeseada = Quaternion.LookRotation(direccionAlJugador.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, Time.deltaTime * rotationSpeed);
        }
    }

    private IEnumerator RutinaHuirYDescansar()
    {
        while (true)
        {
            _isResting = false;
            yield return new WaitForSeconds(FleeTime);

            _isResting = true;
            yield return new WaitForSeconds(aimWarningTime);

            DispararPredictivo();
            yield return new WaitForSeconds(RestingTime);
        }
    }

    private void DispararPredictivo()
    {
        // NUEVO: Si no hay línea de visión (ya sea por distancia o por pared), cancela el disparo
        if (!tieneLineaDeVision) return;

        if (jugadorTransform == null || enemyBulletPrefab == null || firePoint == null) return;

        Vector3 velocidadJugador = Vector3.zero;
        Rigidbody rbJugador = jugadorTransform.GetComponent<Rigidbody>();
        if (rbJugador != null)
        {
            velocidadJugador = rbJugador.linearVelocity;
        }

        float distancia = Vector3.Distance(firePoint.position, jugadorTransform.position);
        float tiempoDeViajeEstimado = distancia / bulletSpeed;

        Vector3 posicionFuturaJugador = jugadorTransform.position + (velocidadJugador * tiempoDeViajeEstimado);
        Vector3 direccionDisparo = (posicionFuturaJugador - firePoint.position).normalized;

        GameObject bala = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.LookRotation(direccionDisparo));
        Rigidbody rbBala = bala.GetComponent<Rigidbody>();
        if (rbBala != null)
        {
            rbBala.linearVelocity = direccionDisparo * bulletSpeed;
        }
    }

    protected new void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (jugadorTransform == null) return;

        // 1. Gizmo de Raycast de Visión Directa (Verde = Línea despejada, Rojo = Bloqueado por pared)
        Gizmos.color = tieneLineaDeVision ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, jugadorTransform.position);

        // 2. Gizmos para los Radios
        // Radio de Detección (Amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);

        // Radio de Huida/Flee (Cyan)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioHuida);
    }
}