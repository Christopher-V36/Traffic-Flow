using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Necesario para la función de barajado (Shuffle)

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

    // --- FUNCIÓN MODIFICADA PARA SER MÁS ROBUSTA ---
    public void GenerarCocheBajoDemanda()
    {
        if (nodosDeOrigen.Count == 0 || nodosDeDestino.Count == 0) return;

        // Barajamos la lista de orígenes para no empezar siempre por el mismo.
        List<WaypointNode> origenesBarajados = nodosDeOrigen.OrderBy(a => Random.value).ToList();

        foreach (WaypointNode origen in origenesBarajados)
        {
            // Para cada origen, barajamos la lista de posibles destinos.
            List<WaypointNode> destinosBarajados = nodosDeDestino.OrderBy(a => Random.value).ToList();

            foreach (WaypointNode destino in destinosBarajados)
            {
                // Nos aseguramos de que el origen y el destino no sean el mismo.
                if (origen == destino) continue;

                // Intentamos calcular una ruta.
                List<WaypointNode> rutaValida = ControladorCocheHibrido.CalcularRutaHaciaDestino(origen, destino);

                // Si encontramos una ruta válida...
                if (rutaValida != null)
                {
                    // ...creamos el coche y terminamos la función. ¡Éxito!
                    GameObject nuevoCoche = Instantiate(cochePrefab, origen.transform.position, origen.transform.rotation);
                    ControladorCocheHibrido controlador = nuevoCoche.GetComponent<ControladorCocheHibrido>();

                    if (controlador != null)
                    {
                        controlador.IniciarViaje(origen, destino);
                    }
                    return; // Salimos de la función porque ya hemos generado un coche.
                }
            }
        }

        // Si hemos recorrido todos los orígenes y destinos y no hemos encontrado
        // ninguna ruta válida, entonces lo notificamos.
        Debug.LogWarning("No se pudo encontrar ninguna combinación de Origen/Destino con una ruta válida.");
    }
}
