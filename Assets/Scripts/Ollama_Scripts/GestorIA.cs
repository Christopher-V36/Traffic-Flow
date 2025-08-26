using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class GestorIA : MonoBehaviour
{
    [Header("Ollama Configuration")]
    private const string ollamaApiEndpoint = "http://localhost:11434/api/generate";
    public string ollamaModelName = "mistral";
    [Range(0.0f, 1.0f)]
    public float temperature = 0.2f;
    public int maxTokens = 100;

    [Header("UI References")]
    public TMP_InputField inputField;
    public Button sendButton;

    [Header("Objetos de la Simulación")]
    public List<InterseccionSemaforizada> interseccionesSemaforizadas;
    public GeneradorDeTrafico generadorDeTrafico;
    public List<ControladorInterseccion> interseccionesControlables;
    public Transform contenedorDestinos;

    [System.Serializable]
    private class ComandoIA
    {
        public string accion;
        public string color;
        public int cantidad;
        public string id_semaforo;
        public string id_interseccion;
        public string grupo;
        public int id_coche;
        public int id_destino;
    }

    void Start()
    {
        sendButton.onClick.AddListener(ProcesarComandoDeUsuario);
    }

    public void ProcesarComandoDeUsuario()
    {
        string comandoUsuario = inputField.text;
        if (string.IsNullOrWhiteSpace(comandoUsuario)) return;

        SetUIInteractable(false);

        string promptParaIA = $@"
        Tu rol es ser un controlador de una simulación de tráfico.
        Convierte el siguiente comando de lenguaje natural a un formato JSON.
        - Para semáforos, usa 'id_semaforo' (string).
        - Para abrir/cerrar intersecciones, usa 'id_interseccion' (string).

        Ejemplos:
        - Usuario: 'Semaforo B3 horizontal rojo' -> {{""accion"":""cambiar_grupo_semaforo"", ""id_semaforo"":""B3"", ""grupo"":""B"", ""color"":""rojo""}}
        - Usuario: 'cierra la intersección E3' -> {{""accion"":""cerrar_interseccion"", ""id_interseccion"":""E3""}}
        - Usuario: 'abre la intersección A1' -> {{""accion"":""abrir_interseccion"", ""id_interseccion"":""A1""}}
        - Usuario: 'agrega 5 coches' -> {{""accion"":""agregar_coches"", ""cantidad"":5}}
        - Usuario: 'elimina 10 coches' -> {{""accion"":""quitar_coches"", ""cantidad"":10}}
        - Usuario: 'envía el coche 7 al destino 14' -> {{""accion"":""cambiar_destino_coche"", ""id_coche"":7, ""id_destino"":14}}

        Ahora, convierte este comando:
        Usuario: '{comandoUsuario}' ->";

        StartCoroutine(EnviarComandoALaIA(promptParaIA));
    }

    private IEnumerator EnviarComandoALaIA(string prompt)
    {
        OllamaOptions options = new OllamaOptions { temperature = this.temperature, num_predict = this.maxTokens };
        OllamaGenerateRequest requestData = new OllamaGenerateRequest { model = ollamaModelName, prompt = prompt, stream = false, options = options };
        string jsonRequestBody = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest(ollamaApiEndpoint, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error al conectar con Ollama: " + request.error);
            }
            else
            {
                try
                {
                    OllamaGenerateResponse response = JsonUtility.FromJson<OllamaGenerateResponse>(request.downloadHandler.text);
                    string respuestaJson = response.response.Trim();
                    ComandoIA comando = JsonUtility.FromJson<ComandoIA>(respuestaJson);
                    if (comando != null)
                    {
                        EjecutarComando(comando);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error al parsear la respuesta JSON de la IA: " + e.Message);
                }
            }
            SetUIInteractable(true);
        }
    }

    void EjecutarComando(ComandoIA comando)
    {
        if (comando == null) return;

        switch (comando.accion)
        {
            case "cambiar_grupo_semaforo":
                InterseccionSemaforizada interseccionSemaforo = interseccionesSemaforizadas.FirstOrDefault(i => i.idInterseccionSemaforo.Equals(comando.id_semaforo, System.StringComparison.OrdinalIgnoreCase));
                if (interseccionSemaforo != null)
                {
                    interseccionSemaforo.ForzarEstadoGrupo(comando.grupo, comando.color);
                }
                break;

            case "cerrar_interseccion":
            case "abrir_interseccion":
                ControladorInterseccion interseccion = interseccionesControlables.FirstOrDefault(i => i.idInterseccion.Equals(comando.id_interseccion, System.StringComparison.OrdinalIgnoreCase));
                if (interseccion != null)
                {
                    interseccion.SetEstado(comando.accion == "abrir_interseccion");
                }
                break;

            case "agregar_coches":
                if (generadorDeTrafico != null)
                {
                    StartCoroutine(GenerarCochesConRetraso(comando.cantidad));
                }
                break;

            case "quitar_coches":
            case "eliminar_coches":
                QuitarCoches(comando.cantidad);
                break;

            case "cambiar_destino_coche":
                ControladorCocheHibrido coche = FindObjectsOfType<ControladorCocheHibrido>().FirstOrDefault(c => c.idCoche == comando.id_coche);
                Transform destinoTransform = contenedorDestinos.Find("wp (" + comando.id_destino + ")");
                if (coche != null && destinoTransform != null)
                {
                    WaypointNode nuevoDestino = destinoTransform.GetComponent<WaypointNode>();
                    if (nuevoDestino != null)
                    {
                        coche.CambiarDestino(nuevoDestino);
                    }
                }
                break;
        }
    }

    private IEnumerator GenerarCochesConRetraso(int cantidad)
    {
        for (int i = 0; i < cantidad; i++)
        {
            generadorDeTrafico.GenerarCocheBajoDemanda();
            yield return new WaitForSeconds(0.75f);
        }
    }

    void QuitarCoches(int cantidad)
    {
        GameObject[] cochesEnEscena = GameObject.FindGameObjectsWithTag("Coche");
        int cochesAEliminar = Mathf.Min(cantidad, cochesEnEscena.Length);
        for (int i = 0; i < cochesAEliminar; i++)
        {
            Destroy(cochesEnEscena[i]);
        }
    }

    private void SetUIInteractable(bool interactable)
    {
        if (inputField != null) inputField.interactable = interactable;
        if (sendButton != null) sendButton.interactable = interactable;
    }
}