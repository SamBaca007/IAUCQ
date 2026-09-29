using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

// Para más explicaciones sobre corrutinas, este video está cortito y muy útil:
// https://youtu.be/kUP6OK36nrM?si=qSSAzcoA13nC6j8m

public class EnemigoAlternante : EnemigoBase
{

    private bool _estaUsandoSeek = true;
    [SerializeField] private float tiempoParaCambiarEntreSeekYFlee = 2.0f;
    private float _tiempoTranscurrido = 0.0f;

    private Coroutine _coroutineImprimirCada5SegundosHastaQueMeDetengan;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        base.Start();

        StartCoroutine(ImprimirCadaSegundoDuranteDiezSegundos());
        if (_coroutineImprimirCada5SegundosHastaQueMeDetengan == null) 
            _coroutineImprimirCada5SegundosHastaQueMeDetengan = StartCoroutine(
            ImprimirCada5SegundosHastaQueMeDetengan());
    }

    // Update is called once per frame
    void Update()
    {
        List<GameObject> objetosConocidos = SentidoDeVision.GetObjetosConocidos();
        if (objetosConocidos.Count > 0)
        {
            AlternarSeekYFlee();

            Objetivo = objetosConocidos[0];

            // Documento original de los steering behaviors: https://www.red3d.com/cwr/papers/1999/gdc99steer.pdf

            Vector3 steeringForce;
            if (_estaUsandoSeek)
            {
                steeringForce = Seek();
            }
            else
            {
                steeringForce = Flee();
            }

            ActualizarAceleracionVelocidadYPosicion(steeringForce);
        }



    }
    private IEnumerator ImprimirCadaSegundoDuranteDiezSegundos()
    {
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(1.0f);
            Debug.Log($"Han pasado: {i} segundos");
        }
        StopCoroutine(_coroutineImprimirCada5SegundosHastaQueMeDetengan);
        _coroutineImprimirCada5SegundosHastaQueMeDetengan = null;
    }
    private IEnumerator ImprimirCada5SegundosHastaQueMeDetengan()
    {
        int tiempoTranscurrido = 0;
        while (true)
        {
            yield return new WaitForSeconds(5.0f);
            tiempoTranscurrido += 5;
            Debug.Log($"Han pasado: {tiempoTranscurrido} segundos");
        }
    }
    void AlternarSeekYFlee()
    {
        _tiempoTranscurrido += Time.deltaTime;
        if (_tiempoTranscurrido >= tiempoParaCambiarEntreSeekYFlee)
        {
            _estaUsandoSeek = !_estaUsandoSeek;
            _tiempoTranscurrido = 0.0f;
        }
    }
}