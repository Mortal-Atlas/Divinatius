using System;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using Divinatius.NPC;
using Divinatius.AI;
using Divinatius.VFX;
using Divinatius.Buffs;

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
        private NPCInteractable _currentInteractable;
        private Action _onCloseCallback;
        private Coroutine _typewriterCoroutine;
        private List<string> _recent5MessagesHistory = new List<string>();

        private void Ensure5MessageHistoryUI()
        {
            if (_recent5MessagesHistory == null)
            {
                _recent5MessagesHistory = new List<string>();
            }
        }

        private void AddMessageToRecent5History(string speaker, string text)
        {
            Ensure5MessageHistoryUI();
            if (string.IsNullOrEmpty(text)) return;
            string entry = $"<b>{speaker}:</b> {text}";
            _recent5MessagesHistory.Add(entry);
            if (_recent5MessagesHistory.Count > 5)
            {
                _recent5MessagesHistory.RemoveAt(0);
            }
        }

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

            EnsureEventSystem();
            EnsureResponsiveCanvasScaling();
            EnsureVFXManager();

            if (dialoguePanelRoot != null)
            {
                dialoguePanelRoot.SetActive(false);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(CloseDialogue);
            }
        }

        private void Update()
        {
            if (!IsDialogueActive) return;

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                CloseDialogue();
            }
        }

        private void EnsureVFXManager()
        {
            if (NPCSpellVFXManager.Instance == null)
            {
                GameObject vfxObj = new GameObject("NPCSpellVFXManager");
                vfxObj.AddComponent<NPCSpellVFXManager>();
            }
            if (PlayerBuffManager.Instance == null)
            {
                GameObject buffObj = new GameObject("PlayerBuffManager");
                buffObj.AddComponent<PlayerBuffManager>();
            }
        }

        private void EnsureResponsiveCanvasScaling()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null) canvas = GetComponentInParent<Canvas>();

            if (canvas != null)
            {
                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();

                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

            if (dialoguePanelRoot != null)
            {
                RectTransform panelRect = dialoguePanelRoot.GetComponent<RectTransform>();
                if (panelRect != null)
                {
                    panelRect.anchorMin = new Vector2(0.05f, 0.02f);
                    panelRect.anchorMax = new Vector2(0.95f, 0.45f);
                    panelRect.offsetMin = Vector2.zero;
                    panelRect.offsetMax = Vector2.zero;
                }
            }

            if (dialogueBodyText != null)
            {
                dialogueBodyText.resizeTextForBestFit = true;
                dialogueBodyText.resizeTextMinSize = 14;
                dialogueBodyText.resizeTextMaxSize = 24;
            }

            if (speakerNameText != null)
            {
                speakerNameText.resizeTextForBestFit = true;
                speakerNameText.resizeTextMinSize = 16;
                speakerNameText.resizeTextMaxSize = 26;
            }
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                EventSystem es = eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<InputSystemUIInputModule>();
            }
            else
            {
                if (EventSystem.current.GetComponent<InputSystemUIInputModule>() == null)
                {
                    var legacyModule = EventSystem.current.GetComponent<StandaloneInputModule>();
                    if (legacyModule != null)
                    {
                        Destroy(legacyModule);
                    }
                    EventSystem.current.gameObject.AddComponent<InputSystemUIInputModule>();
                }
            }
        }

        private ScrollRect _dialogueScrollRect;

        private void EnsureScrollableDialogueContainer()
        {
            if (dialogueBodyText == null) return;

            // Ensure fixed font size for consistent multi-line history rendering
            dialogueBodyText.resizeTextForBestFit = false;
            dialogueBodyText.fontSize = 17;
            dialogueBodyText.supportRichText = true;
            dialogueBodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            dialogueBodyText.verticalOverflow = VerticalWrapMode.Overflow;

            RectTransform bodyRect = dialogueBodyText.rectTransform;
            if (bodyRect.parent == null) return;

            Transform scrollAreaTr = bodyRect.parent.Find("DialogueScrollArea");
            if (scrollAreaTr == null)
            {
                GameObject scrollAreaObj = new GameObject("DialogueScrollArea");
                scrollAreaObj.transform.SetParent(bodyRect.parent, false);

                RectTransform scrollAreaRect = scrollAreaObj.AddComponent<RectTransform>();
                scrollAreaRect.anchorMin = new Vector2(0.03f, 0.22f);
                scrollAreaRect.anchorMax = new Vector2(0.97f, 0.80f);
                scrollAreaRect.offsetMin = Vector2.zero;
                scrollAreaRect.offsetMax = Vector2.zero;

                scrollAreaObj.AddComponent<RectMask2D>();
                _dialogueScrollRect = scrollAreaObj.AddComponent<ScrollRect>();
                _dialogueScrollRect.horizontal = false;
                _dialogueScrollRect.vertical = true;
                _dialogueScrollRect.scrollSensitivity = 25f;

                bodyRect.SetParent(scrollAreaObj.transform, false);
                bodyRect.anchorMin = new Vector2(0f, 1f);
                bodyRect.anchorMax = new Vector2(1f, 1f);
                bodyRect.pivot = new Vector2(0.5f, 1f);
                bodyRect.offsetMin = Vector2.zero;
                bodyRect.offsetMax = Vector2.zero;

                _dialogueScrollRect.content = bodyRect;
            }
            else
            {
                _dialogueScrollRect = scrollAreaTr.GetComponent<ScrollRect>();
            }

            // Adjust body text height dynamically based on text length
            ContentSizeFitter fitter = dialogueBodyText.GetComponent<ContentSizeFitter>();
            if (fitter == null) fitter = dialogueBodyText.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        }

        private string BuildFormattedHistoryText(string currentTypingText = "")
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < _recent5MessagesHistory.Count; i++)
            {
                bool isLast = (i == _recent5MessagesHistory.Count - 1);
                string entry = _recent5MessagesHistory[i];

                if (isLast && !string.IsNullOrEmpty(currentTypingText))
                {
                    // Render latest active typing message in larger, bright white font
                    string speaker = _currentProfile != null ? _currentProfile.characterName : "NPC";
                    sb.AppendLine($"<size=18><color=#FFFFFF><b>{speaker}:</b> {currentTypingText}</color></size>");
                }
                else
                {
                    // Render older history messages in clear, slightly smaller pastel-blue font
                    sb.AppendLine($"<size=15><color=#C8D4EE>{entry}</color></size>");
                }

                if (i < _recent5MessagesHistory.Count - 1)
                {
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        private void ScrollToBottom()
        {
            if (_dialogueScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                _dialogueScrollRect.verticalNormalizedPosition = 0f;
            }
        }

        public void OpenDialogue(NPCProfileSO profile, Action onCloseCallback = null)
        {
            OpenDialogue(profile, null, onCloseCallback);
        }

        public void OpenDialogue(NPCProfileSO profile, NPCInteractable interactable, Action onCloseCallback = null)
        {
            EnsureEventSystem();
            EnsureResponsiveCanvasScaling();
            EnsureVFXManager();
            Ensure5MessageHistoryUI();
            _currentProfile = profile;
            _currentInteractable = interactable;
            _onCloseCallback = onCloseCallback;
            _recent5MessagesHistory.Clear();

            if (dialoguePanelRoot != null)
            {
                dialoguePanelRoot.SetActive(true);
            }

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

            string initialGreeting = $"Greetings. I am {profile.characterName}. How can I assist you?";
            EnsureScrollableDialogueContainer();
            AddMessageToRecent5History(profile.characterName, initialGreeting);

            if (dialogueBodyText != null)
            {
                dialogueBodyText.text = BuildFormattedHistoryText();
                ScrollToBottom();
            }

            if (inputController != null)
            {
                inputController.Initialize(OnPlayerSubmittedInput);
                inputController.FocusInputField();
            }
        }

        public void OnPlayerSubmittedInput(string playerInput)
        {
            if (_currentProfile == null) return;

            AddMessageToRecent5History("MC", playerInput);

            // Trigger Direction Waypoint Arrow if asking for locations/directions
            if (UI.WaypointNavigationArrow.Instance != null)
            {
                UI.WaypointNavigationArrow.Instance.SetTargetByKeyword(playerInput);
            }

            if (CharacterMemoryManager.Instance != null)
            {
                CharacterMemoryManager.Instance.AddMessage(_currentProfile.characterId, "player", playerInput);
            }

            // Prepare System Prompt with Spell/VFX Action Capability, Player Blessings, Curses & Cleansing
            NPCProfileSO profileToSend = Instantiate(_currentProfile);
            profileToSend.systemPersonalityPrompt += "\n\n[SPELL, VFX, BLESSINGS, CURSES & CLEANSING CAPABILITY]:\n" +
                "IMPORTANT RULE FOR BLESSINGS AND CURSES:\n" +
                "Bestowing a blessing or inflicting a curse is a RARE magical event. " +
                "Do NOT include any spell or buff tags unless the player explicitly asks for a blessing/curse/cleanse, or during rare dramatic moments (less than 10% chance). " +
                "Most of your normal dialogue responses MUST NOT include any spell action tags!\n\n" +
                "If acting during a rare event or on explicit request, you may use ONE of these action tags:\n" +
                "• [CAST: GOD_RAY] [BUFF: SAFE_TRAVELS] -> Summons a radiant God Ray and grants +40% Speed Boost (Blessing of Safe Travels)\n" +
                "• [BUFF: FORTUNE] -> Grants +50% Gold & Item Luck Payout\n" +
                "• [CAST: PURIFY] [ACTION: CLEANSE] -> Summons a glowing cyan purification light pillar and PURGES ALL CURSES from the player!\n" +
                "• [CAST: CURSE_AOE] [CURSE: SLOTH] -> Inflicts Curse of Sloth (-40% Movement Speed Penalty)\n" +
                "• [CURSE: MISFORTUNE] -> Inflicts Curse of Misfortune (-50% Gold/Payout Penalty)\n" +
                "If the player asks to be cured or cleansed of curses, use [ACTION: CLEANSE] or [CAST: PURIFY]!";

            // Check NPCPlotKnowledge for keyword matches in player input
            if (_currentInteractable != null)
            {
                var plotKnowledge = _currentInteractable.GetComponent<NPCPlotKnowledge>();
                if (plotKnowledge != null)
                {
                    string loreInfo = plotKnowledge.CheckAndGetPlotLore(playerInput, out var matchedPlot);
                    if (!string.IsNullOrEmpty(loreInfo) && matchedPlot != null)
                    {
                        Debug.Log($"[NPCPlotKnowledge] Plot Match Unlocked! Topic: '{matchedPlot.topicName}' - Info: {loreInfo}");
                        profileToSend.systemPersonalityPrompt += $"\n\n[RELEVANT PLOT LORE UNLOCKED FOR TOPIC '{matchedPlot.topicName.ToUpper()}']:\n" +
                            $"The player asked or mentioned keywords relating to '{matchedPlot.topicName}'. " +
                            $"You MUST reveal the following plot information in your response: {loreInfo}";
                    }
                }
            }

            if (typingIndicator != null) typingIndicator.SetActive(true);
            if (dialogueBodyText != null) dialogueBodyText.text = "...";

            if (GeminiService.Instance != null)
            {
                GeminiService.Instance.GenerateResponse(profileToSend, playerInput, (responseReply) =>
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
            if (CharacterMemoryManager.Instance != null && _currentProfile != null)
            {
                CharacterMemoryManager.Instance.AddMessage(_currentProfile.characterId, "model", npcResponse);
            }

            string cleanResponse = npcResponse;

            // 1. Parse Spell Action Tags like [CAST: GOD_RAY] or [CAST: PURIFY]
            Match castMatch = Regex.Match(npcResponse, @"\[CAST:\s*([A-Za-z0-9_]+)\]", RegexOptions.IgnoreCase);
            if (castMatch.Success)
            {
                string spellName = castMatch.Groups[1].Value;
                Transform speakerTransform = _currentInteractable != null ? _currentInteractable.transform : null;
                Transform playerTransform = null;
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) playerTransform = playerObj.transform;

                if (NPCSpellVFXManager.Instance != null)
                {
                    NPCSpellVFXManager.Instance.CastSpellByName(spellName, speakerTransform, playerTransform);
                }

                if (spellName.ToUpper().Contains("PURIFY") || spellName.ToUpper().Contains("CLEANSE"))
                {
                    if (PlayerBuffManager.Instance != null)
                    {
                        PlayerBuffManager.Instance.CleanseAllCurses();
                    }
                }

                cleanResponse = Regex.Replace(cleanResponse, @"\[CAST:\s*([A-Za-z0-9_]+)\]", "").Trim();
            }

            // 2. Parse Buff Action Tags like [BUFF: SAFE_TRAVELS]
            Match buffMatch = Regex.Match(npcResponse, @"\[BUFF:\s*([A-Za-z0-9_]+)\]", RegexOptions.IgnoreCase);
            if (buffMatch.Success)
            {
                string buffName = buffMatch.Groups[1].Value;
                if (PlayerBuffManager.Instance != null)
                {
                    PlayerBuffManager.Instance.ApplyBuffByName(buffName);
                }
                cleanResponse = Regex.Replace(cleanResponse, @"\[BUFF:\s*([A-Za-z0-9_]+)\]", "").Trim();
            }

            // 3. Parse Curse Action Tags like [CURSE: SLOTH]
            Match curseMatch = Regex.Match(npcResponse, @"\[CURSE:\s*([A-Za-z0-9_]+)\]", RegexOptions.IgnoreCase);
            if (curseMatch.Success)
            {
                string curseName = curseMatch.Groups[1].Value;
                if (PlayerBuffManager.Instance != null)
                {
                    PlayerBuffManager.Instance.ApplyCurseByName(curseName);
                }
                cleanResponse = Regex.Replace(cleanResponse, @"\[CURSE:\s*([A-Za-z0-9_]+)\]", "").Trim();
            }

            // 4. Parse Cleanse Action Tags like [ACTION: CLEANSE]
            if (npcResponse.ToUpper().Contains("[ACTION: CLEANSE]") || npcResponse.ToUpper().Contains("[CLEANSE]"))
            {
                if (PlayerBuffManager.Instance != null)
                {
                    PlayerBuffManager.Instance.CleanseAllCurses();
                }
                cleanResponse = Regex.Replace(cleanResponse, @"\[ACTION:\s*CLEANSE\]", "", RegexOptions.IgnoreCase);
                cleanResponse = Regex.Replace(cleanResponse, @"\[CLEANSE\]", "", RegexOptions.IgnoreCase).Trim();
            }

            AddMessageToRecent5History(_currentProfile != null ? _currentProfile.characterName : "NPC", cleanResponse);

            if (UI.WaypointNavigationArrow.Instance != null)
            {
                UI.WaypointNavigationArrow.Instance.SetTargetByKeyword(cleanResponse);
            }

            if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = StartCoroutine(TypewriterEffectCoroutine(cleanResponse));

            Transform speaker = _currentInteractable != null ? _currentInteractable.transform : null;

            if (ElevenLabsService.Instance != null && _currentProfile != null)
            {
                ElevenLabsService.Instance.SpeakText(cleanResponse, _currentProfile.elevenLabsVoiceId, speaker);
            }
        }

        private IEnumerator TypewriterEffectCoroutine(string text)
        {
            if (dialogueBodyText == null) yield break;

            System.Text.StringBuilder currentResponseBuilder = new System.Text.StringBuilder();
            foreach (char c in text)
            {
                currentResponseBuilder.Append(c);
                dialogueBodyText.text = BuildFormattedHistoryText(currentResponseBuilder.ToString());
                ScrollToBottom();
                yield return new WaitForSeconds(0.025f);
            }

            dialogueBodyText.text = BuildFormattedHistoryText();
            ScrollToBottom();
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
