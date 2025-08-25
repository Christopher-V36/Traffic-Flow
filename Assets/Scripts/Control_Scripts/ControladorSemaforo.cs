using UnityEngine;

public class ControladorSemaforo : MonoBehaviour
{
    [Header("Identificación")]
    public int idSemaforo = 1;

    [Header("Configuración del Material")]
    public MeshRenderer semaforoRenderer;

    [Header("Colores de Emisión (Luces)")]
    public Color colorRojoOn = Color.red;
    public Color colorAmarilloOn = Color.yellow;
    public Color colorVerdeOn = Color.green;

    public enum EstadoSemaforo { Verde, Amarillo, Rojo }
    public EstadoSemaforo estadoActual { get; private set; }

    private Material semaforoMaterial;

    void Awake()
    {
        // Usamos Awake para asegurar que el material esté listo inmediatamente.
        if (semaforoRenderer != null)
        {
            semaforoMaterial = semaforoRenderer.material;
            semaforoMaterial.EnableKeyword("_EMISSION");
        }
    }

    // --- FUNCIONES DE COMANDO PÚBLICAS ---
    public void PonerEnVerde()
    {
        estadoActual = EstadoSemaforo.Verde;
        CambiarLuz(colorVerdeOn);
    }

    public void PonerEnAmarillo()
    {
        estadoActual = EstadoSemaforo.Amarillo;
        CambiarLuz(colorAmarilloOn);
    }

    public void PonerEnRojo()
    {
        estadoActual = EstadoSemaforo.Rojo;
        CambiarLuz(colorRojoOn);
    }

    private void CambiarLuz(Color colorDeEmision)
    {
        if (semaforoMaterial != null)
        {
            semaforoMaterial.SetColor("_EmissionColor", colorDeEmision);
        }
    }

    // La función de control por IA ahora simplemente llama a las funciones de comando.
    public void ForzarEstadoDesdeIA(string color)
    {
        switch (color.ToLower())
        {
            case "verde": PonerEnVerde(); break;
            case "amarillo": PonerEnAmarillo(); break;
            case "rojo": PonerEnRojo(); break;
        }
    }
}
