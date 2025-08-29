using UnityEngine;
using System.Collections.Generic;

public class WaypointNode : MonoBehaviour
{
    public enum TipoDeNodo
    {
        Normal,
        Origen,
        Destino
    }

    [Header("Tipo de Nodo")]
    [Tooltip("Define el rol de este nodo en la simulación.")]
    public TipoDeNodo tipoDeNodo = TipoDeNodo.Normal;

    // --- ¡NUEVA VARIABLE! ---
    [Header("Identificación de Destino")]
    [Tooltip("Si este nodo es un 'Destino', asígnale un nombre único (ej: 'Estadio', 'Centro').")]
    public string nombreDestino = "";


    [Header("Conexiones")]
    [Tooltip("Arrastra aquí los siguientes nodos a los que se puede ir desde este punto.")]
    public List<WaypointNode> siguientesNodos;

    private void OnDrawGizmos()
    {
        // --- LÓGICA DE COLOR MODIFICADA ---
        // Elegimos un color basándonos en el tipo de nodo.
        switch (tipoDeNodo)
        {
            case TipoDeNodo.Origen:
                Gizmos.color = Color.cyan; // Los puntos de origen serán cian.
                break;
            case TipoDeNodo.Destino:
                Gizmos.color = Color.magenta; // Los puntos de destino serán magenta.
                break;
            default:
                Gizmos.color = Color.blue; // Los nodos normales seguirán siendo azules.
                break;
        }

        // Dibuja una esfera en la posición de este waypoint con el color elegido.
        Gizmos.DrawWireSphere(transform.position, 3.0f); // Aumentamos el radio (1.5f * 3)

        // Dibuja una línea y una flecha hacia cada uno de los siguientes nodos conectados.
        Gizmos.color = Color.green;
        if (siguientesNodos != null)
        {
            foreach (WaypointNode siguiente in siguientesNodos)
            {
                if (siguiente != null)
                {
                    Vector3 from = transform.position;
                    Vector3 to = siguiente.transform.position;

                    Gizmos.DrawLine(from, to);

                    Vector3 direction = (to - from).normalized;
                    Quaternion arrowRotation = Quaternion.LookRotation(direction);

                    float arrowHeadLength = 5.5f; // Aumentamos la longitud (2.5f * 3)
                    float arrowHeadAngle = 20.0f;

                    Vector3 right = arrowRotation * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
                    Vector3 left = arrowRotation * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;

                    Gizmos.DrawRay(to, right * arrowHeadLength);
                    Gizmos.DrawRay(to, left * arrowHeadLength);
                }
            }
        }
    }
}
