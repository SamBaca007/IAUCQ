using System.Collections.Generic;
using UnityEngine;
public class EnemigoPesado : EnemigoBase
{


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        List<GameObject> objetosConocidos = SentidoDeVision.GetObjetosConocidos();
        if (objetosConocidos.Count > 0)
        {

            Objetivo = objetosConocidos[0];

            // Documento original de los steering behaviors: https://www.red3d.com/cwr/papers/1999/gdc99steer.pdf

            Vector3 steeringForce = Seek();

            ActualizarAceleracionVelocidadYPosicion(steeringForce);
        }
    }
}