using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GeneradorDeTrafico : MonoBehaviour
{
    [Header("Configuración del Spawner")]
    public List<GameObject> cochePrefabs;
    [Tooltip("El intervalo actual de generación. Este valor cambiará según el nivel de tráfico.")]
    public float intervaloDeGeneracion = 5.0f;

    [Header("Límite de Vehículos")]
    [Tooltip("El número máximo actual de coches. Este valor cambiará según el nivel de tráfico.")]
    public int maximoDeCoches = 50;

    [Header("Niveles de Tráfico Predefinidos")]
    [Tooltip("Valores para tráfico ligero.")]
    public float intervaloLigero = 8.0f;
    public int maxCochesLigero = 25;
    [Tooltip("Valores para tráfico medio.")]
    public float intervaloMedio = 4.0f;
    public int maxCochesMedio = 60;
    [Tooltip("Valores para tráfico alto.")]
    public float intervaloAlto = 1.5f;
    public int maxCochesAlto = 120;

    [Header("Red de Waypoints")]
    public Transform contenedorOrigenes;
    public Transform contenedorDestinos;

    private List<WaypointNode> nodosDeOrigen = new List<WaypointNode>();
    private List<WaypointNode> nodosDeDestino = new List<WaypointNode>();
    private static int proximoIdCoche = 1;

    void Start()
    {
        AjustarNivelDeTrafico("medio");
        PopularListasDeNodos();
        StartCoroutine(GenerarCoches());
    }

    public string AjustarNivelDeTrafico(string nivel)
    {
        string mensajeRespuesta = "";
        nivel = nivel.ToLower();

        switch (nivel)
        {
            case "ligero":
                this.intervaloDeGeneracion = intervaloLigero;
                this.maximoDeCoches = maxCochesLigero;
                mensajeRespuesta = $"Tráfico ajustado a LIGERO (Límite: {maxCochesLigero}, Intervalo: {intervaloLigero}s).";
                break;
            case "medio":
                this.intervaloDeGeneracion = intervaloMedio;
                this.maximoDeCoches = maxCochesMedio;
                mensajeRespuesta = $"Tráfico ajustado a MEDIO (Límite: {maxCochesMedio}, Intervalo: {intervaloMedio}s).";
                break;
            case "alto":
                this.intervaloDeGeneracion = intervaloAlto;
                this.maximoDeCoches = maxCochesAlto;
                mensajeRespuesta = $"Tráfico ajustado a ALTO (Límite: {maxCochesAlto}, Intervalo: {intervaloAlto}s).";
                break;
            default:
                mensajeRespuesta = $"Nivel de tráfico '{nivel}' no reconocido.";
                break;
        }
        Debug.Log(mensajeRespuesta);
        return mensajeRespuesta;
    }

    void PopularListasDeNodos()
    {
        if (contenedorOrigenes != null) contenedorOrigenes.GetComponentsInChildren<WaypointNode>(nodosDeOrigen);
        if (contenedorDestinos != null) contenedorDestinos.GetComponentsInChildren<WaypointNode>(nodosDeDestino);
    }

    IEnumerator GenerarCoches()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervaloDeGeneracion);
            if (!SeHaAlcanzadoElLimite())
            {
                GenerarCocheBajoDemanda();
            }
        }
    }

    private bool SeHaAlcanzadoElLimite()
    {
        if (maximoDeCoches <= 0) return false;
        return GameObject.FindGameObjectsWithTag("Coche").Length >= maximoDeCoches;
    }

    public void IniciarGeneracionDeLote(int cantidad, float intervalo)
    {
        StartCoroutine(GenerarLoteDeCoches(cantidad, intervalo));
    }

    private IEnumerator GenerarLoteDeCoches(int cantidad, float intervalo)
    {
        List<WaypointNode> origenesDisponibles = nodosDeOrigen.OrderBy(a => Random.value).ToList();
        int indiceOrigenActual = 0;

        for (int i = 0; i < cantidad; i++)
        {
            if (SeHaAlcanzadoElLimite())
            {
                Debug.LogWarning("Límite de coches alcanzado durante la generación del lote. Proceso detenido.");
                yield break;
            }

            if (origenesDisponibles.Count > 0)
            {
                WaypointNode origenElegido = origenesDisponibles[indiceOrigenActual];
                bool generadoConExito = GenerarCocheEnPuntoEspecifico(origenElegido);
                indiceOrigenActual = (indiceOrigenActual + 1) % origenesDisponibles.Count;

                if (generadoConExito)
                {
                    yield return new WaitForSeconds(intervalo);
                }
            }
        }
    }

    private bool GenerarCocheEnPuntoEspecifico(WaypointNode origen)
    {
        if (origen == null) return false;

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
                    controlador.IniciarViaje(origen, destino, proximoIdCoche++);
                }
                return true;
            }
        }
        return false;
    }

    public void GenerarCocheBajoDemanda()
    {
        if (SeHaAlcanzadoElLimite())
        {
            Debug.LogWarning("Límite máximo de coches alcanzado. No se generarán más vehículos.");
            return;
        }

        if (cochePrefabs == null || cochePrefabs.Count == 0 || nodosDeOrigen.Count == 0 || nodosDeDestino.Count == 0) return;
        List<WaypointNode> origenesBarajados = nodosDeOrigen.OrderBy(a => Random.value).ToList();
        foreach (WaypointNode origen in origenesBarajados)
        {
            if (GenerarCocheEnPuntoEspecifico(origen))
            {
                return;
            }
        }
        Debug.LogWarning("No se pudo encontrar ninguna combinación de Origen/Destino con una ruta válida.");
    }
}