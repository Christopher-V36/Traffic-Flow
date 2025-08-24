using UnityEngine;

public class VisualizadorDeRuta : MonoBehaviour
{
    // Este método especial dibuja cosas en la ventana de Scene del editor.
    private void OnDrawGizmos()
    {
        // Recorremos cada uno de los "hijos" (waypoints) del objeto que tiene este script.
        for (int i = 0; i < transform.childCount; i++)
        {
            Vector3 puntoActual = transform.GetChild(i).position;

            // Dibujamos una esfera en cada waypoint para poder verlo.
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(puntoActual, 0.5f); // La esfera tendrá un radio de 0.5 metros.

            // Si no es el primer waypoint, dibujamos una línea desde el anterior hasta el actual.
            if (i > 0)
            {
                Vector3 puntoAnterior = transform.GetChild(i - 1).position;
                Gizmos.color = Color.cyan; // Le damos un color a la línea.
                Gizmos.DrawLine(puntoAnterior, puntoActual);
            }
        }
    }
}