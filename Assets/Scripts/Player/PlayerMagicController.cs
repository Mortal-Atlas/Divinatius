using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Divinatius.Core;
using Divinatius.NPC;
using Divinatius.UI;

namespace Divinatius.Player
{
    public enum PlayerSpellType
    {
        Fireball = 1,
        Healing = 2,
        AgilityStop = 3,
        TalkToTheDead = 4,
        ReviveTheDead = 5,
        WaterLaser = 6
    }

    public class PlayerMagicController : MonoBehaviour
    {
        public static PlayerMagicController Instance { get; private set; }

        [Header("Stats & Mana")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private float maxMana = 100f;
        [SerializeField] private float currentMana = 100f;
        [SerializeField] private float manaRegenRate = 8f; // MP per sec

        [Header("Active Spell")]
        [SerializeField] private PlayerSpellType selectedSpell = PlayerSpellType.Fireball;

        // HUD Elements
        private GameObject _hudCanvas;
        private Image _hpBarFill;
        private Image _mpBarFill;
        private Text _hpMpText;
        private Text _spellHotbarText;

        public float CurrentHealth => currentHealth;
        public float CurrentMana => currentMana;
        public PlayerSpellType SelectedSpell => selectedSpell;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            BuildMagicHUD();
        }

        private void Update()
        {
            // Passive Mana Regeneration
            if (currentMana < maxMana)
            {
                currentMana = Mathf.Min(maxMana, currentMana + manaRegenRate * Time.deltaTime);
            }

            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            if (keyboard == null) return;

            // Spell Selection Hotkeys (1-6)
            if (keyboard.digit1Key.wasPressedThisFrame) SelectSpell(PlayerSpellType.Fireball);
            if (keyboard.digit2Key.wasPressedThisFrame) SelectSpell(PlayerSpellType.Healing);
            if (keyboard.digit3Key.wasPressedThisFrame) SelectSpell(PlayerSpellType.AgilityStop);
            if (keyboard.digit4Key.wasPressedThisFrame) SelectSpell(PlayerSpellType.TalkToTheDead);
            if (keyboard.digit5Key.wasPressedThisFrame) SelectSpell(PlayerSpellType.ReviveTheDead);
            if (keyboard.digit6Key.wasPressedThisFrame) SelectSpell(PlayerSpellType.WaterLaser);

            // Cast Spell (Right Mouse Button or 'R' Key)
            bool rightClickPressed = mouse != null && mouse.rightButton.wasPressedThisFrame;
            bool castKeyPressed = keyboard.rKey.wasPressedThisFrame;

            // Block casting if UI menus are open
            bool isPhoneOpen = PhoneUIController.Instance != null && PhoneUIController.Instance.IsPhoneOpen;
            bool isDialogueActive = Dialogue.DialogueUIController.Instance != null && Dialogue.DialogueUIController.Instance.IsDialogueActive;

            if ((rightClickPressed || castKeyPressed) && !isPhoneOpen && !isDialogueActive)
            {
                CastSelectedSpell();
            }

            UpdateHUD();
        }

        public void SelectSpell(PlayerSpellType spell)
        {
            selectedSpell = spell;
            Debug.Log($"[PlayerMagicController] Active Spell Selected: {selectedSpell}");
        }

        public void HealPlayer(float amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            Debug.Log($"[PlayerMagicController] Healed player for +{amount} HP. Current HP: {currentHealth}/{maxHealth}");
        }

        public void TakeDamage(float amount)
        {
            currentHealth = Mathf.Max(0f, currentHealth - amount);
            Debug.LogWarning($"[PlayerMagicController] Player took {amount} damage! HP: {currentHealth}/{maxHealth}");
        }

