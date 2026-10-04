using System;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(SphereCollider), typeof(SentidoDeVision))]
public class EnemigoBase : MonoBehaviour
{
    [Tooltip("Límite de velocidad al cual puede ir este agente")]

    [SerializeField] protected float maxSpeed = 5.0f;

    [SerializeField] protected float maxForce = 5.0f;

    [Tooltip("Velocidad actual que tiene este agente. Está limitada por maxSpeed")]
    protected Vector3 CurrentSpeed = Vector3.zero;

    [SerializeField] protected float mass = 1.0f;

    [Header("Propiedades de Combate")]
    [SerializeField] protected int Health = 5;
    [SerializeField] protected int ContactDamage = 1;
    [SerializeField] protected Collider OwnCollider;

    protected SentidoDeVision SentidoDeVision;
    protected GameObject Target;

    protected int myInt = 0;

    [Header("Comportamiento Idle (Merodear)")]
    [SerializeField] protected float wanderRadius = 5f;
    [SerializeField] protected float wanderInterval = 2f;
    protected float wanderTimer = 0f;
    protected Vector3 wanderTargetPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        SentidoDeVision = GetComponent<SentidoDeVision>();
        if (SentidoDeVision == null)
        {
            Debug.LogError("No hay componente SentidoDeVision asignado", gameObject);
        }

        OwnCollider = GetComponent<Collider>();
        if (OwnCollider == null)
        {
            Debug.LogError("No hay componente Collider asignado a colliderPropio", gameObject);
        }

        // NUEVO: Su primer objetivo de merodeo es donde está parado, no el centro del mapa
        wanderTargetPos = transform.position;
        wanderTimer = 0f;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        // 1. Buscar al jugador con límite de distancia
        List<GameObject> objetosConocidos = SentidoDeVision.GetKnownObjects();
        Target = null;
        float distanciaDeAggro = 10f;

