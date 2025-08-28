using UnityEngine;
using System.Collections.Generic;

public class MedidorDeFlujo : MonoBehaviour
{
    [Header("Identificación")]
    public string nombreDeLaCalle = "Calle sin nombre";

    [Header("Congestión Automática")]
    [Tooltip("Número de coches para que la calle se cierre. Pon 0 para desactivar esta función.")]
    public int limiteDeCongestion = 5;
    [Tooltip("Arrastra aquí el BoxCollider que define el área de esta calle.")]
    public BoxCollider zonaDeLaCalle;

    // --- ¡NUEVA SECCIÓN! ---
    [Header("Bloqueo Manual")]
    [Tooltip("Arrastra aquí los prefabs de barreras y conos que bloquearán la calle.")]
    public List<GameObject> objetosDeBloqueo;

    public int ConteoActual { get; private set; } = 0;

    private List<GameObject> waypointsDeLaCalle = new List<GameObject>();
    private bool cerradaPorCongestion = false;
    // --- ¡NUEVA VARIABLE! ---
    private bool cerradaManualmente = false;

    private void Awake()
    {
        DetectarWaypointsEnLaZona();
    }

    private void Start()
    {
        // Al empezar, nos aseguramos de que los bloqueos manuales estén ocultos.
        foreach (GameObject bloqueo in objetosDeBloqueo)
        {
            if (bloqueo != null) bloqueo.SetActive(false);
        }
    }

    private void OnEnable()
    {
        GestorDeCongestion.Instance?.RegistrarMedidor(this);
    }

    private void OnDisable()
    {
        GestorDeCongestion.Instance?.DesregistrarMedidor(this);
    }

    void Update()
    {
        // La lógica de congestión solo funciona si la calle no está cerrada manualmente.
        if (cerradaManualmente || limiteDeCongestion <= 0) return;

        if (ConteoActual >= limiteDeCongestion && !cerradaPorCongestion)
        {
            SetEstadoCongestion(false);
        }
        else if (ConteoActual < limiteDeCongestion && cerradaPorCongestion)
        {
            SetEstadoCongestion(true);
        }
    }

    // --- ¡NUEVA FUNCIÓN PÚBLICA PARA LA IA! ---
    public void SetEstadoManual(bool activar)
    {
        cerradaManualmente = !activar;
        Debug.Log($"La calle '{nombreDeLaCalle}' cambia su estado MANUAL a {(activar ? "ABIERTA" : "CERRADA")}.");

        // Activa/desactiva los waypoints
        foreach (GameObject wp in waypointsDeLaCalle)
        {
            if (wp != null) wp.SetActive(activar);
        }

        // Muestra/oculta los objetos de bloqueo
        foreach (GameObject bloqueo in objetosDeBloqueo)
        {
            if (bloqueo != null) bloqueo.SetActive(!activar);
        }
    }

    // La función de congestión ahora es privada y separada.
    private void SetEstadoCongestion(bool activar)
    {
        cerradaPorCongestion = !activar;
        Debug.LogWarning($"La calle '{nombreDeLaCalle}' cambia su estado por CONGESTIÓN a {(activar ? "ABIERTA" : "CERRADA")}.");

        foreach (GameObject wp in waypointsDeLaCalle)
        {
            if (wp != null) wp.SetActive(activar);
        }
    }

    // El resto del script (DetectarWaypoints, OnTrigger, etc.) no cambia.
    void DetectarWaypointsEnLaZona()
    {
        if (zonaDeLaCalle == null) return;
        Collider[] collidersEnLaZona = Physics.OverlapBox(zonaDeLaCalle.transform.position + zonaDeLaCalle.center, zonaDeLaCalle.size / 2, zonaDeLaCalle.transform.rotation);
        foreach (Collider col in collidersEnLaZona)
        {
            if (col.GetComponent<WaypointNode>() != null) waypointsDeLaCalle.Add(col.gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coche")) ConteoActual++;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Coche")) ConteoActual--;
    }
}