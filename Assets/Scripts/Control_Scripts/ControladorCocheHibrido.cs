using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(NavMeshAgent))]
public class ControladorCocheHibrido : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [Tooltip("Qué tan cerca debe estar el coche de un nodo para considerarlo 'alcanzado'.")]
    public float distanciaMinimaAlNodo = 2.5f;

    private NavMeshAgent agente;
    private WaypointNode destinoFinal;

    private List<WaypointNode> rutaCalculada;
    private int indiceRutaActual = 0;

    private bool semaforoEnRojo = false;
    private bool obstaculoAdelante = false;
    public float distanciaDeDeteccion = 3.0f;

    void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
    }

    public void IniciarViaje(WaypointNode nodoInicial, WaypointNode nodoFinal)
    {
        if (agente == null) agente = GetComponent<NavMeshAgent>();
        this.destinoFinal = nodoFinal;

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
        }

        rutaCalculada = CalcularRutaHaciaDestino(nodoInicial, nodoFinal);

        if (rutaCalculada != null && rutaCalculada.Count > 0)
        {
            indiceRutaActual = 0;
            MoverAlSiguienteNodoDeLaRuta();
        }
        else
        {
            Debug.LogWarning($"No se encontró ruta para el coche {gameObject.name}. Autodestruyendo.");
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (rutaCalculada == null || rutaCalculada.Count == 0 || !agente.isOnNavMesh) return;

        ComprobarObstaculos();

        agente.isStopped = semaforoEnRojo || obstaculoAdelante;

        if (!agente.isStopped && !agente.pathPending && agente.remainingDistance <= distanciaMinimaAlNodo)
        {
            indiceRutaActual++;
            MoverAlSiguienteNodoDeLaRuta();
        }

        if (indiceRutaActual < rutaCalculada.Count)
        {
            WaypointNode objetivoActual = rutaCalculada[indiceRutaActual];
            if (!objetivoActual.gameObject.activeInHierarchy)
            {
                Debug.Log($"¡Ruta bloqueada para {gameObject.name}! Recalculando...");
                WaypointNode nodoAnterior = rutaCalculada[indiceRutaActual - 1];
                IniciarViaje(nodoAnterior, destinoFinal);
            }
        }
    }

    void MoverAlSiguienteNodoDeLaRuta()
    {
        if (indiceRutaActual >= rutaCalculada.Count)
        {
            Destroy(gameObject);
            return;
        }

        WaypointNode proximoObjetivo = rutaCalculada[indiceRutaActual];
        agente.SetDestination(proximoObjetivo.transform.position);
    }

    // --- ALGORITMO CORREGIDO Y HECHO PÚBLICO ---
    public static List<WaypointNode> CalcularRutaHaciaDestino(WaypointNode inicio, WaypointNode fin)
    {
        Queue<WaypointNode> frontera = new Queue<WaypointNode>();
        frontera.Enqueue(inicio);
        Dictionary<WaypointNode, WaypointNode> vinoDesde = new Dictionary<WaypointNode, WaypointNode>();
        vinoDesde[inicio] = null;

        while (frontera.Count > 0)
        {
            WaypointNode actual = frontera.Dequeue();

            if (actual == fin)
            {
                List<WaypointNode> ruta = new List<WaypointNode>();
                WaypointNode temp = fin;
                while (temp != null)
                {
                    ruta.Add(temp);
                    temp = vinoDesde[temp];
                }
                ruta.Reverse();
                return ruta;
            }

            foreach (WaypointNode siguiente in actual.siguientesNodos)
            {
                if (siguiente != null && siguiente.gameObject.activeInHierarchy && !vinoDesde.ContainsKey(siguiente))
                {
                    frontera.Enqueue(siguiente);
                    vinoDesde[siguiente] = actual;
                }
            }
        }
        return null;
    }

    void ComprobarObstaculos()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, distanciaDeDeteccion))
        {
            obstaculoAdelante = hit.collider.CompareTag("Coche");
        }
        else
        {
            obstaculoAdelante = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        ControladorSemaforo semaforo = other.GetComponent<ControladorSemaforo>();
        if (semaforo != null)
        {
            semaforoEnRojo = (semaforo.estadoActual == ControladorSemaforo.EstadoSemaforo.Rojo);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<ControladorSemaforo>() != null)
        {
            semaforoEnRojo = false;
        }
    }
}
