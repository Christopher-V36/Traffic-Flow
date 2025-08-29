using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public GameObject panelInformacion;
    public GameObject panelOpciones;

    public AudioClip[] listaMusica;
    public AudioSource audioSource;
    public Slider sliderVolumen;

    public Button botonSilenciar;
    public Sprite iconoVolumenOn;
    public Sprite iconoVolumenOff;

    public Button botonPausarMusica;
    public Sprite iconoMusicaOn;
    public Sprite iconoMusicaOff;

    public TMPro.TextMeshProUGUI textoNombreCancion;

    private int indiceCancionActual = 0;
    private bool estaSilenciado = false;

    void Start()
    {
        if (audioSource != null && listaMusica.Length > 0)
        {
            audioSource.clip = listaMusica[indiceCancionActual];
            audioSource.Play();

            if (sliderVolumen != null)
            {
                sliderVolumen.value = audioSource.volume;
            }

            ActualizarTextoCancion();
        }
    }

    public void Jugar()
    {
        SceneManager.LoadScene("Mapa_3.0");
    }

    public void MostrarInformacion()
    {
        panelInformacion.SetActive(true);
    }

    public void OcultarInformacion()
    {
        panelInformacion.SetActive(false);
    }

    public void MostrarOpciones()
    {
        panelInformacion.SetActive(false);
        panelOpciones.SetActive(true);
    }

    public void VolverAlMenuPrincipal()
    {
        panelOpciones.SetActive(false);
    }

    // --- Métodos de control de la música ---

    public void SiguienteCancion()
    {
        if (listaMusica.Length == 0) return;

        bool estabaReproduciendo = audioSource.isPlaying;

        indiceCancionActual = (indiceCancionActual + 1) % listaMusica.Length;
        audioSource.clip = listaMusica[indiceCancionActual];

        if (estabaReproduciendo)
        {
            audioSource.Play();
        }

        ActualizarTextoCancion();
    }

    public void AnteriorCancion()
    {
        if (listaMusica.Length == 0) return;

        bool estabaReproduciendo = audioSource.isPlaying;

        indiceCancionActual--;
        if (indiceCancionActual < 0)
        {
            indiceCancionActual = listaMusica.Length - 1;
        }
        audioSource.clip = listaMusica[indiceCancionActual];

        if (estabaReproduciendo)
        {
            audioSource.Play();
        }

        ActualizarTextoCancion();
    }

    public void CambiarVolumen(float volumen)
    {
        if (audioSource != null)
        {
            audioSource.volume = volumen;
        }
    }

    // --- Nuevos métodos para los botones de audio ---

    public void SilenciarMusica()
    {
        estaSilenciado = !estaSilenciado;
        if (estaSilenciado)
        {
            audioSource.volume = 0;
            sliderVolumen.value = 0;
            botonSilenciar.image.sprite = iconoVolumenOff;
        }
        else
        {
            audioSource.volume = 1;
            sliderVolumen.value = 1;
            botonSilenciar.image.sprite = iconoVolumenOn;
        }
    }

    public void PausarReanudarMusica()
    {
        // ✅ Si el audio se está reproduciendo, lo pausa
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            botonPausarMusica.image.sprite = iconoMusicaOff;
        }
        // ✅ Si no se está reproduciendo (está en pausa o detenido), lo inicia
        else
        {
            audioSource.Play();
            botonPausarMusica.image.sprite = iconoMusicaOn;
        }
    }

    public void ActualizarTextoCancion()
    {
        if (textoNombreCancion != null && listaMusica.Length > 0)
        {
            textoNombreCancion.text = listaMusica[indiceCancionActual].name;
        }
    }
}