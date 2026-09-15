using UnityEngine;

public class DeteccionPorProximidadPorCollider : MonoBehaviour
{
    public float RangoDeDeteccion = 5.0f;

    private SphereCollider _colliderDeDeteccion;

    public GameObject gameObjectEjemplo;

    public GameObject ObjetoADetectar;

    public bool imprimirMensajesDeDebug = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _colliderDeDeteccion = GetComponent<SphereCollider>();
        if(imprimirMensajesDeDebug)
        Debug.Log(message: "Radio del collider propio es: " + _colliderDeDeteccion.radius, gameObject);

        SphereCollider sphereCollider = gameObjectEjemplo.GetComponent<SphereCollider>();
        if(sphereCollider != null)
        {
            if (imprimirMensajesDeDebug)
                Debug.Log(message: "Radio del collider de gameObjectEjemplo es: " + gameObjectEjemplo.GetComponent<SphereCollider>().radius,
            gameObject);
        }
        else
        {
                Debug.LogWarning(message: "el gameObject: " + gameObjectEjemplo.name + " no trae un SphereCollider.", gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 diferenciaDePosicion = gameObject.transform.position - ObjetoADetectar.transform.position;

        float distanciaEntrePosiciones = Mathf.Sqrt(diferenciaDePosicion.x * diferenciaDePosicion.x +
                                         diferenciaDePosicion.y * diferenciaDePosicion.y +
                                         diferenciaDePosicion.z * diferenciaDePosicion.z);

        if (distanciaEntrePosiciones > _colliderDeDeteccion.radius)
        {
            if (imprimirMensajesDeDebug)
                Debug.Log(message: "El objeto a detectar está fuera de mi rango de detección");
        }
        else
        {
            if (imprimirMensajesDeDebug)
                Debug.Log(message: "El objeto a detectar está dentro de mi rango de detección, ha sido detectado.");
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (imprimirMensajesDeDebug)
            Debug.Log(message: "On collision enter contra: " + other.gameObject.name, gameObject);
    }

    private void OnCollisionExit(Collision other)
    {
        if (imprimirMensajesDeDebug)
            Debug.Log(message: "On collision exit contra: " + other.gameObject.name, gameObject);
    }
}
