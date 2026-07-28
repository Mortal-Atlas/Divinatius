using System;
using System.IO;
using UnityEngine;

namespace Divinatius.Core
{
    [Serializable]
    public class ApiConfigData
    {
        public string geminiApiKey = "";
        public string geminiModel = "gemini-1.5-flash";
        public string elevenLabsApiKey = "";
    }

    [ExecuteAlways]
    public class ApiConfig : MonoBehaviour
    {
        public static ApiConfig Instance { get; private set; }

        [Header("🔑 API Credentials (Inspector Editable)")]
        [Tooltip("Your Google Gemini API Key")]
        [SerializeField] private string geminiApiKey = "";

        [Tooltip("Gemini Model ID (e.g. gemini-1.5-flash, gemini-1.5-pro, gemini-2.0-flash)")]
        [SerializeField] private string geminiModel = "gemini-1.5-flash";

        [Tooltip("Your ElevenLabs API Key for Voice Synthesis")]
        [SerializeField] private string elevenLabsApiKey = "";

        private static ApiConfigData _cachedData = new ApiConfigData();

        public static ApiConfigData Data
        {
            get
            {
                if (Instance != null)
                {
                    _cachedData.geminiApiKey = Instance.geminiApiKey;
                    _cachedData.geminiModel = Instance.geminiModel;
                    _cachedData.elevenLabsApiKey = Instance.elevenLabsApiKey;
                }
                return _cachedData;
            }
        }

        private void Awake()
        {
            if (Instance == null || Instance == this)
            {
                Instance = this;
                if (Application.isPlaying)
                {
                    DontDestroyOnLoad(gameObject);
                }
                LoadConfigFromResources();
            }
            else
            {
                if (Application.isPlaying)
                {
                    Destroy(gameObject);
                }
            }
        }

        private void OnValidate()
        {
            _cachedData.geminiApiKey = geminiApiKey;
            _cachedData.geminiModel = geminiModel;
            _cachedData.elevenLabsApiKey = elevenLabsApiKey;
        }

        public void LoadConfigFromResources()
        {
            TextAsset secretAsset = Resources.Load<TextAsset>("Secrets/api_config");
            if (secretAsset != null && !string.IsNullOrEmpty(secretAsset.text))
            {
                try
                {
                    var loaded = JsonUtility.FromJson<ApiConfigData>(secretAsset.text);
                    if (loaded != null)
                    {
                        if (string.IsNullOrEmpty(geminiApiKey)) geminiApiKey = loaded.geminiApiKey;
                        if (string.IsNullOrEmpty(geminiModel)) geminiModel = loaded.geminiModel;
                        if (string.IsNullOrEmpty(elevenLabsApiKey)) elevenLabsApiKey = loaded.elevenLabsApiKey;

                        _cachedData = loaded;
                        Debug.Log("[ApiConfig] Loaded API settings from Resources/Secrets/api_config.json");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ApiConfig] Error parsing api_config.json: {ex.Message}");
                }
            }
        }

        public void SaveConfigToResources()
        {
#if UNITY_EDITOR
            try
            {
                string dirPath = Path.Combine(Application.dataPath, "Resources", "Secrets");
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                ApiConfigData dataToSave = new ApiConfigData
                {
                    geminiApiKey = this.geminiApiKey,
                    geminiModel = this.geminiModel,
                    elevenLabsApiKey = this.elevenLabsApiKey
                };

                string json = JsonUtility.ToJson(dataToSave, true);
                string filePath = Path.Combine(dirPath, "api_config.json");
                File.WriteAllText(filePath, json);
                UnityEditor.AssetDatabase.Refresh();
                Debug.Log($"[ApiConfig] Saved Inspector API keys to: {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ApiConfig] Failed saving API config to file: {ex.Message}");
            }
#endif
        }
    }
}
