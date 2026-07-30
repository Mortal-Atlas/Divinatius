using UnityEngine;
using UnityEngine.UI;

namespace Divinatius.UI
{
    public class MinimapUIController : MonoBehaviour
    {
        private static MinimapUIController _instance;
        public static MinimapUIController Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("MinimapUIController");
                    _instance = go.AddComponent<MinimapUIController>();
                }
                return _instance;
            }
        }

        [Header("Target Tracking")]
        [SerializeField] private Transform playerTransform;

        [Header("Minimap Configuration")]
        [SerializeField] private float cameraHeight = 40.0f;
        [SerializeField] private float orthographicSize = 18.0f;
        [SerializeField] private Vector2 minimapSize = new Vector2(180, 180);

        private Camera _minimapCamera;
        private RenderTexture _minimapRenderTexture;
        private RectTransform _playerArrowIcon;
        private GameObject _minimapCanvasObj;

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

            FindPlayerRef();
            SetupMinimap();
        }

        private void Start()
        {
            FindPlayerRef();
        }

        private void FindPlayerRef()
        {
            if (playerTransform == null)
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    playerTransform = playerObj.transform;
                }
                else
                {
                    var pc = FindFirstObjectByType<Divinatius.Player.PlayerController>();
                    if (pc != null) playerTransform = pc.transform;
                }
            }
        }

        private void SetupMinimap()
        {
            if (_minimapCanvasObj != null) return;

            // 1. Create Top-Down Camera
            GameObject camObj = new GameObject("MinimapCamera");
            camObj.transform.SetParent(transform, false);
            _minimapCamera = camObj.AddComponent<Camera>();
            _minimapCamera.orthographic = true;
            _minimapCamera.orthographicSize = orthographicSize;
            _minimapCamera.clearFlags = CameraClearFlags.SolidColor;
            _minimapCamera.backgroundColor = new Color(0.1f, 0.12f, 0.15f, 1f);
            _minimapCamera.cullingMask = ~0;

            // Set fixed top-down rotation (North stays UP, map does not rotate)
            camObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            // 2. Create Render Texture
            _minimapRenderTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
            _minimapRenderTexture.Create();
            _minimapCamera.targetTexture = _minimapRenderTexture;

            // 3. Create UI Canvas (Top Right Overlay)
            _minimapCanvasObj = new GameObject("MinimapUICanvas");
            _minimapCanvasObj.transform.SetParent(transform, false);

            Canvas canvas = _minimapCanvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 99;
            CanvasScaler scaler = _minimapCanvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            _minimapCanvasObj.AddComponent<GraphicRaycaster>();

            // 4. Create Outer Circular Frame
            GameObject frameObj = new GameObject("MinimapFrame");
            frameObj.transform.SetParent(_minimapCanvasObj.transform, false);
            RectTransform frameRect = frameObj.AddComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(1, 1);
            frameRect.anchorMax = new Vector2(1, 1);
            frameRect.pivot = new Vector2(1, 1);
            frameRect.anchoredPosition = new Vector2(-25, -25);
            frameRect.sizeDelta = minimapSize;

            // Generate Circular Alpha Mask Sprite
            Sprite circleSprite = CreateCircleSprite(256);

            // Add Mask Component to make minimap circular
            Image maskImage = frameObj.AddComponent<Image>();
            maskImage.sprite = circleSprite;
            maskImage.type = Image.Type.Simple;
            Mask mask = frameObj.AddComponent<Mask>();
            mask.showMaskGraphic = true;
            maskImage.color = new Color(0.08f, 0.1f, 0.14f, 0.95f);

            // 5. RawImage displaying top-down Render Texture
            GameObject rawImgObj = new GameObject("MinimapRawImage");
            rawImgObj.transform.SetParent(frameObj.transform, false);
            RectTransform rawImgRect = rawImgObj.AddComponent<RectTransform>();
            rawImgRect.anchorMin = Vector2.zero;
            rawImgRect.anchorMax = Vector2.one;
            rawImgRect.offsetMin = Vector2.zero;
            rawImgRect.offsetMax = Vector2.zero;

            RawImage rawImage = rawImgObj.AddComponent<RawImage>();
            rawImage.texture = _minimapRenderTexture;

            // 6. Circular Outer Border Ring
            GameObject borderObj = new GameObject("MinimapBorderRing");
            borderObj.transform.SetParent(frameObj.transform, false);
            RectTransform borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(-3, -3);
            borderRect.offsetMax = new Vector2(3, 3);

            Image borderImage = borderObj.AddComponent<Image>();
            borderImage.sprite = circleSprite;
            borderImage.type = Image.Type.Simple;
            borderImage.color = new Color(0.2f, 0.7f, 1f, 0.85f); // Cyan accent border

            // 7. Center Player Arrow / View Direction Icon
            GameObject arrowObj = new GameObject("PlayerDirectionIcon");
            arrowObj.transform.SetParent(frameObj.transform, false);
            _playerArrowIcon = arrowObj.AddComponent<RectTransform>();
            _playerArrowIcon.anchorMin = new Vector2(0.5f, 0.5f);
            _playerArrowIcon.anchorMax = new Vector2(0.5f, 0.5f);
            _playerArrowIcon.pivot = new Vector2(0.5f, 0.5f);
            _playerArrowIcon.anchoredPosition = Vector2.zero;
            _playerArrowIcon.sizeDelta = new Vector2(24, 24);

            Image arrowImage = arrowObj.AddComponent<Image>();
            arrowImage.sprite = CreateArrowSprite(64, 64);
            arrowImage.color = new Color(1f, 0.85f, 0.2f, 1f); // Golden Arrow
        }

        private void LateUpdate()
        {
            if (playerTransform == null)
            {
                FindPlayerRef();
                if (playerTransform == null) return;
            }

            // Move minimap camera strictly over player (X, Z track player position, rotation fixed top-down)
            if (_minimapCamera != null)
            {
                Vector3 targetPos = playerTransform.position;
                _minimapCamera.transform.position = new Vector3(targetPos.x, cameraHeight, targetPos.z);
                _minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Fixed map rotation
            }

            // Rotate center player arrow icon to match player's Y view heading
            if (_playerArrowIcon != null)
            {
                float playerYAngle = playerTransform.eulerAngles.y;
                _playerArrowIcon.rotation = Quaternion.Euler(0f, 0f, -playerYAngle);
            }
        }

        private static Sprite CreateCircleSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] cols = new Color[size * size];
            float radius = size * 0.5f;
            Vector2 center = new Vector2(radius, radius);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= radius)
                    {
                        float alpha = Mathf.Clamp01((radius - dist) / 1.5f);
                        cols[y * size + x] = new Color(1f, 1f, 1f, alpha);
                    }
                    else
                    {
                        cols[y * size + x] = Color.clear;
                    }
                }
            }
            tex.SetPixels(cols);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private static Sprite CreateArrowSprite(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] cols = new Color[width * height];
            for (int i = 0; i < cols.Length; i++) cols[i] = Color.clear;

            Vector2 top = new Vector2(width * 0.5f, height - 4);
            Vector2 left = new Vector2(6, 6);
            Vector2 right = new Vector2(width - 7, 6);
            Vector2 innerIndent = new Vector2(width * 0.5f, 14);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2 pt = new Vector2(x, y);
                    if (IsPointInTriangle(pt, top, left, innerIndent) || IsPointInTriangle(pt, top, right, innerIndent))
                    {
                        cols[y * width + x] = Color.white;
                    }
                }
            }
            tex.SetPixels(cols);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        }

        private static bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Sign(p, a, b);
            float d2 = Sign(p, b, c);
            float d3 = Sign(p, c, a);

            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(hasNeg && hasPos);
        }

        private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }

        private void OnDestroy()
        {
            if (_minimapRenderTexture != null)
            {
                _minimapRenderTexture.Release();
                Destroy(_minimapRenderTexture);
            }
        }
    }
}
