using System;
using System.Collections.Generic;
using System.Numerics;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected void Start()
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

    }

    // Update is called once per frame
    void Update()
    {
        List<GameObject> objetosConocidos = SentidoDeVision.GetKnownObjects();
        if (objetosConocidos.Count > 0)
        {
            Target = objetosConocidos[0];

            // Documento original de los steering behaviors: https://www.red3d.com/cwr/papers/1999/gdc99steer.pdf

            Vector3 steeringForce = Seek();

            List<GameObject> obstaculosConocidos = SentidoDeVision.GetKnownObstacles();
            foreach (var obstaculo in obstaculosConocidos)
            {
                float porcentaje = Mathf.Lerp(0, 1.0f,
                    1.0f - Vector3.Distance(transform.position,
                        obstaculo.transform.position) / SentidoDeVision.GetColliderDetectionRadius());
                steeringForce += Flee(obstaculo.transform.position) * porcentaje;
            }

            ActualizarAceleracionVelocidadYPosicion(steeringForce);
 
        }
        else
        {
            Target = null;
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

    protected void OnCollisionEnter(Collision other)
    {
        // 1. Recibir daño de las balas
        if (other.gameObject.layer == LayerMask.NameToLayer("AtaqueDeJugador"))
        {
            Health--;
            // Puntos extra: Aquí podrías llamar a una corrutina para que brille en rojo

            if (Health <= 0)
            {
                Destroy(gameObject); // Destruye al enemigo de la escena
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
        // 3. Frenar en seco al chocar con una pared
        else if (other.gameObject.layer == LayerMask.NameToLayer("Paredes"))
        {
            CurrentSpeed = Vector3.zero;
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
}