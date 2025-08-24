using UnityEngine;
using System.Collections.Generic;

public class ControladorInterseccion : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Identificador único para esta intersección.")]
    public int idInterseccion = 1;

    [Tooltip("Arrastra aquí TODOS los waypoints que están DENTRO de esta intersección.")]
    public List<GameObject> waypointsDeLaInterseccion;

    // --- ¡NUEVA SECCIÓN! ---
    [Header("Bloqueo Visual")]
    [Tooltip("Arrastra aquí los conos, barreras, etc., que bloquearán la calle.")]
    public List<GameObject> objetosDeBloqueo;
    // -------------------------

    private bool estaActiva = true;

    // Start se ejecuta una vez al principio del juego.
    void Start()
    {
        // Al empezar, la intersección está abierta por defecto,
        // así que nos aseguramos de que todos los objetos de bloqueo estén ocultos.
        foreach (GameObject bloqueo in objetosDeBloqueo)
        {
            if (bloqueo != null)
            {
                bloqueo.SetActive(false);
            }
        }
    }

    // Función para activar o desactivar la intersección.
    public void SetEstado(bool activar)
    {
        estaActiva = activar;

        // 1. Activa o desactiva los waypoints.
        foreach (GameObject wp in waypointsDeLaInterseccion)
        {
            if (wp != null)
            {
                wp.SetActive(estaActiva);
            }
        }

        // 2. Muestra u oculta los objetos de bloqueo.
        // Si la intersección se CIERRA (activar = false), los bloqueos se MUESTRAN.
        // Si la intersección se ABRE (activar = true), los bloqueos se OCULTAN.
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