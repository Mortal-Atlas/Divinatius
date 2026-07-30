using UnityEngine;
using UnityEngine.UI;

namespace Divinatius.UI
{
    public class OverheadSpeechBubble : MonoBehaviour
    {
        private Text bubbleText;
        private CanvasGroup canvasGroup;
        private Transform mainCameraTransform;
        private float displayDuration = 4.0f;
        private float timer = 0f;

        public static OverheadSpeechBubble Create(Transform parent, string text, float duration = 4.0f)
        {
            if (parent == null) return null;

            // Destroy previous bubble if exists
            Transform existing = parent.Find("OverheadSpeechBubble_Instance");
            if (existing != null) Destroy(existing.gameObject);

            GameObject bubbleObj = new GameObject("OverheadSpeechBubble_Instance");
            bubbleObj.transform.SetParent(parent, false);
            bubbleObj.transform.localPosition = new Vector3(0, 2.3f, 0);

            Canvas canvas = bubbleObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;

            RectTransform rect = bubbleObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(3.2f, 1.2f);
            bubbleObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            CanvasGroup cg = bubbleObj.AddComponent<CanvasGroup>();

            // Panel Background
            GameObject bgObj = new GameObject("BubbleBG");
            bgObj.transform.SetParent(bubbleObj.transform, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.08f, 0.08f, 0.12f, 0.88f);

            // Text
            GameObject textObj = new GameObject("BubbleText");
            textObj.transform.SetParent(bgObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);

            Text tComp = textObj.AddComponent<Text>();
            tComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tComp.fontSize = 18;
            tComp.alignment = TextAnchor.MiddleCenter;
            tComp.color = Color.white;
            tComp.horizontalOverflow = HorizontalWrapMode.Wrap;
            tComp.verticalOverflow = VerticalWrapMode.Overflow;
            tComp.text = text;

            OverheadSpeechBubble bubble = bubbleObj.AddComponent<OverheadSpeechBubble>();
            bubble.displayDuration = duration;
            bubble.canvasGroup = cg;
            bubble.bubbleText = tComp;

            return bubble;
        }

        private void Start()
        {
            if (Camera.main != null)
            {
                mainCameraTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
            if (mainCameraTransform == null && Camera.main != null)
            {
                mainCameraTransform = Camera.main.transform;
            }

            if (mainCameraTransform != null)
            {
                // Face the camera
                transform.rotation = Quaternion.LookRotation(transform.position - mainCameraTransform.position);
            }

            // Float upward slightly
            transform.position += Vector3.up * (0.15f * Time.deltaTime);

            timer += Time.deltaTime;
            if (timer >= displayDuration - 1.0f && canvasGroup != null)
            {
                // Fade out in last second
                canvasGroup.alpha = Mathf.Lerp(1.0f, 0.0f, (timer - (displayDuration - 1.0f)));
            }

            if (timer >= displayDuration)
            {
                Destroy(gameObject);
            }
        }
    }
}
