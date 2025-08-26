using UnityEngine;
using UnityEditor; // Necesario para crear herramientas de editor
using System.Collections.Generic;

public class NodePlacerWindow : EditorWindow
{
    // Variables para la configuración de la herramienta
    private int numeroDeNodos = 5;
    private float espaciado = 30.0f;
    private GameObject nodoPrefab; // Aquí arrastraremos nuestro prefab de WaypointNode

    // Lista para guardar los nodos que se acaban de crear
    private List<GameObject> nodosCreadosRecientemente = new List<GameObject>();

    // Este método crea la opción en el menú superior de Unity
    [MenuItem("Tools/Colocador de Nodos en Fila")]
    public static void ShowWindow()
    {
        // Muestra la ventana de la herramienta
        GetWindow<NodePlacerWindow>("Colocador de Nodos");
    }

    // Este método dibuja la interfaz de la ventana
    void OnGUI()
    {
        GUILayout.Label("Configuración de Creación", EditorStyles.boldLabel);

        // Campo para asignar el prefab del nodo
        nodoPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab del Nodo", nodoPrefab, typeof(GameObject), false);

        // Campo para definir el número de nodos
        numeroDeNodos = EditorGUILayout.IntField("Número de Nodos", numeroDeNodos);

        // Campo para definir el espaciado
        espaciado = EditorGUILayout.FloatField("Espaciado entre Nodos", espaciado);

        // Dibuja un espacio en la interfaz
        EditorGUILayout.Space();

        // --- Botón 1: Crear la Fila de Nodos ---
        if (GUILayout.Button("Crear Fila de Nodos"))
        {
            CrearFilaDeNodos();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Unir Nodos", EditorStyles.boldLabel);

        // --- Botón 2: Unir los Nodos Creados ---
        // Se activa solo si hay nodos en nuestra lista para unir
        if (nodosCreadosRecientemente.Count > 1)
        {
            if (GUILayout.Button("Unir Nodos Creados"))
            {
                UnirNodos();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Crea una fila de al menos 2 nodos para poder unirlos.", MessageType.Info);
        }
    }

    void CrearFilaDeNodos()
    {
        if (nodoPrefab == null)
        {
            Debug.LogError("¡Error! Por favor, asigna un Prefab de Nodo antes de crear la fila.");
            return;
        }

        // Limpiamos la lista de nodos anteriores
        nodosCreadosRecientemente.Clear();

        // Creamos un objeto padre vacío para mantener la jerarquía organizada
        GameObject filaPadre = new GameObject("Fila de Nodos");

        // Creamos los nodos en una línea recta a lo largo del eje X
        for (int i = 0; i < numeroDeNodos; i++)
        {
            Vector3 posicion = new Vector3(i * espaciado, 0, 0);
            GameObject nuevoNodo = (GameObject)PrefabUtility.InstantiatePrefab(nodoPrefab);
            nuevoNodo.transform.position = posicion;
            nuevoNodo.transform.parent = filaPadre.transform;
            nuevoNodo.name = $"Nodo_{i}";

            // Añadimos el nodo recién creado a nuestra lista
            nodosCreadosRecientemente.Add(nuevoNodo);
        }

        Debug.Log($"Se crearon {numeroDeNodos} nodos en una nueva fila.");
    }

    void UnirNodos()
    {
        // Recorremos la lista de nodos creados, excepto el último
        for (int i = 0; i < nodosCreadosRecientemente.Count - 1; i++)
        {
            WaypointNode nodoActual = nodosCreadosRecientemente[i].GetComponent<WaypointNode>();
            WaypointNode nodoSiguiente = nodosCreadosRecientemente[i + 1].GetComponent<WaypointNode>();

            if (nodoActual != null && nodoSiguiente != null)
            {
                // Limpiamos la lista de conexiones anteriores del nodo actual
                nodoActual.siguientesNodos.Clear();
                // Añadimos el siguiente nodo de la fila como su única conexión
                nodoActual.siguientesNodos.Add(nodoSiguiente);
            }
        }

        Debug.Log($"Se unieron {nodosCreadosRecientemente.Count} nodos secuencialmente.");

        // Opcional: Limpiar la lista después de unir para evitar volver a unirlos por error
        // nodosCreadosRecientemente.Clear();
    }
}