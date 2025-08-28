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
    public Image fondo;
    public Button sendButton;
    public TextMeshProUGUI statusText;

    [Header("Objetos de la Simulación")]
    public List<InterseccionSemaforizada> interseccionesSemaforizadas;
    public GeneradorDeTrafico generadorDeTrafico;
    public List<ControladorInterseccion> interseccionesControlables;
    public List<MedidorDeFlujo> callesControlables; // <--- FUSIONADO
    public Transform contenedorDestinos;
    public RobotController robotController;
    public RobotAnimator robotAnimator;

    [System.Serializable]
    private class ComandoIA
    {
        public string accion;
        public string color;
        public int cantidad;
        public string id_semaforo;
        public string id_interseccion; // <--- FUSIONADO (Cambiado a string)
        public string nombre_calle;   // <--- FUSIONADO
        public string grupo;
        public int id_coche;
        public int id_destino;
        public string mensaje;
    }

    void Start()
    {
        sendButton.onClick.AddListener(ProcesarComandoDeUsuario);
        StartCoroutine(PrecargarModelo());
    }

    private IEnumerator PrecargarModelo()
    {
        if (statusText != null)
        {
            statusText.gameObject.SetActive(true);
            fondo.gameObject.SetActive(true);
            statusText.text = "Precargando modelo de IA...";
        }

        string promptPrecarga = @"Tu rol es un controlador de una simulación de tráfico. SIEMPRE debes convertir el siguiente comando a un formato JSON. No respondas con texto libre. Ahora, convierte este comando: Usuario: 'hola' ->";
        OllamaOptions options = new OllamaOptions { temperature = this.temperature, num_predict = this.maxTokens };
        OllamaGenerateRequest requestData = new OllamaGenerateRequest { model = ollamaModelName, prompt = promptPrecarga, stream = false, options = options };
        string jsonRequestBody = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest(ollamaApiEndpoint, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Modelo precargado exitosamente");
                if (statusText != null)
                {
                    statusText.text = "IA lista para usar";
                    Invoke("OcultarStatusText", 2f);
                }
            }
            else
            {
                Debug.LogError($"Error al precargar modelo: {request.error}");
                if (statusText != null)
                {
                    statusText.text = "Error al precargar IA. Verifica que Ollama esté corriendo.";
                    Invoke("OcultarStatusText", 3f);
                }
            }
        }
    }

    public void ProcesarComandoDeUsuario()
    {
        string comandoUsuario = inputField.text;
        if (string.IsNullOrWhiteSpace(comandoUsuario)) return;
        SetUIInteractable(false);

        // --- PROMPT FUSIONADO ---
        string promptParaIA = $@"
        Tu rol es un controlador de una simulación de tráfico.
        SIEMPRE debes convertir el siguiente comando a un formato JSON. No respondas con texto libre.
        - Para semáforos, usa 'id_semaforo' (string).
        - Para abrir/cerrar intersecciones, usa 'id_interseccion' (string).
        - Para abrir/cerrar calles, usa 'nombre_calle' (string).
        - Para que el robot hable, usa la acción 'mostrar_mensaje_robot' y el campo 'mensaje'.
        - Para comandos no reconocidos, usa la acción 'no_valido'.

        Ejemplos:
        - Usuario: 'Semaforo B3 horizontal rojo' -> {{""accion"":""cambiar_grupo_semaforo"", ""id_semaforo"":""B3"", ""grupo"":""B"", ""color"":""rojo""}}
        - Usuario: 'cierra la intersección E3' -> {{""accion"":""cerrar_interseccion"", ""id_interseccion"":""E3""}}
        - Usuario: 'abre la calle Avenida Principal' -> {{""accion"":""abrir_calle"", ""nombre_calle"":""Avenida Principal""}}
        - Usuario: 'agrega 5 coches' -> {{""accion"":""agregar_coches"", ""cantidad"":5}}
        - Usuario: 'hola' -> {{""accion"":""mostrar_mensaje_robot"", ""mensaje"":""¡Hola! ¿En qué puedo ayudarte?""}}
        - Usuario: 'gracias' -> {{""accion"":""mostrar_mensaje_robot"", ""mensaje"":""¡De nada! ¡Estoy a tu disposición!""}}
        - Usuario: 'por favor, mueve los coches' -> {{""accion"":""no_valido""}}

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
                MostrarEstado("Fallo en la comunicación con la IA. Intenta de nuevo.");
            }
            else
            {
                try
                {
                    string rawResponse = request.downloadHandler.text;
                    string finalResponseJson = "";
                    string[] jsonLines = rawResponse.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string line in jsonLines)
                    {
                        OllamaGenerateResponse responsePart = JsonUtility.FromJson<OllamaGenerateResponse>(line);
                        if (responsePart != null && !string.IsNullOrEmpty(responsePart.response))
                        {
                            finalResponseJson += responsePart.response;
                        }
                    }

                    finalResponseJson = finalResponseJson.Replace("```json", "").Replace("```", "").Trim();
                    Debug.Log("Respuesta JSON limpia de la IA: " + finalResponseJson);
                    ComandoIA comando = JsonUtility.FromJson<ComandoIA>(finalResponseJson);

                    if (comando != null)
                    {
                        EjecutarComando(comando);
                    }
                    else
                    {
                        MostrarEstado("Fallo al procesar el comando. El formato JSON no es válido.");
                        Debug.LogError("Error: El comando parseado es nulo. JSON inválido: " + finalResponseJson);
                    }
                }
                catch (Exception e)
                {
                    MostrarEstado("Fallo en la comunicación con la IA. Intenta de nuevo.");
                    Debug.LogError("Error al parsear la respuesta JSON de la IA: " + e.Message);
                }
            }
            SetUIInteractable(true);
        }
    }

    void EjecutarComando(ComandoIA comando)
    {
        if (comando == null || string.IsNullOrEmpty(comando.accion))
        {
            MostrarEstado("La respuesta de la IA no es válida. Intenta con un comando diferente.");
            return;
        }

        switch (comando.accion)
        {
            case "cambiar_grupo_semaforo":
                InterseccionSemaforizada interseccionSemaforo = interseccionesSemaforizadas.FirstOrDefault(i => i.idInterseccionSemaforo.Equals(comando.id_semaforo, StringComparison.OrdinalIgnoreCase));
                if (interseccionSemaforo != null)
                {
                    interseccionSemaforo.ForzarEstadoGrupo(comando.grupo, comando.color);
                    MostrarEstado($"¡Listo! Cambié el semáforo '{comando.id_semaforo}' a color {comando.color}.");
                    ActivarAnimacionRobot();
                }
                else
                {
                    MostrarEstado($"¡Oh no! No pude encontrar el semáforo '{comando.id_semaforo}'.");
                    ActivarAnimacionRobot(triste: true);
                }
                break;

            case "cerrar_interseccion":
            case "abrir_interseccion":
                ControladorInterseccion interseccion = interseccionesControlables.FirstOrDefault(i => i.idInterseccion.Equals(comando.id_interseccion, StringComparison.OrdinalIgnoreCase));
                if (interseccion != null)
                {
                    bool abrir = comando.accion == "abrir_interseccion";
                    interseccion.SetEstado(abrir);
                    MostrarEstado($"¡Entendido! {(abrir ? "Abriendo" : "Cerrando")} la intersección {comando.id_interseccion}.");
                    ActivarAnimacionRobot();
                }
                else
                {
                    MostrarEstado($"Lo siento, no pude encontrar la intersección '{comando.id_interseccion}'.");
                    ActivarAnimacionRobot(triste: true);
                }
                break;

            // --- CASE FUSIONADO ---
            case "cerrar_calle":
            case "abrir_calle":
                MedidorDeFlujo calle = callesControlables.FirstOrDefault(c => c.nombreDeLaCalle.Equals(comando.nombre_calle, StringComparison.OrdinalIgnoreCase));
                if (calle != null)
                {
                    bool abrir = comando.accion == "abrir_calle";
                    calle.SetEstadoManual(abrir);
                    MostrarEstado($"¡A la orden! {(abrir ? "Abriendo" : "Cerrando")} la calle '{comando.nombre_calle}'.");
                    ActivarAnimacionRobot();
                }
                else
                {
                    MostrarEstado($"Lo siento, no encontré la calle '{comando.nombre_calle}'.");
                    ActivarAnimacionRobot(triste: true);
                }
                break;

            case "agregar_coches":
                StartCoroutine(GenerarCochesConRetraso(comando.cantidad));
                MostrarEstado($"¡Excelente! Agregando {comando.cantidad} coches. ¡Que comience el caos!");
                ActivarAnimacionRobot();
                break;

            case "quitar_coches":
            case "eliminar_coches":
                int cochesEliminados = QuitarCoches(comando.cantidad);
                MostrarEstado($"¡Hecho! Eliminé {cochesEliminados} coche(s). ¡El camino está más despejado!");
                ActivarAnimacionRobot();
                break;

            case "cambiar_destino_coche":
                ControladorCocheHibrido coche = FindObjectsOfType<ControladorCocheHibrido>().FirstOrDefault(c => c.idCoche == comando.id_coche);
                Transform destinoTransform = contenedorDestinos.Find("wp (" + comando.id_destino + ")");
                if (coche != null && destinoTransform != null)
                {
                    coche.CambiarDestino(destinoTransform.GetComponent<WaypointNode>());
                    MostrarEstado($"El coche {comando.id_coche} ahora se dirige al destino {comando.id_destino}.");
                    ActivarAnimacionRobot();
                }
                else
                {
                    MostrarEstado("Fallo: No se encontró el coche o el destino especificado.");
                    ActivarAnimacionRobot(triste: true);
                }
                break;

            case "mostrar_mensaje_robot":
                robotController.MostrarMensaje(comando.mensaje);
                MostrarEstado(comando.mensaje);
                ActivarAnimacionRobot();
                break;

            case "no_valido":
                MostrarEstado("¡Lo siento! No entendí tu comando. Intenta con una instrucción más específica.");
                ActivarAnimacionRobot(triste: true);
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

    int QuitarCoches(int cantidad)
    {
        GameObject[] cochesEnEscena = GameObject.FindGameObjectsWithTag("Coche");
        int cochesAEliminar = Mathf.Min(cantidad, cochesEnEscena.Length);
        for (int i = 0; i < cochesAEliminar; i++)
        {
            if (cochesEnEscena[i] != null)
            {
                Destroy(cochesEnEscena[i]);
            }
        }
        return cochesAEliminar;
    }

    private void SetUIInteractable(bool interactable)
    {
        if (inputField != null) inputField.interactable = interactable;
        if (sendButton != null) sendButton.interactable = interactable;
    }

    private void MostrarEstado(string mensaje)
    {
        Debug.Log(mensaje);
        if (statusText != null)
        {
            if (!statusText.gameObject.activeSelf)
            {
                fondo.gameObject.SetActive(true);
                statusText.gameObject.SetActive(true);
            }
            statusText.text = mensaje;
            Invoke("OcultarStatusText", 5f);
        }
        if (inputField != null) inputField.text = "";
    }

    void ActivarAnimacionRobot(bool triste = false)
    {
        if (robotAnimator == null) return;

        if (triste)
        {
            robotAnimator.StartTalkingSad();
        }
        else
        {
            robotAnimator.StartTalking();
        }
        Invoke("StopTalkingNow", 3f);
    }

    void StopTalkingNow()
    {
        if (robotAnimator != null) robotAnimator.StopTalking();
    }
    void OcultarStatusText()
    {
        if (statusText != null && statusText.gameObject.activeSelf)
        {
            fondo.gameObject.SetActive(false);
            statusText.gameObject.SetActive(false);
        }
    }
}