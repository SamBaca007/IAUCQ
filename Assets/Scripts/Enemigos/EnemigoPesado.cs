using System.Collections.Generic;
using UnityEngine;

public class EnemigoPesado : EnemigoBase
{
    void Update()
    {
        List<GameObject> objetosConocidos = SentidoDeVision.GetKnownObjects();
        if (objetosConocidos.Count > 0)
        {
            Target = objetosConocidos[0];

            // Para predecir, necesitamos saber a qué velocidad se mueve el jugador
            Vector3 velocidadJugador = Vector3.zero;
            Rigidbody rbJugador = Target.GetComponent<Rigidbody>();
            if (rbJugador != null)
            {
                velocidadJugador = rbJugador.linearVelocity;
            }

            // Llamamos a Pursuit en lugar de Seek
            Vector3 steeringForce = SteeringBehaviors.Pursuit(
                Target.transform.position,
                velocidadJugador,
                transform.position,
                maxSpeed,
                CurrentSpeed,
                maxForce
            );

            ActualizarAceleracionVelocidadYPosicion(steeringForce);
        }
        else
        {
            Target = null;
        }
    }
}