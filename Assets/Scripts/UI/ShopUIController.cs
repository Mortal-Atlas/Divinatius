using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Divinatius.Economy;
using Divinatius.Player;

namespace Divinatius.UI
{
    public class ShopUIController : MonoBehaviour
    {
        public static ShopUIController Instance { get; private set; }

        private GameObject _canvasRoot;
        private GameObject _shopWindowObj;
        private Transform _scrollContent;
        private Text _merchantTitleText;
        private bool _isShopOpen = false;

        public bool IsShopOpen => _isShopOpen;

        private readonly List<ShopItemData> _catalog = new List<ShopItemData>
        {
            new ShopItemData { id = "item_01", name = "Astral Health Potion", category = "Potion", buyPrice = 50, sellPrice = 25, icon = "🧪", description = "Restores 50 HP & cleanses ailments." },
            new ShopItemData { id = "item_02", name = "Mana Elixir", category = "Potion", buyPrice = 60, sellPrice = 30, icon = "💧", description = "Restores 60 MP instantly." },
            new ShopItemData { id = "item_03", name = "Iron Broadsword", category = "Weapon", buyPrice = 250, sellPrice = 125, icon = "⚔️", description = "+15 Physical Melee Damage." },
            new ShopItemData { id = "item_04", name = "Silver Spellblade", category = "Weapon", buyPrice = 500, sellPrice = 250, icon = "🗡️", description = "+30 Physical & Magic Damage." },
            new ShopItemData { id = "item_05", name = "Scroll of Fireball", category = "SpellScroll", buyPrice = 200, sellPrice = 100, icon = "🔥", description = "Unlocks & empowers Fireball spell." },
            new ShopItemData { id = "item_06", name = "Scroll of Water Laser", category = "SpellScroll", buyPrice = 350, sellPrice = 175, icon = "🌊", description = "Unlocks Hydro beam continuous spell." },
            new ShopItemData { id = "item_07", name = "Star Crystal Ore", category = "Loot", buyPrice = 150, sellPrice = 75, icon = "✨", description = "Rare crafting mineral." }
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            if (Instance == null)
            {
                GameObject obj = new GameObject("ShopUIController");
                obj.AddComponent<ShopUIController>();
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

            BuildShopUI();
        }

        private void Update()
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame && _isShopOpen)
            {
                CloseShop();
            }
        }

        public void OpenShop(string merchantName = "Merchant Shop")
        {
            _isShopOpen = true;
            if (_merchantTitleText != null) _merchantTitleText.text = $"🛒 {merchantName}";
            if (_shopWindowObj != null) _shopWindowObj.SetActive(true);

            PopulateCatalogItems();
            UpdatePlayerControls(false);
        }

        public void CloseShop()
        {
            _isShopOpen = false;
            if (_shopWindowObj != null) _shopWindowObj.SetActive(false);
            UpdatePlayerControls(true);
        }

        private void UpdatePlayerControls(bool enable)
        {
#if UNITY_2023_1_OR_NEWER
            PlayerController pc = FindFirstObjectByType<PlayerController>();
#else
            PlayerController pc = FindObjectOfType<PlayerController>();
#endif
            if (pc != null) pc.ControlsEnabled = enable;
        }

