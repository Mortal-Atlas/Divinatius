using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Divinatius.NPC;
using Divinatius.AI;

namespace Divinatius.Dialogue
{
    public class DialogueUIController : MonoBehaviour
    {
        public static DialogueUIController Instance { get; private set; }

        [Header("UI Root Panel")]
        [SerializeField] private GameObject dialoguePanelRoot;

        [Header("Visual Novel Portraits")]
        [SerializeField] private Image mcPortraitImage;   // Left portrait (MC)
        [SerializeField] private Image npcPortraitImage;  // Right portrait (NPC)

        [Header("Dialogue Content UI")]
        [SerializeField] private Text speakerNameText;
        [SerializeField] private Text dialogueBodyText;
        [SerializeField] private GameObject typingIndicator;
        [SerializeField] private Button closeButton;

        [Header("Input Component")]
        [SerializeField] private DialogueInputController inputController;

        public bool IsDialogueActive => dialoguePanelRoot != null && dialoguePanelRoot.activeSelf;

        private NPCProfileSO _currentProfile;
        private Action _onCloseCallback;
        private Coroutine _typewriterCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (dialoguePanelRoot != null)
            {
                dialoguePanelRoot.SetActive(false);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseDialogue);
            }
        }

        public void OpenDialogue(NPCProfileSO profile, Action onCloseCallback = null)
        {
            _currentProfile = profile;
            _onCloseCallback = onCloseCallback;

            if (dialoguePanelRoot != null)
            {
                dialoguePanelRoot.SetActive(true);
            }

            // Update portraits
            if (mcPortraitImage != null && profile.playerMcPortraitSprite != null)
            {
                mcPortraitImage.sprite = profile.playerMcPortraitSprite;
                mcPortraitImage.gameObject.SetActive(true);
            }

            if (npcPortraitImage != null && profile.npcPortraitSprite != null)
            {
                npcPortraitImage.sprite = profile.npcPortraitSprite;
                npcPortraitImage.gameObject.SetActive(true);
            }

            if (speakerNameText != null)
            {
                speakerNameText.text = profile.characterName;
            }

            if (dialogueBodyText != null)
            {
                dialogueBodyText.text = $"Greetings. I am {profile.characterName}. How can I assist you?";
            }

            if (inputController != null)
            {
                inputController.Initialize(OnPlayerSubmittedInput);
            }
        }

        public void OnPlayerSubmittedInput(string playerInput)
        {
            if (_currentProfile == null) return;

            // Log player message into memory
            if (CharacterMemoryManager.Instance != null)
            {
                CharacterMemoryManager.Instance.AddMessage(_currentProfile.characterId, "player", playerInput);
            }

            if (typingIndicator != null) typingIndicator.SetActive(true);
            if (dialogueBodyText != null) dialogueBodyText.text = "...";

            // Request AI response from Gemini
            if (GeminiService.Instance != null)
            {
                GeminiService.Instance.GenerateResponse(_currentProfile, playerInput, (responseReply) =>
                {
                    if (typingIndicator != null) typingIndicator.SetActive(false);
                    DisplayNpcResponse(responseReply);
                }, (error) =>
                {
                    if (typingIndicator != null) typingIndicator.SetActive(false);
                    DisplayNpcResponse($"[Error: Unable to generate response for {_currentProfile.characterName}]");
                });
            }
            else
            {
                if (typingIndicator != null) typingIndicator.SetActive(false);
                DisplayNpcResponse("I am listening, traveler.");
            }
        }

        private void DisplayNpcResponse(string npcResponse)
        {
            // Log model message into memory
            if (CharacterMemoryManager.Instance != null && _currentProfile != null)
            {
                CharacterMemoryManager.Instance.AddMessage(_currentProfile.characterId, "model", npcResponse);
            }

            // Start typing effect
            if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = StartCoroutine(TypewriterEffectCoroutine(npcResponse));

            // Synthesize voice via ElevenLabs
            if (ElevenLabsService.Instance != null && _currentProfile != null)
            {
                ElevenLabsService.Instance.SpeakText(npcResponse, _currentProfile.elevenLabsVoiceId);
            }
        }

        private IEnumerator TypewriterEffectCoroutine(string text)
        {
            if (dialogueBodyText == null) yield break;

            dialogueBodyText.text = "";
            foreach (char c in text)
            {
                dialogueBodyText.text += c;
                yield return new WaitForSeconds(0.025f);
            }
        }

        public void CloseDialogue()
        {
            if (dialoguePanelRoot != null)
            {
                dialoguePanelRoot.SetActive(false);
            }

            _onCloseCallback?.Invoke();
            _onCloseCallback = null;
        }
    }
}
