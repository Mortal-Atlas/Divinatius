using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Divinatius.Core;

namespace Divinatius.AI
{
    public class ElevenLabsService : MonoBehaviour
    {
        public static ElevenLabsService Instance { get; private set; }

        private AudioSource _audioSource;

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
                return;
            }

            _audioSource = gameObject.GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        public void SpeakText(string text, string voiceId, Action onPlaybackStarted = null, Action onPlaybackFinished = null)
        {
            StartCoroutine(SpeakTextCoroutine(text, voiceId, onPlaybackStarted, onPlaybackFinished));
        }

        private IEnumerator SpeakTextCoroutine(string text, string voiceId, Action onPlaybackStarted, Action onPlaybackFinished)
        {
            string apiKey = ApiConfig.Data.elevenLabsApiKey;
            if (string.IsNullOrEmpty(voiceId)) voiceId = "21m00Tcm4TlvDq8ikWAM"; // Default voice ID

            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_ELEVEN_LABS_API_KEY_HERE")
            {
                Debug.LogWarning("[ElevenLabsService] ElevenLabs API Key is missing or default. Skipping voice audio synthesis.");
                onPlaybackFinished?.Invoke();
                yield break;
            }

            string url = $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}";

            string jsonBody = $@"{{
  ""text"": ""{EscapeJsonString(text)}"",
  ""model_id"": ""eleven_monolingual_v1"",
  ""voice_settings"": {{
    ""stability"": 0.5,
    ""similarity_boost"": 0.75
  }}
}}";

            byte[] bodyData = Encoding.UTF8.GetBytes(jsonBody);

            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
            {
                request.method = "POST";
                request.uploadHandler = new UploadHandlerRaw(bodyData);
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("xi-api-key", apiKey);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                    if (clip != null && _audioSource != null)
                    {
                        onPlaybackStarted?.Invoke();
                        _audioSource.clip = clip;
                        _audioSource.Play();
                        yield return new WaitForSeconds(clip.length);
                        onPlaybackFinished?.Invoke();
                    }
                    else
                    {
                        onPlaybackFinished?.Invoke();
                    }
                }
                else
                {
                    Debug.LogError($"[ElevenLabsService] TTS Error ({request.responseCode}): {request.error}");
                    onPlaybackFinished?.Invoke();
                }
            }
        }

        private string EscapeJsonString(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return str.Replace("\\", "\\\\")
                      .Replace("\"", "\\\"")
                      .Replace("\n", " ")
                      .Replace("\r", " ");
        }
    }
}
