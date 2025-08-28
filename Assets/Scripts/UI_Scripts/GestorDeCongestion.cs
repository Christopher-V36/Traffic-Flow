using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Muy importante para poder ordenar las listas
using TMPro;

public class GestorDeCongestion : MonoBehaviour
{
    // --- Singleton Pattern ---
    // Esto crea una única instancia estática del gestor para que sea fácil
    // acceder a él desde cualquier otro script (como MedidorDeFlujo).
    public static GestorDeCongestion Instance { get; private set; }

    [Header("Configuración de UI")]
    [Tooltip("Arrastra aquí el objeto de TextMeshPro que mostrará el ranking.")]
    public TextMeshProUGUI textoTopCalles;

    // Lista privada donde se registrarán todos los sensores de las calles.
    private List<MedidorDeFlujo> medidoresDeFlujo = new List<MedidorDeFlujo>();

    // Temporizador para no actualizar la UI en cada frame, optimizando el rendimiento.
    private float tiempoParaActualizar = 1.0f;
    private float temporizador;

    private void Awake()
    {
        // Configuración del Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // Métodos para que los sensores se registren y se den de baja.
    public void RegistrarMedidor(MedidorDeFlujo medidor)
    {
        if (!medidoresDeFlujo.Contains(medidor))
        {
            medidoresDeFlujo.Add(medidor);
        }
    }

    public void DesregistrarMedidor(MedidorDeFlujo medidor)
    {
        if (medidoresDeFlujo.Contains(medidor))
        {
            medidoresDeFlujo.Remove(medidor);
        }
    }

    void Update()
    {
        // Usamos un temporizador para actualizar el ranking solo una vez por segundo.
        temporizador -= Time.deltaTime;
        if (temporizador <= 0f)
        {
            temporizador = tiempoParaActualizar;
            ActualizarRanking();
        }
    }

    void ActualizarRanking()
    {
        if (textoTopCalles == null || medidoresDeFlujo.Count == 0) return;

        // --- La Magia del Ranking ---
        // 1. Ordena la lista de medidores de mayor a menor según su conteo de coches.
        // 2. Toma solo los 3 primeros elementos de la lista ordenada.
        var top3Calles = medidoresDeFlujo.OrderByDescending(medidor => medidor.ConteoActual).Take(3);

        // 3. Construye el texto para mostrarlo en la UI.
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("--- Calles más Ocupadas ---");
        int i = 1;
        foreach (var medidor in top3Calles)
        {
            sb.AppendLine($"{i}. {medidor.nombreDeLaCalle}: <b>{medidor.ConteoActual}</b>");
            i++;
        }

        // 4. Actualiza el texto en la pantalla.
        textoTopCalles.text = sb.ToString();
    }
}