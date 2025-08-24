using UnityEngine;
using TMPro; // No olvides añadir esta línea para usar TextMeshPro

public class GestorUI : MonoBehaviour
{
    [Header("Elementos de la UI")]
    [Tooltip("El texto que mostrará el número de coches.")]
    public TextMeshProUGUI textoContadorCoches;

    // Update se llama una vez por frame
    void Update()
    {
        // En cada frame, buscamos todos los objetos con la etiqueta "Coche".
        GameObject[] coches = GameObject.FindGameObjectsWithTag("Coche");

        // Obtenemos la cantidad de coches encontrados.
        int numeroDeCoches = coches.Length;

        // Actualizamos el texto en la pantalla.
        textoContadorCoches.text = "Coches en Escena: " + numeroDeCoches;
    }
}