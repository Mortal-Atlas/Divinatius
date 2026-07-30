using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Divinatius.Player;
using Divinatius.Dialogue;

namespace Divinatius.UI
{
    public class PhoneUIController : MonoBehaviour
    {
        public static PhoneUIController Instance { get; private set; }

        private GameObject _canvasRoot;
        private RectTransform _phoneFrameRect;
        private GameObject _homeScreenObj;
        private GameObject _appContainerObj;
        private Text _timeText;

        private bool _isPhoneOpen = false;
        private PhoneAppsUI.AppType _currentApp = PhoneAppsUI.AppType.None;

        private readonly Vector2 _hiddenPos = new Vector2(450f, -850f);
        private readonly Vector2 _targetPos = new Vector2(-30f, 35f);

        public bool IsPhoneOpen => _isPhoneOpen;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            if (Instance == null)
            {
                GameObject phoneObj = new GameObject("PhoneUIController");
                phoneObj.AddComponent<PhoneUIController>();
                DontDestroyOnLoad(phoneObj);
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

            BuildPhoneUIHierarchy();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            // Q Key toggles MC's Phone
            if (keyboard.qKey.wasPressedThisFrame)
            {
                // Don't open phone if visual novel dialogue is currently active
                bool isVNActive = DialogueUIController.Instance != null && DialogueUIController.Instance.IsDialogueActive;
                if (!isVNActive)
                {
                    TogglePhone();
                }
            }

            // Esc Key Navigation
            if (keyboard.escapeKey.wasPressedThisFrame && _isPhoneOpen)
            {
                if (_currentApp != PhoneAppsUI.AppType.None)
                {
                    // Exit App and return to Phone Home Screen
                    ReturnToHomeScreen();
                }
                else
                {
                    // Exit Phone completely
                    ClosePhone();
                }
            }

            // Smooth Slide Animation to Bottom-Right Anchor
            if (_phoneFrameRect != null)
            {
                Vector2 target = _isPhoneOpen ? _targetPos : _hiddenPos;
                _phoneFrameRect.anchoredPosition = Vector2.Lerp(_phoneFrameRect.anchoredPosition, target, Time.deltaTime * 14f);
            }

            // Update Header Time of Day
            if (_timeText != null && _isPhoneOpen)
            {
                DateTime now = DateTime.Now;
                string tod = GetTimeOfDayPeriod(now.Hour);
                _timeText.text = $"{now:hh:mm tt} • {tod}";
            }
        }

        private string GetTimeOfDayPeriod(int hour)
        {
            if (hour >= 5 && hour < 12) return "Morning";
            if (hour >= 12 && hour < 17) return "Afternoon";
            if (hour >= 17 && hour < 21) return "Evening";
            return "Night";
        }

        public void TogglePhone()
        {
            if (_isPhoneOpen) ClosePhone();
            else OpenPhone();
        }

        public void OpenPhone()
        {
            _isPhoneOpen = true;
            ReturnToHomeScreen();
            UpdatePlayerControlsState(false);
        }

        public void ClosePhone()
        {
            _isPhoneOpen = false;
            _currentApp = PhoneAppsUI.AppType.None;
            UpdatePlayerControlsState(true);
        }

