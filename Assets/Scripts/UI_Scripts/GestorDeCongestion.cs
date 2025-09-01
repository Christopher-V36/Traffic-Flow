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

            // --- ¡NUEVO! ---
            // Antes de calcular, le pedimos a cada sensor que actualice su conteo.
            foreach (var medidor in medidoresDeFlujo)
            {
                medidor.RecontarCoches();
            }

            // Ahora los cálculos usarán los datos más recientes.
            ActualizarRanking();
            ActualizarNivelDeTraficoGlobal();
        }
    }

    void ActualizarNivelDeTraficoGlobal()
    {
        if (textoNivelDeTraficoGlobal == null || medidoresDeFlujo.Count == 0) return;
        int totalCochesEnCalles = 0;
        foreach (var medidor in medidoresDeFlujo)
        {
            totalCochesEnCalles += medidor.ConteoActual;
        }

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
            sb.AppendLine($"{i}. {medidor.nombreDeLaCalle}: <b>{medidor.ConteoActual}</b>");
            i++;
        }
        textoTopCalles.text = sb.ToString();
    }
}