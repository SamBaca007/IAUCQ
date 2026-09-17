using System;
using System.Collections.Generic;
using UnityEngine;

public class DeteccionPorProximidadPorCollider : MonoBehaviour
{
    public float RangoDeDeteccion = 5.0f;

    private SphereCollider _colliderDeDeteccion;

    public GameObject gameObjectEjemplo;

    public GameObject ObjetoADetectar;

    public bool imprimirMensajesDeDebug = false;

    [SerializeField] private List<GameObject> objetosConocidos = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        objetosConocidos.Add(gameObject);
        foreach (var objeto in objetosConocidos)
        {
            Debug.Log(objeto.name + " está en los objetos conocidos");
        }

        _colliderDeDeteccion = GetComponent<SphereCollider>();
        if (_colliderDeDeteccion != null)
        {
            _colliderDeDeteccion.radius = RangoDeDeteccion;

            if (imprimirMensajesDeDebug)
                Debug.Log("radio del collider propio es: " + _colliderDeDeteccion.radius, gameObject);
        }

        SphereCollider sphereCollider = gameObjectEjemplo.GetComponent<SphereCollider>();
        if(sphereCollider != null)
        {
            if (imprimirMensajesDeDebug)
                Debug.Log(message: "Radio del collider de gameObjectEjemplo es: " +
                    gameObjectEjemplo.GetComponent<SphereCollider>().radius, gameObject);
        }
        else
        {
                Debug.LogWarning(message: "el gameObject: " + gameObjectEjemplo.name + " no trae un SphereCollider.", gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
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

    private void OnTriggerEnter(Collider other)
    {
        if (imprimirMensajesDeDebug)
            Debug.Log("On Trigger enter contra: " + other.gameObject.name, gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (imprimirMensajesDeDebug)
            Debug.Log("On Trigger Exit contra: " + other.gameObject.name, gameObject);
    }
}
