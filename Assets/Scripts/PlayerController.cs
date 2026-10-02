using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float Speed = 5f; // Ajustable al gusto

    [Header("Configuración de Combate")]
    public int hp = 3;
    public GameObject balaPrefab; // Asigna tu prefab de bala en el Inspector
    public Transform puntoDeDisparo; // Un objeto vacío hijo del player para saber de dónde sale la bala
    public float velocidadBala = 15f;

    private Rigidbody rb;
    private Vector3 MovementDirection;

    void Start()
    {
        // Obtenemos la referencia al Rigidbody automáticamente
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 1. Leer el Input
        // Usamos GetAxisRaw para que el movimiento sea responsivo e inmediato (sin derrapes)
        float movX = Input.GetAxisRaw("Horizontal"); // A/D o Flechas Izq/Der
        float movZ = Input.GetAxisRaw("Vertical");   // W/S o Flechas Arr/Aba

        // Creamos el vector de dirección. Asumimos que nos movemos en X y Z (el piso), y Y es arriba.
        // .normalized hace que caminar en diagonal no vaya más rápido.
        MovementDirection = new Vector3(movX, 0f, movZ).normalized;

        // Rotar al Jugador
        // Si nos estamos moviendo (el vector no es cero), hacemos que el frente del jugador apunte a esa dirección
        if (MovementDirection != Vector3.zero)
        {
            // Esto voltea al jugador hacia donde se mueve
            transform.forward = MovementDirection;

            // Nota: Si se quisiera un giro fuera suave, se usaría Quaternion.Slerp, 
            // pero con esto es instantáneo y muy �til para shooters tipo Binding of Isaac.
        }
        // 2. Disparo
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            Disparar();
        }

        // 3. Reinicio de nivel manual
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
    }

    void FixedUpdate()
    {
        // Aplica la velocidad en X y Z (movimiento), pero deja intacta la velocidad actual en Y (gravedad)
        rb.linearVelocity = new Vector3(MovementDirection.x * Speed, rb.linearVelocity.y, MovementDirection.z * Speed);
    }

    void Disparar()
    {
        if (balaPrefab != null && puntoDeDisparo != null)
        {
            // Creamos la bala
            GameObject bala = Instantiate(balaPrefab, puntoDeDisparo.position, transform.rotation);

            // Le damos velocidad hacia el frente del jugador
            Rigidbody rbBala = bala.GetComponent<Rigidbody>();
            if (rbBala != null)
            {
                rbBala.linearVelocity = transform.forward * velocidadBala;
            }
        }
    }
    public void TakeDamage(int cantidad)
    {
        hp -= cantidad;
        Debug.Log("Jugador recibió daño. HP actual: " + hp);

        if (hp <= 0)
        {
            Debug.Log("El jugador ha muerto.");
            RestartLevel(); // O puedes desactivar el gameObject
        }
    }
    void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}