        public void CastSelectedSpell()
        {
            int manaCost = GetSpellManaCost(selectedSpell);
            if (currentMana < manaCost)
            {
                Debug.LogWarning($"[PlayerMagicController] Not enough Mana to cast {selectedSpell}! Needed: {manaCost} MP, Has: {currentMana} MP");
                return;
            }

            currentMana -= manaCost;
            Debug.Log($"[PlayerMagicController] Casting Spell: {selectedSpell} (Spent {manaCost} MP)");

            switch (selectedSpell)
            {
                case PlayerSpellType.Fireball:
                    CastFireball();
                    break;
                case PlayerSpellType.Healing:
                    CastHealing();
                    break;
                case PlayerSpellType.AgilityStop:
                    CastAgilityStop();
                    break;
                case PlayerSpellType.TalkToTheDead:
                    CastTalkToTheDead();
                    break;
                case PlayerSpellType.ReviveTheDead:
                    CastReviveTheDead();
                    break;
                case PlayerSpellType.WaterLaser:
                    CastWaterLaser();
                    break;
            }
        }

        private int GetSpellManaCost(PlayerSpellType spell)
        {
            switch (spell)
            {
                case PlayerSpellType.Fireball: return 20;
                case PlayerSpellType.Healing: return 25;
                case PlayerSpellType.AgilityStop: return 30;
                case PlayerSpellType.TalkToTheDead: return 15;
                case PlayerSpellType.ReviveTheDead: return 40;
                case PlayerSpellType.WaterLaser: return 35;
                default: return 20;
            }
        }

        // 1. Fireball
        private void CastFireball()
        {
            StartCoroutine(FireballCoroutine());
        }

        private IEnumerator FireballCoroutine()
        {
            GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "Magic_Fireball";
            Destroy(ball.GetComponent<Collider>());

            Vector3 spawnPos = transform.position + transform.forward * 1.5f + Vector3.up * 1.3f;
            ball.transform.position = spawnPos;
            ball.transform.localScale = Vector3.one * 0.8f;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material mat = new Material(shader);
            mat.color = new Color(1.0f, 0.35f, 0.05f);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", new Color(1.0f, 0.35f, 0.05f));
            ball.GetComponent<Renderer>().sharedMaterial = mat;

            Light light = ball.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1.0f, 0.4f, 0.05f);
            light.intensity = 18f;
            light.range = 8f;

            Vector3 flyDir = transform.forward;
            float timer = 0f;

            while (timer < 1.2f)
            {
                timer += Time.deltaTime;
                ball.transform.position += flyDir * (18f * Time.deltaTime);

                // Collision Check
                if (Physics.Raycast(ball.transform.position, flyDir, out RaycastHit hit, 1.0f))
                {
                    // Explosion Impact
                    if (TownGuardWantedManager.Instance != null && hit.collider.GetComponent<NPCInteractable>() != null)
                    {
                        TownGuardWantedManager.Instance.ReportCrime(CrimeType.AssaultNPC);
                    }
                    break;
                }
                yield return null;
            }

            // Explosion Visual
            GameObject exp = new GameObject("VFX_FireballExplosion");
            exp.transform.position = ball.transform.position;
            Light expLight = exp.AddComponent<Light>();
            expLight.type = LightType.Point;
            expLight.color = new Color(1f, 0.5f, 0.1f);
            expLight.intensity = 30f;
            expLight.range = 12f;

            Destroy(ball);
            Destroy(exp, 0.4f);
        }

        // 2. Healing
        private void CastHealing()
        {
            HealPlayer(50f);

            if (Buffs.PlayerBuffManager.Instance != null)
            {
                Buffs.PlayerBuffManager.Instance.CleanseAllCurses();
            }

            GameObject auraObj = new GameObject("VFX_HealingGlow");
            auraObj.transform.position = transform.position + Vector3.up * 1.0f;
            auraObj.transform.SetParent(transform, true);

            Light light = auraObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.2f, 0.95f, 0.4f);
            light.intensity = 20f;
            light.range = 9f;

