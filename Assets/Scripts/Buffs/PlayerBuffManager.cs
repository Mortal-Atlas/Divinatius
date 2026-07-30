using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Divinatius.VFX;

namespace Divinatius.Buffs
{
    public enum BuffType
    {
        SafeTravels,  // Speed Boost (+40% Movement Speed)
        Fortune,      // Luck & Payout Boost (+50% Gold/Payout)
        HolyLight,    // Radiant Shield (+25% Speed & Light Aura)
        IronStrength  // Defense Boost (+30% Defense)
    }

    public enum CurseType
    {
        Sloth,       // Movement Penalty (-40% Movement Speed)
        Misfortune,  // Gold / Luck Penalty (-50% Gold/Payout)
        Shadows,     // Vulnerability (-30% Defense)
        Frailty      // Weakness & Stamina Drain (-30% Speed)
    }

    [Serializable]
    public class ActiveBuff
    {
        public BuffType type;
        public string buffName;
        public string description;
        public float durationRemaining;
        public float maxDuration;
        public float speedMultiplier = 1.0f;
        public float luckMultiplier = 1.0f;
    }

    [Serializable]
    public class ActiveCurse
    {
        public CurseType type;
        public string curseName;
        public string description;
        public float durationRemaining;
        public float maxDuration;
        public float speedMultiplier = 1.0f;
        public float luckMultiplier = 1.0f;
    }

    public class PlayerBuffManager : MonoBehaviour
    {
        public static PlayerBuffManager Instance { get; private set; }

        private List<ActiveBuff> _activeBuffs = new List<ActiveBuff>();
        private List<ActiveCurse> _activeCurses = new List<ActiveCurse>();

        private Text _hudBuffText;
        private GameObject _buffNotificationBanner;
        private Text _notificationBannerText;
        private Image _notificationBannerBg;

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

        private void Start()
        {
            CreateBuffHUDUI();
        }

        private void Update()
        {
            // Tick down active buff durations
            for (int i = _activeBuffs.Count - 1; i >= 0; i--)
            {
                _activeBuffs[i].durationRemaining -= Time.deltaTime;
                if (_activeBuffs[i].durationRemaining <= 0)
                {
                    Debug.Log($"[PlayerBuffManager] Buff Expired: {_activeBuffs[i].buffName}");
                    _activeBuffs.RemoveAt(i);
                }
            }

            // Tick down active curse durations
            for (int i = _activeCurses.Count - 1; i >= 0; i--)
            {
                _activeCurses[i].durationRemaining -= Time.deltaTime;
                if (_activeCurses[i].durationRemaining <= 0)
                {
                    Debug.Log($"[PlayerBuffManager] Curse Expired: {_activeCurses[i].curseName}");
                    _activeCurses.RemoveAt(i);
                }
            }

            UpdateHUDUI();
        }

        public void ApplyBuffByName(string name)
        {
            string clean = name.Trim().ToUpper();
            if (clean.Contains("SAFE_TRAVELS") || clean.Contains("SPEED") || clean.Contains("TRAVEL"))
            {
                ApplyBuff(BuffType.SafeTravels, 60f);
            }
            else if (clean.Contains("FORTUNE") || clean.Contains("LUCK") || clean.Contains("PAYOUT") || clean.Contains("GOLD"))
            {
                ApplyBuff(BuffType.Fortune, 60f);
            }
            else if (clean.Contains("HOLY_LIGHT") || clean.Contains("GOD_RAY") || clean.Contains("LIGHT") || clean.Contains("BLESSING"))
            {
                ApplyBuff(BuffType.HolyLight, 60f);
            }
            else if (clean.Contains("IRON") || clean.Contains("STRENGTH") || clean.Contains("FORGE") || clean.Contains("DEFENSE"))
            {
                ApplyBuff(BuffType.IronStrength, 60f);
            }
            else
            {
                ApplyBuff(BuffType.SafeTravels, 60f);
            }
        }

        public void ApplyCurseByName(string name)
        {
            string clean = name.Trim().ToUpper();
            if (clean.Contains("SLOTH") || clean.Contains("SLOW") || clean.Contains("HEAVY"))
            {
                ApplyCurse(CurseType.Sloth, 60f);
            }
            else if (clean.Contains("MISFORTUNE") || clean.Contains("UNLUCKY") || clean.Contains("POVERTY"))
            {
                ApplyCurse(CurseType.Misfortune, 60f);
            }
            else if (clean.Contains("SHADOW") || clean.Contains("DARK") || clean.Contains("VULNERABLE"))
            {
                ApplyCurse(CurseType.Shadows, 60f);
            }
            else
            {
                ApplyCurse(CurseType.Sloth, 60f);
            }
        }

