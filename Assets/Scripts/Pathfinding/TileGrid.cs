/*
 * Nombres de los integrantes: Samuel Baca
 * Práctica: Implementación de BFS (Breadth-First Search) en TileGrid
 * Notas: Se eliminaron los valores hardcodeados en los ciclos for utilizando System.Enum.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


// Enum = Enumeration -> enumeración
// Dicen que un valor numérico tiene cierta interpretación específica
public enum EDirecciones : byte
{
    Arriba = 0,
    ArribaDerecha = 1,
    Derecha = 2,
    DerechaAbajo, // se entiende que es el número que sigue del 2
    Abajo,
    AbajoIzquierda,
    Izquierda,
    IzquierdaArriba
}

public enum EEstadosEscapista
{
    Idle,
    Atacando,
    Cansado
}

// Sirven para identificar máscaras de bits (entre otras cosas).
public enum ELayers_PotenciasDeDos : Int32 // si tiene que ser un entero, porque tiene que poder tener valores de hasta 2^32
{
    Default = 0,
    TransparentFX = 1,
    IgnoreRaycast = 2,
    Vacio = 4,
    Water = 8,
    UI = 16,
    custom1 = 32,
    custom2 = 64,
    etc = 128

}


public class Nodo
{
    public Nodo(string id, int x, int y, bool esCaminable)
    {
        _id = id;
        _x = x;
        _y = y;
        _esCaminable = esCaminable;
        _parent = null;
    }

    // Identificador
    private string _id;
    public string ID => _id; // esta es de solo lectura.

    // como es para una cuadrícula, yo le quiero poner su coordenada X y Y
    private int _x;
    public int X => _x;
    private int _y;
    public int Y => _y;

    private bool _esCaminable;
    public bool EsCaminable => _esCaminable;

    public void HacerCaminable()
    {
        _esCaminable = true;
    }

    // Propiedades del NodoDelÁrbol
    private Nodo _parent;

    public void AsignarParent(Nodo parent)
    {
        _parent = parent;
    }
    public Nodo Parent => _parent;
}

public class TileGrid : MonoBehaviour
{
    [Header("Configuración de cuadricula")]
    public int ancho = 5;
    public int alto = 5;
    public float probabilidadDeNoCaminable = 0.2f;
    public bool permitirDiagonales = false;

    [Header("Configuración del pathfinding")]
    [Min(0)]
    public int origenX = 0;
    [Min(0)]
    public int origenY = 0;

    [Min(0)]
    public int destinoX = 2;
    [Min(0)]
    public int destinoY = 2;


    public Nodo[,] Grid;

    // hace que en el editor salga un slider para este valor, el cual únicamente se va a poder mover del 0.1f al 1.0f.
    // OJO: esto no impide que el valor de dicha variable se pueda cambiar a fuera de ese rango mediante código.
    [Header("Control de Gizmos")]
    [Range(0.1f, 1.0f)]
    public float tamanioPorCuadroDelGizmo = 0.8f;

    public float tiempoEntrePasos = 0.5f;


    public Color colorCaminable = Color.white;
    public Color colorNoCaminable = Color.darkGoldenRod;
    public Color colorOrigen = Color.darkCyan;
    public Color colorDestino = Color.gray5;
    public Color colorLineaParent = Color.red;
    public Color colorListaAbierta = Color.lightGray;
    public Color colorListaCerrada = Color.darkGray;
    public Color colorParteDelCamino = Color.lightBlue;


    // para cuáles nodos quedan por visitar, es necesario almacenarlos en algún lugar
    // son los nodos que ya sabe el algoritmo que existen, pero que todavía tienen cosas por hacer.
    private Stack<Nodo> listaAbierta = new Stack<Nodo>();

    private Queue<Nodo> filaAbierta = new Queue<Nodo>(); // ESTA ES LA QUE DEBEN USAR PARA SU TAREA

    // y tenemos otro contenedor donde vamos a poner los nodos que ya conocemos y que ya no tienen nada por hacer.
    private HashSet<Nodo> listaCerrada = new HashSet<Nodo>();

    private List<Nodo> caminoALaMeta = new List<Nodo>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // checamos que las coordenadas del origen y destino no se salgan de la cuadrícula
        if (origenX >= ancho || origenY >= alto ||
        destinoX >= ancho || destinoY >= alto)
        {
            Debug.LogError("Alguno de las coordenadas de Origen o destino está fuera del rango de la cuadrícula," +
                           "el programa no inicializará la cuadrícula");
            return;
        }

        // For bidimensional
        // en el primer for vamos a crear las filas, pero todavía no están rellenas
        // en el segundo for vamos a crear cada uno de los Nodos de la fila actual para que estén rellenas.
        Grid = new Nodo[alto, ancho];
        for (int y = 0; y < alto; y++)
        {
            // Grid[y] = new Node[ancho];
            for (int x = 0; x < ancho; x++)
            {
                float rand = Random.Range(0.0f, 1.0f);

                // // If clásico que es el más didáctico pero el más extenso
                // bool esCaminable = true;
                //
                // if (rand < probabilidadDeNoCaminable)
                // {
                //     esCaminable = false;
                // }

                // Estructura de un If de una sola línea. Creo que se le llama Operador Ternario.
                // (condición a checar) + ? + (lo que pasa si sí se cumplió la condición) + : + (lo que pasa si no se cumplió);
                bool esCaminable = rand < probabilidadDeNoCaminable ? false : true; // este de acá es para lo que sea.

                // bool esCaminableDirecto = rand < probabilidadDeNoCaminable; // exclusivo para booleanos

                Grid[y, x] = new Nodo($"{x}_{y}", x, y, esCaminable);
                // por que "[y, x]" y no "[x, y]"?
            }
        }

        Nodo origen = Grid[origenY, origenX];
        origen.AsignarParent(origen); // recuerden, el nodo raíz siempre es su propio parent.
        origen.HacerCaminable(); // lo hacemos caminable para siempre poder ejecutar al menos un paso del pathfinding

        Nodo destino = Grid[destinoY, destinoX];
        destino.HacerCaminable(); // lo hacemos caminable para siempre poder entrar al nodo destino si hubo un camino.

        // StartCoroutine(DepthFirstSearchIterativoCorrutina(origen, destino));

        if (BreadthFirstSearchIterativo(origen, destino))
        {
            Debug.Log($"Sí hubo camino de: {origen.ID} a {destino.ID}");
        }
        else
        {
            Debug.Log($"No hubo camino de: {origen.ID} a {destino.ID}");
        }
    }

    bool BreadthFirstSearchIterativo(Nodo origen, Nodo destino)
    {
        filaAbierta = new Queue<Nodo>();
        listaCerrada = new HashSet<Nodo>();

        filaAbierta.Enqueue(origen);

        // Obtenemos la cantidad de direcciones del Enum para evitar el HARDCODE (el número 8)
        int numeroDeDirecciones = System.Enum.GetNames(typeof(EDirecciones)).Length;

        while (filaAbierta.Count > 0)
        {
            Nodo actual = filaAbierta.Dequeue(); // Saca al elemento que está al frente
            listaCerrada.Add(actual); // Lo metemos a la lista cerrada

            // Checamos si ya llegamos a la meta
            if (actual == destino)
            {
                caminoALaMeta = Backtracking(destino);
                ImprimirCaminoCorrecto(caminoALaMeta); // (Inciso c) Imprimir ruta al derecho
                return true;
            }

            // Visitar vecinos sin usar hardcode
            for (int i = 0; i < numeroDeDirecciones; i++)
            {
                // si no se permiten diagonales, solo checar 0, 2, 4, y 6
                if (!permitirDiagonales && i % 2 == 1)
                {
                    continue;
                }

                Nodo vecino = ObtenerVecino(actual, (EDirecciones)i);
                if (vecino == null) continue; // Si nos salimos de la cuadrícula, ignorar

                // Si el vecino no tiene padre asignado (es decir, no ha sido visitado) y es caminable
                if (vecino.Parent == null && vecino.EsCaminable)
                {
                    vecino.AsignarParent(actual); // Registramos de dónde venimos
                    filaAbierta.Enqueue(vecino);  // Lo encolamos para visitarlo después
                }
            }
        }

        // Si la fila se vació y nunca se llegó al return true, no hay camino
        return false;
    }

    List<Nodo> Backtracking(Nodo meta)
    {
        List<Nodo> camino = new List<Nodo>();
        Nodo actual = meta;

        while (actual != actual.Parent)
        {
            camino.Add(actual);
            actual = actual.Parent;
        }
        camino.Add(actual); // MUY IMPORTANTE: Agregar el nodo de origen al final del ciclo

        // (Inciso c) Invertimos la lista para que el orden sea de Origen -> Objetivo
        camino.Reverse();

        return camino;
    }
    void ImprimirCaminoCorrecto(List<Nodo> camino)
    {
        string rutaCompleta = "Ruta tomada: ";
        for (int i = 0; i < camino.Count; i++)
        {
            rutaCompleta += camino[i].ID;
            if (i < camino.Count - 1) rutaCompleta += " -> ";
        }
        Debug.Log(rutaCompleta);
    }

    bool DepthFirstSearchIterativo(Nodo origen, Nodo destino)
    {
        Nodo actual = origen;

        listaAbierta = new Stack<Nodo>();
        listaCerrada = new HashSet<Nodo>();

        bool encontroVecino = false;
        listaAbierta.Push(actual); // necesitamos meter al actual como primer elemento de la lista, para que no se salte el while.

        // ciclos for / foreach / while son las únicas formas de ciclos que no son recursión.
        // nuestro ciclo se acaba 1) cuando llegamos al destino o 2) cuando ya no queda ningún nodo por visitar
        while (listaAbierta.Count > 0) // si alguna de estas dos condiciones NO se cumple, se acaba el ciclo.
        {
            encontroVecino = false;

            if (actual == destino)
            {
                caminoALaMeta = Backtracking(destino);
                return true;
            }


            for (int i = 0; i < 8; i++)
            {
                // si no se permiten diagonales, solo checar 0, 2, 4, y 6, que son arriba, derecha, abajo e izquierda, respectivamente.
                if (!permitirDiagonales && i % 2 == 1)
                {
                    continue;
                }

                Nodo vecino = ObtenerVecino(actual, (EDirecciones)i);
                if (vecino == null) // Checamos que no nos salgamos de la matriz por la izquierda.
                    continue; // no hubo vecino, entonces nos vamos al siguiente vecino posible.

                // Meter cosas a la lista abierta sucede cuando nosotros mandaríamos a llamar la recursión.
                if (vecino.Parent == null && vecino.EsCaminable)
                {
                    // se asigna el nodo actual (actual) como parent de su vecino
                    vecino.AsignarParent(actual);
                    // como el ciclo while es el que está remplazando la recursión, pues necesitamos re-empezar hasta arriba
                    // del while, pero ahora nuestro actual va a ser este vecino.
                    listaAbierta.Push(vecino);
                    actual = vecino;
                    encontroVecino = true;
                    break; // esto hace que te salgas del ciclo en el que esté, y en este caso nos manda al inicio del while.
                }
            }

            // esta parte es la más "distinta", porque está simulando la Pila de llamadas de la recursión.
            if (encontroVecino == false)
            {
                Debug.Log($"el nodo {actual.ID} NO fue parte del camino");

                // si llega aquí, es que ya este nodo no tuvo ningún vecino disponible, y por lo tanto él ya está cerrado.
                listaCerrada.Add(actual); // ya no hay nada que hacer con este nodo.
                // cuando este nodo se sale de la lista abierta (que es una Pila/Stack), pues el que quede hasta arriba
                // de la lista es el que debe continuar el proceso.
                listaAbierta.Pop(); // sacamos al de hasta arriba
                if (listaAbierta.Count == 0)
                    return false; // si ya quedó vacía, regresamos false.

                actual = listaAbierta.Peek(); // hacemos que el actual sea el que quedó hasta arriba tras la salida del otro.
            }
        }

        // llegamos aquí porque la lista abierta quedó vacía, y por lo tanto ya no había más nodos que visitar.
        return false;
    }


    bool DepthFirstSearchRecursiva(Nodo origen, Nodo destino)
    {
        // Condición de parada
        if (origen.ID == destino.ID)
        {
            Debug.Log($"ya llegamos a la meta");
            return true; // ya llegamos al destino.
        }

        for (int i = 0; i < 8; i++)
        {
            // si no se permiten diagonales, solo checar 0, 2, 4, y 6, que son arriba, derecha, abajo e izquierda, respectivamente.
            if (!permitirDiagonales && i % 2 == 1)
            {
                continue;
            }

            if (ChecarVecino(origen, destino, (EDirecciones)i))
            {
                Debug.Log($"el nodo {origen.ID} Sí fue parte del camino");
                return true;
            }
        }

        // Nodo vecinoArribaDerecha = Grid[origen.Y - 1, origen.X + 1]; // diagonal hacia arriba y hacia la derecha

        Debug.Log($"el nodo {origen.ID} NO fue parte del camino");
        return false;
    }

    bool EstaDentroDeLaCuadricula(int x, int y)
    {
        if (x >= ancho || x < 0 ||
            y >= alto || y < 0)
            return false;
        Debug.Log($"{x}, {y} está dentro de la cuadricula");
        return true;
    }

    Nodo ObtenerVecino(Nodo actual, EDirecciones direccion)
    {
        int xOffset = 0;
        int yOffset = 0;
        switch (direccion)
        {
            case EDirecciones.Arriba:
                yOffset = -1;
                break;
            case EDirecciones.ArribaDerecha:
                yOffset = -1;
                xOffset = 1;
                break;
            case EDirecciones.Derecha:
                xOffset = 1;
                break;
            case EDirecciones.DerechaAbajo:
                xOffset = 1;
                yOffset = 1;
                break;
            case EDirecciones.Abajo:
                yOffset = 1;
                break;
            case EDirecciones.AbajoIzquierda:
                xOffset = -1;
                yOffset = 1;
                break;
            case EDirecciones.Izquierda:
                xOffset = -1;
                break;
            case EDirecciones.IzquierdaArriba:
                xOffset = -1;
                yOffset = -1;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(direccion), direccion, null);
        }

        if (EstaDentroDeLaCuadricula(actual.X + xOffset, actual.Y + yOffset))
            return Grid[actual.Y + yOffset, actual.X + xOffset];

        return null;
    }

    bool IntentarRecursion(Nodo origen, Nodo destino, Nodo vecino)
    {
        if (vecino.Parent == null && vecino.EsCaminable)
        {
            // se asigna el nodo actual (origen) como parent de su vecino
            vecino.AsignarParent(origen);

            // sí podemos mandar a llamar la recursión a partir de él
            if (DepthFirstSearchRecursiva(vecino, destino))
            {
                Debug.Log($"el nodo {origen.ID} fue parte del camino");
                return true;
            }
        }

        return false;
    }

    bool ChecarVecino(Nodo origen, Nodo destino, EDirecciones direccion)
    {
        Nodo vecino = ObtenerVecino(origen, direccion);
        if (vecino != null) // Checamos que no nos salgamos de la matriz por la izquierda.
        {
            if (IntentarRecursion(origen, destino, vecino))
            {
                return true;
            } // y si no, pues nada
        }

        return false;
    }


    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            // Dibujamos la cuadrícula, y lo hacemos cuadrito por cuadrito
            for (int y = 0; y < alto; y++)
            {
                // Grid[y] = new Node[ancho];
                for (int x = 0; x < ancho; x++)
                {
                    Gizmos.color = Grid[y, x].EsCaminable ? colorCaminable : colorNoCaminable;

                    if (filaAbierta.Contains(Grid[y, x]))
                    {
                        Gizmos.color = colorListaAbierta;
                    }
                    else if (listaCerrada.Contains(Grid[y, x]))
                    {
                        Gizmos.color = colorListaCerrada;
                    }

                    if (caminoALaMeta.Contains(Grid[y, x]))
                    {
                        Gizmos.color = colorParteDelCamino;
                    }

                    Gizmos.DrawCube(new Vector3(x, -y, 0.0f), Vector3.one * tamanioPorCuadroDelGizmo);

                    // si tiene asignado un parent del pathfinding
                    Nodo parent = Grid[y, x].Parent;
                    if (parent != null)
                    {
                        Gizmos.color = colorLineaParent;
                        // entonces dibujamos una línea que los conecte
                        Gizmos.DrawLine(new Vector3(x, -y, 0.0f), new Vector3(parent.X, -parent.Y, 0.0f));
                    }
                }
            }

            Gizmos.color = colorOrigen;
            Gizmos.DrawCube(new Vector3(origenX, -origenY, 0.0f), Vector3.one * tamanioPorCuadroDelGizmo);

            Gizmos.color = colorDestino;
            Gizmos.DrawCube(new Vector3(destinoX, -destinoY, 0.0f), Vector3.one * tamanioPorCuadroDelGizmo);
        }
    }

    IEnumerator DepthFirstSearchIterativoCorrutina(Nodo origen, Nodo destino)
    {
        Nodo actual = origen;

        listaAbierta = new Stack<Nodo>();
        listaCerrada = new HashSet<Nodo>();

        bool encontroVecino = false;
        listaAbierta.Push(actual); // necesitamos meter al actual como primer elemento de la lista, para que no se salte el while.


        // ciclos for / foreach / while son las únicas formas de ciclos que no son recursión.
        // nuestro ciclo se acaba 1) cuando llegamos al destino o 2) cuando ya no queda ningún nodo por visitar
        while (listaAbierta.Count > 0) // si alguna de estas dos condiciones NO se cumple, se acaba el ciclo.
        {
            yield return new WaitForSeconds(tiempoEntrePasos);
            encontroVecino = false;

            if (actual == destino)
            {
                Debug.Log("Sí se encontró camino");
                caminoALaMeta = Backtracking(destino);

                yield break; // detiene la corrutina porque ya llegamos a la meta.
                // return true;
            }


            for (int i = 0; i < 8; i++)
            {
                // si no se permiten diagonales, solo checar 0, 2, 4, y 6, que son arriba, derecha, abajo e izquierda, respectivamente.
                if (!permitirDiagonales && i % 2 == 1)
                {
                    continue;
                }

                // (EDirecciones)i es un "Casteo", que significa interpretar una variable X como si fuera de otro tipo de dato distinto.
                Nodo vecino = ObtenerVecino(actual, (EDirecciones)i);
                if (vecino == null) // Checamos que no nos salgamos de la matriz por la izquierda.
                    continue; // no hubo vecino, entonces nos vamos al siguiente vecino posible.

                // Meter cosas a la lista abierta sucede cuando nosotros mandaríamos a llamar la recursión.
                if (vecino.Parent == null && vecino.EsCaminable)
                {
                    // se asigna el nodo actual (actual) como parent de su vecino
                    vecino.AsignarParent(actual);
                    // como el ciclo while es el que está remplazando la recursión, pues necesitamos re-empezar hasta arriba
                    // del while, pero ahora nuestro actual va a ser este vecino.
                    listaAbierta.Push(vecino);
                    actual = vecino;
                    encontroVecino = true;
                    break; // esto hace que te salgas del ciclo en el que esté, y en este caso nos manda al inicio del while.
                }
            }

            // esta parte es la más "distinta", porque está simulando la Pila de llamadas de la recursión.
            if (encontroVecino == false)
            {
                Debug.Log($"el nodo {actual.ID} NO fue parte del camino");

                // si llega aquí, es que ya este nodo no tuvo ningún vecino disponible, y por lo tanto él ya está cerrado.
                listaCerrada.Add(actual); // ya no hay nada que hacer con este nodo.
                // cuando este nodo se sale de la lista abierta (que es una Pila/Stack), pues el que quede hasta arriba
                // de la lista es el que debe continuar el proceso.
                listaAbierta.Pop(); // sacamos al de hasta arriba
                if (listaAbierta.Count == 0)
                {
                    Debug.Log("No se encontró camino");
                    yield break; // si ya quedó vacía, terminamos la corrutina, porque no hubo camino.
                }

                actual = listaAbierta.Peek(); // hacemos que el actual sea el que quedó hasta arriba tras la salida del otro.
            }
        }

        // llegamos aquí porque la lista abierta quedó vacía, y por lo tanto ya no había más nodos que visitar.
        // yield break; // no es necesario porque ya es el final de la función.
    }

}

/*
 * APUNTES SOBRE LOS TAMAÑOS DE TIPOS DE DATOS
 * 

Tamaños de los tipos de dato básicos

1 bit (0 o 1)

1 Byte 8bits (0000 0000)

Sistema operativo de 32 bits, sus apuntadores de memoria son de 32 bits por defecto

Sistema operativo de 64 bits, 
64/8 = 8 Bytes

Memoria RAM 64GB

64,000 MB

64,000,000 KB

64,000,000,000 Bytes
4,294,967,296 esto es lo más a lo que llegan 32 bits, entonces una variable de 32 bits no alcanza a apuntar a todas esas direcciones de memoria.

Necesitas que en tu apuntador de memoria puedas tener esa cantidad de valores distintos

-------------------------------------------------------------------------------------------------------
bool 1 byte
char 1 byte
int 4 bytes (32 bits)
float 4 bytes (32 bits)
short 2 bytes (16 bits)

string 1 apuntador (64 bits por el sistema operativo) + 1 byte por cada caracter de la string.
	Ejemplo: "hola", sería los 64bits + 1 byte por cada una de las 4 letras, + 1 byte para el caracter nulo '\0'. entonces 4 bytes más 64bits + 4 bytes = 12 bytes
	Ejemplo: "Independencia" 64bits + 13 bytes = 21 bytes
	apuntador que apunta al primero elemento de un array char[X], donde X es el tamaño del array.
	las strings terminan con '\0'. Entonces, realmente la cadena "hola" en memoria queda como "hola'\0'"
	['H', 'o', 'l', 'a', '\0']
	[72, 111, 108, 97, 0]
	[01001000, 01101111, 01101100, 01100001, 00000000]
	
Vector3 (3 floats, 3 floats cada uno de 4 bytes, 3*4 = 12 bytes)

Esto es importante cuando vas a tener un montón de variables o instancias de clases con muchos de éstos.

Nuestro grid es Ancho*alto
ancho = 5
alto = 10
total = 50
50 nodos, donde cada nodo tiene:
	1 string (8bytes del apuntador + 3 chars + el caracter null. Total 12 bytes)
	int x (4 bytes)
	int y (4 bytes)
	bool caminable (1 bytes)
	// Nodo parent
	por ahora son 21 bytes por nodo

50 * 21 = 1050 bytes, poquito más de un KB

Age of empires tiene un map basada en cuadrícula de 4096*4096
16777216 * 21 bytes de cada nodo = 352,321,536, que serían como 350 megabytes 
PERO, su string acá alcanza los 9 caracteres, por lo que mediría 18 bytes, no 12, lo que nos daría:
452,984,832, 450MB

cómo podríamos ahorrar
1) sabemos que el mapa lo más grande que puede ser es 4096*4096, entonces la x y y solo necesitan una variable que represente al menos 4096 números.
a un char puede representar 256 números (2^8), no podemos usar un char para esto porque 256 < 4096,
a un short puede representar (2^16) 65536 valores, y como 65536 > 4096, entonces sí podemos usar tan solo un short para representar la X y Y
	int X -> short X
	int Y -> short Y
con esto nos ahorramos 2 bytes de la X y 2 bytes de la Y, 4 bytes menos por cada nodo.
2) como la string únicamente es lo mismo que el valor de X + '_' + Y, entonces es redundante con la información que ya tenemos en las variables X y Y,
entonces la podríamos quitar completamente, y con eso ahorrarnos 18 bytes por cada nodo.

Después de hacer 1) y 2) nuestor nodo ya solo ocupa 5 bytes en memoria en vez de 27. Lo que nos da:
83,886,080 = 83MB

La variable Nodo Parent es una referencia, a un Objeto en memoria, en otras palabras, es un apuntador a la memoria de alguien más.
En este caso es un apuntar a la dirección de memoria de otro Nodo de nuestra cuadrícula.
Y como es un apuntador, ocupa 64 bits de memoria (8 bytes) porque el sistema operativo es de 64 bits.


 * 
 */