using UnityEngine;
using TMPro;

public class MedidorDeFlujo : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("El texto que mostrará el número de coches en la zona.")]
    public TextMeshProUGUI textoFlujo;

    // --- LÓGICA COMPLETAMENTE NUEVA Y SIMPLIFICADA ---
    // Un simple contador para llevar la cuenta de los coches.
    private int cochesDentroDelTrigger = 0;

    void Start()
    {
        // Nos aseguramos de que el contador empiece en 0 al iniciar.
        ActualizarTexto();
    }

    // Este método se llama UNA VEZ cuando un objeto ENTRA en el trigger.
    private void OnTriggerEnter(Collider other)
    {
        // Si el objeto que entró tiene la etiqueta "Coche"...
        if (other.CompareTag("Coche"))
        {
            // ...incrementamos nuestro contador.
            cochesDentroDelTrigger++;
            // Actualizamos el texto en la pantalla inmediatamente.
            ActualizarTexto();
        }
    }

    // Este método se llama UNA VEZ cuando un objeto SALE del trigger.
    private void OnTriggerExit(Collider other)
    {
        // Si el objeto que salió tiene la etiqueta "Coche"...
        if (other.CompareTag("Coche"))
        {
            // ...decrementamos nuestro contador.
            cochesDentroDelTrigger--;
            // Actualizamos el texto en la pantalla inmediatamente.
            ActualizarTexto();
        }
    }

    // Una función central para actualizar la UI y mantener el código limpio.
    void ActualizarTexto()
    {
        if (textoFlujo != null)
        {
            textoFlujo.text = $"Coches en la calle: {cochesDentroDelTrigger}";
        }
    }
}
