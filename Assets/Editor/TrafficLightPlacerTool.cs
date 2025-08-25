using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class TrafficLightPlacerTool : EditorWindow
{
    private GameObject trafficLightPrefab;
    private GameObject waypointParent;
    private float sideOffset = 3.5f; // Distancia hacia la DERECHA del carril

    [MenuItem("Tools/Traffic Light Placer")]
    public static void ShowWindow()
    {
        GetWindow<TrafficLightPlacerTool>("Traffic Light Placer");
    }

    void OnGUI()
    {
        GUILayout.Label("Colocador Automático de Semáforos", EditorStyles.boldLabel);

        trafficLightPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab del Semáforo", trafficLightPrefab, typeof(GameObject), false);
        waypointParent = (GameObject)EditorGUILayout.ObjectField("Objeto Padre de Waypoints", waypointParent, typeof(GameObject), true);
        sideOffset = EditorGUILayout.FloatField("Offset Lateral del Semáforo", sideOffset);

        if (GUILayout.Button("Colocar Semáforos en Intersecciones"))
        {
            if (trafficLightPrefab == null || waypointParent == null)
            {
                Debug.LogError("Por favor, asigna el Prefab del Semáforo y el Objeto Padre de los Waypoints.");
                return;
            }
            PlaceTrafficLights();
        }

        if (GUILayout.Button("Eliminar Semáforos Existentes"))
        {
            ClearTrafficLights();
        }
    }

    void PlaceTrafficLights()
    {
        GameObject trafficLightsParent = GameObject.Find("Semaforos");
        if (trafficLightsParent == null)
        {
            trafficLightsParent = new GameObject("Semaforos");
        }

        WaypointNode[] allWaypoints = waypointParent.GetComponentsInChildren<WaypointNode>();
        if (allWaypoints.Length == 0) return;

        List<WaypointNode> intersectionNodes = allWaypoints.Where(wp => wp.siguientesNodos.Count > 1).ToList();

        foreach (WaypointNode intersectionNode in intersectionNodes)
        {
            List<WaypointNode> entryNodes = allWaypoints.Where(wp => wp.siguientesNodos.Contains(intersectionNode)).ToList();

            foreach (WaypointNode entryNode in entryNodes)
            {
                // --- LÓGICA DE POSICIONAMIENTO CORREGIDA ---
                Vector3 directionToIntersection = (intersectionNode.transform.position - entryNode.transform.position).normalized;

                // Calculamos un vector hacia la derecha del sentido de la marcha.
                Vector3 rightOffsetVector = Vector3.Cross(directionToIntersection, Vector3.up).normalized * sideOffset;

                // Posicionamos el semáforo EN el nodo de entrada, pero desplazado a la derecha.
                Vector3 lightPosition = entryNode.transform.position + rightOffsetVector;

                // Orientamos el semáforo para que mire hacia la intersección.
                Quaternion lightRotation = Quaternion.LookRotation(directionToIntersection);

                GameObject newLight = (GameObject)PrefabUtility.InstantiatePrefab(trafficLightPrefab);
                newLight.transform.position = lightPosition;
                newLight.transform.rotation = lightRotation;
                newLight.transform.parent = trafficLightsParent.transform;

                ControladorSemaforo cs = newLight.GetComponent<ControladorSemaforo>();
                if (cs != null)
                {
                    cs.idSemaforo = newLight.GetInstanceID();
                }
            }
        }
        Debug.Log($"¡Se han colocado semáforos en {intersectionNodes.Count} intersecciones!");
    }

    void ClearTrafficLights()
    {
        GameObject trafficLightsParent = GameObject.Find("Semaforos");
        if (trafficLightsParent != null)
        {
            DestroyImmediate(trafficLightsParent);
        }
        Debug.Log("Todos los semáforos han sido eliminados.");
    }
}
