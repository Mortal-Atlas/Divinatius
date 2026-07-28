using System;
using UnityEngine;
using UnityEngine.UI;

namespace Divinatius.Dialogue
{
    public class DialogueInputController : MonoBehaviour
    {
        [Header("UI Inputs")]
        [SerializeField] private InputField textInputField;
        [SerializeField] private Button sendButton;
        [SerializeField] private Button micRecordButton;
        [SerializeField] private Text micButtonText;

        private bool _isRecording = false;
        private AudioClip _recordedClip;
        private Action<string> _onInputSubmitted;

        public void Initialize(Action<string> onInputSubmitted)
        {
            _onInputSubmitted = onInputSubmitted;

            if (sendButton != null)
            {
                sendButton.onClick.RemoveAllListeners();
                sendButton.onClick.AddListener(SubmitTextInput);
            }

            if (micRecordButton != null)
            {
                micRecordButton.onClick.RemoveAllListeners();
                micRecordButton.onClick.AddListener(ToggleMicrophoneRecording);
            }
        }

        private void Update()
        {
            if (textInputField != null && textInputField.isFocused && Input.GetKeyDown(KeyCode.Return))
            {
                SubmitTextInput();
            }
        }

        public void SubmitTextInput()
        {
            if (textInputField == null) return;
            string inputStr = textInputField.text?.Trim();
            if (!string.IsNullOrEmpty(inputStr))
            {
                textInputField.text = "";
                _onInputSubmitted?.Invoke(inputStr);
            }
        }

        public void ToggleMicrophoneRecording()
        {
            if (!_isRecording)
            {
                StartRecording();
            }
            else
            {
                StopRecordingAndProcess();
            }
        }

        private void StartRecording()
        {
            if (Microphone.devices.Length == 0)
            {
                Debug.LogWarning("[DialogueInputController] No microphone devices found!");
                return;
            }

            _isRecording = true;
            if (micButtonText != null) micButtonText.text = "🔴 Stop Rec";

            string device = Microphone.devices[0];
            _recordedClip = Microphone.Start(device, false, 10, 44100);
            Debug.Log($"[DialogueInputController] Recording started on microphone device: {device}");
        }

        private void StopRecordingAndProcess()
        {
            if (!_isRecording) return;
            _isRecording = false;
            if (micButtonText != null) micButtonText.text = "🎤 Voice STT";

            if (Microphone.devices.Length > 0)
            {
                string device = Microphone.devices[0];
                Microphone.End(device);
                Debug.Log("[DialogueInputController] Microphone recording stopped.");

                // For STT: simulate or pass audio buffer to STT endpoint.
                // Fallback demo text if STT service key is not configured:
                string recognizedSpeechText = "[Voice Speech-To-Text Input]";
                _onInputSubmitted?.Invoke(recognizedSpeechText);
            }
        }
    }
}
