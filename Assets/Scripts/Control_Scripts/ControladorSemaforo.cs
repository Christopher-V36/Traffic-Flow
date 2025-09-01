using UnityEngine;

public class ControladorSemaforo : MonoBehaviour
{
    [Header("Identificación")]
    public int idSemaforo = 1;

    [Header("Configuración del Renderer")]
    public MeshRenderer semaforoRenderer;

    [Header("Materiales de Emisión (Luces)")]
    public Material materialRojoOn;
    public Material materialAmarilloOn;
    public Material materialVerdeOn;
    public Material materialApagado;

    public enum EstadoSemaforo { Verde, Amarillo, Rojo }
    public EstadoSemaforo estadoActual { get; private set; }

    void Awake()
    {
        if (materialApagado != null)
        {
            semaforoRenderer.material = materialApagado;
        }
        else
        {
            PonerEnRojo();
        }
    }

    public void PonerEnVerde()
    {
        estadoActual = EstadoSemaforo.Verde;
        CambiarLuz(materialVerdeOn);
    }

    public void PonerEnAmarillo()
    {
        estadoActual = EstadoSemaforo.Amarillo;
        CambiarLuz(materialAmarilloOn);
    }

    public void PonerEnRojo()
    {
        estadoActual = EstadoSemaforo.Rojo;
        CambiarLuz(materialRojoOn);
    }

    private void CambiarLuz(Material materialDeLuz)
    {
        if (semaforoRenderer != null && materialDeLuz != null)
        {
            semaforoRenderer.material = materialDeLuz;
        }
    }

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