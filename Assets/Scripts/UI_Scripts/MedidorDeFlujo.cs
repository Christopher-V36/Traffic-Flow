using UnityEngine;
using System.Collections.Generic;

public class MedidorDeFlujo : MonoBehaviour
{
    [Header("Identificación")]
    public string nombreDeLaCalle = "Calle sin nombre";

    [Header("Congestión Automática")]
    public int limiteDeCongestion = 5;
    public BoxCollider zonaDeLaCalle;

    [Header("Bloqueo Manual")]
    public List<GameObject> objetosDeBloqueo;

    public int ConteoActual { get; private set; } = 0;

    private List<GameObject> waypointsDeLaCalle = new List<GameObject>();
    private bool cerradaPorCongestion = false;
    private bool cerradaManualmente = false;

    private void Awake()
    {
        DetectarWaypointsEnLaZona();
    }

    private void Start()
    {
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

    public void RecontarCoches()
    {
        if (zonaDeLaCalle == null) return;

        Collider[] collidersEnLaZona = Physics.OverlapBox(
            zonaDeLaCalle.transform.position + zonaDeLaCalle.center,
            zonaDeLaCalle.size / 2,
            zonaDeLaCalle.transform.rotation
        );

        int nuevoConteo = 0;
        foreach (Collider col in collidersEnLaZona)
        {
            if (col.CompareTag("Coche"))
            {
                nuevoConteo++;
            }
        }
        ConteoActual = nuevoConteo;
    }

    public void SetEstadoManual(bool activar)
    {
        cerradaManualmente = !activar;
        Debug.Log($"La calle '{nombreDeLaCalle}' cambia su estado MANUAL a {(activar ? "ABIERTA" : "CERRADA")}.");
        foreach (GameObject wp in waypointsDeLaCalle)
        {
            if (wp != null) wp.SetActive(activar);
        }
        foreach (GameObject bloqueo in objetosDeBloqueo)
        {
            if (bloqueo != null) bloqueo.SetActive(!activar);
        }
    }

    private void SetEstadoCongestion(bool activar)
    {
        cerradaPorCongestion = !activar;
        Debug.LogWarning($"La calle '{nombreDeLaCalle}' cambia su estado por CONGESTIÓN a {(activar ? "ABIERTA" : "CERRADA")}.");
        foreach (GameObject wp in waypointsDeLaCalle)
        {
            if (wp != null) wp.SetActive(activar);
        }
    }

    void DetectarWaypointsEnLaZona()
    {
        if (zonaDeLaCalle == null) return;
        Collider[] collidersEnLaZona = Physics.OverlapBox(zonaDeLaCalle.transform.position + zonaDeLaCalle.center, zonaDeLaCalle.size / 2, zonaDeLaCalle.transform.rotation);
        foreach (Collider col in collidersEnLaZona)
        {
            if (col.GetComponent<WaypointNode>() != null) waypointsDeLaCalle.Add(col.gameObject);
        }
    }
}