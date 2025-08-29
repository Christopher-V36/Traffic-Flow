using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class GestorDeCongestion : MonoBehaviour
{
    public static GestorDeCongestion Instance { get; private set; }

    [Header("UI Ranking de Calles")]
    [Tooltip("Arrastra aquí el objeto de TextMeshPro que mostrará el ranking de las 3 calles más ocupadas.")]
    public TextMeshProUGUI textoTopCalles;

    // --- ¡NUEVA SECCIÓN! ---
    [Header("UI Nivel de Tráfico Global")]
    [Tooltip("Arrastra aquí el objeto de TextMeshPro que mostrará el nivel de tráfico promedio.")]
    public TextMeshProUGUI textoNivelDeTraficoGlobal;

    [Tooltip("Número total de coches para que el tráfico se considere 'Ligero' o inferior.")]
    public int umbralTraficoLigero = 30;
    [Tooltip("Número total de coches para que el tráfico se considere 'Medio' o inferior.")]
    public int umbralTraficoMedio = 70;
    // Nota: Cualquier valor por encima de 'umbralTraficoMedio' se considerará 'Alto'.

    private List<MedidorDeFlujo> medidoresDeFlujo = new List<MedidorDeFlujo>();
    private float tiempoParaActualizar = 1.0f;
    private float temporizador;

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
            ActualizarRanking();

            // --- ¡NUEVO! ---
            // Llamamos a la nueva función para medir el tráfico global.
            ActualizarNivelDeTraficoGlobal();
        }
    }

    // --- ¡NUEVA FUNCIÓN! ---
    // Calcula el total de coches y actualiza la UI con el nivel de tráfico.
    void ActualizarNivelDeTraficoGlobal()
    {
        if (textoNivelDeTraficoGlobal == null || medidoresDeFlujo.Count == 0) return;

        // 1. Suma los coches de todos los sensores registrados.
        int totalCochesEnCalles = 0;
        foreach (var medidor in medidoresDeFlujo)
        {
            totalCochesEnCalles += medidor.ConteoActual;
        }

        // 2. Determina el nivel de tráfico y el color asociado.
        string nivelDeTrafico = "";
        Color colorDeTrafico = Color.white;

        if (totalCochesEnCalles <= umbralTraficoLigero)
        {
            nivelDeTrafico = "Ligero";
            colorDeTrafico = Color.green;
        }
        else if (totalCochesEnCalles <= umbralTraficoMedio)
        {
            nivelDeTrafico = "Medio";
            colorDeTrafico = Color.yellow;
        }
        else
        {
            nivelDeTrafico = "Alto";
            colorDeTrafico = Color.red;
        }

        // 3. Actualiza el texto de la UI usando rich text para el color.
        textoNivelDeTraficoGlobal.text = $"<color=#{ColorUtility.ToHtmlStringRGB(colorDeTrafico)}>{nivelDeTrafico}</color>";
    }

    void ActualizarRanking()
    {
        if (textoTopCalles == null || medidoresDeFlujo.Count == 0) return;

        var top3Calles = medidoresDeFlujo.OrderByDescending(medidor => medidor.ConteoActual).Take(3);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        int i = 1;
        foreach (var medidor in top3Calles)
        {
            sb.AppendLine($"{i}. {medidor.nombreDeLaCalle}: <b>({medidor.ConteoActual})</b>");
            i++;
        }
        textoTopCalles.text = sb.ToString();
    }
}