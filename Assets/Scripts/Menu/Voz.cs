using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;
using TMPro;
using System.Linq;
using System.Collections;

public class WindowsVoiceRecognizer : MonoBehaviour
{
    // === Referencias de la UI ===
    public TextMeshProUGUI statusText;

    // === Referencia al Ollama Connector ===
    public GestorIA ollamaConnector;

    private DictationRecognizer dictationRecognizer;
    // Nueva bandera para controlar el estado de escucha.
    private bool isListening = false;

    void Start()
    {
        if (statusText == null || ollamaConnector == null)
        {
            Debug.LogError("Las referencias UI y/o el Ollama Connector no están asignadas.");
            this.enabled = false;
            return;
        }

        dictationRecognizer = new DictationRecognizer();

        dictationRecognizer.DictationResult += OnDictationResult;
        dictationRecognizer.DictationComplete += OnDictationComplete;
        dictationRecognizer.DictationError += OnDictationError;
    }

    /// <summary>
    /// Método para empezar a escuchar.
    /// </summary>
    public void StartRecognizing()
    {
        if (dictationRecognizer.Status == SpeechSystemStatus.Stopped)
        {
            ShowStatus("Escuchando... Di tu comando.");
            isListening = true; // Activa la bandera de escucha
            dictationRecognizer.Start();
        }
    }

    /// <summary>
    /// Método para detener la escucha.
    /// </summary>
    public void StopRecognizing()
    {
        if (dictationRecognizer.Status == SpeechSystemStatus.Running)
        {
            ShowStatus("Deteniendo la escucha...");
            isListening = false; // Desactiva la bandera de escucha antes de detener el reconocedor
            dictationRecognizer.Stop();
        }
    }

    /// <summary>
    /// Evento que se dispara cuando se reconoce una frase completa.
    /// </summary>
    private void OnDictationResult(string text, ConfidenceLevel confidence)
    {
        // Solo procesa el resultado si la bandera de escucha está activa
        if (isListening)
        {
            ShowStatus($"Comando reconocido: '{text}'");

            // Pasa el texto reconocido al campo de entrada de OllamaConnector
            ollamaConnector.inputField.text= text;
            ollamaConnector.ProcesarComandoDeUsuario();
        }
    }

    /// <summary>
    /// Evento que se dispara al finalizar la sesión de dictado.
    /// </summary>
    private void OnDictationComplete(DictationCompletionCause cause)
    {
        if (cause != DictationCompletionCause.Complete)
        {
            ShowStatus($"Dictado completado con causa: {cause}");
        }
        // Desactiva la bandera por si acaso el proceso de detención es inesperado
        isListening = false;
    }

    /// <summary>
    /// Evento que se dispara si ocurre un error.
    /// </summary>
    private void OnDictationError(string error, int hresult)
    {
        ShowStatus($"Error de dictado: {error}");
        isListening = false; // Desactiva la bandera en caso de error
    }

    private void ShowStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log(message);
    }

    private void OnDestroy()
    {
        if (dictationRecognizer != null)
        {
            dictationRecognizer.DictationResult -= OnDictationResult;
            dictationRecognizer.DictationComplete -= OnDictationComplete;
            dictationRecognizer.DictationError -= OnDictationError;
            dictationRecognizer.Dispose();
        }
    }
}