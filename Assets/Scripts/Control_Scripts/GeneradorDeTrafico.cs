using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GeneradorDeTrafico : MonoBehaviour
{
    [Header("Configuración del Spawner")]
    public List<GameObject> cochePrefabs;
    public float intervaloDeGeneracion = 5.0f;

    // --- ¡NUEVA SECCIÓN! ---
    [Header("Límite de Vehículos")]
    [Tooltip("El número máximo de coches permitidos en la escena. Pon 0 para no tener límite.")]
    public int maximoDeCoches = 50;

    [Header("Red de Waypoints")]
    public Transform contenedorOrigenes;
    public Transform contenedorDestinos;

    private List<WaypointNode> nodosDeOrigen = new List<WaypointNode>();
    private List<WaypointNode> nodosDeDestino = new List<WaypointNode>();

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

            // --- ¡CAMBIO! ---
            // Antes de generar un coche, comprobamos si hay espacio.
            if (!SeHaAlcanzadoElLimite())
            {
                GenerarCocheBajoDemanda();
            }
        }
    }

    // --- ¡NUEVA FUNCIÓN AUXILIAR! ---
    // Comprueba si el número actual de coches ha alcanzado el máximo permitido.
    private bool SeHaAlcanzadoElLimite()
    {
        // Si el límite es 0 o menor, consideramos que no hay límite.
        if (maximoDeCoches <= 0)
        {
            return false;
        }
        // Contamos los coches actuales y comparamos con el máximo.
        return GameObject.FindGameObjectsWithTag("Coche").Length >= maximoDeCoches;
    }

    public void GenerarCocheBajoDemanda()
    {
        // --- ¡CAMBIO! ---
        // Añadimos una comprobación al inicio de la función.
        if (SeHaAlcanzadoElLimite())
        {
            Debug.LogWarning("Límite máximo de coches alcanzado. No se generarán más vehículos.");
            return; // Salimos de la función para no generar el coche.
        }

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
                    GameObject prefabAUsar = cochePrefabs[Random.Range(0, cochePrefabs.Count)];
                    GameObject nuevoCoche = Instantiate(prefabAUsar, origen.transform.position, origen.transform.rotation);
                    ControladorCocheHibrido controlador = nuevoCoche.GetComponent<ControladorCocheHibrido>();
                    if (controlador != null)
                    {
                        controlador.IniciarViaje(origen, destino, proximoIdCoche);
                        proximoIdCoche++;
                    }
                    return;
                }
            }
        }
        Debug.LogWarning("No se pudo encontrar ninguna combinación de Origen/Destino con una ruta válida.");
    }
}