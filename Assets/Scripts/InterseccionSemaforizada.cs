using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InterseccionSemaforizada : MonoBehaviour
{
    [Header("Identificación")]
    [Tooltip("Identificador único para esta intersección semaforizada (ej: 'A1', 'B3')")]
    public string idInterseccionSemaforo = "A1";

    [Header("Grupos de Semáforos")]
    [Tooltip("Semáforos que controlan el tráfico vertical (Grupo A)")]
    public List<ControladorSemaforo> grupoA;
    [Tooltip("Semáforos que controlan el tráfico horizontal (Grupo B)")]
    public List<ControladorSemaforo> grupoB;

    [Header("Tiempos del Ciclo")]
    public float tiempoVerde = 8.0f;
    public float tiempoAmarillo = 2.0f;
    // --- ¡NUEVA VARIABLE! ---
    [Tooltip("Segundos que el semáforo permanecerá en el estado forzado por la IA antes de volver al ciclo automático.")]
    public float tiempoDeControlManual = 10.0f;


    private Coroutine cicloAutomatico;
    // --- ¡NUEVA VARIABLE! ---
    // Referencia a la rutina que reanudará el ciclo automático.
    private Coroutine rutinaDeReanudacion;

    void Start()
    {
        // Inicia el ciclo automático al empezar la simulación.
        ReanudarCicloAutomatico();
    }

    IEnumerator CicloDeLaInterseccion()
    {
        Debug.Log($"Intersección {idInterseccionSemaforo}: Iniciando ciclo automático.");
        // El ciclo siempre empieza con un estado predecible.
        foreach (var semaforo in grupoA) semaforo.PonerEnVerde();
        foreach (var semaforo in grupoB) semaforo.PonerEnRojo();

        while (true)
        {
            yield return new WaitForSeconds(tiempoVerde);
            foreach (var semaforo in grupoA) semaforo.PonerEnAmarillo();
            yield return new WaitForSeconds(tiempoAmarillo);
            foreach (var semaforo in grupoA) semaforo.PonerEnRojo();
            foreach (var semaforo in grupoB) semaforo.PonerEnVerde();
            yield return new WaitForSeconds(tiempoVerde);
            foreach (var semaforo in grupoB) semaforo.PonerEnAmarillo();
            yield return new WaitForSeconds(tiempoAmarillo);
            foreach (var semaforo in grupoB) semaforo.PonerEnRojo();
            foreach (var semaforo in grupoA) semaforo.PonerEnVerde();
        }
    }

    // --- FUNCIÓN MODIFICADA ---
    public void ForzarEstadoGrupo(string grupo, string color)
    {
        // 1. Detenemos tanto el ciclo automático como cualquier cuenta atrás de reanudación previa.
        if (cicloAutomatico != null)
        {
            StopCoroutine(cicloAutomatico);
            cicloAutomatico = null;
        }
        if (rutinaDeReanudacion != null)
        {
            StopCoroutine(rutinaDeReanudacion);
        }

        // 2. Aplicamos el cambio de color solicitado por la IA.
        Debug.Log($"Intersección {idInterseccionSemaforo}: Forzando estado manual. Grupo {grupo} a {color}.");
        List<ControladorSemaforo> grupoSeleccionado = (grupo.ToUpper() == "A") ? grupoA : grupoB;
        List<ControladorSemaforo> grupoOpuesto = (grupo.ToUpper() == "A") ? grupoB : grupoA;

        foreach (var semaforo in grupoSeleccionado)
        {
            semaforo.ForzarEstadoDesdeIA(color);
        }

        // Por seguridad, si el grupo seleccionado se pone en verde, el opuesto se pone en rojo.
        if (color.ToLower() == "verde")
        {
            foreach (var semaforo in grupoOpuesto)
            {
                semaforo.ForzarEstadoDesdeIA("rojo");
            }
        }

        // 3. Iniciamos la cuenta atrás para volver al modo automático.
        rutinaDeReanudacion = StartCoroutine(ReanudarCicloTrasRetraso());
    }

    // --- ¡NUEVA FUNCIÓN! ---
    // Corrutina que espera un tiempo y luego devuelve el control al ciclo automático.
    private IEnumerator ReanudarCicloTrasRetraso()
    {
        Debug.Log($"Intersección {idInterseccionSemaforo}: Volviendo a modo automático en {tiempoDeControlManual} segundos...");
        yield return new WaitForSeconds(tiempoDeControlManual);
        ReanudarCicloAutomatico();
    }

    // --- ¡NUEVA FUNCIÓN! ---
    // Centraliza el inicio del ciclo para evitar código repetido.
    private void ReanudarCicloAutomatico()
    {
        // Si ya hay un ciclo, lo detenemos antes de empezar uno nuevo para evitar duplicados.
        if (cicloAutomatico != null)
        {
            StopCoroutine(cicloAutomatico);
        }
        cicloAutomatico = StartCoroutine(CicloDeLaInterseccion());
    }
}