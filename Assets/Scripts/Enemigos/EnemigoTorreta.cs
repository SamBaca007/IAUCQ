using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemigoTorreta : EnemigoBase
{
    [Header("Configuración Torreta")]
    [SerializeField] private float RotationDegrees = 45f;
    [SerializeField] private float DegreeInterval = 2f;
    [SerializeField] private float ForgetTime = 3f;

    [Header("Disparo")]
    [SerializeField] private GameObject balaPrefab;
    [SerializeField] private Transform puntoDisparo;
    [SerializeField] private float cadenciaDisparo = 1f;

    [Header("Visualización del Cono")]
    [SerializeField] private Light focoVision; // Asigna una Spotlight aquí

    private bool jugadorDetectado = false;
    private float temporizadorOlvido;
    private float temporizadorDisparo;
    private Coroutine rutinaRotacion;

    // Nota: Asegúrate de que en EnemigoBase.cs el Start() sea "protected virtual void Start()"
    protected override void Start()
    {
        base.Start();

        // ¡NUEVO!: Congelamiento forzado por código
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Desconecta la física
            rb.constraints = RigidbodyConstraints.FreezeAll; // Congela posición y rotación física
        }

        // B) Iniciar la rotación mediante una Corrutina
        rutinaRotacion = StartCoroutine(RutinaRotarTorreta());

        // E) Sincronizar el componente visual (Spotlight) con los datos matemáticos
        if (focoVision != null && SentidoDeVision != null)
        {
            focoVision.type = LightType.Spot;
            // Configurar el ángulo del foco para que coincida con el campo de visión matemático
            // focoVision.spotAngle = SentidoDeVision.GetVisionAngle();
        }
    }

    // Corrutina que maneja los giros pausados de la torreta
    private IEnumerator RutinaRotarTorreta()
    {
        while (true)
        {
            yield return new WaitForSeconds(DegreeInterval);
            transform.Rotate(0, RotationDegrees, 0);
        }
    }

    protected override void Update()
    {
        List<GameObject> objetosConocidos = SentidoDeVision.GetKnownObjects();
        bool viendoAlJugador = false;

        // A) Filtrar la visión
        foreach (var obj in objetosConocidos)
        {
            if (obj != null && obj.layer == LayerMask.NameToLayer("Player"))
            {
                Target = obj;
                viendoAlJugador = true;
                break;
            }
        }

        if (viendoAlJugador)
        {
            // C) Si lo detecta, detiene la corrutina de giro
            if (!jugadorDetectado)
            {
                jugadorDetectado = true;
                if (rutinaRotacion != null)
                {
                    StopCoroutine(rutinaRotacion);
                    rutinaRotacion = null;
                }
                if (focoVision != null) focoVision.color = Color.red; // Feedback visual de alerta
            }

            ApuntarYDisparar(Target.transform.position);

            // Reinicia la memoria mientras lo siga viendo
            temporizadorOlvido = ForgetTime;
        }
        else if (jugadorDetectado)
        {
            // D) Ya no lo ve, pero lo sigue recordando y disparando hacia su última posición
            temporizadorOlvido -= Time.deltaTime;

            if (Target != null)
            {
                // "Aunque ya no estés en su cono de visión, te seguirá disparando"
                ApuntarYDisparar(Target.transform.position);
            }

            // D) Se le acaba la memoria, vuelve a la normalidad
            if (temporizadorOlvido <= 0f)
            {
                jugadorDetectado = false;
                Target = null;
                rutinaRotacion = StartCoroutine(RutinaRotarTorreta());

                if (focoVision != null) focoVision.color = Color.white; // Vuelve a color normal
            }
        }
    }

    private void ApuntarYDisparar(Vector3 posicionObjetivo)
    {
        // 1. Apuntar directamente a la posición (Sobrescribe la rotación de la corrutina)
        Vector3 direccionAlJugador = posicionObjetivo - transform.position;
        direccionAlJugador.y = 0f;
        if (direccionAlJugador != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direccionAlJugador.normalized);
        }

        // 2. Controlar la cadencia y disparar
        temporizadorDisparo -= Time.deltaTime;
        if (temporizadorDisparo <= 0f)
        {
            if (balaPrefab != null && puntoDisparo != null)
            {
                // Aquí instancias la bala. Dependiendo de cómo funcione tu script de bala,
                // tal vez necesites pasarle la dirección o la velocidad.
                Instantiate(balaPrefab, puntoDisparo.position, puntoDisparo.rotation);
            }
            temporizadorDisparo = cadenciaDisparo;
        }
    }

    // Sobrescribimos el Wander para que la torreta nunca merodee por accidente
    protected new Vector3 Wander()
    {
        return Vector3.zero;
    }

    // Bloqueamos cualquier intento del padre de mover el transform de esta torreta
    protected new void ActualizarAceleracionVelocidadYPosicion(Vector3 steeringForce)
    {
        // La dejamos vacía. Aquí no se mueve nada.
    }
}