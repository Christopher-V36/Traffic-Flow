using UnityEngine;
using UnityEngine.AI;
using System.Collections;
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

    private bool estabaDetenidoAnteriormente = false;

    private Coroutine vigilanteDeAtasco;

    void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (rutaCalculada == null || rutaCalculada.Count == 0 || !agente.isOnNavMesh || indiceRutaActual >= rutaCalculada.Count)
        {
            return;
        }

        ComprobarObstaculos();

        bool debeDetenerse = semaforoEnRojo || obstaculoAdelante;

        if (debeDetenerse)
        {
            agente.isStopped = true;
            estabaDetenidoAnteriormente = true;

            if (vigilanteDeAtasco != null)
            {
                StopCoroutine(vigilanteDeAtasco);
                vigilanteDeAtasco = null;
            }
        }
        else
        {
            if (estabaDetenidoAnteriormente)
            {
                agente.isStopped = false;
                WaypointNode objetivoActual = rutaCalculada[indiceRutaActual];
                agente.SetDestination(objetivoActual.transform.position);
                estabaDetenidoAnteriormente = false;

                if (vigilanteDeAtasco != null) StopCoroutine(vigilanteDeAtasco);
                vigilanteDeAtasco = StartCoroutine(VigilarSiEstaAtascado());
            }
            else if (!agente.pathPending && agente.remainingDistance <= distanciaMinimaAlNodo)
            {
                if (vigilanteDeAtasco != null)
                {
                    StopCoroutine(vigilanteDeAtasco);
                    vigilanteDeAtasco = null;
                }

                indiceRutaActual++;
                MoverAlSiguienteNodoDeLaRuta(false);
            }
        }
    }

    private IEnumerator VigilarSiEstaAtascado()
    {
        // Espera 1.5 segundos para darle tiempo al coche de acelerar.
        yield return new WaitForSeconds(1.5f);

        if (agente.velocity.sqrMagnitude < 0.01f && !semaforoEnRojo && !obstaculoAdelante)
        {
            Debug.LogWarning($"¡FALLO DETECTADO! Coche {idCoche} atascado. Forzando avance al siguiente nodo.");

            indiceRutaActual++;
            MoverAlSiguienteNodoDeLaRuta(false);
        }
        vigilanteDeAtasco = null;
    }


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
        if (rutaCalculada != null && rutaCalculada.Count > 1)
        {
            indiceRutaActual = 1;
            MoverAlSiguienteNodoDeLaRuta(true);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CambiarDestino(WaypointNode nuevoDestino)
    {
        if (indiceRutaActual >= rutaCalculada.Count) return;
        WaypointNode nodoDePartida = rutaCalculada[indiceRutaActual];
        this.destinoFinal = nuevoDestino;
        rutaCalculada = CalcularRutaHaciaDestino(nodoDePartida, nuevoDestino);
        if (rutaCalculada != null && rutaCalculada.Count > 0)
        {
            indiceRutaActual = 0;
            MoverAlSiguienteNodoDeLaRuta(false);
        }
        else
        {
            Debug.LogWarning($"No se encontró una nueva ruta para el coche {idCoche}. Continuando con la anterior.");
            this.destinoFinal = rutaCalculada.Last();
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
            // --- CAMBIO: Ahora detecta otros coches O las barreras ---
            obstaculoAdelante = hit.collider.CompareTag("Coche") || hit.collider.CompareTag("Barrera");
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

