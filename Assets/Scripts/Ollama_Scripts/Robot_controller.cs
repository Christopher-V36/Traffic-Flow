using UnityEngine;
using TMPro;

public class RobotController : MonoBehaviour
{
    // Referencia al TextMeshPro que mostrará los mensajes del robot
    public TextMeshProUGUI robotDisplay;

    /// <summary>
    /// Muestra un mensaje en la pantalla del robot.
    /// </summary>
    public void MostrarMensaje(string mensaje)
    {
        if (robotDisplay != null)
        {
            robotDisplay.text = mensaje;
        }
    }
}