using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GeneradorDeTrafico : MonoBehaviour
{
    [Header("Configuración del Spawner")]
    // Ahora es una lista para que puedas añadir varios modelos de coches.
    public List<GameObject> cochePrefabs;
    public float intervaloDeGeneracion = 5.0f;

    [Header("Red de Waypoints")]
    public Transform contenedorOrigenes;
    public Transform contenedorDestinos;

    private List<WaypointNode> nodosDeOrigen = new List<WaypointNode>();
    private List<WaypointNode> nodosDeDestino = new List<WaypointNode>();

    // Contador estático para asegurar que cada coche tenga un ID único.
    private static int proximoIdCoche = 1;

    void Start()
    {
        PopularListasDeNodos();
        StartCoroutine(GenerarCoches());
    }

    void PopularListasDeNodos()
    {
        if (contenedorOrigenes != null)
        {
            contenedorOrigenes.GetComponentsInChildren<WaypointNode>(nodosDeOrigen);
        }
        if (contenedorDestinos != null)
        {
            contenedorDestinos.GetComponentsInChildren<WaypointNode>(nodosDeDestino);
        }
    }

    IEnumerator GenerarCoches()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervaloDeGeneracion);
            GenerarCocheBajoDemanda();
        }
    }

    public void GenerarCocheBajoDemanda()
    {
        // Medida de seguridad para evitar errores.
        if (cochePrefabs == null || cochePrefabs.Count == 0 || nodosDeOrigen.Count == 0 || nodosDeDestino.Count == 0)
        {
            Debug.LogWarning("Faltan prefabs de coches o nodos de origen/destino.");
            return;
        }

        List<WaypointNode> origenesBarajados = nodosDeOrigen.OrderBy(a => Random.value).ToList();

        foreach (WaypointNode origen in origenesBarajados)
        {
            List<WaypointNode> destinosBarajados = nodosDeDestino.OrderBy(a => Random.value).ToList();

            foreach (WaypointNode destino in destinosBarajados)
            {
                if (origen == destino) continue;

                List<WaypointNode> rutaValida = ControladorCocheHibrido.CalcularRutaHaciaDestino(origen, destino);

                if (rutaValida != null)
                {
                    // 1. Elegimos un prefab al azar de nuestra lista.
                    GameObject prefabAUsar = cochePrefabs[Random.Range(0, cochePrefabs.Count)];

                    // 2. Creamos el coche usando el prefab elegido.
                    GameObject nuevoCoche = Instantiate(prefabAUsar, origen.transform.position, origen.transform.rotation);
                    ControladorCocheHibrido controlador = nuevoCoche.GetComponent<ControladorCocheHibrido>();

                    if (controlador != null)
                    {
                        // 3. Le damos su misión con un ID único.
                        controlador.IniciarViaje(origen, destino, proximoIdCoche);
                        proximoIdCoche++; // Incrementamos el contador para el siguiente.
                    }
                    return; // Salimos de la función al generar un coche con éxito.
                }
            }
        }
        Debug.LogWarning("No se pudo encontrar ninguna combinación de Origen/Destino con una ruta válida.");
    }
}
