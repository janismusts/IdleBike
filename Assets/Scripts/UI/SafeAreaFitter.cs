using UnityEngine;

namespace IdleBike
{
    /// <summary>
    /// Keeps a full-stretch RectTransform inside Screen.safeArea (notches, cutouts,
    /// home indicator). Optionally keeps the ad banner glued below the safe area.
    /// </summary>
    public class SafeAreaFitter : MonoBehaviour
    {
        /// <summary>Banner spans the raw bottom edge up to safe-bottom + BannerHeight.</summary>
        public RectTransform BannerRect;
        public float BannerHeight;

        RectTransform _rt;
        Rect _applied = new Rect(0f, 0f, -1f, -1f);

        void OnEnable()
        {
            _rt = (RectTransform)transform;
            Apply();
        }

        void Update()
        {
            if (Screen.safeArea != _applied) Apply();
        }

        void Apply()
        {
            float w = Screen.width, h = Screen.height;
            if (w <= 0f || h <= 0f || _rt == null) return;
            var sa = Screen.safeArea;
            _applied = sa;

            _rt.anchorMin = new Vector2(sa.xMin / w, sa.yMin / h);
            _rt.anchorMax = new Vector2(sa.xMax / w, sa.yMax / h);
            _rt.offsetMin = Vector2.zero;
            _rt.offsetMax = Vector2.zero;

            if (BannerRect != null)
            {
                BannerRect.anchorMin = new Vector2(0f, 0f);
                BannerRect.anchorMax = new Vector2(1f, sa.yMin / h);
                BannerRect.pivot = new Vector2(0.5f, 0f);
                BannerRect.offsetMin = Vector2.zero;
                BannerRect.offsetMax = new Vector2(0f, BannerHeight);
            }
        }
    }
}
