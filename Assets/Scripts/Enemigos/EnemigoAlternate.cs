using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemigoAlternante : EnemigoBase
{
    [Header("Configuración de Comportamiento")]
    [SerializeField] private float FleeTime = 3.0f;
    [SerializeField] private float RestingTime = 2.0f;
    [SerializeField] private float aimWarningTime = 0.75f;
    private bool _isResting = false;

    [Header("Configuración de Percepción")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float distanciaDeAggro = 10f;

    [Header("Configuración de Disparo")]
    public GameObject enemyBulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 15f;
    [SerializeField] private float rotationSpeed = 10f;

    private NavMeshAgent navAgent;
    private NavigatorAgentFlee motorHuida; // Referencia al nuevo script

    new void Start()
    {
        base.Start();
        navAgent = GetComponent<NavMeshAgent>();
        motorHuida = GetComponent<NavigatorAgentFlee>(); // Obtenemos el componente
        StartCoroutine(RutinaHuirYDescansar());
    }

    protected override void Update()
    {
        List<GameObject> objetosConocidos = SentidoDeVision.GetKnownObjects();
        Target = null;

        foreach (var obj in objetosConocidos)
        {
            if (obj != null && ((1 << obj.layer) & playerLayer) != 0)
            {
                if (Vector3.Distance(transform.position, obj.transform.position) <= distanciaDeAggro)
                {
                    Target = obj;
                    break;
                }
            }
        }

        if (Target != null)
        {
            if (!_isResting)
            {
                if (navAgent != null) navAgent.isStopped = false;

                // Aquí le damos la orden de huir solo si NO está descansando
                if (motorHuida != null) motorHuida.EjecutarHuida();
            }
            else
            {
                if (navAgent != null) navAgent.isStopped = true;

                Vector3 direccionAlJugador = Target.transform.position - transform.position;
                direccionAlJugador.y = 0f;
                if (direccionAlJugador != Vector3.zero)
                {
                    Quaternion rotacionDeseada = Quaternion.LookRotation(direccionAlJugador.normalized);
                    transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, Time.deltaTime * rotationSpeed);
                }
            }
        }
        else
        {
            if (navAgent != null) navAgent.isStopped = true;
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
        if (Target == null || enemyBulletPrefab == null || firePoint == null) return;

        Vector3 velocidadJugador = Vector3.zero;
        Rigidbody rbJugador = Target.GetComponent<Rigidbody>();
        if (rbJugador != null)
        {
            velocidadJugador = rbJugador.linearVelocity;
        }

        float distancia = Vector3.Distance(firePoint.position, Target.transform.position);
        float tiempoDeViajeEstimado = distancia / bulletSpeed;

        Vector3 posicionFuturaJugador = Target.transform.position + (velocidadJugador * tiempoDeViajeEstimado);
        Vector3 direccionDisparo = (posicionFuturaJugador - firePoint.position).normalized;

        GameObject bala = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.LookRotation(direccionDisparo));
        Rigidbody rbBala = bala.GetComponent<Rigidbody>();
        if (rbBala != null)
        {
            rbBala.linearVelocity = direccionDisparo * bulletSpeed;
        }
    }
}