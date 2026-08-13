using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace IdleBike
{
    /// <summary>Code-built uGUI helpers (no prefabs, no scene setup).</summary>
    public static class UIFactory
    {
        public static readonly Color WindowBg = new Color(0.11f, 0.12f, 0.17f, 0.98f);
        public static readonly Color RowBg = new Color(1f, 1f, 1f, 0.06f);
        public static readonly Color Accent = new Color(0.30f, 0.75f, 0.35f);
        public static readonly Color AccentBlue = new Color(0.25f, 0.55f, 0.95f);
        public static readonly Color Danger = new Color(0.85f, 0.30f, 0.25f);
        public static readonly Color TextMain = new Color(0.95f, 0.95f, 0.97f);
        public static readonly Color TextDim = new Color(0.65f, 0.67f, 0.72f);

        static Font _font;
        public static Font DefaultFont
        {
            get
            {
                if (_font == null) _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return _font;
            }
        }

        public static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }

        public static Canvas CreateCanvas(string name, int sortOrder, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.layer = LayerMask.NameToLayer("UI");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static Image Image(Transform parent, string name, Color color, Sprite sprite = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.layer = LayerMask.NameToLayer("UI");
            var img = go.AddComponent<Image>();
            img.color = color;
            img.sprite = sprite;
            return img;
        }

        public static Text Text(Transform parent, string name, string content, int size, Color color,
            TextAnchor anchor = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Bold)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.layer = LayerMask.NameToLayer("UI");
            var txt = go.AddComponent<Text>();
            txt.font = DefaultFont;
            txt.text = content;
            txt.fontSize = size;
            txt.color = color;
            txt.alignment = anchor;
            txt.fontStyle = style;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;
            return txt;
        }

        public static Button Button(Transform parent, string name, string label, int fontSize, Color bg,
            Action onClick, bool feedback = true)
        {
            var img = Image(parent, name, bg);
            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            if (!string.IsNullOrEmpty(label))
            {
                var txt = Text(img.transform, "Label", label, fontSize, TextMain);
                Fill(txt.rectTransform);
            }
            btn.onClick.AddListener(() =>
            {
                if (feedback)
                {
                    if (AudioManager.I != null) AudioManager.I.PlayClick();
                    Haptics.Light();
                }
                onClick?.Invoke();
            });
            return btn;
        }

        public static Slider Slider(Transform parent, string name, float value, Action<float> onChanged)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.layer = LayerMask.NameToLayer("UI");
            root.AddComponent<RectTransform>();
            var slider = root.AddComponent<Slider>();

            var bg = Image(root.transform, "Background", new Color(0f, 0f, 0f, 0.45f), PixelSprites.White());
            SetStretch(bg.rectTransform, new Vector2(0f, 0.30f), new Vector2(1f, 0.70f));

            var fillArea = new GameObject("Fill Area").AddComponent<RectTransform>();
            fillArea.gameObject.layer = LayerMask.NameToLayer("UI");
            fillArea.SetParent(root.transform, false);
            SetStretch(fillArea, new Vector2(0f, 0.30f), new Vector2(1f, 0.70f));
            fillArea.offsetMin = new Vector2(6f, 0f);
            fillArea.offsetMax = new Vector2(-6f, 0f);

            var fill = Image(fillArea, "Fill", AccentBlue, PixelSprites.White());
            fill.rectTransform.anchorMin = new Vector2(0f, 0f);
            fill.rectTransform.anchorMax = new Vector2(0f, 1f);
            fill.rectTransform.sizeDelta = new Vector2(10f, 0f);

            var handleArea = new GameObject("Handle Slide Area").AddComponent<RectTransform>();
            handleArea.gameObject.layer = LayerMask.NameToLayer("UI");
            handleArea.SetParent(root.transform, false);
            SetStretch(handleArea, Vector2.zero, Vector2.one);
            handleArea.offsetMin = new Vector2(18f, 0f);
            handleArea.offsetMax = new Vector2(-18f, 0f);

            var handle = Image(handleArea, "Handle", TextMain, PixelSprites.White());
            handle.rectTransform.anchorMin = new Vector2(0f, 0f);
            handle.rectTransform.anchorMax = new Vector2(0f, 1f);
            handle.rectTransform.sizeDelta = new Vector2(36f, 0f);

            slider.targetGraphic = handle;
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = value;
            if (onChanged != null) slider.onValueChanged.AddListener(v => onChanged(v));
            return slider;
        }

        public static Toggle Toggle(Transform parent, string name, bool value, Action<bool> onChanged)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.layer = LayerMask.NameToLayer("UI");
            root.AddComponent<RectTransform>();
            var toggle = root.AddComponent<Toggle>();

            var bg = Image(root.transform, "Background", new Color(0f, 0f, 0f, 0.45f), PixelSprites.White());
            Fill(bg.rectTransform);

            var check = Image(bg.transform, "Checkmark", Accent, PixelSprites.White());
            SetStretch(check.rectTransform, Vector2.zero, Vector2.one);
            check.rectTransform.offsetMin = new Vector2(10f, 10f);
            check.rectTransform.offsetMax = new Vector2(-10f, -10f);

            toggle.targetGraphic = bg;
            toggle.graphic = check;
            toggle.isOn = value;
            if (onChanged != null) toggle.onValueChanged.AddListener(v => onChanged(v));
            return toggle;
        }

        public static InputField InputField(Transform parent, string name, string placeholderText, int charLimit = 18)
        {
            var bg = Image(parent, name, new Color(0f, 0f, 0f, 0.45f), PixelSprites.White());
            var input = bg.gameObject.AddComponent<InputField>();

            var ph = Text(bg.transform, "Placeholder", placeholderText, 32, new Color(1f, 1f, 1f, 0.35f),
                TextAnchor.MiddleLeft, FontStyle.Italic);
            Fill(ph.rectTransform);
            ph.rectTransform.offsetMin = new Vector2(24f, 0f);
            ph.rectTransform.offsetMax = new Vector2(-24f, 0f);

            var txt = Text(bg.transform, "Text", "", 32, TextMain, TextAnchor.MiddleLeft);
            txt.supportRichText = false;
            Fill(txt.rectTransform);
            txt.rectTransform.offsetMin = new Vector2(24f, 0f);
            txt.rectTransform.offsetMax = new Vector2(-24f, 0f);

            input.targetGraphic = bg;
            input.textComponent = txt;
            input.placeholder = ph;
            input.characterLimit = charLimit;
            return input;
        }

        /// <summary>Vertical scroll view. Returns the content transform to add rows to.</summary>
        public static RectTransform ScrollView(Transform parent, string name, out ScrollRect scrollRect)
        {
            var rootImg = Image(parent, name, new Color(0f, 0f, 0f, 0.25f), PixelSprites.White());
            scrollRect = rootImg.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.scrollSensitivity = 30f;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            var viewport = Image(rootImg.transform, "Viewport", new Color(1f, 1f, 1f, 0.01f), PixelSprites.White());
            Fill(viewport.rectTransform);
            viewport.gameObject.AddComponent<RectMask2D>();

            var content = new GameObject("Content").AddComponent<RectTransform>();
            content.gameObject.layer = LayerMask.NameToLayer("UI");
            content.SetParent(viewport.transform, false);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = new Vector2(0f, 0f);

            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 12f;
            layout.padding = new RectOffset(16, 16, 16, 16);

            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewport.rectTransform;
            scrollRect.content = content;
            return content;
        }

        // ---------- RectTransform helpers ----------

        public static void Fill(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static void SetStretch(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        /// <summary>Anchor to a point with a fixed size.</summary>
        public static void SetPoint(RectTransform rt, Vector2 anchor, Vector2 anchoredPos, Vector2 size)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
        }
    }
}
