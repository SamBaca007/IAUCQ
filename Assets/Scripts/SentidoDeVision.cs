using System;
using System.Collections.Generic;
using UnityEngine;

public class SentidoDeVision : MonoBehaviour
{
    [SerializeField]
    private float radioDeColliderDeDeteccion = 5.0f;

    public float GetColliderDetectionRadius()
    {
        return radioDeColliderDeDeteccion;
    }

    private SphereCollider _colliderDeDeteccion;

    [SerializeField]
    private List<GameObject> objetosConocidos = new List<GameObject>();

    public List<GameObject> GetKnownObjects()
    {
        return objetosConocidos;
    }

    private List<GameObject> obstaculosConocidos = new List<GameObject>();
    public List<GameObject> GetKnownObstacles()
    {
        return obstaculosConocidos;
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
        // 1. Si el objeto está en la capa "Paredes" (o "Obstaculos"), lo añade a la lista de obstáculos
        if (other.gameObject.layer == LayerMask.NameToLayer("Paredes"))
        {
            if (!obstaculosConocidos.Contains(other.gameObject))
            {
                obstaculosConocidos.Add(other.gameObject);
            }
            return;
        }

        // 2. Si no es pared, se procesa como objeto conocido (Jugador, otros entes)
        if (!objetosConocidos.Contains(other.gameObject))
        {
            objetosConocidos.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Remueve obstáculos o paredes cuando el enemigo se aleja de ellos
        if (other.gameObject.layer == LayerMask.NameToLayer("Paredes"))
        {
            obstaculosConocidos.Remove(other.gameObject);
            return;
        }

        objetosConocidos.Remove(other.gameObject);
    }
}