using UnityEngine;
using UnityEngine.UI;

namespace IdleBike
{
    /// <summary>Modal panel base: dim background, centered window, title, close button.</summary>
    public abstract class UIPanel : MonoBehaviour
    {
        protected UIRoot Root;
        protected RectTransform Window;
        public CanvasGroup Group { get; private set; }

        protected abstract string Title { get; }
        protected virtual Vector2 WindowSize => new Vector2(960f, 1400f);

        public void Create(UIRoot root)
        {
            Root = root;
            gameObject.layer = LayerMask.NameToLayer("UI");
            var rt = gameObject.AddComponent<RectTransform>();
            UIFactory.Fill(rt);
            Group = gameObject.AddComponent<CanvasGroup>();

            var dim = UIFactory.Image(transform, "Dim", new Color(0f, 0f, 0f, 0.66f));
            UIFactory.Fill(dim.rectTransform);
            var dimBtn = dim.gameObject.AddComponent<Button>();
            dimBtn.transition = Selectable.Transition.None;
            dimBtn.onClick.AddListener(() => Root.ClosePanel());

            var win = UIFactory.Image(transform, "Window", UIFactory.WindowBg, PixelSprites.White());
            UIFactory.SetPoint(win.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), WindowSize);
            Window = win.rectTransform;

            var title = UIFactory.Text(Window, "Title", Title, 56, UIFactory.TextMain);
            UIFactory.SetPoint(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(800f, 80f));

            UIFactory.Button(Window, "Close", "X", 44, new Color(1f, 1f, 1f, 0.10f), () => Root.ClosePanel());
            var closeRt = Window.Find("Close").GetComponent<RectTransform>();
            UIFactory.SetPoint(closeRt, new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(88f, 88f));

            BuildContent();
            gameObject.SetActive(false);
        }

        protected abstract void BuildContent();

        /// <summary>Called every time the panel is opened.</summary>
        public virtual void OnOpened() { }

        /// <summary>Called when the panel finishes closing.</summary>
        public virtual void OnClosed() { }
    }
}
