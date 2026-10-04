using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemigoAlternante : EnemigoBase
{
    [Header("Configuración de Comportamiento")]
    [SerializeField] private float FleeTime = 3.0f;
    [SerializeField] private float RestingTime = 2.0f;
    private bool _isResting = false;

    [Header("Configuración de Disparo")]
    public GameObject enemyBulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 15f;

    new void Start()
    {
        base.Start(); // Muy importante para inicializar el SentidoDeVision de la clase padre

        // Iniciamos el ciclo de huir y descansar
        StartCoroutine(RutinaHuirYDescansar());
    }

    protected override void Update()
    {
        // 1. Filtrar la visión para encontrar EXCLUSIVAMENTE al jugador a una distancia justa
        List<GameObject> objetosConocidos = SentidoDeVision.GetKnownObjects();
        Target = null;

        // Distancia máxima a la que el enemigo "decide" huir, sin importar qué tan grande sea su collider
        float distanciaDeAggro = 10f;

        foreach (var obj in objetosConocidos)
        {
            if (obj != null && obj.layer == LayerMask.NameToLayer("Player"))
            {
                // Solo se asusta si el jugador está realmente cerca
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
                // ESTADO: Huir (Alejarse del jugador de forma pura)
                Vector3 direccionHuir = (transform.position - Target.transform.position).normalized;
                Vector3 velocidadDeseada = direccionHuir * maxSpeed;

                // ESTADO: Esquivar paredes (deslizarse, no atascarse)
                List<GameObject> obstaculosConocidos = SentidoDeVision.GetKnownObstacles();
                foreach (var obstaculo in obstaculosConocidos)
                {
                    if (obstaculo == this.gameObject || obstaculo.transform.IsChildOf(this.transform) || obstaculo == Target)
                        continue;

                    Collider col = obstaculo.GetComponent<Collider>();
                    Vector3 puntoPared = col != null ? col.ClosestPoint(transform.position) : obstaculo.transform.position;
                    float distancia = Vector3.Distance(transform.position, puntoPared);
                    float radio = SentidoDeVision.GetColliderDetectionRadius();

                    if (distancia < radio)
                    {
                        float porcentaje = 1.0f - (distancia / radio);
                        Vector3 repulsion = (transform.position - puntoPared).normalized;
                        if (repulsion == Vector3.zero) repulsion = transform.forward;

                        // Empuje lateral para resbalar de la pared
                        velocidadDeseada += repulsion * maxSpeed * porcentaje * 3f;
                    }
                }

                velocidadDeseada.y = 0f;
                Vector3 steeringForce = velocidadDeseada - CurrentSpeed;
                steeringForce = Vector3.ClampMagnitude(steeringForce, maxForce);

                ActualizarAceleracionVelocidadYPosicion(steeringForce);

                // NUEVO: Mantener la mirada clavada en el jugador mientras huye hacia atrás
                Vector3 direccionAlJugador = Target.transform.position - transform.position;
                direccionAlJugador.y = 0f;
                if (direccionAlJugador != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(direccionAlJugador.normalized);
                }
            }
            else
            {
                // ESTADO: Descansar y apuntar
                CurrentSpeed = Vector3.Lerp(CurrentSpeed, Vector3.zero, Time.deltaTime * 5f);
                CurrentSpeed.y = 0f;
                transform.position += CurrentSpeed * Time.deltaTime;

                Vector3 direccionAlJugador = Target.transform.position - transform.position;
                direccionAlJugador.y = 0f;
                if (direccionAlJugador != Vector3.zero)
                {
                    Quaternion rotacionDeseada = Quaternion.LookRotation(direccionAlJugador.normalized);
                    transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, Time.deltaTime * 10f);
                }
            }
        }
        else
        {
            // ESTADO: Merodear (Idle)
            Vector3 fuerzaWander = Wander();
            Vector3 fuerzaEvasion = CalcularEvasionParedes(4f);

            // Sumamos la caminata y la evasión suavemente. 
            // Ya no dejamos que la evasión tome el control absoluto.
            Vector3 steeringForce = fuerzaWander + fuerzaEvasion;

            steeringForce.y = 0f;
            steeringForce = Vector3.ClampMagnitude(steeringForce, maxForce);

            ActualizarAceleracionVelocidadYPosicion(steeringForce);

            // Girar de forma natural hacia donde está caminando
            if (CurrentSpeed.sqrMagnitude > 0.01f)
            {
                Vector3 direccionMirada = CurrentSpeed;
                direccionMirada.y = 0f;
                transform.rotation = Quaternion.LookRotation(direccionMirada.normalized);
            }
        }
    }

    private IEnumerator RutinaHuirYDescansar()
    {
        while (true)
        {
            // ESTADO 1: Huir
            _isResting = false;
            yield return new WaitForSeconds(FleeTime);

            // ESTADO 2: Cansarse/Descansar (Inicia el giro hacia el jugador)
            _isResting = true;

            // Le damos 0.75 segundos para que voltee a verte ANTES de disparar.
            // Esto evita que le dispare a la pared y le da una advertencia al jugador.
            yield return new WaitForSeconds(0.75f);

            DispararPredictivo();

            // Espera su tiempo de descanso restante antes de volver a correr
            yield return new WaitForSeconds(RestingTime);
        }
    }

    private void DispararPredictivo()
    {
        if (Target == null || enemyBulletPrefab == null || firePoint == null) return;

        // 1. Necesitamos la velocidad actual del jugador
        Vector3 velocidadJugador = Vector3.zero;
        Rigidbody rbJugador = Target.GetComponent<Rigidbody>();
        if (rbJugador != null)
        {
            velocidadJugador = rbJugador.linearVelocity;
        }

        // 2. ¿Cuánto tardará la bala en llegar desde aquí hasta donde está el jugador ahora?
        float distancia = Vector3.Distance(firePoint.position, Target.transform.position);
        float tiempoDeViajeEstimado = distancia / bulletSpeed;

        // 3. Sabiendo el tiempo, ¿dónde estará el jugador para cuando la bala llegue ahí?
        Vector3 posicionFuturaJugador = Target.transform.position + (velocidadJugador * tiempoDeViajeEstimado);

        // 4. Calculamos la dirección de disparo hacia esa posición futura
        Vector3 direccionDisparo = (posicionFuturaJugador - firePoint.position).normalized;

        // 5. Creamos la bala y la disparamos
        GameObject bala = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.LookRotation(direccionDisparo));
        Rigidbody rbBala = bala.GetComponent<Rigidbody>();
        if (rbBala != null)
        {
            rbBala.linearVelocity = direccionDisparo * bulletSpeed;
        }
    }
}