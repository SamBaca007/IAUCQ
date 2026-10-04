using UnityEngine;
using System.Collections.Generic;

public class EnemigoPesado : EnemigoBase
{
    // B) Variable para recordar que ya te vio y NUNCA dejar de perseguirte
    private bool jugadorDetectado = false;

    protected override void Update()
    {
        // B) Te persigue en cuanto entras al cuarto (o te detecta por primera vez)
        if (!jugadorDetectado)
        {
            List<GameObject> objetosConocidos = SentidoDeVision.GetKnownObjects();
            foreach (var obj in objetosConocidos)
            {
                if (obj != null && obj.layer == LayerMask.NameToLayer("Player"))
                {
                    Target = obj;
                    jugadorDetectado = true; // Se activa para siempre
                    break;
                }
            }
        }

        if (jugadorDetectado && Target != null)
        {
            // D) Steering behavior de predicción: Pursuit
            Vector3 velocidadJugador = Vector3.zero;
            Rigidbody rbJugador = Target.GetComponent<Rigidbody>();
            if (rbJugador != null)
            {
                velocidadJugador = rbJugador.linearVelocity;
            }

            Vector3 steeringForce = SteeringBehaviors.Pursuit(
                Target.transform.position,
                velocidadJugador,
                transform.position,
                maxSpeed,
                CurrentSpeed,
                maxForce
            );

            // A) No dispara. 
            // OMITIMOS el CalcularEvasionParedes() aquí. Para poder cumplir el punto E 
            // (chocar con las paredes), necesitamos que sea torpe y se estrelle, no que las esquive.

            steeringForce.y = 0f;
            steeringForce = Vector3.ClampMagnitude(steeringForce, maxForce);
            ActualizarAceleracionVelocidadYPosicion(steeringForce);

            // Mirar al jugador constantemente
            Vector3 direccionMirada = Target.transform.position - transform.position;
            direccionMirada.y = 0f;
            if (direccionMirada != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direccionMirada.normalized);
            }
        }
        else
        {
            // ESTADO INICIAL: Merodear pacíficamente hasta que te vea por primera vez
            Vector3 fuerzaWander = Wander();
            Vector3 fuerzaEvasion = CalcularEvasionParedes(4f);
            Vector3 steeringForce = fuerzaWander + fuerzaEvasion;

            steeringForce.y = 0f;
            steeringForce = Vector3.ClampMagnitude(steeringForce, maxForce);
            ActualizarAceleracionVelocidadYPosicion(steeringForce);

            if (CurrentSpeed.sqrMagnitude > 0.01f)
            {
                Vector3 direccionMirada = CurrentSpeed;
                direccionMirada.y = 0f;
                transform.rotation = Quaternion.LookRotation(direccionMirada.normalized);
            }
        }
    }

    // Puntos E y F: Interacciones físicas mediante Unity
    new private void OnCollisionEnter(Collision collision)
    {
        // F) Puntos extra: Multiplicación de la velocidad por el daño
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            float velocidadDeImpacto = CurrentSpeed.magnitude;

            // Ejemplo de cálculo: Daño base de 10 + (velocidad * 3)
            float danoTotal = 10f + (velocidadDeImpacto * 3f);

            Debug.Log($"¡Embestida del Enemigo Pesado! Velocidad: {velocidadDeImpacto:F1} | Daño infligido: {danoTotal:F1}");

            // Aquí mandarías a llamar al script de salud de tu jugador:
            // collision.gameObject.GetComponent<PlayerHealth>().RecibirDano(danoTotal);

            // Opcional pero recomendado: perder inercia tras golpear al jugador
            CurrentSpeed = Vector3.zero;
        }
        // E) Cuando choque con una pared, su velocidad actual baja completamente a 0.0
        else if (collision.gameObject.CompareTag("Wall") || collision.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            // Asegúrate de excluir el suelo para que no se congele al tocar el piso
            if (!collision.gameObject.CompareTag("Floor"))
            {
                CurrentSpeed = Vector3.zero;
                Debug.Log("¡El Enemigo Pesado chocó con una pared! Velocidad reducida a 0.");
            }
        }
    }
}