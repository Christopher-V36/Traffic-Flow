using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class GestorDeCongestion : MonoBehaviour
{
    public static GestorDeCongestion Instance { get; private set; }

    [Header("UI Ranking de Calles")]
    public TextMeshProUGUI textoTopCalles;

    [Header("UI Nivel de Tráfico Global")]
    public TextMeshProUGUI textoNivelDeTraficoGlobal;
    public int umbralTraficoLigero = 30;
    public int umbralTraficoMedio = 70;

    // --- ¡NUEVA VARIABLE! ---
    [Tooltip("Margen para evitar que el nivel de tráfico cambie constantemente. Un valor de 5 significa que para bajar de 'Medio' a 'Ligero', el conteo debe ser menor que (umbral - 5).")]
    public int margenHisteresis = 5;

    private List<MedidorDeFlujo> medidoresDeFlujo = new List<MedidorDeFlujo>();
    private float tiempoParaActualizar = 1.0f;
    private float temporizador;

    // --- ¡NUEVO! Guardamos el estado actual para evitar cambios rápidos ---
    private enum NivelTrafico { Ligero, Medio, Alto }
    private NivelTrafico nivelActual = NivelTrafico.Ligero;


    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void RegistrarMedidor(MedidorDeFlujo medidor)
    {
        if (!medidoresDeFlujo.Contains(medidor)) medidoresDeFlujo.Add(medidor);
    }

    public void DesregistrarMedidor(MedidorDeFlujo medidor)
    {
        if (medidoresDeFlujo.Contains(medidor)) medidoresDeFlujo.Remove(medidor);
    }

    void Update()
    {
        temporizador -= Time.deltaTime;
        if (temporizador <= 0f)
        {
            temporizador = tiempoParaActualizar;
            foreach (var medidor in medidoresDeFlujo)
            {
                medidor.RecontarCoches();
            }
            ActualizarRanking();
            ActualizarNivelDeTraficoGlobal();
        }
    }

    // --- ¡FUNCIÓN MODIFICADA CON LÓGICA DE HISTÉRESIS! ---
    void ActualizarNivelDeTraficoGlobal()
    {
        if (textoNivelDeTraficoGlobal == null) return;

        int totalCochesEnCalles = medidoresDeFlujo.Sum(medidor => medidor.ConteoActual);

        // Decidimos si cambiamos de nivel basándonos en el nivel actual y los umbrales + el margen.
        switch (nivelActual)
        {
            case NivelTrafico.Ligero:
                if (totalCochesEnCalles > umbralTraficoLigero)
                {
                    nivelActual = NivelTrafico.Medio;
                }
                break;
            case NivelTrafico.Medio:
                if (totalCochesEnCalles > umbralTraficoMedio)
                {
                    nivelActual = NivelTrafico.Alto;
                }
                else if (totalCochesEnCalles < umbralTraficoLigero - margenHisteresis)
                {
                    nivelActual = NivelTrafico.Ligero;
                }
                break;
            case NivelTrafico.Alto:
                if (totalCochesEnCalles < umbralTraficoMedio - margenHisteresis)
                {
                    nivelActual = NivelTrafico.Medio;
                }
                break;
        }

        // Actualizamos la UI con el estado actual ya estabilizado.
        string textoNivel = "";
        Color colorDeTrafico = Color.white;
        switch (nivelActual)
        {
            case NivelTrafico.Ligero:
                textoNivel = "Ligero";
                colorDeTrafico = Color.green;
                break;
            case NivelTrafico.Medio:
                textoNivel = "Medio";
                colorDeTrafico = Color.yellow;
                break;
            case NivelTrafico.Alto:
                textoNivel = "Alto";
                colorDeTrafico = Color.red;
                break;
        }

        textoNivelDeTraficoGlobal.text = $"<color=#{ColorUtility.ToHtmlStringRGB(colorDeTrafico)}>{textoNivel}</color>";
    }

    void ActualizarRanking()
    {
        if (textoTopCalles == null || medidoresDeFlujo.Count == 0) return;
        var top3Calles = medidoresDeFlujo.OrderByDescending(medidor => medidor.ConteoActual).Take(3);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        int i = 1;
        foreach (var medidor in top3Calles)
        {
            sb.AppendLine($"{i}. {medidor.nombreDeLaCalle}: <b>{medidor.ConteoActual}</b>");
            i++;
        }
        textoTopCalles.text = sb.ToString();
    }
}