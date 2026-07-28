using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Divinatius.Core;
using Divinatius.NPC;

namespace Divinatius.AI
{
    public class GeminiService : MonoBehaviour
    {
        public static GeminiService Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void GenerateResponse(NPCProfileSO profile, string latestPlayerInput, Action<string> onComplete, Action<string> onError = null)
        {
            StartCoroutine(SendGeminiRequestCoroutine(profile, latestPlayerInput, onComplete, onError));
        }

        private IEnumerator SendGeminiRequestCoroutine(NPCProfileSO profile, string latestPlayerInput, Action<string> onComplete, Action<string> onError)
        {
            string apiKey = ApiConfig.Data.geminiApiKey;
            string modelName = !string.IsNullOrEmpty(ApiConfig.Data.geminiModel) ? ApiConfig.Data.geminiModel : "gemini-1.5-flash";

            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                Debug.LogWarning("[GeminiService] Gemini API Key is missing. Please set your key in the ApiConfig Inspector component on '--- MANAGERS ---'.");
                onComplete?.Invoke($"[Demo Mode for {profile.characterName}]: Please enter your Google Gemini API Key in the Unity Inspector under the '--- MANAGERS ---' object to enable live responses.");
                yield break;
            }

            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={apiKey}";

            string systemPrompt = CharacterMemoryManager.Instance != null ?
                CharacterMemoryManager.Instance.FormatContextPrompt(profile) :
                profile.systemPersonalityPrompt;

            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.Append("{\n");
            jsonBuilder.Append("  \"system_instruction\": {\n");
            jsonBuilder.Append($"    \"parts\": [ {{ \"text\": \"{EscapeJsonString(systemPrompt)}\" }} ]\n");
            jsonBuilder.Append("  },\n");
            jsonBuilder.Append("  \"contents\": [\n");

            List<DialogueMessage> history = new List<DialogueMessage>();
            if (CharacterMemoryManager.Instance != null)
            {
                var memory = CharacterMemoryManager.Instance.GetOrCreateMemory(profile.characterId, profile.characterName);
                if (memory != null && memory.conversationHistory != null)
                {
                    history = memory.conversationHistory;
                }
            }

            bool hasPreviousTurns = false;
            foreach (var msg in history)
            {
                if (string.IsNullOrEmpty(msg.text)) continue;
                if (hasPreviousTurns) jsonBuilder.Append(",\n");

                string role = msg.speakerRole == "player" ? "user" : "model";
                jsonBuilder.Append($"    {{\n      \"role\": \"{role}\",\n      \"parts\": [ {{ \"text\": \"{EscapeJsonString(msg.text)}\" }} ]\n    }}");
                hasPreviousTurns = true;
            }

            if (hasPreviousTurns) jsonBuilder.Append(",\n");
            jsonBuilder.Append($"    {{\n      \"role\": \"user\",\n      \"parts\": [ {{ \"text\": \"{EscapeJsonString(latestPlayerInput)}\" }} ]\n    }}\n");

            jsonBuilder.Append("  ]\n}");

            string jsonBody = jsonBuilder.ToString();
            byte[] postData = Encoding.UTF8.GetBytes(jsonBody);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(postData);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;
                    string reply = ParseGeminiResponse(jsonResponse);
                    onComplete?.Invoke(reply);
                }
                else
                {
                    string err = $"[GeminiService] API Error ({request.responseCode}): {request.error}\n{request.downloadHandler.text}";
                    Debug.LogError(err);
                    if (onError != null) onError.Invoke(err);
                    else onComplete?.Invoke($"[Error generating response for {profile.characterName}]");
                }
            }
        }

        private string ParseGeminiResponse(string json)
        {
            try
            {
                int textIdx = json.IndexOf("\"text\": \"");
                if (textIdx != -1)
                {
                    int start = textIdx + 9;
                    int end = json.IndexOf("\"", start);
                    if (end != -1)
                    {
                        string extracted = json.Substring(start, end - start);
                        return UnescapeJsonString(extracted);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GeminiService] Error parsing Gemini JSON response: {ex.Message}");
            }
            return "I understand clearly.";
        }

        private string EscapeJsonString(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return str.Replace("\\", "\\\\")
                      .Replace("\"", "\\\"")
                      .Replace("\n", "\\n")
                      .Replace("\r", "\\r")
                      .Replace("\t", "\\t");
        }

        private string UnescapeJsonString(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return str.Replace("\\n", "\n")
                      .Replace("\\r", "\r")
                      .Replace("\\\"", "\"")
                      .Replace("\\\\", "\\");
        }
    }
}
