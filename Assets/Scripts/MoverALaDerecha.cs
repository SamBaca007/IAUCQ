using UnityEngine;

public class MoverALaDerecha : MonoBehaviour
{
    public int MyNumber;
    public bool ImprimirHolaMundo = true;

    public float TiempoParaCrear = 3.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (ImprimirHolaMundo)
        {
            Debug.Log(message: "Hola Mundo", gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(x: transform.position.x + 1, transform.position.y, transform.position.z);
        Debug.Log(message: "Update. La posición ahora es: " + transform.position.x);

        transform.position = new Vector3(x: transform.position.x + 1, transform.position.y + 1 * Time.deltaTime, transform.position.z);
        Debug.Log(message: "Update. La posición ahora es: " + transform.position.y);
    }
}
