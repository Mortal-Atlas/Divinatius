using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Divinatius.NPC;
using Divinatius.AI;
using Divinatius.Save;

namespace Divinatius.UI
{
    public class PhoneAppsUI : MonoBehaviour
    {
        public enum AppType
        {
            None,
            Map,
            SocialLinks,
            Quests,
            Inventory,
            Messages,
            DialogueRecall,
            Settings
        }

        private static Font _uiFont;
        public static Font UIFont
        {
            get
            {
                if (_uiFont == null)
                {
                    _uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }
                return _uiFont;
            }
        }

        // Active Messaging State
        private static string _activeContactId = "npc_01_celeste";

        public static void BuildAppView(AppType type, GameObject container, Action onBackToHome)
        {
            // Clear existing children
            foreach (Transform child in container.transform)
            {
                Destroy(child.gameObject);
            }

            // Header Bar (App Title + Back Button)
            BuildHeader(container, GetAppTitle(type), onBackToHome);

            // App Content Container
            GameObject contentObj = new GameObject("AppContent", typeof(RectTransform));
            contentObj.transform.SetParent(container.transform, false);
            RectTransform contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.02f, 0.02f);
            contentRect.anchorMax = new Vector2(0.98f, 0.88f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            switch (type)
            {
                case AppType.Map:
                    BuildMapView(contentObj);
                    break;
                case AppType.SocialLinks:
                    BuildSocialLinksView(contentObj);
                    break;
                case AppType.Quests:
                    BuildQuestsView(contentObj);
                    break;
                case AppType.Inventory:
                    BuildInventoryView(contentObj);
                    break;
                case AppType.Messages:
                    BuildMessagesView(contentObj);
                    break;
                case AppType.DialogueRecall:
                    BuildDialogueRecallView(contentObj);
                    break;
                case AppType.Settings:
                    BuildSettingsView(contentObj);
                    break;
            }
        }

        private static string GetAppTitle(AppType type)
        {
            switch (type)
            {
                case AppType.Map: return "🗺️ World Map";
                case AppType.SocialLinks: return "❤️ Social Links";
                case AppType.Quests: return "📜 Quest Journal";
                case AppType.Inventory: return "🎒 Inventory";
                case AppType.Messages: return "💬 Messages";
                case AppType.DialogueRecall: return "📖 Dialogue Recall";
                case AppType.Settings: return "⚙️ Settings";
                default: return "App";
            }
        }

        private static void BuildHeader(GameObject parent, string titleText, Action onBack)
        {
            GameObject headerObj = new GameObject("HeaderBar", typeof(RectTransform), typeof(Image));
            headerObj.transform.SetParent(parent.transform, false);
            RectTransform headerRect = headerObj.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 0.90f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;
            headerObj.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.22f, 0.95f);

            // Back Button
            GameObject backBtnObj = new GameObject("BackButton", typeof(RectTransform), typeof(Image), typeof(Button));
            backBtnObj.transform.SetParent(headerObj.transform, false);
            RectTransform backRect = backBtnObj.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0.02f, 0.15f);
            backRect.anchorMax = new Vector2(0.22f, 0.85f);
            backRect.offsetMin = Vector2.zero;
            backRect.offsetMax = Vector2.zero;
            backBtnObj.GetComponent<Image>().color = new Color(0.25f, 0.3f, 0.45f, 1f);
            backBtnObj.GetComponent<Button>().onClick.AddListener(() => onBack?.Invoke());