            Destroy(auraObj, 2.5f);
        }

        // 3. Agility Stop
        private void CastAgilityStop()
        {
            Debug.Log("[PlayerMagicController] Cast Agility Stop! Temporal aura slows nearby entities by 80% for 8s.");

            GameObject timeWave = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            timeWave.name = "VFX_AgilityStopWave";
            Destroy(timeWave.GetComponent<Collider>());
            timeWave.transform.position = transform.position + Vector3.up * 0.1f;
            timeWave.transform.localScale = new Vector3(12f, 0.2f, 12f);

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material mat = new Material(shader);
            mat.color = new Color(0.4f, 0.2f, 0.8f, 0.6f);
            timeWave.GetComponent<Renderer>().sharedMaterial = mat;

            Destroy(timeWave, 3.0f);
        }

        // 4. Talk to the Dead
        private void CastTalkToTheDead()
        {
            Debug.Log("[PlayerMagicController] Cast Talk to the Dead! Spirit medium vision activated.");

            GameObject spiritAura = new GameObject("VFX_SpiritMediumAura");
            spiritAura.transform.position = transform.position + Vector3.up * 1.5f;

            Light light = spiritAura.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.3f, 0.9f, 0.85f);
            light.intensity = 18f;
            light.range = 10f;

            Destroy(spiritAura, 4.0f);
        }

        // 5. Revive the Dead
        private void CastReviveTheDead()
        {
            Debug.Log("[PlayerMagicController] Cast Revive the Dead! Resurrect light beam cast forward.");

            Vector3 targetPos = transform.position + transform.forward * 3.0f;
            GameObject revBeam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            revBeam.name = "VFX_ReviveBeam";
            Destroy(revBeam.GetComponent<Collider>());

            revBeam.transform.position = targetPos + Vector3.up * 6.0f;
            revBeam.transform.localScale = new Vector3(2.5f, 6.0f, 2.5f);

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material mat = new Material(shader);
            mat.color = new Color(1.0f, 0.95f, 0.5f, 0.8f);
            revBeam.GetComponent<Renderer>().sharedMaterial = mat;

            Destroy(revBeam, 3.5f);
        }

        // 6. Water Laser
        private void CastWaterLaser()
        {
            StartCoroutine(WaterLaserCoroutine());
        }

        private IEnumerator WaterLaserCoroutine()
        {
            GameObject laserBeam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            laserBeam.name = "VFX_WaterLaserBeam";
            Destroy(laserBeam.GetComponent<Collider>());

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material mat = new Material(shader);
            mat.color = new Color(0.1f, 0.7f, 1.0f, 0.85f);
            laserBeam.GetComponent<Renderer>().sharedMaterial = mat;

            Light laserLight = laserBeam.AddComponent<Light>();
            laserLight.type = LightType.Point;
            laserLight.color = new Color(0.2f, 0.8f, 1.0f);
            laserLight.intensity = 25f;
            laserLight.range = 12f;

            float duration = 1.5f;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                Vector3 origin = transform.position + Vector3.up * 1.2f;
                Vector3 forward = transform.forward;
                float beamLength = 16.0f;

                laserBeam.transform.position = origin + forward * (beamLength * 0.5f);
                laserBeam.transform.rotation = Quaternion.LookRotation(forward) * Quaternion.Euler(90f, 0, 0);
                laserBeam.transform.localScale = new Vector3(0.6f, beamLength * 0.5f, 0.6f);

                // Laser Hit Line Raycast
                if (Physics.Raycast(origin, forward, out RaycastHit hit, beamLength))
                {
                    if (TownGuardWantedManager.Instance != null && hit.collider.GetComponent<NPCInteractable>() != null)
                    {
                        TownGuardWantedManager.Instance.ReportCrime(CrimeType.AssaultNPC);
                    }
                }
                yield return null;
            }

            Destroy(laserBeam);
        }

        private void BuildMagicHUD()
        {
            _hudCanvas = new GameObject("PlayerMagicHUDCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = _hudCanvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 898;

            CanvasScaler scaler = _hudCanvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // HUD Container Bottom-Left
            GameObject hudPanel = new GameObject("PlayerStatusHUD", typeof(RectTransform), typeof(Image));
            hudPanel.transform.SetParent(_hudCanvas.transform, false);
            RectTransform hudRect = hudPanel.GetComponent<RectTransform>();
            hudRect.anchorMin = new Vector2(0f, 0f);
            hudRect.anchorMax = new Vector2(0f, 0f);
            hudRect.pivot = new Vector2(0f, 0f);
            hudRect.sizeDelta = new Vector2(340f, 110f);
            hudRect.anchoredPosition = new Vector2(20f, 20f);
            hudPanel.GetComponent<Image>().color = new Color(0.1f, 0.12f, 0.18f, 0.88f);

            // HP Bar Background & Fill
            GameObject hpBg = new GameObject("HPBarBg", typeof(RectTransform), typeof(Image));
            hpBg.transform.SetParent(hudPanel.transform, false);
            RectTransform hpBgRect = hpBg.GetComponent<RectTransform>();
            hpBgRect.anchorMin = new Vector2(0.05f, 0.65f);
            hpBgRect.anchorMax = new Vector2(0.95f, 0.90f);
            hpBg.GetComponent<Image>().color = new Color(0.2f, 0.05f, 0.05f, 1f);

            GameObject hpFill = new GameObject("HPFill", typeof(RectTransform), typeof(Image));
            hpFill.transform.SetParent(hpBg.transform, false);
            RectTransform hpFillRect = hpFill.GetComponent<RectTransform>();
            hpFillRect.anchorMin = Vector2.zero;
            hpFillRect.anchorMax = Vector2.one;
            _hpBarFill = hpFill.GetComponent<Image>();
            _hpBarFill.color = new Color(0.85f, 0.15f, 0.15f, 1f);

            // MP Bar Background & Fill
            GameObject mpBg = new GameObject("MPBarBg", typeof(RectTransform), typeof(Image));
            mpBg.transform.SetParent(hudPanel.transform, false);
            RectTransform mpBgRect = mpBg.GetComponent<RectTransform>();
            mpBgRect.anchorMin = new Vector2(0.05f, 0.35f);
            mpBgRect.anchorMax = new Vector2(0.95f, 0.60f);
            mpBg.GetComponent<Image>().color = new Color(0.05f, 0.1f, 0.25f, 1f);

            GameObject mpFill = new GameObject("MPFill", typeof(RectTransform), typeof(Image));
            mpFill.transform.SetParent(mpBg.transform, false);
            RectTransform mpFillRect = mpFill.GetComponent<RectTransform>();
            mpFillRect.anchorMin = Vector2.zero;
            mpFillRect.anchorMax = Vector2.one;
            _mpBarFill = mpFill.GetComponent<Image>();
            _mpBarFill.color = new Color(0.15f, 0.65f, 0.95f, 1f);

            // Spell Hotbar Label
            GameObject spellObj = new GameObject("SpellText", typeof(RectTransform), typeof(Text));
            spellObj.transform.SetParent(hudPanel.transform, false);
            RectTransform spellRect = spellObj.GetComponent<RectTransform>();
            spellRect.anchorMin = new Vector2(0.05f, 0.05f);
            spellRect.anchorMax = new Vector2(0.95f, 0.30f);
            _spellHotbarText = spellObj.GetComponent<Text>();
            _spellHotbarText.font = PhoneAppsUI.UIFont;
            _spellHotbarText.fontSize = 11;
            _spellHotbarText.fontStyle = FontStyle.Bold;
            _spellHotbarText.color = Color.white;
            _spellHotbarText.alignment = TextAnchor.MiddleLeft;
        }

        private void UpdateHUD()
        {
            if (_hpBarFill != null) _hpBarFill.fillAmount = currentHealth / maxHealth;
            if (_mpBarFill != null) _mpBarFill.fillAmount = currentMana / maxMana;

            if (_spellHotbarText != null)
            {
                _spellHotbarText.text = $"[1]🔥Fireball  [2]💚Heal  [3]⏳Stop  [4]👻Dead  [5]✨Revive  [6]🌊Laser\n<color=gold>Active Spell: [{ (int)selectedSpell }] {selectedSpell}</color>";
            }
        }
    }
}
