using System;
using System.Collections.Generic;

[System.Serializable]
public class OllamaGenerateRequest
{
    public string model;
    public string prompt;
    public bool stream = false;
    public OllamaOptions options;
}

[System.Serializable]
public class OllamaOptions
{
    public float temperature = 0.7f;
    public int num_predict = 200;
}

[System.Serializable]
public class OllamaGenerateResponse
{
    public string model;
    public string created_at;
    public string response;
    public bool done;
    public string done_reason;
    public List<int> context;
}