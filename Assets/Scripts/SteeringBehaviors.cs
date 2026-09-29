using UnityEngine;

public class SteeringBehaviors
{
    public static Vector3 Seek(Vector3 objetivo, Vector3 posicionActual,
        float maximaVelocidadParam, Vector3 velocidadActual, float maximaFuerzaParam)
    {
 
        Vector3 puntaMenosCola = objetivo - posicionActual;

        Vector3 direccionDeseada = puntaMenosCola.normalized;

        Vector3 velocidadDeseada = direccionDeseada * maximaVelocidadParam;

        Vector3 steeringVelocity = velocidadDeseada - velocidadActual;

        Vector3 steeringForce = Vector3.ClampMagnitude(steeringVelocity, maximaFuerzaParam);

        return steeringForce;
    }

    public static Vector3 Flee(Vector3 objetivo, Vector3 posicionActual,
        float maximaVelocidad, Vector3 velocidadActual, float maximaFuerza)
    {
        return Seek(objetivo, posicionActual, maximaVelocidad, velocidadActual, maximaFuerza) * -1f;
    }

    public static Vector3 ActualizarAceleracionVelocidadYPosicion(Vector3 steeringForce, float masaParam,
        float maximaVelocidad, ref Vector3 velocidadActual, Vector3 posicion)
    {
        Vector3 aceleracion = steeringForce / masaParam;

        velocidadActual = Vector3.ClampMagnitude(velocidadActual + aceleracion * Time.deltaTime,
            maximaVelocidad);

        return posicion + velocidadActual * Time.deltaTime;
    }

}