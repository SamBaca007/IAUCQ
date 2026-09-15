using UnityEngine;
using UnityEngine.Rendering;

public class DeteccionPorProximidad : MonoBehaviour
{
    public GameObject ObjetoADetectar;
    public Transform PosicionDeObjetoADetectar;
    public float RangoDeDeteccion = 5.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 diferenciaDePosicion = gameObject.transform.position - ObjetoADetectar.transform.position;

        float distanciaEntrePosiciones = Mathf.Sqrt(diferenciaDePosicion.x * diferenciaDePosicion.x +
                                         diferenciaDePosicion.y * diferenciaDePosicion.y +
                                         diferenciaDePosicion.z * diferenciaDePosicion.z);

        if(distanciaEntrePosiciones > RangoDeDeteccion)
        {
            Debug.Log(message: "El objeto a detectar está fuera de mi rango de detección");
        }
        else
        {
            Debug.Log(message: "El objeto a detectar está dentro de mi rango de detección, ha sido detectado.");
        }
    }
}
