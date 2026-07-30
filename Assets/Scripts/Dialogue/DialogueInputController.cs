using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

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

        private void Awake()
        {
            SetupInputLayout();
        }

        private void SetupInputLayout()
        {
            if (micRecordButton != null)
            {
                micRecordButton.gameObject.SetActive(true);
                RectTransform micRect = micRecordButton.GetComponent<RectTransform>();
                if (micRect != null)
                {
                    micRect.anchorMin = new Vector2(0.69f, 0f);
                    micRect.anchorMax = new Vector2(0.82f, 1f);
                    micRect.offsetMin = Vector2.zero;
                    micRect.offsetMax = Vector2.zero;
                }
            }

            if (micButtonText != null)
            {
                micButtonText.gameObject.SetActive(true);
                micButtonText.text = "🎤 Voice";
            }

            if (textInputField != null)
            {
                RectTransform inputRect = textInputField.GetComponent<RectTransform>();
                if (inputRect != null)
                {
                    inputRect.anchorMin = new Vector2(0f, 0f);
                    inputRect.anchorMax = new Vector2(0.68f, 1f);
                    inputRect.offsetMin = Vector2.zero;
                    inputRect.offsetMax = Vector2.zero;
                }
            }

            if (sendButton != null)
            {
                RectTransform sendRect = sendButton.GetComponent<RectTransform>();
                if (sendRect != null)
                {
                    sendRect.anchorMin = new Vector2(0.83f, 0f);
                    sendRect.anchorMax = new Vector2(1.0f, 1f);
                    sendRect.offsetMin = Vector2.zero;
                    sendRect.offsetMax = Vector2.zero;
                }
            }
        }

        public void Initialize(Action<string> onInputSubmitted)
        {
            _onInputSubmitted = onInputSubmitted;
            SetupInputLayout();

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

            if (textInputField != null)
            {
                textInputField.onEndEdit.RemoveAllListeners();
                textInputField.onEndEdit.AddListener(OnInputFieldEndEdit);
            }
        }

        private void OnInputFieldEndEdit(string text)
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame))
            {
                SubmitTextInput();
            }
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            bool enterPressed = keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame);
            bool leftClicked = mouse != null && mouse.leftButton.wasPressedThisFrame;

            // Re-focus text input field when left clicking or typing so player is never locked out of text input
            if (textInputField != null && !textInputField.isFocused)
            {
                if (leftClicked || (keyboard != null && keyboard.anyKey.wasPressedThisFrame))
                {
                    FocusInputField();
                }
            }

            if (enterPressed)
            {
                if (_isRecording)
                {
                    StopRecordingAndProcess();
                }
                else
                {
                    SubmitTextInput();
                }
            }
        }

        public void FocusInputField()
        {
            if (textInputField != null)
            {
                textInputField.Select();
                textInputField.ActivateInputField();
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
                FocusInputField();
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
            if (micButtonText != null) micButtonText.text = "🔴 Stop & Send (Enter)";

            string device = Microphone.devices[0];
            _recordedClip = Microphone.Start(device, false, 10, 44100);
            Debug.Log($"[DialogueInputController] Recording started on microphone device: {device}");
        }

        private void StopRecordingAndProcess()
        {
            if (!_isRecording) return;
            _isRecording = false;
            if (micButtonText != null) micButtonText.text = "🎤 Voice";

            if (Microphone.devices.Length > 0)
            {
                string device = Microphone.devices[0];
                Microphone.End(device);
                Debug.Log("[DialogueInputController] Microphone recording stopped.");

                string recognizedSpeechText = "[Voice Speech-To-Text Input]";
                _onInputSubmitted?.Invoke(recognizedSpeechText);
                FocusInputField();
            }
        }
    }
}
