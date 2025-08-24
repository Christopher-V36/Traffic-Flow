using UnityEngine;
using System.Collections.Generic;

public class WaypointNode : MonoBehaviour
{
    [Header("Conexiones")]
    [Tooltip("Arrastra aquí los siguientes nodos a los que se puede ir desde este punto.")]
    public List<WaypointNode> siguientesNodos;

    // --- Ayuda Visual para el Editor ---
    private void OnDrawGizmos()
    {
        // Dibuja una esfera en la posición de este waypoint.
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // Dibuja una línea hacia cada uno de los siguientes nodos conectados.
        Gizmos.color = Color.green;
        if (siguientesNodos != null)
        {
            foreach (WaypointNode siguiente in siguientesNodos)
            {
                if (siguiente != null)
                {
                    Gizmos.DrawLine(transform.position, siguiente.transform.position);
                }
            }
        }
    }
}
