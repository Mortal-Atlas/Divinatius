using System;
using UnityEngine;
using UnityEngine.UI;

namespace Divinatius.UI
{
    public class WaypointNavigationArrow : MonoBehaviour
    {
        public static WaypointNavigationArrow Instance { get; private set; }

        private GameObject _hudCanvas;
        private GameObject _arrowPanel;
        private Text _arrowLabelText;
        private Transform _arrow3D;

        private Vector3 _targetWorldPosition;
        private string _targetName = "";
        private bool _isNavigating = false;

        public bool IsNavigating => _isNavigating;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            if (Instance == null)
            {
                GameObject obj = new GameObject("WaypointNavigationArrow");
                obj.AddComponent<WaypointNavigationArrow>();
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

            BuildArrowUI();
        }

        private void Update()
        {
            if (!_isNavigating) return;

            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null) return;

            Vector3 playerPos = playerObj.transform.position;
            float dist = Vector3.Distance(playerPos, _targetWorldPosition);

            // Hide when arriving near target
            if (dist <= 4.0f)
            {
                Debug.Log($"[WaypointNavigationArrow] Arrived at destination: {_targetName}");
                HideArrow();
                return;
            }

            // Direction calculation from player to target
            Vector3 dirToTarget = (_targetWorldPosition - playerPos).normalized;
            dirToTarget.y = 0;

            // Rotate 3D Arrow over player
            if (_arrow3D != null)
            {
                _arrow3D.position = playerPos + Vector3.up * 2.6f;
                if (dirToTarget.sqrMagnitude > 0.01f)
                {
                    _arrow3D.rotation = Quaternion.LookRotation(dirToTarget) * Quaternion.Euler(90f, 0, 0);
                }
            }

            // Update Screen Banner Label
            if (_arrowLabelText != null)
            {
                _arrowLabelText.text = $"📍 <b>GUIDE TO {_targetName.ToUpper()}</b>: {dist:F1}m ➔";
            }
        }

        public void SetNavigationTarget(Vector3 worldPos, string locationName)
        {
            _targetWorldPosition = worldPos;
            _targetName = locationName;
            _isNavigating = true;

            if (_arrowPanel != null) _arrowPanel.SetActive(true);
            if (_arrow3D != null) _arrow3D.gameObject.SetActive(true);

            Debug.Log($"[WaypointNavigationArrow] Waypoint set to '{locationName}' at position {worldPos}");
        }

        public void HideArrow()
        {
            _isNavigating = false;
            if (_arrowPanel != null) _arrowPanel.SetActive(false);
            if (_arrow3D != null) _arrow3D.gameObject.SetActive(false);
        }

        public void SetTargetByKeyword(string queryText)
        {
            if (string.IsNullOrEmpty(queryText)) return;
            string text = queryText.ToLower();

            Vector3 playerPos = Vector3.zero;
            var pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) playerPos = pObj.transform.position;

            if (text.Contains("celeste") || text.Contains("temple") || text.Contains("astral"))
            {
                SetNavigationTarget(playerPos + new Vector3(15f, 0, 35f), "Astral Temple (Celeste)");
            }
            else if (text.Contains("ignatius") || text.Contains("forge") || text.Contains("smith") || text.Contains("blacksmith"))
            {
                SetNavigationTarget(playerPos + new Vector3(-25f, 0, 18f), "Master Forge (Ignatius)");
            }
            else if (text.Contains("thorne") || text.Contains("guard") || text.Contains("barracks"))
            {
                SetNavigationTarget(playerPos + new Vector3(30f, 0, -12f), "Guard Barracks (Thorne)");
            }
            else if (text.Contains("vespera") || text.Contains("alchemist") || text.Contains("potion") || text.Contains("arcana"))
            {
                SetNavigationTarget(playerPos + new Vector3(-18f, 0, -28f), "Shadow Alchemist Shop (Vespera)");
            }
            else if (text.Contains("lyra") || text.Contains("zephyr") || text.Contains("tavern") || text.Contains("bard") || text.Contains("smuggler"))
            {
                SetNavigationTarget(playerPos + new Vector3(8f, 0, -32f), "Wandering Tavern (Lyra & Zephyr)");
            }
            else if (text.Contains("orion") || text.Contains("observatory") || text.Contains("star"))
            {
                SetNavigationTarget(playerPos + new Vector3(-32f, 0, 30f), "Astral Observatory (Orion)");
            }
            else if (text.Contains("direction") || text.Contains("where") || text.Contains("how to get to") || text.Contains("way"))
            {
                SetNavigationTarget(playerPos + new Vector3(15f, 0, 35f), "Astral Temple Square");
            }
        }

        private void BuildArrowUI()
        {
            _hudCanvas = new GameObject("WaypointHUDCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = _hudCanvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 910;

            CanvasScaler scaler = _hudCanvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // Screen Top Banner for Direction Waypoint Label
            _arrowPanel = new GameObject("WaypointBanner", typeof(RectTransform), typeof(Image));
            _arrowPanel.transform.SetParent(_hudCanvas.transform, false);
            RectTransform bannerRect = _arrowPanel.GetComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0.5f, 1f);
            bannerRect.anchorMax = new Vector2(0.5f, 1f);
            bannerRect.pivot = new Vector2(0.5f, 1f);
            bannerRect.sizeDelta = new Vector2(440f, 42f);
            bannerRect.anchoredPosition = new Vector2(0f, -60f);
            _arrowPanel.GetComponent<Image>().color = new Color(0.12f, 0.16f, 0.28f, 0.92f);

            GameObject textObj = new GameObject("LabelText", typeof(RectTransform), typeof(Text));
            textObj.transform.SetParent(_arrowPanel.transform, false);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            _arrowLabelText = textObj.GetComponent<Text>();
            _arrowLabelText.font = PhoneAppsUI.UIFont;
            _arrowLabelText.fontSize = 14;
            _arrowLabelText.fontStyle = FontStyle.Bold;
            _arrowLabelText.alignment = TextAnchor.MiddleCenter;
            _arrowLabelText.color = Color.cyan;

            // 3D Pointer Arrow over player in world
            GameObject arrow3DObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            arrow3DObj.name = "3D_WaypointNavigationArrow";
            Destroy(arrow3DObj.GetComponent<Collider>());

            arrow3DObj.transform.localScale = new Vector3(0.6f, 1.4f, 0.6f);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            Material mat = new Material(shader);
            mat.color = new Color(0.2f, 0.9f, 1.0f, 0.95f);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", new Color(0.2f, 0.9f, 1.0f, 0.95f));
            arrow3DObj.GetComponent<Renderer>().sharedMaterial = mat;

            _arrow3D = arrow3DObj.transform;

            _arrowPanel.SetActive(false);
            _arrow3D.gameObject.SetActive(false);
        }
    }
}
