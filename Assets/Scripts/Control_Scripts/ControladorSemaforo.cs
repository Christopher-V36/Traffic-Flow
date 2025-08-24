using UnityEngine;
using System.Collections;

public class ControladorSemaforo : MonoBehaviour
{
    [Header("Identificación")]
    [Tooltip("ID único para este semáforo.")]
    public int idSemaforo = 1; // ¡NUEVA VARIABLE!

    [Header("Configuración del Material")]
    public MeshRenderer semaforoRenderer;

    [Header("Colores de Emisión (Luces)")]
    public Color colorRojoOn = Color.red;
    public Color colorAmarilloOn = Color.yellow;
    public Color colorVerdeOn = Color.green;

    [Header("Tiempos del Ciclo")]
    public float tiempoVerde = 5.0f;
    public float tiempoAmarillo = 2.0f;
    public float tiempoRojo = 5.0f;

    public enum EstadoSemaforo { Verde, Amarillo, Rojo }
    public EstadoSemaforo estadoActual { get; private set; }

    private Material semaforoMaterial;

    void Start()
    {
        if (semaforoRenderer != null)
        {
            semaforoMaterial = semaforoRenderer.material;
            semaforoMaterial.EnableKeyword("_EMISSION");
        }
        else
        {
            Debug.LogError("No se ha asignado el MeshRenderer del semáforo.");
            return;
        }

        StartCoroutine(CicloDelSemaforo());
    }

    IEnumerator CicloDelSemaforo()
    {
        while (true)
        {
            estadoActual = EstadoSemaforo.Verde;
            CambiarLuz(colorVerdeOn);
            yield return new WaitForSeconds(tiempoVerde);

            estadoActual = EstadoSemaforo.Amarillo;
            CambiarLuz(colorAmarilloOn);
            yield return new WaitForSeconds(tiempoAmarillo);

            estadoActual = EstadoSemaforo.Rojo;
            CambiarLuz(colorRojoOn);
            yield return new WaitForSeconds(tiempoRojo);
        }
    }

    void CambiarLuz(Color colorDeEmision)
    {
        semaforoMaterial.SetColor("_EmissionColor", colorDeEmision);
    }

    public void ForzarEstadoDesdeIA(string color)
    {
        StopAllCoroutines();
        switch (color.ToLower())
        {
            case "verde":
                estadoActual = EstadoSemaforo.Verde;
                CambiarLuz(colorVerdeOn);
                break;
            case "amarillo":
                estadoActual = EstadoSemaforo.Amarillo;
                CambiarLuz(colorAmarilloOn);
                break;
            case "rojo":
                estadoActual = EstadoSemaforo.Rojo;
                CambiarLuz(colorRojoOn);
                break;
        }
    }
}
