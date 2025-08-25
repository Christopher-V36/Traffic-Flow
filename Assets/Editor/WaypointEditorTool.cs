using UnityEngine;
using UnityEditor; // Necesario para crear herramientas de editor
using System.Collections.Generic;

public class WaypointEditorTool : EditorWindow
{
    // --- Variables para la UI de la herramienta ---
    private int gridWidth = 10;
    private int gridHeight = 8;
    private float nodeSpacing = 10f;
    private GameObject waypointParent;

    // Esto crea la opción en el menú superior de Unity para abrir nuestra herramienta.
    [MenuItem("Tools/Waypoint Editor")]
    public static void ShowWindow()
    {
        GetWindow<WaypointEditorTool>("Waypoint Editor");
    }

    // Dibuja la interfaz de nuestra ventana personalizada.
    void OnGUI()
    {
        GUILayout.Label("Generador de Red de Waypoints", EditorStyles.boldLabel);

        // Campos para configurar la cuadrícula
        gridWidth = EditorGUILayout.IntField("Ancho de la Cuadrícula (X)", gridWidth);
        gridHeight = EditorGUILayout.IntField("Alto de la Cuadrícula (Z)", gridHeight);
        nodeSpacing = EditorGUILayout.FloatField("Espaciado entre Nodos", nodeSpacing);

        // Campo para asignar el objeto padre que contendrá los waypoints
        waypointParent = (GameObject)EditorGUILayout.ObjectField("Objeto Padre para Waypoints", waypointParent, typeof(GameObject), true);

        // Botón para generar la cuadrícula
        if (GUILayout.Button("Generar Cuadrícula Detallada"))
        {
            GenerateDetailedGrid();
        }

        // Botón para conectar los waypoints automáticamente
        if (GUILayout.Button("Conectar Waypoints Automáticamente"))
        {
            if (waypointParent != null)
            {
                ConnectWaypoints();
            }
            else
            {
                Debug.LogError("Por favor, asigna un objeto padre antes de conectar los waypoints.");
            }
        }
    }

    void GenerateDetailedGrid()
    {
        if (waypointParent == null)
        {
            waypointParent = new GameObject("RedDeWaypoints");
        }

        for (int i = waypointParent.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(waypointParent.transform.GetChild(i).gameObject);
        }

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                // 1. Creamos el nodo de la intersección
                CreateWaypoint(x, z);

                // 2. Creamos el nodo intermedio horizontal (si no estamos en el último borde)
                if (x < gridWidth - 1)
                {
                    CreateMidpointWaypoint(x, z, true); // true for horizontal
                }

                // 3. Creamos el nodo intermedio vertical (si no estamos en el último borde)
                if (z < gridHeight - 1)
                {
                    CreateMidpointWaypoint(x, z, false); // false for vertical
                }
            }
        }
        Debug.Log("¡Cuadrícula detallada de waypoints generada!");
    }

    void CreateWaypoint(int x, int z)
    {
        GameObject wpObject = new GameObject($"wp_{x}_{z}");
        wpObject.transform.parent = waypointParent.transform;
        wpObject.transform.position = new Vector3(x * nodeSpacing, 0, z * nodeSpacing);
        wpObject.AddComponent<WaypointNode>();
    }

    void CreateMidpointWaypoint(int x, int z, bool isHorizontal)
    {
        // Usamos un nombre más claro para los nodos intermedios
        string name = isHorizontal ? $"wp_mid_h_{x}_{z}" : $"wp_mid_v_{x}_{z}";
        Vector3 position = isHorizontal
            ? new Vector3((x + 0.5f) * nodeSpacing, 0, z * nodeSpacing)
            : new Vector3(x * nodeSpacing, 0, (z + 0.5f) * nodeSpacing);

        GameObject wpObject = new GameObject(name);
        wpObject.transform.parent = waypointParent.transform;
        wpObject.transform.position = position;
        wpObject.AddComponent<WaypointNode>();
    }

    void ConnectWaypoints()
    {
        WaypointNode[] allWaypoints = waypointParent.GetComponentsInChildren<WaypointNode>();
        Dictionary<string, WaypointNode> waypointMap = new Dictionary<string, WaypointNode>();

        foreach (WaypointNode wp in allWaypoints)
        {
            waypointMap[wp.gameObject.name] = wp;
        }

        foreach (WaypointNode wp in allWaypoints)
        {
            if (wp.siguientesNodos == null)
            {
                wp.siguientesNodos = new List<WaypointNode>();
            }
            wp.siguientesNodos.Clear();

            string name = wp.gameObject.name;
            string[] parts = name.Split('_');

            // Si es un nodo de intersección
            if (parts[1] != "mid")
            {
                int x = int.Parse(parts[1]);
                int z = int.Parse(parts[2]);

                // Conexión Horizontal
                if (z % 2 == 0 && x < gridWidth - 1) // Filas pares van a la derecha
                    TryAddConnection(wp, $"wp_mid_h_{x}_{z}", waypointMap);
                else if (z % 2 != 0 && x > 0) // Filas impares van a la izquierda
                    TryAddConnection(wp, $"wp_mid_h_{x - 1}_{z}", waypointMap);

                // Conexión Vertical
                if (x % 2 == 0 && z < gridHeight - 1) // Columnas pares van hacia arriba
                    TryAddConnection(wp, $"wp_mid_v_{x}_{z}", waypointMap);
                else if (x % 2 != 0 && z > 0) // Columnas impares van hacia abajo
                    TryAddConnection(wp, $"wp_mid_v_{x}_{z - 1}", waypointMap);
            }
            // Si es un nodo intermedio
            else
            {
                bool isHorizontal = parts[2] == "h";
                int x = int.Parse(parts[3]);
                int z = int.Parse(parts[4]);

                if (isHorizontal)
                {
                    if (z % 2 == 0) // Filas pares van a la derecha, conectamos con el siguiente nodo de intersección
                        TryAddConnection(wp, $"wp_{x + 1}_{z}", waypointMap);
                    else // Filas impares van a la izquierda
                        TryAddConnection(wp, $"wp_{x}_{z}", waypointMap);
                }
                else // Es vertical
                {
                    if (x % 2 == 0) // Columnas pares van hacia arriba
                        TryAddConnection(wp, $"wp_{x}_{z + 1}", waypointMap);
                    else // Columnas impares van hacia abajo
                        TryAddConnection(wp, $"wp_{x}_{z}", waypointMap);
                }
            }
        }
        Debug.Log("¡Waypoints conectados automáticamente!");
    }

    void TryAddConnection(WaypointNode from, string toName, Dictionary<string, WaypointNode> map)
    {
        if (map.ContainsKey(toName))
        {
            from.siguientesNodos.Add(map[toName]);
        }
    }
}
