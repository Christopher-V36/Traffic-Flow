using UnityEngine;
using System.Collections.Generic;

public class ControladorInterseccion : MonoBehaviour
{
    [Header("Configuraci�n")]
    [Tooltip("Identificador �nico para esta intersecci�n.")]
    public int idInterseccion = 1;

    [Tooltip("Arrastra aqu� TODOS los waypoints que est�n DENTRO de esta intersecci�n.")]
    public List<GameObject> waypointsDeLaInterseccion;

    // --- �NUEVA SECCI�N! ---
    [Header("Bloqueo Visual")]
    [Tooltip("Arrastra aqu� los conos, barreras, etc., que bloquear�n la calle.")]
    public List<GameObject> objetosDeBloqueo;
    // -------------------------

    private bool estaActiva = true;

    // Start se ejecuta una vez al principio del juego.
    void Start()
    {
        // Al empezar, la intersecci�n est� abierta por defecto,
        // as� que nos aseguramos de que todos los objetos de bloqueo est�n ocultos.
        foreach (GameObject bloqueo in objetosDeBloqueo)
        {
            if (bloqueo != null)
            {
                bloqueo.SetActive(false);
            }
        }
    }

    // Funci�n para activar o desactivar la intersecci�n.
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
        // Si la intersecci�n se CIERRA (activar = false), los bloqueos se MUESTRAN.
        // Si la intersecci�n se ABRE (activar = true), los bloqueos se OCULTAN.
        foreach (GameObject bloqueo in objetosDeBloqueo)
        {
            if (bloqueo != null)
            {
                bloqueo.SetActive(!estaActiva);
            }
        }
        Debug.Log($"Intersecci�n {idInterseccion} ahora est� {(estaActiva ? "ABIERTA" : "CERRADA")}");
    }
}