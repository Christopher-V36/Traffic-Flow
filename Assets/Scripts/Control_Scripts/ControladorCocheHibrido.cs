using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(NavMeshAgent))]
public class ControladorCocheHibrido : MonoBehaviour
{
    [Header("Identificación")]
    public int idCoche;

    [Header("Configuración de Movimiento")]
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

    // La función ahora acepta un ID.
    public void IniciarViaje(WaypointNode nodoInicial, WaypointNode nodoFinal, int id)
    {
        this.idCoche = id;
        gameObject.name = "Coche_" + id;

        if (agente == null) agente = GetComponent<NavMeshAgent>();
        this.destinoFinal = nodoFinal;

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
        }

        rutaCalculada = CalcularRutaHaciaDestino(nodoInicial, nodoFinal);

        if (rutaCalculada != null && rutaCalculada.Count > 1) // Se necesita al menos un origen y un destino.
        {
            indiceRutaActual = 1; // Nuestro primer objetivo es el SEGUNDO nodo de la ruta.
            MoverAlSiguienteNodoDeLaRuta(true); // El 'true' indica que es el primer movimiento.
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- ¡FUNCIÓN CORREGIDA! ---
    public void CambiarDestino(WaypointNode nuevoDestino)
    {
        // El punto de partida para el nuevo cálculo es el nodo al que nos dirigimos actualmente.
        WaypointNode nodoDePartida = rutaCalculada[indiceRutaActual];

        // Asignamos el nuevo destino final.
        this.destinoFinal = nuevoDestino;

        // Recalculamos la ruta desde nuestro nodo de partida.
        rutaCalculada = CalcularRutaHaciaDestino(nodoDePartida, nuevoDestino);

        if (rutaCalculada != null && rutaCalculada.Count > 0)
        {
            // Reiniciamos el índice y nos movemos al primer nodo de la NUEVA ruta.
            indiceRutaActual = 0;
            MoverAlSiguienteNodoDeLaRuta(false); // No es el primer movimiento, no necesita reorientación brusca.
        }
        else
        {
            Debug.LogWarning($"No se encontró una nueva ruta para el coche {idCoche}. Continuando con la anterior.");
            // Si no se encuentra ruta, revertimos el destino final al que tenía antes.
            this.destinoFinal = rutaCalculada.Last();
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
            MoverAlSiguienteNodoDeLaRuta(false);
        }

        if (indiceRutaActual < rutaCalculada.Count)
        {
            WaypointNode objetivoActual = rutaCalculada[indiceRutaActual];
            if (!objetivoActual.gameObject.activeInHierarchy)
            {
                WaypointNode nodoAnterior = rutaCalculada[indiceRutaActual - 1];
                IniciarViaje(nodoAnterior, destinoFinal, this.idCoche);
            }
        }
    }

    void MoverAlSiguienteNodoDeLaRuta(bool esPrimerMovimiento)
    {
        if (indiceRutaActual >= rutaCalculada.Count)
        {
            Destroy(gameObject);
            return;
        }

        WaypointNode proximoObjetivo = rutaCalculada[indiceRutaActual];

        // --- LÓGICA DE ORIENTACIÓN CORREGIDA ---
        // Solo forzamos la rotación en el primer movimiento para evitar giros bruscos después.
        if (esPrimerMovimiento)
        {
            Vector3 direccion = proximoObjetivo.transform.position - transform.position;
            if (direccion != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direccion);
            }
        }

        agente.SetDestination(proximoObjetivo.transform.position);
    }

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
