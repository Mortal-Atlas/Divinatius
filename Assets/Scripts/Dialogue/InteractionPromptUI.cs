using UnityEngine;
using UnityEngine.UI;

namespace Divinatius.Dialogue
{
    public class InteractionPromptUI : MonoBehaviour
    {
        private static InteractionPromptUI _instance;
        public static InteractionPromptUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("InteractionPromptUI");
                    _instance = go.AddComponent<InteractionPromptUI>();
                }
                return _instance;
            }
        }

        private GameObject _promptCanvasObj;
        private Text _promptText;
        private Transform _targetTransform;
        private Vector3 _offsetAboveHead = new Vector3(0, 2.2f, 0);

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            CreatePromptUI();
        }

        private void CreatePromptUI()
        {
            if (_promptCanvasObj != null) return;

            _promptCanvasObj = new GameObject("WorldSpaceInteractionCanvas");
            _promptCanvasObj.transform.SetParent(transform, false);

            Canvas canvas = _promptCanvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            RectTransform canvasRect = _promptCanvasObj.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(400, 60);
            _promptCanvasObj.transform.localScale = Vector3.one * 0.01f;

            // Plain White Text Component Only
            GameObject textObj = new GameObject("PlainWhitePromptText");
            textObj.transform.SetParent(_promptCanvasObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            _promptText = textObj.AddComponent<Text>();

            // Robust font initialization
            Font font = Font.CreateDynamicFontFromOSFont("Arial", 28);
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _promptText.font = font;

            _promptText.alignment = TextAnchor.MiddleCenter;
            _promptText.fontSize = 28;
            _promptText.fontStyle = FontStyle.Bold;
            _promptText.color = Color.white; // Plain white text ONLY
            _promptText.horizontalOverflow = HorizontalWrapMode.Overflow;
            _promptText.verticalOverflow = VerticalWrapMode.Overflow;

            _promptCanvasObj.SetActive(false);
        }

        private void LateUpdate()
        {
            if (_targetTransform == null || _promptCanvasObj == null || !_promptCanvasObj.activeSelf) return;

            // Position plain white text directly above the head of the target
            _promptCanvasObj.transform.position = _targetTransform.position + _offsetAboveHead;

            // Rotate toward main camera
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                _promptCanvasObj.transform.rotation = mainCam.transform.rotation;
            }
        }

        public void ShowPrompt(Transform target, string name = "")
        {
            if (_promptCanvasObj == null) CreatePromptUI();

            _targetTransform = target;
            if (_promptText != null)
            {
                if (!string.IsNullOrEmpty(name))
                    _promptText.text = $"Press [E] to talk to {name}";
                else
                    _promptText.text = "Press [E] to interact";
            }

            if (_promptCanvasObj != null)
            {
                _promptCanvasObj.SetActive(true);
            }
        }

        public void HidePrompt()
        {
            _targetTransform = null;
            if (_promptCanvasObj != null)
            {
                _promptCanvasObj.SetActive(false);
            }
        }
    }
}
