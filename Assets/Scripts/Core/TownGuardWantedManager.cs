using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Divinatius.UI;

namespace Divinatius.Core
{
    public enum CrimeType
    {
        AssaultNPC,
        AssaultGuard,
        Theft,
        PublicDisturbance
    }

    public class TownGuardWantedManager : MonoBehaviour
    {
        public static TownGuardWantedManager Instance { get; private set; }

        [Header("Wanted System State")]
        private int _wantedLevel = 0; // 0 to 5 Stars
        private float _heatPoints = 0f;
        private bool _isBeingPursued = false;
        private float _cooldownTimer = 0f;

        [Header("Fine Costs")]
        [SerializeField] private int fineCostPerStar = 100;

        // HUD Elements
        private GameObject _hudCanvas;
        private Text _wantedStarsText;
        private GameObject _finePopupObj;
        private Text _finePopupText;

        public int WantedLevel => _wantedLevel;
        public bool IsBeingPursued => _isBeingPursued;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            if (Instance == null)
            {
                GameObject obj = new GameObject("TownGuardWantedManager");
                obj.AddComponent<TownGuardWantedManager>();
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

            BuildHUDUI();
        }

        private void Update()
        {
            // Cooldown heat when not in active combat
            if (_wantedLevel > 0)
            {
                _cooldownTimer += Time.deltaTime;
                if (_cooldownTimer >= 20.0f)
                {
                    _cooldownTimer = 0f;
                    ReduceWantedLevel(1);
                }
            }

            UpdateHUD();
        }

        public void ReportCrime(CrimeType crime)
        {
            int starsToAdd = 1;
            switch (crime)
            {
                case CrimeType.AssaultNPC: starsToAdd = 1; break;
                case CrimeType.AssaultGuard: starsToAdd = 2; break;
                case CrimeType.Theft: starsToAdd = 1; break;
                case CrimeType.PublicDisturbance: starsToAdd = 1; break;
            }

            _wantedLevel = Mathf.Clamp(_wantedLevel + starsToAdd, 0, 5);
            _isBeingPursued = true;
            _cooldownTimer = 0f;

            Debug.LogWarning($"[TownGuardWantedManager] CRIME REPORTED: {crime}! Wanted Level increased to {_wantedLevel} Stars! Guards alerted!");

            // Alert nearby Town Guards
            AlertNearbyGuards();
        }

        public void ReduceWantedLevel(int amount)
        {
            _wantedLevel = Mathf.Clamp(_wantedLevel - amount, 0, 5);
            if (_wantedLevel == 0)
            {
                _isBeingPursued = false;
                Debug.Log("[TownGuardWantedManager] Wanted status CLEARED! Guards returned to peace.");
            }
        }

        public bool PayFine()
        {
            if (_wantedLevel <= 0) return true;

            int totalFine = _wantedLevel * fineCostPerStar;
            if (Divinatius.Economy.EconomyShopManager.Instance != null)
            {
                if (Divinatius.Economy.EconomyShopManager.Instance.SpendGold(totalFine))
                {
                    Debug.Log($"[TownGuardWantedManager] Paid fine of {totalFine} Gold. Wanted status cleared peacefully!");
                    _wantedLevel = 0;
                    _isBeingPursued = false;
                    CloseFinePopup();
                    return true;
                }
                else
                {
                    Debug.LogWarning($"[TownGuardWantedManager] Not enough gold to pay fine of {totalFine} Gold!");
                    return false;
                }
            }
            else
            {
                _wantedLevel = 0;
                _isBeingPursued = false;
                CloseFinePopup();
                return true;
            }
        }

        public void OpenFinePopup()
        {
            if (_finePopupObj != null)
            {
                int totalFine = Mathf.Max(1, _wantedLevel) * fineCostPerStar;
                if (_finePopupText != null)
                {
                    _finePopupText.text = $"⚠️ <b>SURRENDER TO TOWN GUARD</b> ⚠️\n\n" +
                        $"Wanted Level: <color=yellow>{_wantedLevel} Stars</color>\n" +
                        $"Fine Amount: <color=gold>{totalFine} Gold</color>\n\n" +
                        $"Pay fine peacefully to clear charges?";
                }
                _finePopupObj.SetActive(true);
            }
        }

        public void CloseFinePopup()
        {
            if (_finePopupObj != null)
            {
                _finePopupObj.SetActive(false);
            }
        }

        private void AlertNearbyGuards()
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null) return;

