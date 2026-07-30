using System;
using UnityEngine;
using UnityEngine.UI;
using Divinatius.UI;

namespace Divinatius.Economy
{
    [Serializable]
    public class ShopItemData
    {
        public string id;
        public string name;
        public string category; // "Weapon", "Potion", "SpellScroll", "Loot"
        public int buyPrice;
        public int sellPrice;
        public string icon;
        public string description;
    }

    public class EconomyShopManager : MonoBehaviour
    {
        public static EconomyShopManager Instance { get; private set; }

        [Header("Player Economy State")]
        [SerializeField] private int playerGold = 500;

        private GameObject _hudCanvas;
        private Text _goldText;

        public int PlayerGold => playerGold;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            if (Instance == null)
            {
                GameObject obj = new GameObject("EconomyShopManager");
                obj.AddComponent<EconomyShopManager>();
                DontDestroyOnLoad(obj);
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

            BuildGoldHUD();
        }

        private void Update()
        {
            if (_goldText != null)
            {
                _goldText.text = $"🪙 Gold: <color=gold>{playerGold}</color>";
            }
        }

        public void AddGold(int amount)
        {
            playerGold += amount;
            Debug.Log($"[EconomyShopManager] Added {amount} Gold. Total Gold: {playerGold}");
        }

        public bool SpendGold(int amount)
        {
            if (playerGold >= amount)
            {
                playerGold -= amount;
                Debug.Log($"[EconomyShopManager] Spent {amount} Gold. Remaining Gold: {playerGold}");
                return true;
            }
            return false;
        }

        private void BuildGoldHUD()
        {
            _hudCanvas = new GameObject("EconomyGoldHUDCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = _hudCanvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 899;

            CanvasScaler scaler = _hudCanvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject goldObj = new GameObject("GoldHUDPanel", typeof(RectTransform), typeof(Image));
            goldObj.transform.SetParent(_hudCanvas.transform, false);
            RectTransform goldRect = goldObj.GetComponent<RectTransform>();
            goldRect.anchorMin = new Vector2(1f, 1f);
            goldRect.anchorMax = new Vector2(1f, 1f);
            goldRect.pivot = new Vector2(1f, 1f);
            goldRect.sizeDelta = new Vector2(180f, 40f);
            goldRect.anchoredPosition = new Vector2(-290f, -22f);
            goldObj.GetComponent<Image>().color = new Color(0.1f, 0.12f, 0.18f, 0.85f);

            GameObject textObj = new GameObject("GoldText", typeof(RectTransform), typeof(Text));
            textObj.transform.SetParent(goldObj.transform, false);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            _goldText = textObj.GetComponent<Text>();
            _goldText.font = PhoneAppsUI.UIFont;
            _goldText.fontSize = 14;
            _goldText.fontStyle = FontStyle.Bold;
            _goldText.alignment = TextAnchor.MiddleCenter;
            _goldText.color = Color.white;
        }
    }
}
