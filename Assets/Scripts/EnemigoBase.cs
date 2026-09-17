using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class EnemigoBase : MonoBehaviour
{
    [SerializeField]
    private float maxSpeed = 5.0f;

    private Vector3 posicionInicial;
    private float tiempoTranscurrido = 0;

    private SentidoDeVision _sentidoDeVision;
    private GameObject _objetivo;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        posicionInicial = transform.position;

        _sentidoDeVision = GetComponent<SentidoDeVision>();
        if (_sentidoDeVision == null)
        {
            Debug.LogError("No hay componente SentidoDeVision asignado", gameObject);
        }

    }

    // Update is called once per frame
    void Update()
    {
        List<GameObject> objetosConocidos = _sentidoDeVision.GetObjetosConocidos();

        if (objetosConocidos.Count > 0)
        {
            _objetivo = objetosConocidos[0];
        }

        Vector3 puntaMenosCola = _objetivo.transform.position - transform.position;

        Vector3 puntMenosColaNormalizado = puntaMenosCola.normalized;

        transform.position += puntMenosColaNormalizado * (Time.deltaTime * maxSpeed);
        Debug.Log("El transform.position que va aumentando cada cuadro es: " + transform.position);

    }
}