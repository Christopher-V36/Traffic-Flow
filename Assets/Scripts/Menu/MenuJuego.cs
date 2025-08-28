using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuJuego : MonoBehaviour
{
    public GameObject panelOpciones;
    public GameObject panelCam;
    public GameObject panelMov;

    public AudioClip[] listaMusica;
    public AudioSource audioSource;
    public Slider sliderVolumen;

    public Button botonSilenciar;
    public Sprite iconoVolumenOn;
    public Sprite iconoVolumenOff;

    public Button botonPausarMusica;
    public Sprite iconoMusicaOn;
    public Sprite iconoMusicaOff;

    public GameObject cameraAerea;
    public GameObject cameraIso;
    public float step = 10f;
    private Vector3 initialPosAerea;
    private Vector3 initialPosIso;
    /*public GameObject cameraPers;*/

    public float zoomSpeed = 5f;
    public float minZoom = 30f;
    public float maxZoom = 70f;

    public TMPro.TextMeshProUGUI textoNombreCancion;

    private int indiceCancionActual = 0;
    private bool estaSilenciado = false;
    private bool estaActivo = false;
    private bool estaAbierto = false;

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
        initialPosAerea = cameraAerea.GetComponent<Camera>().transform.position;
        initialPosIso= cameraIso.GetComponent<Camera>().transform.position;
    }

    private void Update()
    {
        ManejarMov();
    }

    public void Inicio()
    {
        SceneManager.LoadScene("Menu");
    }

    public void MostrarOpciones()
    {
        panelOpciones.SetActive(true);
    }

    public void VolverAlMenuPrincipal()
    {
        panelOpciones.SetActive(false);
    }

    public void MostrarCam()
    {
        if (estaActivo == false)
        {
            panelCam.SetActive(true);
            estaActivo = true;
        }
        else
        {
            panelCam.SetActive(false);
            estaActivo = false;
        }
    }

    public void MostrarMov()
    {
        if (estaAbierto == false)
        {
            panelMov.SetActive(true);
            estaAbierto = true;
        }
        else
        {
            panelMov.SetActive(false);
            estaAbierto = false;
        }
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
    public void CamaraAerea()
    {
        cameraIso.SetActive(false);
        cameraIso.GetComponent<AudioListener>().enabled = false;
        //cameraPers.SetActive(false);
        //cameraPers.GetComponent<AudioListener>().enabled = false;
        cameraAerea.SetActive(true);
        cameraAerea.GetComponent<AudioListener>().enabled = true;
    }

    public void CamaraIso()
    {
        cameraAerea.SetActive(false);
        cameraAerea.GetComponent<AudioListener>().enabled = false;
        //cameraPers.SetActive(false);
        //cameraPers.GetComponent<AudioListener>().enabled = false;
        cameraIso.SetActive(true);
        cameraIso.GetComponent<AudioListener>().enabled = true;
    }

    /*public void CamaraPers()
    {
        cameraIso.SetActive(false);
        cameraIso.GetComponent<AudioListener>().enabled = false;
        cameraAerea.SetActive(false);
        cameraAerea.GetComponent<AudioListener>().enabled = false;
        cameraPers.SetActive(true);
        cameraPers.GetComponent<AudioListener>().enabled = true;
    }*/
    private Camera CamaraActiva()
    {
        if (cameraAerea.activeInHierarchy)
        {
            return cameraAerea.GetComponent<Camera>();
        }
        if (cameraIso.activeInHierarchy)
        {
            return cameraIso.GetComponent<Camera>();
        }
        /*if (cameraPers.activeInHierarchy)
        {
            return cameraPers.GetComponent<Camera>();
        }*/
        return null;
    }

    public void ZoomMas()
    {
        Camera activeCamera = CamaraActiva();
        if (activeCamera != null)
        {
            activeCamera.fieldOfView = Mathf.Max(activeCamera.fieldOfView - zoomSpeed, minZoom);
        }
    }

    public void ZoomMenos()
    {
        Camera activeCamera = CamaraActiva();
        if (activeCamera != null)
        {
            activeCamera.fieldOfView = Mathf.Min(activeCamera.fieldOfView + zoomSpeed, maxZoom);
        }
    }

    void ManejarMov()
    {
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            Der();
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            Izq();
        }
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            Arriba();
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            Abajo();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Reiniciar();
        }
    }

    public void Der()
    {
        Camera camActiva = CamaraActiva();
        Vector3 newPosition = camActiva.transform.position;
        newPosition.x += step;
        camActiva.transform.position = newPosition;
    }

    public void Izq()
    {
        Camera camActiva = CamaraActiva();
        Vector3 newPosition = camActiva.transform.position;
        newPosition.x -= step;
        camActiva.transform.position = newPosition;
    }

    public void Arriba()
    {
        Camera camActiva = CamaraActiva();
        Vector3 newPosition = camActiva.transform.position;
        newPosition.z += step;
        camActiva.transform.position = newPosition;
    }

    public void Abajo()
    {
        Camera camActiva = CamaraActiva();
        Vector3 newPosition = camActiva.transform.position;
        newPosition.z -= step;
        camActiva.transform.position = newPosition;
    }

    public void Reiniciar()
    {
        Camera camActiva = CamaraActiva();
        if (camActiva.gameObject == cameraAerea)
        {
            camActiva.transform.position = initialPosAerea;
        }
        else if (camActiva.gameObject == cameraIso)
        {
            camActiva.transform.position = initialPosIso;
        }
        camActiva.fieldOfView = 60f;
    }
}