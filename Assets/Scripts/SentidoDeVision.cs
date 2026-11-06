using System;
using System.Collections.Generic;
using UnityEngine;

public class SentidoDeVision : MonoBehaviour
{
    [Header("Configuración de Detección")]
    [SerializeField] private float radioDeColliderDeDeteccion = 5.0f;

    [Tooltip("Nombre de la capa para añadir a la lista de obstáculos conocidos")]
    [SerializeField] private string capaParedes = "Paredes";

    [Tooltip("Capas físicas que cortarán la línea de visión (Ej. Paredes, Obstáculos)")]
    [SerializeField] private LayerMask capasQueBloqueanVision;

    private SphereCollider _colliderDeDeteccion;

    [SerializeField] private List<GameObject> objetosConocidos = new List<GameObject>();
    private List<GameObject> obstaculosConocidos = new List<GameObject>();

    public float GetColliderDetectionRadius()
    {
        return radioDeColliderDeDeteccion;
    }

    public List<GameObject> GetKnownObjects()
    {
        List<GameObject> objetosVisibles = new List<GameObject>();

        foreach (var obj in objetosConocidos)
        {
            if (obj == null) continue;

            Vector3 direccion = obj.transform.position - transform.position;
            float distancia = direccion.magnitude;

            // Trazamos un rayo desde el enemigo hacia el objeto. 
            // Si NO choca con las capas bloqueadoras, significa que lo estamos viendo.
            if (!Physics.Raycast(transform.position, direccion.normalized, distancia, capasQueBloqueanVision))
            {
                objetosVisibles.Add(obj);
            }
        }
        return objetosVisibles;
    }

    public List<GameObject> GetKnownObstacles()
    {
        return obstaculosConocidos;
    }

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(capaParedes))
        {
            if (!obstaculosConocidos.Contains(other.gameObject))
            {
                obstaculosConocidos.Add(other.gameObject);
            }
            return;
        }

        if (!objetosConocidos.Contains(other.gameObject))
        {
            objetosConocidos.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(capaParedes))
        {
            obstaculosConocidos.Remove(other.gameObject);
            return;
        }

        objetosConocidos.Remove(other.gameObject);
    }
}