            Collider[] hits = Physics.OverlapSphere(playerObj.transform.position, 30.0f);
            foreach (var hit in hits)
            {
                if (hit.name.ToLower().Contains("guard") || hit.name.ToLower().Contains("thorne"))
                {
                    Debug.Log($"[TownGuardWantedManager] Guard '{hit.name}' responding to crime!");
                }
            }
        }

        private void BuildHUDUI()
        {
            _hudCanvas = new GameObject("TownGuardHUDCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = _hudCanvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 900;

            CanvasScaler scaler = _hudCanvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // Wanted Stars Container (Top-Right HUD)
            GameObject starsObj = new GameObject("WantedStarsContainer", typeof(RectTransform), typeof(Image));
            starsObj.transform.SetParent(_hudCanvas.transform, false);
            RectTransform starsRect = starsObj.GetComponent<RectTransform>();
            starsRect.anchorMin = new Vector2(1f, 1f);
            starsRect.anchorMax = new Vector2(1f, 1f);
            starsRect.pivot = new Vector2(1f, 1f);
            starsRect.sizeDelta = new Vector2(260f, 45f);
            starsRect.anchoredPosition = new Vector2(-20f, -20f);
            starsObj.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.85f);

            GameObject textObj = new GameObject("StarsText", typeof(RectTransform), typeof(Text));
            textObj.transform.SetParent(starsObj.transform, false);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            _wantedStarsText = textObj.GetComponent<Text>();
            _wantedStarsText.font = PhoneAppsUI.UIFont;
            _wantedStarsText.fontSize = 15;
            _wantedStarsText.fontStyle = FontStyle.Bold;
            _wantedStarsText.alignment = TextAnchor.MiddleCenter;
            _wantedStarsText.color = Color.white;

            // Surrender / Pay Fine Modal Popup
            _finePopupObj = new GameObject("SurrenderFinePopup", typeof(RectTransform), typeof(Image));
            _finePopupObj.transform.SetParent(_hudCanvas.transform, false);
            RectTransform fineRect = _finePopupObj.GetComponent<RectTransform>();
            fineRect.anchorMin = new Vector2(0.5f, 0.5f);
            fineRect.anchorMax = new Vector2(0.5f, 0.5f);
            fineRect.pivot = new Vector2(0.5f, 0.5f);
            fineRect.sizeDelta = new Vector2(420f, 240f);
            _finePopupObj.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.22f, 0.96f);

            GameObject popupTxtObj = new GameObject("PopupText", typeof(RectTransform), typeof(Text));
            popupTxtObj.transform.SetParent(_finePopupObj.transform, false);
            RectTransform popTxtRect = popupTxtObj.GetComponent<RectTransform>();
            popTxtRect.anchorMin = new Vector2(0.05f, 0.35f);
            popTxtRect.anchorMax = new Vector2(0.95f, 0.95f);
            _finePopupText = popupTxtObj.GetComponent<Text>();
            _finePopupText.font = PhoneAppsUI.UIFont;
            _finePopupText.fontSize = 13;
            _finePopupText.color = Color.white;
            _finePopupText.alignment = TextAnchor.MiddleCenter;

            // Pay Fine Button
            GameObject payBtnObj = new GameObject("PayFineBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            payBtnObj.transform.SetParent(_finePopupObj.transform, false);
            RectTransform payRect = payBtnObj.GetComponent<RectTransform>();
            payRect.anchorMin = new Vector2(0.08f, 0.08f);
            payRect.anchorMax = new Vector2(0.46f, 0.30f);
            payBtnObj.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.3f, 1f);
            payBtnObj.GetComponent<Button>().onClick.AddListener(() => PayFine());

            GameObject payTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(Text));
            payTxtObj.transform.SetParent(payBtnObj.transform, false);
            RectTransform pTxtRect = payTxtObj.GetComponent<RectTransform>();
            pTxtRect.anchorMin = Vector2.zero;
            pTxtRect.anchorMax = Vector2.one;
            Text pTxt = payTxtObj.GetComponent<Text>();
            pTxt.font = PhoneAppsUI.UIFont;
            pTxt.text = "💰 Pay Fine";
            pTxt.fontSize = 13;
            pTxt.color = Color.white;
            pTxt.alignment = TextAnchor.MiddleCenter;

            // Resist / Cancel Button
            GameObject cancelBtnObj = new GameObject("RefuseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            cancelBtnObj.transform.SetParent(_finePopupObj.transform, false);
            RectTransform cancelRect = cancelBtnObj.GetComponent<RectTransform>();
            cancelRect.anchorMin = new Vector2(0.54f, 0.08f);
            cancelRect.anchorMax = new Vector2(0.92f, 0.30f);
            cancelBtnObj.GetComponent<Image>().color = new Color(0.7f, 0.2f, 0.2f, 1f);
            cancelBtnObj.GetComponent<Button>().onClick.AddListener(() => CloseFinePopup());

            GameObject cancelTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(Text));
            cancelTxtObj.transform.SetParent(cancelBtnObj.transform, false);
            RectTransform cTxtRect = cancelTxtObj.GetComponent<RectTransform>();
            cTxtRect.anchorMin = Vector2.zero;
            cTxtRect.anchorMax = Vector2.one;
            Text cTxt = cancelTxtObj.GetComponent<Text>();
            cTxt.font = PhoneAppsUI.UIFont;
            cTxt.text = "⚔️ Resist / Flee";
            cTxt.fontSize = 13;
            cTxt.color = Color.white;
            cTxt.alignment = TextAnchor.MiddleCenter;

            _finePopupObj.SetActive(false);
        }

        private void UpdateHUD()
        {
            if (_wantedStarsText == null) return;

            if (_wantedLevel <= 0)
            {
                _wantedStarsText.text = "🛡️ Town Status: <color=lime>Peaceful</color>";
            }
            else
            {
                string stars = "";
                for (int i = 0; i < 5; i++)
                {
                    if (i < _wantedLevel) stars += "★ ";
                    else stars += "☆ ";
                }
                _wantedStarsText.text = $"⚠️ WANTED: <color=yellow>{stars.Trim()}</color>";
            }
        }
    }
}
