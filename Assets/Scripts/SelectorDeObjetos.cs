using UnityEngine;
using TMPro;
using System.Collections;
using System.Linq;
using TMPro.EditorUtilities;

public class SelectorDeObjetos : MonoBehaviour
{
    public static ControladorCocheHibrido CocheSeleccionado { get; private set; }

    [Header("UI de Selección")]
    [Tooltip("Arrastra aquí el objeto de TextMeshPro que mostrará la información.")]
    public TextMeshProUGUI textoDeSeleccion;
    public GameObject panel;

    [Tooltip("Segundos que el texto permanecerá visible en pantalla.")]
    public float tiempoVisible = 4.0f;

    private Camera camaraPrincipal;
    private Coroutine rutinaDeOcultar;

    void Start()
    {
        camaraPrincipal = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray rayo = camaraPrincipal.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(rayo).OrderBy(h => h.distance).ToArray();

            ControladorCocheHibrido cocheDetectado = null;
            MedidorDeFlujo calleDetectada = null;
            ControladorInterseccion interseccionDetectada = null; // <-- Variable añadida

            // Recorremos todos los objetos que hemos golpeado con el clic.
            foreach (RaycastHit hit in hits)
            {
                // Buscamos un coche
                if (cocheDetectado == null)
                {
                    cocheDetectado = hit.collider.GetComponentInParent<ControladorCocheHibrido>();
                }

                // Buscamos una calle
                if (calleDetectada == null)
                {
                    calleDetectada = hit.collider.GetComponent<MedidorDeFlujo>();
                }

                // --- ¡LÓGICA RESTAURADA! ---
                // Buscamos una intersección
                if (interseccionDetectada == null)
                {
                    interseccionDetectada = hit.collider.GetComponent<ControladorInterseccion>();
                }
            }

            // Decidimos qué información mostrar basándonos en lo que encontramos.
            if (cocheDetectado != null)
            {
                SeleccionarCoche(cocheDetectado);
                string textoAMostrar = $"Coche Seleccionado: ID {cocheDetectado.idCoche}";

                // Añadimos información contextual (calle o intersección)
                if (calleDetectada != null)
                {
                    textoAMostrar += $"\nEn: {calleDetectada.nombreDeLaCalle}";
                }
                else if (interseccionDetectada != null)
                {
                    textoAMostrar += $"\nEn: Intersección {interseccionDetectada.idInterseccion}";
                }
                MostrarTexto(textoAMostrar);
            }
            else if (calleDetectada != null)
            {
                DeseleccionarCoche();
                MostrarTexto($"Calle: {calleDetectada.nombreDeLaCalle}");
            }
            else if (interseccionDetectada != null) // <-- Lógica restaurada
            {
                DeseleccionarCoche();
                MostrarTexto($"Intersección: {interseccionDetectada.idInterseccion}");
            }
            else
            {
                DeseleccionarCoche();
                OcultarTexto();
            }
        }
    }

    // El resto del script (SeleccionarCoche, MostrarTexto, etc.) no cambia.

    void SeleccionarCoche(ControladorCocheHibrido coche)
    {
        CocheSeleccionado = coche;
        Debug.Log($"Coche seleccionado: ID {coche.idCoche}");
    }

    void DeseleccionarCoche()
    {
        CocheSeleccionado = null;
    }

    void MostrarTexto(string mensaje)
    {
        if (textoDeSeleccion == null) return;
        if (rutinaDeOcultar != null) StopCoroutine(rutinaDeOcultar);
        panel.SetActive(true);
        textoDeSeleccion.gameObject.SetActive(true);
        textoDeSeleccion.text = mensaje;
        rutinaDeOcultar = StartCoroutine(OcultarDespuesDeRetraso());
    }

    void OcultarTexto()
    {
        if (textoDeSeleccion != null)
        {
            textoDeSeleccion.gameObject.SetActive(false);
            panel.SetActive(false);
        }
    }

    IEnumerator OcultarDespuesDeRetraso()
    {
        yield return new WaitForSeconds(tiempoVisible);
        OcultarTexto();
    }
}