using System;
using System.Collections.Generic;
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

        private const string DEFAULT_FALLBACK_VOICE_ID = "EXAVITQu4vr4xnSDxMaL"; // Sarah (Verified Premade Voice)

        private AudioSource _defaultAudioSource;

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

            _defaultAudioSource = gameObject.GetComponent<AudioSource>();
            if (_defaultAudioSource == null)
            {
                _defaultAudioSource = gameObject.AddComponent<AudioSource>();
            }
            _defaultAudioSource.spatialBlend = 0f; // 2D clean fallback
        }

        public void SpeakText(string text, string voiceId, Action onPlaybackStarted = null, Action onPlaybackFinished = null)
        {
            SpeakText(text, voiceId, null, onPlaybackStarted, onPlaybackFinished);
        }

        public void SpeakText(string text, string voiceId, Transform speakerTransform, Action onPlaybackStarted = null, Action onPlaybackFinished = null)
        {
            StartCoroutine(SpeakTextCoroutine(text, voiceId, speakerTransform, false, onPlaybackStarted, onPlaybackFinished));
        }

        private IEnumerator SpeakTextCoroutine(string text, string voiceId, Transform speakerTransform, bool isRetry, Action onPlaybackStarted, Action onPlaybackFinished)
        {
            string apiKey = ApiConfig.Data.elevenLabsApiKey;
            if (string.IsNullOrEmpty(voiceId)) voiceId = DEFAULT_FALLBACK_VOICE_ID;

            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_ELEVEN_LABS_API_KEY_HERE")
            {
                Debug.LogWarning("[ElevenLabsService] ElevenLabs API Key is missing or default. Skipping voice audio synthesis.");
                onPlaybackFinished?.Invoke();
                yield break;
            }

            string url = $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}?output_format=mp3_44100_128";

            string jsonBody = $@"{{
  ""text"": ""{EscapeJsonString(text)}"",
  ""model_id"": ""eleven_multilingual_v2"",
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
                    if (clip != null)
                    {
                        AudioSource sourceToUse = _defaultAudioSource;

                        if (speakerTransform != null)
                        {
                            sourceToUse = speakerTransform.GetComponent<AudioSource>();
                            if (sourceToUse == null)
                            {
                                sourceToUse = speakerTransform.gameObject.AddComponent<AudioSource>();
                            }
                            sourceToUse.spatialBlend = 0.35f;
                            sourceToUse.minDistance = 2.0f;
                            sourceToUse.maxDistance = 20.0f;
                            sourceToUse.rolloffMode = AudioRolloffMode.Logarithmic;
                        }

                        onPlaybackStarted?.Invoke();
                        sourceToUse.clip = clip;
                        sourceToUse.volume = 1.0f;
                        sourceToUse.Play();
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
                    string responseBody = "";
                    if (request.downloadHandler != null && request.downloadHandler.data != null)
                    {
                        responseBody = Encoding.UTF8.GetString(request.downloadHandler.data);
                    }

                    // If 404 Voice Not Found and not already retried, fall back automatically to default premade voice!
                    if (!isRetry && (request.responseCode == 404 || responseBody.Contains("voice_not_found")))
                    {
                        Debug.LogWarning($"[ElevenLabsService] Voice ID '{voiceId}' not found on ElevenLabs. Automatically falling back to default voice '{DEFAULT_FALLBACK_VOICE_ID}'...");
                        StartCoroutine(SpeakTextCoroutine(text, DEFAULT_FALLBACK_VOICE_ID, speakerTransform, true, onPlaybackStarted, onPlaybackFinished));
                        yield break;
                    }

                    Debug.LogError($"[ElevenLabsService] TTS Error ({request.responseCode}): {request.error} | VoiceId: {voiceId} | Response: {responseBody}");
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
