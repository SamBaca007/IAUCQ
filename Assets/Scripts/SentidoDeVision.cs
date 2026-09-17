using System;
using System.Collections.Generic;
using UnityEngine;

public class SentidoDeVision : MonoBehaviour
{
    [SerializeField]
    private float radioDeColliderDeDeteccion = 5.0f;

    private SphereCollider _colliderDeDeteccion;

    [SerializeField]
    private List<GameObject> objetosConocidos = new List<GameObject>();

    public List<GameObject> GetObjetosConocidos()
    {
        return objetosConocidos;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _colliderDeDeteccion = GetComponent<SphereCollider>();
        if (_colliderDeDeteccion == null)
        {
            Debug.LogError("No hay un sphere collider asignado a este gameObject", gameObject);
            return;
        }

        _colliderDeDeteccion.radius = radioDeColliderDeDeteccion;

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("On trigger Enter contra: " + other.gameObject.name, gameObject);

        foreach (var conocido in objetosConocidos)
        {
            if (conocido == other.gameObject)
            {
                Debug.LogWarning("On trigger enter con el objeto " + other.gameObject.name + " pero ya lo conocía", gameObject);
                return;
            }
        }

        objetosConocidos.Add(other.gameObject);

        foreach (var conocido in objetosConocidos)
        {
            Debug.Log("el objeto " + conocido + " está en los objetos conocidos", gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("On trigger exit contra: " + other.gameObject.name, gameObject);
        objetosConocidos.Remove(other.gameObject);

        if (objetosConocidos.Count == 0)
        {
            Debug.Log("ya no se conoce ningún objeto", gameObject);
        }
        else
        {
            foreach (var conocido in objetosConocidos)
            {
                Debug.Log("el objeto " + conocido + " está en los objetos conocidos", gameObject);
            }
        }
    }
}