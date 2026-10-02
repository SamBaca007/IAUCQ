using UnityEngine;

public class Bala : MonoBehaviour
{
    [Tooltip("Tiempo en segundos antes de que la bala se autodestruya si no choca con nada")]
    [SerializeField] private float lifeTime = 3f;

    void Start()
    {
        // Se destruye automáticamente después de 'lifeTime' segundos
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision other)
    {
        // Detectar con qué chocamos usando las Layers
        int layerParedes = LayerMask.NameToLayer("Paredes");
        int layerEnemigos = LayerMask.NameToLayer("Enemigos");

        if (other.gameObject.layer == layerParedes || other.gameObject.layer == layerEnemigos)
        {
            // OJO: Tu EnemigoBase ya se encarga de restarse vida a sí mismo al chocar con esta bala,
            // así que aquí solo necesitamos destruir la bala.
            Destroy(gameObject);
        }
    }
}