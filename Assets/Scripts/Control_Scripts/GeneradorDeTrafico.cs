using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GeneradorDeTrafico : MonoBehaviour
{
    [Header("Configuración del Spawner")]
    public GameObject cochePrefab;
    public float intervaloDeGeneracion = 5.0f;

    [Header("Red de Waypoints")]
    public Transform contenedorOrigenes;
    public Transform contenedorDestinos;

    private List<WaypointNode> nodosDeOrigen = new List<WaypointNode>();
    private List<WaypointNode> nodosDeDestino = new List<WaypointNode>();

    // --- ¡NUEVO CONTADOR DE IDs! ---
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
        if (nodosDeOrigen.Count == 0 || nodosDeDestino.Count == 0) return;

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
                    GameObject nuevoCoche = Instantiate(cochePrefab, origen.transform.position, origen.transform.rotation);
                    ControladorCocheHibrido controlador = nuevoCoche.GetComponent<ControladorCocheHibrido>();

                    if (controlador != null)
                    {
                        // --- ASIGNAMOS EL ID AL COCHE ---
                        controlador.IniciarViaje(origen, destino, proximoIdCoche);
                        proximoIdCoche++; // Incrementamos el contador para el siguiente coche.
                    }
                    return;
                }
            }
        }
        Debug.LogWarning("No se pudo encontrar ninguna combinación de Origen/Destino con una ruta válida.");
    }
}
