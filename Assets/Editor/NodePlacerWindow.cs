using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

public class NodePlacerWindow : EditorWindow
{
    // --- NUEVA CLASE INTERNA ---
    // Para guardar la información de cada fila de forma organizada.
    private class FilaDeNodos
    {
        public List<GameObject> nodos = new List<GameObject>();
        public bool invertirConexion = false;
    }

    public enum Direccion
    {
        Adelante_Z_Positivo,
        Atras_Z_Negativo,
        Derecha_X_Positivo,
        Izquierda_X_Negativo
    }

    private Direccion orientacionDeLaFila = Direccion.Derecha_X_Positivo;
    private Direccion direccionFilasParalelas = Direccion.Adelante_Z_Positivo;

    private int numeroDeNodos = 5;
    private float espaciado = 10.0f;
    private GameObject nodoPrefab;
    private int numeroDeFilas = 1;
    private float espaciadoDeFilas = 10.0f;

    // --- CAMBIO: La lista ahora es de nuestra nueva clase ---
    private List<FilaDeNodos> filasCreadasRecientemente = new List<FilaDeNodos>();

    [MenuItem("Herramientas/Colocador de Nodos en Fila")]
    public static void ShowWindow()
    {
        GetWindow<NodePlacerWindow>("Colocador de Nodos");
    }

    void OnGUI()
    {
        GUILayout.Label("1. Configuración de Creación", EditorStyles.boldLabel);
        nodoPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab del Nodo", nodoPrefab, typeof(GameObject), false);
        numeroDeFilas = EditorGUILayout.IntField("Número de Filas", numeroDeFilas);
        if (numeroDeFilas < 1) numeroDeFilas = 1;
        numeroDeNodos = EditorGUILayout.IntField("Nodos por Fila", numeroDeNodos);
        if (numeroDeNodos < 1) numeroDeNodos = 1;
        espaciado = EditorGUILayout.FloatField("Espaciado entre Nodos", espaciado);
        if (numeroDeFilas > 1)
        {
            espaciadoDeFilas = EditorGUILayout.FloatField("Espaciado entre Filas", espaciadoDeFilas);
        }

        orientacionDeLaFila = (Direccion)EditorGUILayout.EnumPopup("Orientación de la Fila", orientacionDeLaFila);
        if (numeroDeFilas > 1)
        {
            direccionFilasParalelas = (Direccion)EditorGUILayout.EnumPopup("Dirección Filas Paralelas", direccionFilasParalelas);
        }

        if (GUILayout.Button("Crear Filas de Nodos"))
        {
            CrearFilasDeNodos();
        }

        EditorGUILayout.Space();

        // --- CAMBIO: La sección de unión ahora es dinámica ---
        if (filasCreadasRecientemente.Count > 0)
        {
            GUILayout.Label("2. Configuración de Unión", EditorStyles.boldLabel);

            // Dibujamos una opción para cada fila que hemos creado
            for (int i = 0; i < filasCreadasRecientemente.Count; i++)
            {
                filasCreadasRecientemente[i].invertirConexion = EditorGUILayout.Toggle($"Invertir Conexión Fila {i}", filasCreadasRecientemente[i].invertirConexion);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Unir Nodos de Cada Fila"))
            {
                UnirNodos();
            }
        }
    }

    void CrearFilasDeNodos()
    {
        if (nodoPrefab == null)
        {
            Debug.LogError("¡Error! Asigna un Prefab de Nodo.");
            return;
        }

        filasCreadasRecientemente.Clear();
        GameObject conjuntoPadre = new GameObject("Conjunto de Filas de Nodos");
        Vector3 vectorDireccionNodos = ObtenerVectorDeDireccion(orientacionDeLaFila);
        Vector3 vectorDireccionFilas = ObtenerVectorDeDireccion(direccionFilasParalelas);

        for (int j = 0; j < numeroDeFilas; j++)
        {
            GameObject filaPadre = new GameObject($"Fila_{j}");
            filaPadre.transform.parent = conjuntoPadre.transform;

            // Creamos una nueva instancia de nuestra clase para guardar la fila
            FilaDeNodos nuevaFilaInfo = new FilaDeNodos();

            for (int i = 0; i < numeroDeNodos; i++)
            {
                Vector3 posicion = (vectorDireccionNodos * i * espaciado) + (vectorDireccionFilas * j * espaciadoDeFilas);
                GameObject nuevoNodo = (GameObject)PrefabUtility.InstantiatePrefab(nodoPrefab);
                nuevoNodo.transform.position = posicion;
                nuevoNodo.transform.parent = filaPadre.transform;
                nuevoNodo.name = $"Nodo_{j}-{i}";
                nuevaFilaInfo.nodos.Add(nuevoNodo);
            }
            filasCreadasRecientemente.Add(nuevaFilaInfo);
        }
        Debug.Log($"Se crearon {numeroDeFilas} filas con {numeroDeNodos} nodos cada una.");
    }

    // --- CAMBIO: La función ahora lee la opción de inversión de cada fila ---
    void UnirNodos()
    {
        if (filasCreadasRecientemente.Count == 0) return;

        foreach (FilaDeNodos filaInfo in filasCreadasRecientemente)
        {
            for (int i = 0; i < filaInfo.nodos.Count - 1; i++)
            {
                // Leemos la opción 'invertirConexion' propia de esta fila
                bool invertir = filaInfo.invertirConexion;

                WaypointNode nodoOrigen = !invertir ? filaInfo.nodos[i].GetComponent<WaypointNode>() : filaInfo.nodos[i + 1].GetComponent<WaypointNode>();
                WaypointNode nodoDestino = !invertir ? filaInfo.nodos[i + 1].GetComponent<WaypointNode>() : filaInfo.nodos[i].GetComponent<WaypointNode>();

                if (nodoOrigen != null && nodoDestino != null)
                {
                    Undo.RecordObject(nodoOrigen, "Unir Waypoint Nodes");
                    nodoOrigen.siguientesNodos.Clear();
                    nodoOrigen.siguientesNodos.Add(nodoDestino);
                    EditorUtility.SetDirty(nodoOrigen);
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"Se unieron los nodos de {filasCreadasRecientemente.Count} filas. ¡Recuerda guardar la escena (Ctrl+S)!");
    }

    private Vector3 ObtenerVectorDeDireccion(Direccion dir)
    {
        switch (dir)
        {
            case Direccion.Adelante_Z_Positivo: return Vector3.forward;
            case Direccion.Atras_Z_Negativo: return Vector3.back;
            case Direccion.Derecha_X_Positivo: return Vector3.right;
            case Direccion.Izquierda_X_Negativo: return Vector3.left;
            default: return Vector3.right;
        }
    }
}