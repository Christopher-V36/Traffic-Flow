using UnityEngine;
using System.Collections.Generic;

public class ControladorCoche : MonoBehaviour
{
    [Header("Configuración de la Ruta")]
    public Transform rutaContenedor;

    [Header("¡AJUSTA ESTOS VALORES!")]
    public float velocidad = 5.0f;
    public float velocidadDeGiro = 3.0f;
    public float distanciaMinimaAlWaypoint = 3.0f;

    // --- NUEVA VARIABLE DE DETECCIÓN ---
    [Header("Detección de Obstáculos")]
    [Tooltip("Distancia del 'láser' para detectar coches delante.")]
    public float distanciaDeDeteccion = 2.0f;

    private List<Transform> waypoints;
    private int indiceWaypointActual = 0;

    // --- NUEVOS ESTADOS DEL COCHE ---
    private bool semaforoEnRojo = false;
    private bool obstaculoAdelante = false;

    void Start()
    {
        waypoints = new List<Transform>();
        if (rutaContenedor != null)
        {
            foreach (Transform wp in rutaContenedor)
            {
                waypoints.Add(wp);
            }
        }
    }

    void Update()
    {
        // Comprobamos los obstáculos ANTES de decidir si nos movemos.
        ComprobarObstaculos();

        // El coche solo se mueve si el semáforo NO está en rojo Y NO hay un obstáculo delante.
        if (!semaforoEnRojo && !obstaculoAdelante)
        {
            MoverCoche();
        }
    }

    void ComprobarObstaculos()
    {
        // Creamos un rayo que sale desde la posición del coche, hacia adelante.
        RaycastHit hit;
        // El 'transform.forward' es la dirección hacia donde mira el coche.
        // El 'transform.position' es el punto de inicio del rayo.
        if (Physics.Raycast(transform.position, transform.forward, out hit, distanciaDeDeteccion))
        {
            // Si el rayo choca con algo que tenga la etiqueta "Coche"...
            if (hit.collider.CompareTag("Coche"))
            {
                // ...hay un obstáculo delante.
                obstaculoAdelante = true;
            }
            else
            {
                obstaculoAdelante = false;
            }
        }
        else
        {
            // Si el rayo no choca con nada, no hay obstáculo.
            obstaculoAdelante = false;
        }

        // --- DIBUJAMOS EL RAYO EN EL EDITOR PARA PODER VERLO ---
        // Esto es solo una ayuda visual, no afecta al juego.
        Color rayColor = obstaculoAdelante ? Color.red : Color.green;
        Debug.DrawRay(transform.position, transform.forward * distanciaDeDeteccion, rayColor);
    }

    void MoverCoche()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        Transform targetWaypoint = waypoints[indiceWaypointActual];

        // --- ¡NUEVA LÍNEA DE CÓDIGO! ---
        // Si nuestro waypoint objetivo está inactivo, simplemente no hacemos nada este frame.
        // El coche se detendrá porque no puede encontrar un objetivo válido.
        if (!targetWaypoint.gameObject.activeInHierarchy)
        {
            return; // Termina la función aquí y no te muevas.
        }
        // ---------------------------------

        Vector3 direccion = targetWaypoint.position - transform.position;

        if (direccion.sqrMagnitude > 0.01f)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, Time.deltaTime * velocidadDeGiro);
        }

        transform.Translate(Vector3.forward * velocidad * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetWaypoint.position) < distanciaMinimaAlWaypoint)
        {
            indiceWaypointActual++;
            if (indiceWaypointActual >= waypoints.Count)
            {
                indiceWaypointActual = 0;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        ControladorSemaforo semaforo = other.GetComponent<ControladorSemaforo>();
        if (semaforo != null)
        {
            // En lugar de parar el coche directamente, ahora solo actualizamos nuestro estado.
            semaforoEnRojo = (semaforo.estadoActual == ControladorSemaforo.EstadoSemaforo.Rojo);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<ControladorSemaforo>() != null)
        {
            // Al salir de la zona, el semáforo ya no nos afecta.
            semaforoEnRojo = false;
        }
    }
}