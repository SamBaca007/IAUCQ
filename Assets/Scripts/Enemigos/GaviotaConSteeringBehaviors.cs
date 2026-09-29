using UnityEngine;

public class GaviotaConSteeringBehaviors : MonoBehaviour
{
    private Vector3 _objetivo = Vector3.zero;

    [Tooltip("Límite de velocidad al cual puede ir este agente")]
    [SerializeField] protected float maximaVelocidad = 5.0f;
    [SerializeField] protected float maximaFuerza = 5.0f;

    [Tooltip("Velocidad actual que tiene este agente. Está limitada por maxSpeed")]
    protected Vector3 VelocidadActual = Vector3.zero;

    [SerializeField] protected float masa = 1.0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _objetivo = new Vector3(Random.Range(-15f, 15f), Random.Range(0, 25f), Random.Range(-15f, 15f));
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 steeringForce = Seek();

        Vector3 steeringForceGenerica = SteeringBehaviors.Seek(_objetivo, transform.position,
            maximaVelocidad, VelocidadActual, maximaFuerza);

        ActualizarAceleracionVelocidadYPosicion(steeringForce);

        transform.position = SteeringBehaviors.ActualizarAceleracionVelocidadYPosicion(
            steeringForce, masa, maximaVelocidad, ref VelocidadActual, transform.position);
    }
    protected Vector3 Seek()
    {
        return SteeringBehaviors.Seek(_objetivo, transform.position,
            maximaVelocidad, VelocidadActual, maximaFuerza);
    }
    void ActualizarAceleracionVelocidadYPosicion(Vector3 steeringForce)
    {
        transform.position = SteeringBehaviors.ActualizarAceleracionVelocidadYPosicion(
            steeringForce, masa, maximaVelocidad, ref VelocidadActual, transform.position);
    }

}