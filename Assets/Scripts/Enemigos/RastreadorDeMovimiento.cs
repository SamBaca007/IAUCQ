using UnityEngine;

public class RastreadorDeMovimiento : MonoBehaviour
{
    private Vector3 ultimaPosicion;

    void Start()
    {
        ultimaPosicion = transform.position;
    }

    void LateUpdate()
    {
        if (Vector3.Distance(transform.position, ultimaPosicion) > 0.001f)
        {
            // Imprime la pila de llamadas exacta que provocó el cambio en este fotograma
            Debug.LogError($"¡MOVIMIENTO DETECTADO!\n" +
                           $"De: {ultimaPosicion} -> A: {transform.position}\n" +
                           $"ORIGEN DEL CAMBIO:\n{System.Environment.StackTrace}");

            Debug.Break(); // Pausa la escena
            ultimaPosicion = transform.position;
        }
    }
}