        public void ApplyBuff(BuffType type, float duration = 60f)
        {
            ActiveBuff buff = _activeBuffs.Find(b => b.type == type);
            if (buff == null)
            {
                buff = new ActiveBuff { type = type, maxDuration = duration, durationRemaining = duration };
                _activeBuffs.Add(buff);
            }
            else
            {
                buff.durationRemaining = duration;
            }

            switch (type)
            {
                case BuffType.SafeTravels:
                    buff.buffName = "Blessing of Safe Travels";
                    buff.description = "+40% Movement Speed on Open Roads";
                    buff.speedMultiplier = 1.4f;
                    buff.luckMultiplier = 1.0f;
                    break;
                case BuffType.Fortune:
                    buff.buffName = "Blessing of Fortune";
                    buff.description = "+50% Gold & Item Payout Luck";
                    buff.speedMultiplier = 1.0f;
                    buff.luckMultiplier = 1.5f;
                    break;
                case BuffType.HolyLight:
                    buff.buffName = "Blessing of Holy Light";
                    buff.description = "+25% Movement Speed & Divine Shield";
                    buff.speedMultiplier = 1.25f;
                    buff.luckMultiplier = 1.25f;
                    break;
                case BuffType.IronStrength:
                    buff.buffName = "Blessing of Iron Strength";
                    buff.description = "+30% Physical Stamina & Defense";
                    buff.speedMultiplier = 1.15f;
                    buff.luckMultiplier = 1.0f;
                    break;
            }

            ShowNotificationBanner($"✨ BLESSING RECEIVED: {buff.buffName} ({buff.description})!", false);
            Debug.Log($"[PlayerBuffManager] Applied Buff: {buff.buffName} for {duration}s");
        }

        public void ApplyCurse(CurseType type, float duration = 60f)
        {
            ActiveCurse curse = _activeCurses.Find(c => c.type == type);
            if (curse == null)
            {
                curse = new ActiveCurse { type = type, maxDuration = duration, durationRemaining = duration };
                _activeCurses.Add(curse);
            }
            else
            {
                curse.durationRemaining = duration;
            }

            switch (type)
            {
                case CurseType.Sloth:
                    curse.curseName = "Curse of Sloth";
                    curse.description = "-40% Movement Speed Penalty";
                    curse.speedMultiplier = 0.60f;
                    curse.luckMultiplier = 1.0f;
                    break;
                case CurseType.Misfortune:
                    curse.curseName = "Curse of Misfortune";
                    curse.description = "-50% Gold & Luck Payout Penalty";
                    curse.speedMultiplier = 1.0f;
                    curse.luckMultiplier = 0.50f;
                    break;
                case CurseType.Shadows:
                    curse.curseName = "Curse of Shadows";
                    curse.description = "-30% Defense & Dark Vulnerability";
                    curse.speedMultiplier = 0.85f;
                    curse.luckMultiplier = 0.75f;
                    break;
                case CurseType.Frailty:
                    curse.curseName = "Curse of Frailty";
                    curse.description = "-30% Physical Weakness";
                    curse.speedMultiplier = 0.70f;
                    curse.luckMultiplier = 1.0f;
                    break;
            }

            // Trigger Curse AoE VFX
            if (NPCSpellVFXManager.Instance != null)
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                Transform pTrans = playerObj != null ? playerObj.transform : null;
                NPCSpellVFXManager.Instance.CastSpell(NPCSpellType.CurseAoE, pTrans, pTrans);
            }

            ShowNotificationBanner($"💀 CURSE INFLICTED: {curse.curseName} ({curse.description})!", true);
            Debug.Log($"[PlayerBuffManager] Applied Curse: {curse.curseName} for {duration}s");
        }

        public void CleanseAllCurses()
        {
            int count = _activeCurses.Count;
            _activeCurses.Clear();

            // Trigger Purification Aura VFX
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            Transform pTrans = playerObj != null ? playerObj.transform : null;
            if (NPCSpellVFXManager.Instance != null)
            {
                NPCSpellVFXManager.Instance.CastSpell(NPCSpellType.PurificationAura, pTrans, pTrans);
            }

            ShowNotificationBanner($"✨ PURIFIED & CLEANSED: All active curses have been removed from your soul!", false);
            Debug.Log($"[PlayerBuffManager] Cleanised {count} active curses.");
        }

        public float GetSpeedMultiplier()
        {
            float speedMult = 1.0f;

            foreach (var b in _activeBuffs)
            {
                if (b.speedMultiplier > speedMult) speedMult = b.speedMultiplier;
            }

            foreach (var c in _activeCurses)
            {
                speedMult *= c.speedMultiplier;
            }

            return Mathf.Max(0.2f, speedMult);
        }

        public float GetLuckMultiplier()
        {
            float luckMult = 1.0f;

            foreach (var b in _activeBuffs)
            {
                if (b.luckMultiplier > luckMult) luckMult = b.luckMultiplier;
            }

            foreach (var c in _activeCurses)
            {
                luckMult *= c.luckMultiplier;
            }

            return Mathf.Max(0.1f, luckMult);
        }

        private void CreateBuffHUDUI()
        {
            GameObject hudCanvas = GameObject.Find("ControlsHUDCanvas");
            if (hudCanvas == null)
            {
                Canvas c = FindFirstObjectByType<Canvas>();
                if (c != null) hudCanvas = c.gameObject;
                else return;
            }

            // Transparent HUD Text positioned directly BELOW the circular Minimap (Top Right)
            GameObject buffPanelObj = new GameObject("PlayerBuffHUDPanel");
            buffPanelObj.transform.SetParent(hudCanvas.transform, false);
            RectTransform rect = buffPanelObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-25, -215); // Directly below circular Minimap frame (-205)
            rect.sizeDelta = new Vector2(220, 250);

