using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InterseccionSemaforizada : MonoBehaviour
{
    [Header("Grupos de Semáforos")]
    [Tooltip("Semáforos que controlan el tráfico en un eje (ej: Norte-Sur)")]
    public List<ControladorSemaforo> grupoA;
    [Tooltip("Semáforos que controlan el tráfico en el eje perpendicular (ej: Este-Oeste)")]
    public List<ControladorSemaforo> grupoB;

    [Header("Tiempos del Ciclo")]
    public float tiempoVerde = 8.0f;
    public float tiempoAmarillo = 2.0f;

    void Start()
    {
        // Inicia el ciclo de cambio de luces para toda la intersección.
        StartCoroutine(CicloDeLaInterseccion());
    }

    IEnumerator CicloDeLaInterseccion()
    {
        // Aseguramos un estado inicial predecible: Grupo A en verde, Grupo B en rojo.
        foreach (var semaforo in grupoA) semaforo.PonerEnVerde();
        foreach (var semaforo in grupoB) semaforo.PonerEnRojo();

        while (true)
        {
            // --- FASE 1: Grupo A en Verde, Grupo B en Rojo ---
            yield return new WaitForSeconds(tiempoVerde);

            // --- FASE 2: Grupo A cambia a Amarillo ---
            foreach (var semaforo in grupoA) semaforo.PonerEnAmarillo();
            yield return new WaitForSeconds(tiempoAmarillo);

            // --- FASE 3: Grupo A cambia a Rojo, Grupo B a Verde ---
            foreach (var semaforo in grupoA) semaforo.PonerEnRojo();
            foreach (var semaforo in grupoB) semaforo.PonerEnVerde();
            yield return new WaitForSeconds(tiempoVerde);

            // --- FASE 4: Grupo B cambia a Amarillo ---
            foreach (var semaforo in grupoB) semaforo.PonerEnAmarillo();
            yield return new WaitForSeconds(tiempoAmarillo);

            // --- FASE 5: Grupo B cambia a Rojo, Grupo A a Verde (se reinicia el ciclo) ---
            foreach (var semaforo in grupoB) semaforo.PonerEnRojo();
            foreach (var semaforo in grupoA) semaforo.PonerEnVerde();
        }
    }
}
