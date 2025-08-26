using UnityEngine;
using System.Collections.Generic;

public class ControladorInterseccion : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Identificador único para esta intersección (ej: 'A1', 'E3').")]
    public string idInterseccion = "A1";

    [Tooltip("Arrastra aquí el BoxCollider que define el área de la intersección.")]
    public BoxCollider zonaDeDeteccion;

    [Header("Bloqueo Visual")]
    [Tooltip("Arrastra aquí los conos, barreras, etc., que bloquearán la calle.")]
    public List<GameObject> objetosDeBloqueo;

    private List<GameObject> waypointsDeLaInterseccion = new List<GameObject>();
    private bool estaActiva = true;

    void Awake()
    {
        DetectarWaypointsEnLaZona();
    }

    void Start()
    {
        foreach (GameObject bloqueo in objetosDeBloqueo)
        {
            if (bloqueo != null)
            {
                bloqueo.SetActive(false);
            }
        }
    }

    void DetectarWaypointsEnLaZona()
    {
        if (zonaDeDeteccion == null)
        {
            Debug.LogError($"¡Falta asignar la Zona de Detección en la intersección {idInterseccion}!");
            return;
        }

        Collider[] collidersEnLaZona = Physics.OverlapBox(
            zonaDeDeteccion.transform.position + zonaDeDeteccion.center,
            zonaDeDeteccion.size / 2,
            zonaDeDeteccion.transform.rotation
        );

        foreach (Collider col in collidersEnLaZona)
        {
            if (col.GetComponent<WaypointNode>() != null)
            {
                waypointsDeLaInterseccion.Add(col.gameObject);
            }
        }
    }

    // La función SetEstado vuelve a ser simple, sin la lógica de aviso.
    public void SetEstado(bool activar)
    {
        estaActiva = activar;

        foreach (GameObject wp in waypointsDeLaInterseccion)
        {
            if (wp != null)
            {
                wp.SetActive(estaActiva);
            }
        }

        foreach (GameObject bloqueo in objetosDeBloqueo)
        {
            if (bloqueo != null)
            {
                bloqueo.SetActive(!estaActiva);
            }
        }
        Debug.Log($"Intersección {idInterseccion} ahora está {(estaActiva ? "ABIERTA" : "CERRADA")}");
    }
}