            GameObject backTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(Text));
            backTxtObj.transform.SetParent(backBtnObj.transform, false);
            RectTransform backTxtRect = backTxtObj.GetComponent<RectTransform>();
            backTxtRect.anchorMin = Vector2.zero;
            backTxtRect.anchorMax = Vector2.one;
            Text backTxt = backTxtObj.GetComponent<Text>();
            backTxt.font = UIFont;
            backTxt.text = "◀ Back (Esc)";
            backTxt.alignment = TextAnchor.MiddleCenter;
            backTxt.fontSize = 11;
            backTxt.color = Color.white;

            // Title Text
            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(Text));
            titleObj.transform.SetParent(headerObj.transform, false);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.25f, 0f);
            titleRect.anchorMax = new Vector2(0.98f, 1f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            Text txt = titleObj.GetComponent<Text>();
            txt.font = UIFont;
            txt.text = titleText;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 15;
            txt.fontStyle = FontStyle.Bold;
            txt.color = Color.gold;
        }

        // ==================== 1. MAP APP ====================
        private static void BuildMapView(GameObject parent)
        {
            GameObject bgObj = new GameObject("MapBg", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(parent.transform, false);
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgObj.GetComponent<Image>().color = new Color(0.08f, 0.15f, 0.12f, 0.95f);

            // Map Header Info
            GameObject infoObj = new GameObject("MapInfo", typeof(RectTransform), typeof(Text));
            infoObj.transform.SetParent(parent.transform, false);
            RectTransform infoRect = infoObj.GetComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0.05f, 0.88f);
            infoRect.anchorMax = new Vector2(0.95f, 0.98f);
            Text infoTxt = infoObj.GetComponent<Text>();
            infoTxt.font = UIFont;
            infoTxt.text = "📍 Region: Divinatius Town Square & Astral Realm";
            infoTxt.fontSize = 13;
            infoTxt.color = Color.cyan;
            infoTxt.alignment = TextAnchor.MiddleLeft;

            // Visual Grid Layout of Points of Interest
            string[] landmarks = new string[]
            {
                "🏛️ Astral Temple (High Priestess Celeste)",
                "⚔️ Town Guard Barracks (Captain Thorne)",
                "🔨 Master Forge (Blacksmith Ignatius)",
                "🧪 Shadow Alchemist Shop (Vespera)",
                "🍷 Wandering Tavern (Bard Lyra & Smuggler Zephyr)",
                "✨ Astral Observatory (Astronomer Orion)"
            };

            float top = 0.82f;
            foreach (var landmark in landmarks)
            {
                GameObject lmObj = new GameObject("Landmark", typeof(RectTransform), typeof(Image));
                lmObj.transform.SetParent(parent.transform, false);
                RectTransform lmRect = lmObj.GetComponent<RectTransform>();
                lmRect.anchorMin = new Vector2(0.05f, top - 0.10f);
                lmRect.anchorMax = new Vector2(0.95f, top);
                lmObj.GetComponent<Image>().color = new Color(0.15f, 0.22f, 0.28f, 0.85f);

                GameObject lmTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(Text));
                lmTxtObj.transform.SetParent(lmObj.transform, false);
                RectTransform lmTxtRect = lmTxtObj.GetComponent<RectTransform>();
                lmTxtRect.anchorMin = new Vector2(0.04f, 0f);
                lmTxtRect.anchorMax = new Vector2(0.96f, 1f);
                lmTxtRect.offsetMin = Vector2.zero;
                lmTxtRect.offsetMax = Vector2.zero;
                Text lmTxt = lmTxtObj.GetComponent<Text>();
                lmTxt.font = UIFont;
                lmTxt.text = landmark;
                lmTxt.fontSize = 12;
                lmTxt.color = Color.white;
                lmTxt.alignment = TextAnchor.MiddleLeft;

                top -= 0.12f;
            }
        }

        // ==================== 2. SOCIAL LINKS (RELATIONSHIPS) APP ====================
        private static void BuildSocialLinksView(GameObject parent)
        {
            var roster = NPCCharacterRoster.Instance != null ? NPCCharacterRoster.Instance.GetAllProfiles() : NPCCharacterRoster.CreateDefaultRoster();

            GameObject scrollObj = CreateScrollView(parent);
            Transform scrollContent = scrollObj.transform.Find("Viewport/Content");

            float yOffset = -10f;
            foreach (var profile in roster)
            {
                if (profile == null) continue;

                GameObject cardObj = new GameObject($"Card_{profile.characterId}", typeof(RectTransform), typeof(Image));
                cardObj.transform.SetParent(scrollContent, false);
                RectTransform cardRect = cardObj.GetComponent<RectTransform>();
                cardRect.sizeDelta = new Vector2(310f, 65f);
                cardRect.anchoredPosition = new Vector2(0f, yOffset);
                cardObj.GetComponent<Image>().color = new Color(0.14f, 0.16f, 0.25f, 0.95f);

                // Portrait Image or Icon
                GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconObj.transform.SetParent(cardObj.transform, false);
                RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.03f, 0.1f);
                iconRect.anchorMax = new Vector2(0.20f, 0.9f);
                iconRect.offsetMin = Vector2.zero;
                iconRect.offsetMax = Vector2.zero;
                Image iconImg = iconObj.GetComponent<Image>();
                if (profile.npcPortraitSprite != null)
                {
                    iconImg.sprite = profile.npcPortraitSprite;
                }
                else
                {
                    iconImg.color = profile.npcColor;
                }

                // Name & Bio
                GameObject nameObj = new GameObject("Name", typeof(RectTransform), typeof(Text));
                nameObj.transform.SetParent(cardObj.transform, false);
                RectTransform nameRect = nameObj.GetComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0.23f, 0.5f);
                nameRect.anchorMax = new Vector2(0.97f, 0.95f);
                nameRect.offsetMin = Vector2.zero;
                nameRect.offsetMax = Vector2.zero;
                Text nameTxt = nameObj.GetComponent<Text>();
                nameTxt.font = UIFont;
                nameTxt.text = $"{profile.characterName}";
                nameTxt.fontSize = 13;
                nameTxt.fontStyle = FontStyle.Bold;
                nameTxt.color = Color.yellow;

                // Relationship Rank Meter
                int relScore = 50;
                if (CharacterMemoryManager.Instance != null)
                {
                    var mem = CharacterMemoryManager.Instance.GetOrCreateMemory(profile.characterId, profile.characterName);
                    relScore = mem.relationshipScore;
                }

                GameObject rankObj = new GameObject("Rank", typeof(RectTransform), typeof(Text));
                rankObj.transform.SetParent(cardObj.transform, false);
                RectTransform rankRect = rankObj.GetComponent<RectTransform>();
                rankRect.anchorMin = new Vector2(0.23f, 0.05f);
                rankRect.anchorMax = new Vector2(0.97f, 0.5f);
                rankRect.offsetMin = Vector2.zero;
                rankRect.offsetMax = Vector2.zero;
                Text rankTxt = rankObj.GetComponent<Text>();
                rankTxt.font = UIFont;
                rankTxt.text = $"Bond: Lv. {relScore / 20} (Affection: {relScore}/100)\n{profile.characterDescription}";
                rankTxt.fontSize = 10;
                rankTxt.color = Color.white;

                yOffset -= 72f;
            }

            RectTransform contentRect = scrollContent.GetComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(0f, Mathf.Abs(yOffset) + 20f);
        }

        // ==================== 3. QUESTS APP ====================
        private static void BuildQuestsView(GameObject parent)
        {
            GameObject scrollObj = CreateScrollView(parent);
            Transform scrollContent = scrollObj.transform.Find("Viewport/Content");

            (string title, string status, string desc, string reward)[] questData = new[]
            {
                ("📜 Main Quest: Speak with High Priestess Celeste", "ACTIVE", "Seek wisdom regarding the mysterious curse spreading through Divinatius.", "Reward: +500 XP, Astral Ring"),
                ("🔨 Side Quest: Blacksmith's Favor", "ACTIVE", "Bring Ignatius 3 Iron Ores from the valley mines.", "Reward: +200 Gold, Steel Sword"),
                ("🧪 Side Quest: Forbidden Arcana", "ACTIVE", "Ask Vespera about ancient cleansing rituals.", "Reward: Purify Blessing"),
                ("🍷 Side Quest: Tales of the Bard", "COMPLETED", "Listen to Lyra's song at the town fountain.", "Reward: +100 XP"),
                ("🛡️ Side Quest: Patrol Duty", "COMPLETED", "Report to Captain Thorne near the gate.", "Reward: Town Honor Badge")
            };

            float yOffset = -10f;
            foreach (var q in questData)
            {
                GameObject cardObj = new GameObject($"QuestCard", typeof(RectTransform), typeof(Image));
                cardObj.transform.SetParent(scrollContent, false);
                RectTransform cardRect = cardObj.GetComponent<RectTransform>();
                cardRect.sizeDelta = new Vector2(310f, 75f);
                cardRect.anchoredPosition = new Vector2(0f, yOffset);
                cardObj.GetComponent<Image>().color = q.status == "ACTIVE" ? new Color(0.12f, 0.22f, 0.18f, 0.95f) : new Color(0.18f, 0.18f, 0.22f, 0.8f);

                GameObject qTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(Text));
                qTxtObj.transform.SetParent(cardObj.transform, false);
                RectTransform qTxtRect = qTxtObj.GetComponent<RectTransform>();
                qTxtRect.anchorMin = new Vector2(0.04f, 0.04f);
                qTxtRect.anchorMax = new Vector2(0.96f, 0.96f);
                qTxtRect.offsetMin = Vector2.zero;
                qTxtRect.offsetMax = Vector2.zero;
                Text qTxt = qTxtObj.GetComponent<Text>();
                qTxt.font = UIFont;
                string statusBadge = q.status == "ACTIVE" ? "<color=lime>[ACTIVE]</color>" : "<color=grey>[COMPLETED]</color>";
                qTxt.text = $"{q.title} {statusBadge}\n<color=white>{q.desc}</color>\n<color=gold>{q.reward}</color>";
                qTxt.fontSize = 11;

                yOffset -= 82f;
            }

            RectTransform contentRect = scrollContent.GetComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(0f, Mathf.Abs(yOffset) + 20f);
        }

        // ==================== 4. INVENTORY APP ====================
        private static void BuildInventoryView(GameObject parent)
        {
            (string name, string icon, int qty, string desc)[] items = new[]
            {
                ("Astral Potion", "🧪", 5, "Restores health & purges minor ailments."),
                ("Iron Sword", "⚔️", 1, "Sturdy weapon crafted by Ignatius."),
                ("Star Shard", "✨", 3, "Radiant crystal glowing with celestial light."),
                ("Gold Coins", "🪙", 450, "Standard currency of Divinatius."),
                ("Town Honor Badge", "🛡️", 1, "Proof of service to Captain Thorne."),
                ("Ancient Scroll", "📜", 2, "Contains unread lore of forgotten gods.")
            };

            GameObject scrollObj = CreateScrollView(parent);
            Transform scrollContent = scrollObj.transform.Find("Viewport/Content");

            float yOffset = -10f;
            foreach (var item in items)
            {
                GameObject cardObj = new GameObject($"ItemCard", typeof(RectTransform), typeof(Image));
                cardObj.transform.SetParent(scrollContent, false);
                RectTransform cardRect = cardObj.GetComponent<RectTransform>();
                cardRect.sizeDelta = new Vector2(310f, 55f);
                cardRect.anchoredPosition = new Vector2(0f, yOffset);
                cardObj.GetComponent<Image>().color = new Color(0.18f, 0.16f, 0.28f, 0.95f);

                GameObject iTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(Text));
                iTxtObj.transform.SetParent(cardObj.transform, false);
                RectTransform iTxtRect = iTxtObj.GetComponent<RectTransform>();
                iTxtRect.anchorMin = new Vector2(0.04f, 0.04f);
                iTxtRect.anchorMax = new Vector2(0.96f, 0.96f);
                iTxtRect.offsetMin = Vector2.zero;
                iTxtRect.offsetMax = Vector2.zero;
                Text iTxt = iTxtObj.GetComponent<Text>();
                iTxt.font = UIFont;
                iTxt.text = $"{item.icon} <b>{item.name}</b> (x{item.qty})\n<color=cyan>{item.desc}</color>";
                iTxt.fontSize = 11;

                yOffset -= 62f;
            }

            RectTransform contentRect = scrollContent.GetComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(0f, Mathf.Abs(yOffset) + 20f);
        }

        // ==================== 5. MESSAGES / TEXTING APP (REMOTE AI COMMUNICATION) ====================
        private static void BuildMessagesView(GameObject parent)
        {
            var roster = NPCCharacterRoster.Instance != null ? NPCCharacterRoster.Instance.GetAllProfiles() : NPCCharacterRoster.CreateDefaultRoster();

            // Contact Selector Dropdown / Row at Top
            GameObject contactRowObj = new GameObject("ContactRow", typeof(RectTransform), typeof(Image));
            contactRowObj.transform.SetParent(parent.transform, false);
            RectTransform contactRect = contactRowObj.GetComponent<RectTransform>();
            contactRect.anchorMin = new Vector2(0f, 0.84f);
            contactRect.anchorMax = new Vector2(1f, 1f);
            contactRect.offsetMin = Vector2.zero;
            contactRect.offsetMax = Vector2.zero;
            contactRowObj.GetComponent<Image>().color = new Color(0.15f, 0.18f, 0.28f, 0.95f);

            GameObject contactLabelObj = new GameObject("Label", typeof(RectTransform), typeof(Text));
            contactLabelObj.transform.SetParent(contactRowObj.transform, false);
            RectTransform labelRect = contactLabelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.03f, 0f);
            labelRect.anchorMax = new Vector2(0.35f, 1f);
            Text labelTxt = contactLabelObj.GetComponent<Text>();
            labelTxt.font = UIFont;
            labelTxt.text = "💬 Contact:";
            labelTxt.fontSize = 11;
            labelTxt.color = Color.yellow;
            labelTxt.alignment = TextAnchor.MiddleLeft;

            // Scrollable Contact Buttons horizontally
            GameObject contactListObj = new GameObject("ContactList", typeof(RectTransform));
            contactListObj.transform.SetParent(contactRowObj.transform, false);
            RectTransform listRect = contactListObj.GetComponent<RectTransform>();
            listRect.anchorMin = new Vector2(0.32f, 0.1f);
            listRect.anchorMax = new Vector2(0.98f, 0.9f);

            float xOffset = 0f;
            foreach (var p in roster)
            {
                if (p == null) continue;
                string charId = p.characterId;
                string charName = p.characterName;

                GameObject cBtnObj = new GameObject($"CBtn_{charId}", typeof(RectTransform), typeof(Image), typeof(Button));
                cBtnObj.transform.SetParent(contactListObj.transform, false);
                RectTransform cBtnRect = cBtnObj.GetComponent<RectTransform>();
                cBtnRect.anchorMin = new Vector2(0f, 0f);
                cBtnRect.anchorMax = new Vector2(0f, 1f);
                cBtnRect.pivot = new Vector2(0f, 0.5f);
                cBtnRect.sizeDelta = new Vector2(70f, 0f);
                cBtnRect.anchoredPosition = new Vector2(xOffset, 0f);

                Color btnCol = (charId == _activeContactId) ? new Color(0.2f, 0.6f, 0.9f, 1f) : new Color(0.25f, 0.28f, 0.38f, 1f);
                cBtnObj.GetComponent<Image>().color = btnCol;

                GameObject cTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(Text));
                cTxtObj.transform.SetParent(cBtnObj.transform, false);
                RectTransform cTxtRect = cTxtObj.GetComponent<RectTransform>();
                cTxtRect.anchorMin = Vector2.zero;
                cTxtRect.anchorMax = Vector2.one;
                Text cTxt = cTxtObj.GetComponent<Text>();
                cTxt.font = UIFont;
                cTxt.text = charName.Split(' ')[0];
                cTxt.fontSize = 10;
                cTxt.alignment = TextAnchor.MiddleCenter;
                cTxt.color = Color.white;

                cBtnObj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    _activeContactId = charId;
                    BuildAppView(AppType.Messages, parent.transform.parent.gameObject, null);
                });

                xOffset += 74f;
            }

            // Chat Message Scroll View (Center)
            GameObject chatScrollObj = CreateScrollView(parent);
            RectTransform chatScrollRect = chatScrollObj.GetComponent<RectTransform>();
            chatScrollRect.anchorMin = new Vector2(0f, 0.16f);
            chatScrollRect.anchorMax = new Vector2(1f, 0.83f);
            chatScrollRect.offsetMin = Vector2.zero;
            chatScrollRect.offsetMax = Vector2.zero;

            Transform chatContent = chatScrollObj.transform.Find("Viewport/Content");

            // Populate current messages with active contact
            PopulateChatMessages(chatContent, _activeContactId);

            // Bottom Texting Input Row
            GameObject textInputRow = new GameObject("TextInputRow", typeof(RectTransform), typeof(Image));
            textInputRow.transform.SetParent(parent.transform, false);
            RectTransform inputRowRect = textInputRow.GetComponent<RectTransform>();
            inputRowRect.anchorMin = new Vector2(0f, 0f);
            inputRowRect.anchorMax = new Vector2(1f, 0.14f);
            inputRowRect.offsetMin = Vector2.zero;
            inputRowRect.offsetMax = Vector2.zero;
            textInputRow.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.22f, 0.95f);

            // Input Field
            GameObject inputFieldObj = new GameObject("InputField", typeof(RectTransform), typeof(Image), typeof(InputField));
            inputFieldObj.transform.SetParent(textInputRow.transform, false);
            RectTransform inputFieldRect = inputFieldObj.GetComponent<RectTransform>();
            inputFieldRect.anchorMin = new Vector2(0.02f, 0.15f);
            inputFieldRect.anchorMax = new Vector2(0.76f, 0.85f);
            inputFieldRect.offsetMin = Vector2.zero;
            inputFieldRect.offsetMax = Vector2.zero;
            inputFieldObj.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.08f, 1f);

            InputField inputComp = inputFieldObj.GetComponent<InputField>();

            GameObject placeholderObj = new GameObject("Placeholder", typeof(RectTransform), typeof(Text));
            placeholderObj.transform.SetParent(inputFieldObj.transform, false);
            RectTransform phRect = placeholderObj.GetComponent<RectTransform>();
            phRect.anchorMin = Vector2.zero;
            phRect.anchorMax = Vector2.one;
            phRect.offsetMin = new Vector2(8, 0);
            phRect.offsetMax = new Vector2(-8, 0);
            Text phText = placeholderObj.AddComponent<Text>();
            phText.font = UIFont;
            phText.text = "Send text message...";
            phText.fontSize = 11;
            phText.color = Color.gray;
            inputComp.placeholder = phText;

            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textObj.transform.SetParent(inputFieldObj.transform, false);
            RectTransform tRect = textObj.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = new Vector2(8, 0);
            tRect.offsetMax = new Vector2(-8, 0);
            Text tText = textObj.AddComponent<Text>();
            tText.font = UIFont;
            tText.fontSize = 11;
            tText.color = Color.white;
            inputComp.textComponent = tText;

            // Send Button
            GameObject sendBtnObj = new GameObject("SendBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            sendBtnObj.transform.SetParent(textInputRow.transform, false);
            RectTransform sendRect = sendBtnObj.GetComponent<RectTransform>();
            sendRect.anchorMin = new Vector2(0.78f, 0.15f);
            sendRect.anchorMax = new Vector2(0.98f, 0.85f);
            sendRect.offsetMin = Vector2.zero;
            sendRect.offsetMax = Vector2.zero;
            sendBtnObj.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.9f, 1f);

            GameObject sendTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(Text));
            sendTxtObj.transform.SetParent(sendBtnObj.transform, false);
            RectTransform sendTxtRect = sendTxtObj.GetComponent<RectTransform>();
            sendTxtRect.anchorMin = Vector2.zero;
            sendTxtRect.anchorMax = Vector2.one;
            Text sendTxt = sendTxtObj.GetComponent<Text>();
            sendTxt.font = UIFont;
            sendTxt.text = "Send";
            sendTxt.fontSize = 12;
            sendTxt.color = Color.white;
            sendTxt.alignment = TextAnchor.MiddleCenter;

            Action sendMsgAction = () =>
            {
                string msg = inputComp.text?.Trim();
                if (!string.IsNullOrEmpty(msg))
                {
                    inputComp.text = "";
                    SendTextMessageToNPC(_activeContactId, msg, chatContent);
                }
            };

            sendBtnObj.GetComponent<Button>().onClick.AddListener(() => sendMsgAction());
        }

        private static void PopulateChatMessages(Transform scrollContent, string characterId)
        {
            foreach (Transform child in scrollContent)
            {
                Destroy(child.gameObject);
            }

            NPCProfileSO profile = NPCCharacterRoster.Instance != null ? NPCCharacterRoster.Instance.GetProfile(characterId) : null;
            string charName = profile != null ? profile.characterName : characterId;

            List<DialogueMessage> history = new List<DialogueMessage>();
            if (CharacterMemoryManager.Instance != null)
            {
                var mem = CharacterMemoryManager.Instance.GetOrCreateMemory(characterId, charName);
                history = mem.conversationHistory;
            }

            float yOffset = -10f;
            if (history.Count == 0)
            {
                GameObject placeholderBubble = new GameObject("EmptyMsg", typeof(RectTransform), typeof(Text));
                placeholderBubble.transform.SetParent(scrollContent, false);
                RectTransform pRect = placeholderBubble.GetComponent<RectTransform>();
                pRect.sizeDelta = new Vector2(300f, 40f);
                pRect.anchoredPosition = new Vector2(0f, -20f);
                Text pTxt = placeholderBubble.GetComponent<Text>();
                pTxt.font = UIFont;
                pTxt.text = $"No previous text messages with {charName}.\nSend a message to start chatting!";
                pTxt.fontSize = 11;
                pTxt.color = Color.gray;
                pTxt.alignment = TextAnchor.MiddleCenter;
                return;
            }

            foreach (var m in history)
            {
                bool isPlayer = m.speakerRole == "player";

                GameObject bubbleObj = new GameObject("Bubble", typeof(RectTransform), typeof(Image));
                bubbleObj.transform.SetParent(scrollContent, false);
                RectTransform bRect = bubbleObj.GetComponent<RectTransform>();
                bRect.sizeDelta = new Vector2(240f, 45f);

                if (isPlayer)
                {
                    bRect.anchoredPosition = new Vector2(35f, yOffset);
                    bubbleObj.GetComponent<Image>().color = new Color(0.18f, 0.45f, 0.75f, 0.95f);
                }
                else
                {
                    bRect.anchoredPosition = new Vector2(-35f, yOffset);
                    bubbleObj.GetComponent<Image>().color = new Color(0.22f, 0.25f, 0.35f, 0.95f);
                }

                GameObject bTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(Text));
                bTxtObj.transform.SetParent(bubbleObj.transform, false);
                RectTransform bTxtRect = bTxtObj.GetComponent<RectTransform>();
                bTxtRect.anchorMin = new Vector2(0.05f, 0.05f);
                bTxtRect.anchorMax = new Vector2(0.95f, 0.95f);
                bTxtRect.offsetMin = Vector2.zero;
                bTxtRect.offsetMax = Vector2.zero;
                Text bTxt = bTxtObj.GetComponent<Text>();
                bTxt.font = UIFont;
                string sender = isPlayer ? "MC" : charName;
                bTxt.text = $"<b>{sender}:</b> {m.text}";
                bTxt.fontSize = 10;
                bTxt.color = Color.white;

                yOffset -= 52f;
            }

            RectTransform contentRect = scrollContent.GetComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(0f, Mathf.Abs(yOffset) + 20f);
        }

        private static void SendTextMessageToNPC(string characterId, string playerInput, Transform scrollContent)
        {
            if (CharacterMemoryManager.Instance != null)
            {
                CharacterMemoryManager.Instance.AddMessage(characterId, "player", playerInput);
            }

            PopulateChatMessages(scrollContent, characterId);

            NPCProfileSO profile = NPCCharacterRoster.Instance != null ? NPCCharacterRoster.Instance.GetProfile(characterId) : null;
            if (profile == null) return;

            if (GeminiService.Instance != null)
            {
                GeminiService.Instance.GenerateResponse(profile, playerInput, (npcReply) =>
                {
                    if (CharacterMemoryManager.Instance != null)
                    {
                        CharacterMemoryManager.Instance.AddMessage(characterId, "model", npcReply);
                    }
                    if (scrollContent != null)
                    {
                        PopulateChatMessages(scrollContent, characterId);
                    }
                }, (err) =>
                {
                    if (CharacterMemoryManager.Instance != null)
                    {
                        CharacterMemoryManager.Instance.AddMessage(characterId, "model", "[Text delivered. NPC is busy in temple.]");
                    }
                    if (scrollContent != null)
                    {
                        PopulateChatMessages(scrollContent, characterId);
                    }
                });
            }
        }

        // ==================== 6. DIALOGUE RECALL APP ====================
        private static void BuildDialogueRecallView(GameObject parent)
        {
            GameObject scrollObj = CreateScrollView(parent);
            Transform scrollContent = scrollObj.transform.Find("Viewport/Content");

            List<NPCMemoryData> allMemories = CharacterMemoryManager.Instance != null ? CharacterMemoryManager.Instance.GetAllMemoryData() : new List<NPCMemoryData>();

            float yOffset = -10f;
            if (allMemories.Count == 0)
            {
                GameObject emptyObj = new GameObject("EmptyTxt", typeof(RectTransform), typeof(Text));
                emptyObj.transform.SetParent(scrollContent, false);
                RectTransform eRect = emptyObj.GetComponent<RectTransform>();
                eRect.sizeDelta = new Vector2(300f, 40f);
                eRect.anchoredPosition = new Vector2(0f, -20f);
                Text eTxt = emptyObj.GetComponent<Text>();
                eTxt.font = UIFont;
                eTxt.text = "No dialogue logs saved yet.\nTalk to NPCs in-person or text them on the phone!";
                eTxt.fontSize = 11;
                eTxt.color = Color.gray;
                eTxt.alignment = TextAnchor.MiddleCenter;
                return;
            }

            foreach (var mem in allMemories)
            {
                if (mem.conversationHistory == null || mem.conversationHistory.Count == 0) continue;

                foreach (var msg in mem.conversationHistory)
                {
                    GameObject cardObj = new GameObject("RecallCard", typeof(RectTransform), typeof(Image));
                    cardObj.transform.SetParent(scrollContent, false);
                    RectTransform cardRect = cardObj.GetComponent<RectTransform>();
                    cardRect.sizeDelta = new Vector2(310f, 50f);
                    cardRect.anchoredPosition = new Vector2(0f, yOffset);
                    cardObj.GetComponent<Image>().color = msg.speakerRole == "player" ? new Color(0.15f, 0.22f, 0.35f, 0.9f) : new Color(0.25f, 0.20f, 0.30f, 0.9f);

                    GameObject rTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(Text));
                    rTxtObj.transform.SetParent(cardObj.transform, false);
                    RectTransform rTxtRect = rTxtObj.GetComponent<RectTransform>();
                    rTxtRect.anchorMin = new Vector2(0.04f, 0.04f);
                    rTxtRect.anchorMax = new Vector2(0.96f, 0.96f);
                    rTxtRect.offsetMin = Vector2.zero;
                    rTxtRect.offsetMax = Vector2.zero;
                    Text rTxt = rTxtObj.GetComponent<Text>();
                    rTxt.font = UIFont;
                    string speaker = msg.speakerRole == "player" ? "MC" : mem.characterName;
                    rTxt.text = $"<b>[{speaker}]:</b> {msg.text}";
                    rTxt.fontSize = 10;
                    rTxt.color = Color.white;

                    yOffset -= 56f;
                }
            }

            RectTransform contentRect = scrollContent.GetComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(0f, Mathf.Abs(yOffset) + 20f);
        }

        // ==================== 7. SETTINGS APP ====================
        private static void BuildSettingsView(GameObject parent)
        {
            GameObject bgObj = new GameObject("SettingsBg", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(parent.transform, false);
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgObj.GetComponent<Image>().color = new Color(0.10f, 0.12f, 0.18f, 0.95f);

            // BGM Volume Row
            CreateSettingSlider(parent, "🎵 BGM Volume", 0.75f, 0.78f);
            // SFX Volume Row
            CreateSettingSlider(parent, "🔊 SFX Volume", 0.85f, 0.62f);
            // Voice Volume Row
            CreateSettingSlider(parent, "🗣️ Voice Volume", 1.00f, 0.46f);

            // Save Game & Load Game Buttons
            GameObject saveBtnObj = new GameObject("SaveButton", typeof(RectTransform), typeof(Image), typeof(Button));
            saveBtnObj.transform.SetParent(parent.transform, false);
            RectTransform saveRect = saveBtnObj.GetComponent<RectTransform>();
            saveRect.anchorMin = new Vector2(0.08f, 0.22f);
            saveRect.anchorMax = new Vector2(0.48f, 0.35f);
            saveBtnObj.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.3f, 1f);
            saveBtnObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                Vector3 pos = playerObj != null ? playerObj.transform.position : Vector3.zero;
                Vector3 rot = playerObj != null ? playerObj.transform.eulerAngles : Vector3.zero;
                SaveSystem.SaveGame(pos, rot);
            });

            GameObject saveTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(Text));
            saveTxtObj.transform.SetParent(saveBtnObj.transform, false);
            RectTransform saveTxtRect = saveTxtObj.GetComponent<RectTransform>();
            saveTxtRect.anchorMin = Vector2.zero;
            saveTxtRect.anchorMax = Vector2.one;
            Text saveTxt = saveTxtObj.GetComponent<Text>();
            saveTxt.font = UIFont;
            saveTxt.text = "💾 Save Game";
            saveTxt.fontSize = 12;
            saveTxt.color = Color.white;
            saveTxt.alignment = TextAnchor.MiddleCenter;

            GameObject loadBtnObj = new GameObject("LoadButton", typeof(RectTransform), typeof(Image), typeof(Button));
            loadBtnObj.transform.SetParent(parent.transform, false);
            RectTransform loadRect = loadBtnObj.GetComponent<RectTransform>();
            loadRect.anchorMin = new Vector2(0.52f, 0.22f);
            loadRect.anchorMax = new Vector2(0.92f, 0.35f);
            loadBtnObj.GetComponent<Image>().color = new Color(0.7f, 0.4f, 0.2f, 1f);
            loadBtnObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                var data = SaveSystem.LoadGame();
                if (data != null && data.playerPosition != null)
                {
                    var playerObj = GameObject.FindGameObjectWithTag("Player");
                    if (playerObj != null)
                    {
                        playerObj.transform.position = data.playerPosition.ToVector3();
                        playerObj.transform.eulerAngles = data.playerRotation.ToVector3();
                    }
                }
            });

            GameObject loadTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(Text));
            loadTxtObj.transform.SetParent(loadBtnObj.transform, false);
            RectTransform loadTxtRect = loadTxtObj.GetComponent<RectTransform>();
            loadTxtRect.anchorMin = Vector2.zero;
            loadTxtRect.anchorMax = Vector2.one;
            Text loadTxt = loadTxtObj.GetComponent<Text>();
            loadTxt.font = UIFont;
            loadTxt.text = "📂 Load Game";
            loadTxt.fontSize = 12;
            loadTxt.color = Color.white;
            loadTxt.alignment = TextAnchor.MiddleCenter;
        }

        private static void CreateSettingSlider(GameObject parent, string labelText, float defaultVal, float topAnchor)
        {
            GameObject rowObj = new GameObject("SliderRow", typeof(RectTransform));
            rowObj.transform.SetParent(parent.transform, false);
            RectTransform rowRect = rowObj.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.05f, topAnchor - 0.12f);
            rowRect.anchorMax = new Vector2(0.95f, topAnchor);

            GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelObj.transform.SetParent(rowObj.transform, false);
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            Text txt = labelObj.GetComponent<Text>();
            txt.font = UIFont;
            txt.text = labelText;
            txt.fontSize = 12;
            txt.color = Color.white;

            GameObject sliderObj = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            sliderObj.transform.SetParent(rowObj.transform, false);
            RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0f, 0f);
            sliderRect.anchorMax = new Vector2(1f, 0.45f);

            Slider slider = sliderObj.GetComponent<Slider>();
            slider.value = defaultVal;
        }

        // Helper UI method to generate custom scroll views dynamically
        private static GameObject CreateScrollView(GameObject parent)
        {
            GameObject scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
            scrollObj.transform.SetParent(parent.transform, false);
            RectTransform scrollRectTransform = scrollObj.GetComponent<RectTransform>();
            scrollRectTransform.anchorMin = Vector2.zero;
            scrollRectTransform.anchorMax = Vector2.one;
            scrollRectTransform.offsetMin = Vector2.zero;
            scrollRectTransform.offsetMax = Vector2.zero;

            ScrollRect scrollRect = scrollObj.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            GameObject viewportObj = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
            viewportObj.transform.SetParent(scrollObj.transform, false);
            RectTransform viewportRect = viewportObj.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewportObj.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.05f);

            GameObject contentObj = new GameObject("Content", typeof(RectTransform));
            contentObj.transform.SetParent(viewportObj.transform, false);
            RectTransform contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = new Vector2(0f, 300f);

            scrollRect.content = contentRect;
            scrollRect.viewport = viewportRect;

            return scrollObj;
        }
    }
}
