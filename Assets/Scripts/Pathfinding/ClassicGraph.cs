using System.Collections.Generic;
using UnityEngine;

public class Arista
{
    public Arista(NodoDeGrafo a, NodoDeGrafo b)
    {
        A = a;
        B = b;
    }

    // NodoDeGrafo A
    public NodoDeGrafo A;

    // NodoDeGrafo B
    public NodoDeGrafo B;

    // Peso o propiedades
    public float Peso;
}

public class NodoDeGrafo
{
    public NodoDeGrafo(char _id)
    {
        ID = _id;
        Padre = null;
    }

    // Representan "algo de importancia"
    // Por ejemplo, una identidad
    public char ID; // nos permite diferenciar entre nodos, y pues también saber si ya llegamos al nodo deseado o aún no.

    public NodoDeGrafo Padre;

    // la forma complicada de hacer esto es que el NodoDeGrafo no conoce a sus vecinos tal cual, sino que 
    // conoce a las aristas que están conectadas a él.
    public List<Arista> AristasVecinas = new List<Arista>();
}


public class ClassicGraph : MonoBehaviour
{
    public List<NodoDeGrafo> NodosGrafo = new List<NodoDeGrafo>();
    public List<Arista> AristasGrafo = new List<Arista>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Creamos los nodos y aristas de nuestro grafo
        NodoDeGrafo nodoDeGrafoA = new NodoDeGrafo('A');
        NodoDeGrafo nodoDeGrafoB = new NodoDeGrafo('B');
        NodoDeGrafo nodoDeGrafoC = new NodoDeGrafo('C');
        NodoDeGrafo nodoDeGrafoD = new NodoDeGrafo('D');
        NodoDeGrafo nodoDeGrafoE = new NodoDeGrafo('E');
        NodoDeGrafo nodoDeGrafoF = new NodoDeGrafo('F');
        NodoDeGrafo nodoDeGrafoG = new NodoDeGrafo('G');
        NodoDeGrafo nodoDeGrafoH = new NodoDeGrafo('H');

        // OJO: aquí todavía no les asignamos sus vecinos (las aristas a sus vecinos)
        // Primero hay que crear todas las aristas.
        Arista AB = new Arista(nodoDeGrafoA, nodoDeGrafoB);
        Arista AE = new Arista(nodoDeGrafoA, nodoDeGrafoE);
        Arista BC = new Arista(nodoDeGrafoB, nodoDeGrafoC);
        Arista BD = new Arista(nodoDeGrafoB, nodoDeGrafoD);
        Arista EF = new Arista(nodoDeGrafoE, nodoDeGrafoF);
        Arista EG = new Arista(nodoDeGrafoE, nodoDeGrafoG);
        Arista EH = new Arista(nodoDeGrafoE, nodoDeGrafoH);

        // Ahora sí podemos decirle a los nodos cuáles son las aristas que conocen.
        nodoDeGrafoA.AristasVecinas.Add(AB);
        nodoDeGrafoB.AristasVecinas.Add(AB);

        nodoDeGrafoA.AristasVecinas.Add(AE);
        nodoDeGrafoE.AristasVecinas.Add(AE);

        nodoDeGrafoB.AristasVecinas.Add(BC);
        nodoDeGrafoC.AristasVecinas.Add(BC);

        nodoDeGrafoB.AristasVecinas.Add(BD);
        nodoDeGrafoD.AristasVecinas.Add(BD);

        nodoDeGrafoE.AristasVecinas.Add(EF);
        nodoDeGrafoF.AristasVecinas.Add(EF);

        nodoDeGrafoE.AristasVecinas.Add(EG);
        nodoDeGrafoG.AristasVecinas.Add(EG);

        nodoDeGrafoE.AristasVecinas.Add(EH);
        nodoDeGrafoH.AristasVecinas.Add(EH);

        NodosGrafo.Add(nodoDeGrafoA);
        NodosGrafo.Add(nodoDeGrafoB);
        NodosGrafo.Add(nodoDeGrafoC);
        NodosGrafo.Add(nodoDeGrafoD);
        NodosGrafo.Add(nodoDeGrafoE);
        NodosGrafo.Add(nodoDeGrafoF);
        NodosGrafo.Add(nodoDeGrafoG);
        NodosGrafo.Add(nodoDeGrafoH);

        AristasGrafo.Add(AB);
        AristasGrafo.Add(AE);
        AristasGrafo.Add(BC);
        AristasGrafo.Add(BD);
        AristasGrafo.Add(EF);
        AristasGrafo.Add(EG);
        AristasGrafo.Add(EH);