        private void BuildShopUI()
        {
            _canvasRoot = new GameObject("ShopCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = _canvasRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 950;

            CanvasScaler scaler = _canvasRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            _shopWindowObj = new GameObject("ShopWindow", typeof(RectTransform), typeof(Image));
            _shopWindowObj.transform.SetParent(_canvasRoot.transform, false);
            RectTransform winRect = _shopWindowObj.GetComponent<RectTransform>();
            winRect.anchorMin = new Vector2(0.5f, 0.5f);
            winRect.anchorMax = new Vector2(0.5f, 0.5f);
            winRect.pivot = new Vector2(0.5f, 0.5f);
            winRect.sizeDelta = new Vector2(620f, 480f);
            _shopWindowObj.GetComponent<Image>().color = new Color(0.10f, 0.12f, 0.18f, 0.96f);

            // Title Header
            GameObject headerObj = new GameObject("HeaderBar", typeof(RectTransform), typeof(Image));
            headerObj.transform.SetParent(_shopWindowObj.transform, false);
            RectTransform headRect = headerObj.GetComponent<RectTransform>();
            headRect.anchorMin = new Vector2(0f, 0.90f);
            headRect.anchorMax = new Vector2(1f, 1f);
            headerObj.GetComponent<Image>().color = new Color(0.16f, 0.18f, 0.28f, 1f);

            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(Text));
            titleObj.transform.SetParent(headerObj.transform, false);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            _merchantTitleText = titleObj.GetComponent<Text>();
            _merchantTitleText.font = PhoneAppsUI.UIFont;
            _merchantTitleText.text = "🛒 Divinatius Merchant Shop";
            _merchantTitleText.fontSize = 16;
            _merchantTitleText.fontStyle = FontStyle.Bold;
            _merchantTitleText.color = Color.gold;
            _merchantTitleText.alignment = TextAnchor.MiddleCenter;

            // Close Button
            GameObject closeBtnObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtnObj.transform.SetParent(headerObj.transform, false);
            RectTransform closeRect = closeBtnObj.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.90f, 0.15f);
            closeRect.anchorMax = new Vector2(0.98f, 0.85f);
            closeBtnObj.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f);
            closeBtnObj.GetComponent<Button>().onClick.AddListener(() => CloseShop());

            GameObject cTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(Text));
            cTxtObj.transform.SetParent(closeBtnObj.transform, false);
            RectTransform cTxtRect = cTxtObj.GetComponent<RectTransform>();
            cTxtRect.anchorMin = Vector2.zero;
            cTxtRect.anchorMax = Vector2.one;
            Text cTxt = cTxtObj.GetComponent<Text>();
            cTxt.font = PhoneAppsUI.UIFont;
            cTxt.text = "X";
            cTxt.fontSize = 14;
            cTxt.color = Color.white;
            cTxt.alignment = TextAnchor.MiddleCenter;

            // Scroll View Container
            GameObject scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
            scrollObj.transform.SetParent(_shopWindowObj.transform, false);
            RectTransform scrollRectTransform = scrollObj.GetComponent<RectTransform>();
            scrollRectTransform.anchorMin = new Vector2(0.03f, 0.03f);
            scrollRectTransform.anchorMax = new Vector2(0.97f, 0.88f);

            ScrollRect scrollRect = scrollObj.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            GameObject viewportObj = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
            viewportObj.transform.SetParent(scrollObj.transform, false);
            RectTransform viewportRect = viewportObj.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportObj.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.05f);

            GameObject contentObj = new GameObject("Content", typeof(RectTransform));
            contentObj.transform.SetParent(viewportObj.transform, false);
            _scrollContent = contentObj.GetComponent<RectTransform>();
            ((RectTransform)_scrollContent).anchorMin = new Vector2(0f, 1f);
            ((RectTransform)_scrollContent).anchorMax = new Vector2(1f, 1f);
            ((RectTransform)_scrollContent).pivot = new Vector2(0.5f, 1f);

            scrollRect.content = (RectTransform)_scrollContent;
            scrollRect.viewport = viewportRect;

            _shopWindowObj.SetActive(false);
        }

        private void PopulateCatalogItems()
        {
            if (_scrollContent == null) return;
            foreach (Transform child in _scrollContent) Destroy(child.gameObject);

            float yOffset = -10f;
            foreach (var item in _catalog)
            {
                GameObject cardObj = new GameObject($"Card_{item.id}", typeof(RectTransform), typeof(Image));
                cardObj.transform.SetParent(_scrollContent, false);
                RectTransform cardRect = cardObj.GetComponent<RectTransform>();
                cardRect.sizeDelta = new Vector2(560f, 58f);
                cardRect.anchoredPosition = new Vector2(0f, yOffset);
                cardObj.GetComponent<Image>().color = new Color(0.14f, 0.16f, 0.25f, 0.95f);

                // Text Description
                GameObject txtObj = new GameObject("ItemText", typeof(RectTransform), typeof(Text));
                txtObj.transform.SetParent(cardObj.transform, false);
                RectTransform txtRect = txtObj.GetComponent<RectTransform>();
                txtRect.anchorMin = new Vector2(0.03f, 0.05f);
                txtRect.anchorMax = new Vector2(0.68f, 0.95f);
                Text itemTxt = txtObj.GetComponent<Text>();
                itemTxt.font = PhoneAppsUI.UIFont;
                itemTxt.text = $"{item.icon} <b>{item.name}</b> (<color=gold>{item.buyPrice} Gold</color>)\n<color=cyan>{item.description}</color>";
                itemTxt.fontSize = 11;

                // Buy Button
                GameObject buyBtnObj = new GameObject("BuyBtn", typeof(RectTransform), typeof(Image), typeof(Button));
                buyBtnObj.transform.SetParent(cardObj.transform, false);
                RectTransform buyRect = buyBtnObj.GetComponent<RectTransform>();
                buyRect.anchorMin = new Vector2(0.70f, 0.15f);
                buyRect.anchorMax = new Vector2(0.83f, 0.85f);
                buyBtnObj.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.3f, 1f);

                int cost = item.buyPrice;
                string itemName = item.name;
                buyBtnObj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (EconomyShopManager.Instance != null)
                    {
                        if (EconomyShopManager.Instance.SpendGold(cost))
                        {
                            Debug.Log($"[ShopUIController] Purchased '{itemName}' for {cost} Gold!");
                        }
                    }
                });

                GameObject bTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(Text));
                bTxtObj.transform.SetParent(buyBtnObj.transform, false);
                RectTransform bRect = bTxtObj.GetComponent<RectTransform>();
                bRect.anchorMin = Vector2.zero;
                bRect.anchorMax = Vector2.one;
                Text bTxt = bTxtObj.GetComponent<Text>();
                bTxt.font = PhoneAppsUI.UIFont;
                bTxt.text = "Buy";
                bTxt.fontSize = 11;
                bTxt.color = Color.white;
                bTxt.alignment = TextAnchor.MiddleCenter;

                // Sell Button
                GameObject sellBtnObj = new GameObject("SellBtn", typeof(RectTransform), typeof(Image), typeof(Button));
                sellBtnObj.transform.SetParent(cardObj.transform, false);
                RectTransform sellRect = sellBtnObj.GetComponent<RectTransform>();
                sellRect.anchorMin = new Vector2(0.85f, 0.15f);
                sellRect.anchorMax = new Vector2(0.98f, 0.85f);
                sellBtnObj.GetComponent<Image>().color = new Color(0.7f, 0.4f, 0.2f, 1f);

                int sellVal = item.sellPrice;
                sellBtnObj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (EconomyShopManager.Instance != null)
                    {
                        EconomyShopManager.Instance.AddGold(sellVal);
                        Debug.Log($"[ShopUIController] Sold '{itemName}' for {sellVal} Gold!");
                    }
                });

                GameObject sTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(Text));
                sTxtObj.transform.SetParent(sellBtnObj.transform, false);
                RectTransform sRect = sTxtObj.GetComponent<RectTransform>();
                sRect.anchorMin = Vector2.zero;
                sRect.anchorMax = Vector2.one;
                Text sTxt = sTxtObj.GetComponent<Text>();
                sTxt.font = PhoneAppsUI.UIFont;
                sTxt.text = "Sell";
                sTxt.fontSize = 11;
                sTxt.color = Color.white;
                sTxt.alignment = TextAnchor.MiddleCenter;

                yOffset -= 64f;
            }

            RectTransform contentRect = (RectTransform)_scrollContent;
            contentRect.sizeDelta = new Vector2(0f, Mathf.Abs(yOffset) + 20f);
        }
    }
}
