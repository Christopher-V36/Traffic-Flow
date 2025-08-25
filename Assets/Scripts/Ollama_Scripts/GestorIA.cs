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
    public List<ControladorSemaforo> semaforosControlables;
    public GeneradorDeTrafico generadorDeTrafico;
    public List<ControladorInterseccion> interseccionesControlables;
    public Transform contenedorDestinos; // Necesitamos esto para encontrar los destinos por ID.

    [System.Serializable]
    private class ComandoIA
    {
        public string accion;
        public string color;
        public int id_semaforo;
        public int cantidad;
        public int id_interseccion;
        public int id_coche; // ¡NUEVO CAMPO!
        public int id_destino; // ¡NUEVO CAMPO!
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
        
        Ejemplos:
        - Usuario: 'pon el semáforo 2 en rojo' -> {{""accion"":""cambiar_semaforo"", ""id_semaforo"":2, ""color"":""rojo""}}
        - Usuario: 'agrega 5 coches' -> {{""accion"":""agregar_coches"", ""cantidad"":5}}
        - Usuario: 'elimina 3 carros' -> {{""accion"":""quitar_coches"", ""cantidad"":3}}
        - Usuario: 'cierra la intersección 1' -> {{""accion"":""cerrar_interseccion"", ""id_interseccion"":1}}
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
                    string respuestaJson = response.response;
                    ComandoIA comando = JsonUtility.FromJson<ComandoIA>(respuestaJson);
                    if (comando != null)
                    {
                        EjecutarComando(comando);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Error al parsear la respuesta JSON de la IA: " + e.Message);
                }
            }
        }
        SetUIInteractable(true);
    }

    void EjecutarComando(ComandoIA comando)
    {
        if (comando == null) return;

        switch (comando.accion)
        {
            // ... (casos anteriores sin cambios)
            case "cambiar_semaforo":
                ControladorSemaforo semaforo = semaforosControlables.FirstOrDefault(s => s.idSemaforo == comando.id_semaforo);
                if (semaforo != null)
                {
                    Debug.Log($"Ejecutando comando: Cambiar semáforo {comando.id_semaforo} a {comando.color}.");
                    semaforo.ForzarEstadoDesdeIA(comando.color);
                }
                else
                {
                    Debug.LogWarning($"No se encontró un semáforo con ID: {comando.id_semaforo}");
                }
                SetUIInteractable(true);
                break;

            case "agregar_coches":
                Debug.Log($"Ejecutando comando: Agregar {comando.cantidad} coche(s).");
                if (generadorDeTrafico != null)
                {
                    StartCoroutine(GenerarCochesConRetraso(comando.cantidad));
                }
                else
                {
                    SetUIInteractable(true);
                }
                break;

            case "quitar_coches":
                Debug.Log($"Ejecutando comando: Quitar {comando.cantidad} coche(s).");
                QuitarCoches(comando.cantidad);
                SetUIInteractable(true);
                break;

            case "cerrar_interseccion":
            case "abrir_interseccion":
                ControladorInterseccion interseccion = interseccionesControlables.Find(i => i.idInterseccion == comando.id_interseccion);
                if (interseccion != null)
                {
                    bool activar = (comando.accion == "abrir_interseccion");
                    interseccion.SetEstado(activar);
                }
                else
                {
                    Debug.LogWarning($"No se encontró la intersección con ID: {comando.id_interseccion}");
                }
                SetUIInteractable(true);
                break;

            // --- ¡NUEVO CASO DE ACCIÓN! ---
            case "cambiar_destino_coche":
                // 1. Buscamos el coche por su ID.
                ControladorCocheHibrido coche = FindObjectsOfType<ControladorCocheHibrido>().FirstOrDefault(c => c.idCoche == comando.id_coche);
                // 2. Buscamos el nodo de destino por su nombre (ej: "wp (14)").
                Transform destinoTransform = contenedorDestinos.Find("wp (" + comando.id_destino + ")");

                if (coche != null && destinoTransform != null)
                {
                    WaypointNode nuevoDestino = destinoTransform.GetComponent<WaypointNode>();
                    if (nuevoDestino != null)
                    {
                        Debug.Log($"Enviando coche {comando.id_coche} al nuevo destino {nuevoDestino.name}");
                        coche.CambiarDestino(nuevoDestino);
                    }
                }
                else
                {
                    Debug.LogWarning($"No se pudo encontrar el coche {comando.id_coche} o el destino {comando.id_destino}");
                }
                SetUIInteractable(true);
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
        SetUIInteractable(true);
    }

    void QuitarCoches(int cantidad)
    {
        GameObject[] cochesEnEscena = GameObject.FindGameObjectsWithTag("Coche");
        for (int i = 0; i < cantidad; i++)
        {
            if (i < cochesEnEscena.Length)
            {
                Destroy(cochesEnEscena[i]);
            }
        }
    }

    private void SetUIInteractable(bool interactable)
    {
        inputField.interactable = interactable;
        sendButton.interactable = interactable;
    }
}