        public void ReturnToHomeScreen()
        {
            _currentApp = PhoneAppsUI.AppType.None;
            if (_homeScreenObj != null) _homeScreenObj.SetActive(true);
            if (_appContainerObj != null)
            {
                _appContainerObj.SetActive(false);
                foreach (Transform child in _appContainerObj.transform)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        public void OpenApp(PhoneAppsUI.AppType appType)
        {
            _currentApp = appType;
            if (_homeScreenObj != null) _homeScreenObj.SetActive(false);
            if (_appContainerObj != null)
            {
                _appContainerObj.SetActive(true);
                PhoneAppsUI.BuildAppView(appType, _appContainerObj, ReturnToHomeScreen);
            }
        }

        private void UpdatePlayerControlsState(bool enablePlayerControls)
        {
#if UNITY_2023_1_OR_NEWER
            PlayerController player = FindFirstObjectByType<PlayerController>();
#else
            PlayerController player = FindObjectOfType<PlayerController>();
#endif
            if (player != null)
            {
                player.ControlsEnabled = enablePlayerControls;
            }
            else
            {
                Cursor.lockState = enablePlayerControls ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !enablePlayerControls;
            }
        }

        private void BuildPhoneUIHierarchy()
        {
            // Canvas Root
            _canvasRoot = new GameObject("MCPhoneCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = _canvasRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

            CanvasScaler scaler = _canvasRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // Outer Phone Frame (Anchored Bottom-Right)
            GameObject frameObj = new GameObject("PhoneFrame", typeof(RectTransform), typeof(Image));
            frameObj.transform.SetParent(_canvasRoot.transform, false);
            _phoneFrameRect = frameObj.GetComponent<RectTransform>();
            _phoneFrameRect.anchorMin = new Vector2(1f, 0f);
            _phoneFrameRect.anchorMax = new Vector2(1f, 0f);
            _phoneFrameRect.pivot = new Vector2(1f, 0f);
            _phoneFrameRect.sizeDelta = new Vector2(360f, 620f);
            _phoneFrameRect.anchoredPosition = _hiddenPos;

            // Phone Body Glassmorphic Styling
            Image frameImg = frameObj.GetComponent<Image>();
            frameImg.color = new Color(0.08f, 0.09f, 0.14f, 0.96f);

            // Inner Screen Panel
            GameObject screenObj = new GameObject("ScreenPanel", typeof(RectTransform), typeof(Image));
            screenObj.transform.SetParent(frameObj.transform, false);
            RectTransform screenRect = screenObj.GetComponent<RectTransform>();
            screenRect.anchorMin = new Vector2(0.03f, 0.03f);
            screenRect.anchorMax = new Vector2(0.97f, 0.97f);
            screenRect.offsetMin = Vector2.zero;
            screenRect.offsetMax = Vector2.zero;
            screenObj.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.22f, 1f);

            // Phone Header Bar (Time of Day & Status)
            GameObject headerObj = new GameObject("StatusBar", typeof(RectTransform), typeof(Image));
            headerObj.transform.SetParent(screenObj.transform, false);
            RectTransform headerRect = headerObj.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 0.93f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;
            headerObj.GetComponent<Image>().color = new Color(0.06f, 0.07f, 0.12f, 0.9f);

            GameObject timeObj = new GameObject("TimeText", typeof(RectTransform), typeof(Text));
            timeObj.transform.SetParent(headerObj.transform, false);
            RectTransform timeRect = timeObj.GetComponent<RectTransform>();
            timeRect.anchorMin = Vector2.zero;
            timeRect.anchorMax = Vector2.one;
            timeRect.offsetMin = new Vector2(10, 0);
            timeRect.offsetMax = new Vector2(-10, 0);
            _timeText = timeObj.GetComponent<Text>();
            _timeText.font = PhoneAppsUI.UIFont;
            _timeText.fontSize = 12;
            _timeText.fontStyle = FontStyle.Bold;
            _timeText.color = Color.white;
            _timeText.alignment = TextAnchor.MiddleCenter;

            // Home Screen App Grid Panel
            _homeScreenObj = new GameObject("HomeScreenPanel", typeof(RectTransform));
            _homeScreenObj.transform.SetParent(screenObj.transform, false);
            RectTransform homeRect = _homeScreenObj.GetComponent<RectTransform>();
            homeRect.anchorMin = new Vector2(0f, 0f);
            homeRect.anchorMax = new Vector2(1f, 0.93f);
            homeRect.offsetMin = Vector2.zero;
            homeRect.offsetMax = Vector2.zero;

            BuildAppGrid(_homeScreenObj);

            // App Container Panel (For rendered sub-views)
            _appContainerObj = new GameObject("AppContainerPanel", typeof(RectTransform));
            _appContainerObj.transform.SetParent(screenObj.transform, false);
            RectTransform appRect = _appContainerObj.GetComponent<RectTransform>();
            appRect.anchorMin = new Vector2(0f, 0f);
            appRect.anchorMax = new Vector2(1f, 0.93f);
            appRect.offsetMin = Vector2.zero;
            appRect.offsetMax = Vector2.zero;
            _appContainerObj.SetActive(false);
        }

        private void BuildAppGrid(GameObject parent)
        {
            // Title Label on Home Screen
            GameObject titleObj = new GameObject("HomeTitle", typeof(RectTransform), typeof(Text));
            titleObj.transform.SetParent(parent.transform, false);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.05f, 0.88f);
            titleRect.anchorMax = new Vector2(0.95f, 0.98f);
            Text titleTxt = titleObj.GetComponent<Text>();
            titleTxt.font = PhoneAppsUI.UIFont;
            titleTxt.text = "📱 Divinatius OS";
            titleTxt.fontSize = 16;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.color = Color.cyan;
            titleTxt.alignment = TextAnchor.MiddleCenter;

            (string name, string icon, Color color, PhoneAppsUI.AppType type)[] apps = new[]
            {
                ("World Map", "🗺️", new Color(0.2f, 0.5f, 0.4f, 1f), PhoneAppsUI.AppType.Map),
                ("Social Links", "❤️", new Color(0.7f, 0.2f, 0.4f, 1f), PhoneAppsUI.AppType.SocialLinks),
                ("Quests", "📜", new Color(0.8f, 0.6f, 0.2f, 1f), PhoneAppsUI.AppType.Quests),
                ("Inventory", "🎒", new Color(0.3f, 0.4f, 0.7f, 1f), PhoneAppsUI.AppType.Inventory),
                ("Messages", "💬", new Color(0.2f, 0.6f, 0.9f, 1f), PhoneAppsUI.AppType.Messages),
                ("Recall", "📖", new Color(0.5f, 0.3f, 0.7f, 1f), PhoneAppsUI.AppType.DialogueRecall),
                ("Settings", "⚙️", new Color(0.4f, 0.4f, 0.5f, 1f), PhoneAppsUI.AppType.Settings)
            };

            // Grid Container (3 Columns x 3 Rows)
            GameObject gridObj = new GameObject("GridContainer", typeof(RectTransform), typeof(GridLayoutGroup));
            gridObj.transform.SetParent(parent.transform, false);
            RectTransform gridRect = gridObj.GetComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.05f, 0.12f);
            gridRect.anchorMax = new Vector2(0.95f, 0.85f);
            gridRect.offsetMin = Vector2.zero;
            gridRect.offsetMax = Vector2.zero;

            GridLayoutGroup grid = gridObj.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(92f, 92f);
            grid.spacing = new Vector2(15f, 20f);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;

            foreach (var app in apps)
            {
                PhoneAppsUI.AppType targetType = app.type;

                GameObject btnObj = new GameObject($"AppBtn_{app.name}", typeof(RectTransform), typeof(Image), typeof(Button));
                btnObj.transform.SetParent(gridObj.transform, false);
                btnObj.GetComponent<Image>().color = app.color;

                Button btn = btnObj.GetComponent<Button>();
                btn.onClick.AddListener(() => OpenApp(targetType));

                // Icon Text
                GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Text));
                iconObj.transform.SetParent(btnObj.transform, false);
                RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0f, 0.32f);
                iconRect.anchorMax = new Vector2(1f, 1f);
                Text iconTxt = iconObj.GetComponent<Text>();
                iconTxt.font = PhoneAppsUI.UIFont;
                iconTxt.text = app.icon;
                iconTxt.fontSize = 28;
                iconTxt.alignment = TextAnchor.MiddleCenter;

                // Name Text
                GameObject nameObj = new GameObject("Name", typeof(RectTransform), typeof(Text));
                nameObj.transform.SetParent(btnObj.transform, false);
                RectTransform nameRect = nameObj.GetComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0f, 0f);
                nameRect.anchorMax = new Vector2(1f, 0.35f);
                Text nameTxt = nameObj.GetComponent<Text>();
                nameTxt.font = PhoneAppsUI.UIFont;
                nameTxt.text = app.name;
                nameTxt.fontSize = 11;
                nameTxt.fontStyle = FontStyle.Bold;
                nameTxt.color = Color.white;
                nameTxt.alignment = TextAnchor.MiddleCenter;
            }

            // Bottom Navigation Bar indicator
            GameObject navBarObj = new GameObject("NavHomeBar", typeof(RectTransform), typeof(Image));
            navBarObj.transform.SetParent(parent.transform, false);
            RectTransform navBarRect = navBarObj.GetComponent<RectTransform>();
            navBarRect.anchorMin = new Vector2(0.35f, 0.02f);
            navBarRect.anchorMax = new Vector2(0.65f, 0.04f);
            navBarObj.GetComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f, 0.7f);
        }
    }
}