        // SumarA10(1);
        //
        // Suma1(0);

        NodoDeGrafo nodoDeGrafoOrigen = nodoDeGrafoA;
        // El nodo raíz es su propio nodo padre, para evitar que alguien más intente convertirse en su padre.
        nodoDeGrafoOrigen.Padre = nodoDeGrafoOrigen;

        NodoDeGrafo nodoDeGrafoDestino = nodoDeGrafoD;

        if (DFSRecursiva(nodoDeGrafoOrigen, nodoDeGrafoDestino))
        {
            Debug.Log($"Sí hubo un camino desde el nodo {nodoDeGrafoOrigen.ID} hasta el nodo {nodoDeGrafoDestino.ID}");
        }
        else
        {
            Debug.Log($"No hubo un camino desde el nodo {nodoDeGrafoOrigen.ID} hasta el nodo {nodoDeGrafoDestino.ID}");
        }
    }

    // Búsqueda en Profundidad o "Depth-First Search" (por eso es DFS)
    // Nos dice si sí llegó o no, desde un punto A hasta un punto B.
    bool DFSRecursiva(NodoDeGrafo origen, NodoDeGrafo destino)
    {

        // La condición de parada principal es: ¿ya llegué a mi destino?
        // Cuando el origen y del destino sean iguales.
        if (origen.ID == destino.ID)
        {
            // Entonces ya llegamos, y terminamos la recursión.
            return true;
        }

        // Una función recursiva es una función que se manda a llamar a sí para resolver un problema que es cada vez más pequeño.
        // La parte que hace "más pequeño" el problema aquí es que lo mandas a llamar a partir de un vecino, y ese
        // vecino ya no puede volver a ti.
        foreach (var aristaVecina in origen.AristasVecinas)
        {
            // Tenemos que saber en qué sentido se va a usar la arista
            if (aristaVecina.A.ID == origen.ID) // entonces este es el nodo que está mandando a llamar esta función actualmente
            {
                // Entonces este nodo no ha sido visitado, y por lo tanto tiene que seguir la recursión.
                if (aristaVecina.B.Padre == null)
                {
                    aristaVecina.B.Padre = origen;

                    // entonces queremos al nodo del otro lado de la arista
                    if (DFSRecursiva(aristaVecina.B, destino) == true)
                    {
                        Debug.Log($"el nodo {origen.ID} fue parte del camino");

                        // Si entra aquí, significa que uno de sus vecinos sí encontró la ruta, entonces este nodo es parte de la ruta 
                        // al destino y por lo tanto también ya puede terminar.
                        return true;
                    }
                }
            }
            else
            {
                // Entonces este nodo no ha sido visitado, y por lo tanto tiene que seguir la recursión.
                if (aristaVecina.A.Padre == null)
                {
                    aristaVecina.A.Padre = origen;

                    if (DFSRecursiva(aristaVecina.A, destino) == true)
                    {
                        Debug.Log($"el nodo {origen.ID} fue parte del camino");

                        // Si entra aquí, significa que uno de sus vecinos sí encontró la ruta, entonces este nodo es parte de la ruta 
                        // al destino y por lo tanto también ya puede terminar.
                        return true;
                    }
                }
            }
        }

        Debug.Log($"el nodo {origen.ID} NO fue parte del camino");

        // Si el nodo "origen" actual llega a este return, es porque ninguno de sus vecinos llevó al destino, 
        // y pues regresa false para salirse de la pila de llamadas.
        // Si el primer nodo que mando a llamar la recursión llega a este punto, quiere decir que no hay camino, tal cual.
        return false;
    }

    // Función recursiva que suma de 1 en 1 desde el número que se le dé hasta el 10.
    void SumarA10(int inicio)
    {
        // Stop Condition (condición para parar la recursión). Toda función recursiva debe tenerla, si no se cicla infinitamente.
        if (inicio > 10)
        {
            return; // detenemos la recursión, ya no se vuelve a llamar a sí misma.
        }

        Debug.Log("El valor actual de la suma es: " + inicio);
        SumarA10(inicio + 1);
    }

    void Suma1(int a)
    {
        a = a + 1;
        Debug.Log("El valor de a es: " + a);
        Suma2(a);
    }

    void Suma2(int a)
    {
        a = a + 2;
        Debug.Log("El valor de a es: " + a);
        Suma3(a);
    }

    void Suma3(int a)
    {
        a = a + 3;
        Debug.Log("El valor de a es: " + a);
    }

    // Update is called once per frame
    void Update()
    {

    }
}