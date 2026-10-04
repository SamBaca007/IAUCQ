using UnityEngine;

public class BalaEnemiga : MonoBehaviour
{
    public int Daño = 1;
    [SerializeField] private float velocidadBala = 15f; // Ajusta la velocidad aquí
    [SerializeField] private float lifeTime = 3f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Le da impulso hacia adelante apenas nace (Compatible con Unity 6)
            rb.linearVelocity = transform.forward * velocidadBala;
        }

        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision other)
    {
        int layerParedes = LayerMask.NameToLayer("Paredes");
        int layerJugador = LayerMask.NameToLayer("Player");

        if (other.gameObject.layer == layerParedes || other.gameObject.layer == layerJugador)
        {
            Destroy(gameObject);
        }
    }
}