            // NO BACKGROUND IMAGE per request! Clean transparent HUD layer

            GameObject textObj = new GameObject("BuffHUDText");
            textObj.transform.SetParent(buffPanelObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            _hudBuffText = textObj.AddComponent<Text>();
            _hudBuffText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _hudBuffText.fontSize = 15;
            _hudBuffText.fontStyle = FontStyle.Bold;
            _hudBuffText.alignment = TextAnchor.UpperRight;
            _hudBuffText.color = Color.white;
            _hudBuffText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _hudBuffText.verticalOverflow = VerticalWrapMode.Overflow;

            // Notification Banner Popup (Top Center)
            _buffNotificationBanner = new GameObject("BuffNotificationBanner");
            _buffNotificationBanner.transform.SetParent(hudCanvas.transform, false);
            RectTransform bannerRect = _buffNotificationBanner.AddComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0.2f, 0.90f);
            bannerRect.anchorMax = new Vector2(0.8f, 0.97f);
            bannerRect.offsetMin = Vector2.zero;
            bannerRect.offsetMax = Vector2.zero;

            _notificationBannerBg = _buffNotificationBanner.AddComponent<Image>();
            _notificationBannerBg.color = new Color(0.1f, 0.45f, 0.25f, 0.92f);

            GameObject bannerTextObj = new GameObject("BannerText");
            bannerTextObj.transform.SetParent(_buffNotificationBanner.transform, false);
            RectTransform btRect = bannerTextObj.AddComponent<RectTransform>();
            btRect.anchorMin = Vector2.zero;
            btRect.anchorMax = Vector2.one;
            btRect.offsetMin = Vector2.zero;
            btRect.offsetMax = Vector2.zero;

            _notificationBannerText = bannerTextObj.AddComponent<Text>();
            _notificationBannerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _notificationBannerText.fontSize = 16;
            _notificationBannerText.fontStyle = FontStyle.Bold;
            _notificationBannerText.alignment = TextAnchor.MiddleCenter;
            _notificationBannerText.color = Color.white;

            _buffNotificationBanner.SetActive(false);
        }

        private void ShowNotificationBanner(string text, bool isCurse)
        {
            if (_buffNotificationBanner == null) CreateBuffHUDUI();

            if (_buffNotificationBanner != null && _notificationBannerText != null)
            {
                _notificationBannerText.text = text;
                if (_notificationBannerBg != null)
                {
                    _notificationBannerBg.color = isCurse ? new Color(0.6f, 0.1f, 0.15f, 0.95f) : new Color(0.1f, 0.45f, 0.25f, 0.95f);
                }

                _buffNotificationBanner.SetActive(true);
                CancelInvoke(nameof(HideNotificationBanner));
                Invoke(nameof(HideNotificationBanner), 5.0f);
            }
        }

        private void HideNotificationBanner()
        {
            if (_buffNotificationBanner != null)
            {
                _buffNotificationBanner.SetActive(false);
            }
        }

        private void UpdateHUDUI()
        {
            if (_hudBuffText == null) return;

            if (_activeBuffs.Count == 0 && _activeCurses.Count == 0)
            {
                _hudBuffText.text = "";
                return;
            }

            string txt = "";

            // Active Blessings: Icon, Short Name, Ticking Countdown Timer
            foreach (var b in _activeBuffs)
            {
                string icon = "✨";
                string shortName = b.buffName;
                switch (b.type)
                {
                    case BuffType.SafeTravels: icon = "⚡"; shortName = "Speed"; break;
                    case BuffType.Fortune: icon = "🪙"; shortName = "Fortune"; break;
                    case BuffType.HolyLight: icon = "🛡️"; shortName = "Holy Light"; break;
                    case BuffType.IronStrength: icon = "⚔️"; shortName = "Strength"; break;
                }

                txt += $"<color=#50FF70>{icon} <b>{shortName}</b>: {Mathf.CeilToInt(b.durationRemaining)}s</color>\n";
            }

            // Active Curses: Icon, Short Name, Ticking Countdown Timer
            foreach (var c in _activeCurses)
            {
                string icon = "💀";
                string shortName = c.curseName;
                switch (c.type)
                {
                    case CurseType.Sloth: icon = "💀"; shortName = "Sloth"; break;
                    case CurseType.Misfortune: icon = "💸"; shortName = "Misfortune"; break;
                    case CurseType.Shadows: icon = "👁️"; shortName = "Shadows"; break;
                    case CurseType.Frailty: icon = "🥀"; shortName = "Frailty"; break;
                }

                txt += $"<color=#FF5050>{icon} <b>{shortName}</b>: {Mathf.CeilToInt(c.durationRemaining)}s</color>\n";
            }

            _hudBuffText.text = txt;
        }
    }
}