        foreach (var obj in objetosConocidos)
        {
            if (obj != null && obj.layer == LayerMask.NameToLayer("Player"))
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
            // ESTADO: Perseguir al jugador (Comportamiento agresivo del cubo blanco)
            Vector3 steeringForce = SteeringBehaviors.Seek(Target.transform.position, transform.position, maxSpeed, CurrentSpeed, maxForce);

            // Esquivar paredes mientras persigue
            steeringForce += CalcularEvasionParedes(3f);

            steeringForce.y = 0f;
            steeringForce = Vector3.ClampMagnitude(steeringForce, maxForce);
            ActualizarAceleracionVelocidadYPosicion(steeringForce);

            // Mirar al jugador mientras lo persigue
            Vector3 direccionMirada = Target.transform.position - transform.position;
            direccionMirada.y = 0f;
            if (direccionMirada != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direccionMirada.normalized);
            }
        }
        else
        {
            // ESTADO: Merodear (Idle)
            Vector3 fuerzaWander = Wander();
            Vector3 fuerzaEvasion = CalcularEvasionParedes(4f);

            Vector3 steeringForce = fuerzaWander + fuerzaEvasion;
            steeringForce.y = 0f;
            steeringForce = Vector3.ClampMagnitude(steeringForce, maxForce);
            ActualizarAceleracionVelocidadYPosicion(steeringForce);

            // Mirar hacia donde camina
            if (CurrentSpeed.sqrMagnitude > 0.01f)
            {
                Vector3 direccionMirada = CurrentSpeed;
                direccionMirada.y = 0f;
                transform.rotation = Quaternion.LookRotation(direccionMirada.normalized);
            }
        }
    }

    protected Vector3 Flee()
    {
        return SteeringBehaviors.Flee(Target.transform.position, transform.position,
            maxSpeed, CurrentSpeed, maxForce);
    }

    protected Vector3 Flee(Vector3 position)
    {
        return SteeringBehaviors.Flee(position, transform.position,
            maxSpeed, CurrentSpeed, maxForce);
    }

    protected Vector3 Seek()
    {
        return SteeringBehaviors.Seek(Target.transform.position, transform.position,
            maxSpeed, CurrentSpeed, maxForce);
    }

    protected void ActualizarAceleracionVelocidadYPosicion(Vector3 steeringForce)
    {
        transform.position = SteeringBehaviors.ActualizarAceleracionVelocidadYPosicion(steeringForce, mass, maxSpeed,
            ref CurrentSpeed, transform.position);
    }

    void AplicarGravedad()
    {
        Vector3 direccionDeGravedad = new Vector3(0.0f, -1, 0.0f);
        float magnitudDeGravedad = 9.81f;

        CurrentSpeed += direccionDeGravedad * (Time.deltaTime * magnitudDeGravedad);

        transform.position += CurrentSpeed * Time.deltaTime;

        Debug.Log($"Aceleración es: {direccionDeGravedad * magnitudDeGravedad}, " +
                  $"velocidad es: {CurrentSpeed}, posición es: {transform.position}");

    }

    // Usamos 'virtual' para que las clases hijas puedan usar 'override'
    protected virtual void OnCollisionEnter(Collision other)
    {
        // 1. Recibir daño de las balas
        if (other.gameObject.layer == LayerMask.NameToLayer("AtaqueDeJugador"))
        {
            // Buscamos el componente Bala para saber cuánto daño hace
            Bala scriptBala = other.gameObject.GetComponent<Bala>();

            if (scriptBala != null)
            {
                Health -= scriptBala.Damage; // Se resta el daño configurable
            }
            else
            {
                Health--; // Por si acaso choca con algo que no tiene el script
            }

            if (Health <= 0)
            {
                Destroy(gameObject);
            }
        }
        // 2. Hacerle daño al jugador al tocarlo
        else if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            PlayerController player = other.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(ContactDamage);
            }
        }
    }
    protected void OnDrawGizmos()
    {
        if (Target != null)
            Gizmos.DrawLine(transform.position, Target.transform.position);
    }
    int FuncionEjemplo(int miEntero)
    {
        return miEntero + 3;
    }
    protected void OnDrawGizmosSelected()
    {
        // Debug.Log("se está viendo la pestaña de Scene y este GameObject está seleccionado", gameObject);
    }

    // Genera una fuerza para que el enemigo camine a puntos aleatorios cercanos
    protected bool isWanderInitialized = false;

    protected Vector3 Wander()
    {
        // 1. Inicialización garantizada que ignora los problemas del Start()
        if (!isWanderInitialized)
        {
            wanderTargetPos = transform.position;
            wanderTimer = 0f;
            isWanderInitialized = true;
        }

        wanderTimer -= Time.deltaTime;

        // 2. Si el tiempo se acaba o llega a su destino
        if (wanderTimer <= 0f || Vector3.Distance(transform.position, wanderTargetPos) < 1f)
        {
            // Busca un nuevo punto
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * 3f;
            wanderTargetPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            // Le damos entre 1 y 3 segundos de caminata antes de cambiar de rumbo
            wanderTimer = wanderInterval + UnityEngine.Random.Range(-0.5f, 1f);
        }

        return SteeringBehaviors.Seek(wanderTargetPos, transform.position, maxSpeed * 0.4f, CurrentSpeed, maxForce);
    }

    protected Vector3 CalcularEvasionParedes(float multiplicador = 3f)
    {
        Vector3 fuerzaEvasion = Vector3.zero;
        List<GameObject> obstaculosConocidos = SentidoDeVision.GetKnownObstacles();

        foreach (var obstaculo in obstaculosConocidos)
        {
            if (obstaculo == this.gameObject || obstaculo.transform.IsChildOf(this.transform) || obstaculo == Target)
                continue;

            // EL SECRETO: Evitamos que huya de sus compañeros desde el otro lado del mapa
            if (obstaculo.GetComponent<EnemigoBase>() != null) continue;

            Collider col = obstaculo.GetComponent<Collider>();
            Vector3 puntoPared = col != null ? col.ClosestPoint(transform.position) : obstaculo.transform.position;
            float distancia = Vector3.Distance(transform.position, puntoPared);

            // SEGUNDO SECRETO: Radio de evasión corto (3 unidades), independiente del enorme radio de visión
            float radioEvasion = 3.0f;

            if (distancia < radioEvasion)
            {
                float porcentaje = 1.0f - (distancia / radioEvasion);
                Vector3 repulsion = (transform.position - puntoPared).normalized;
                if (repulsion == Vector3.zero) repulsion = transform.forward;

                fuerzaEvasion += repulsion * maxSpeed * porcentaje * multiplicador;
            }
        }
        return fuerzaEvasion;
    }
}