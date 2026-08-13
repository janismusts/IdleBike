using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace IdleBike
{
    /// <summary>
    /// Placeholder rewarded-ad flow (real ads SDK later). Shows a fake ad for a few
    /// seconds, then lets the player claim the reward or close without it.
    /// </summary>
    public class RewardedAdOverlay : MonoBehaviour
    {
        const float AdSeconds = 3f;

        Action _onReward;
        Text _countdown;
        Button _claim;
        Button _close;
        CanvasGroup _group;

        public static RewardedAdOverlay Create(Transform canvas)
        {
            var go = new GameObject("RewardedAd");
            go.transform.SetParent(canvas, false);
            go.layer = LayerMask.NameToLayer("UI");
            var overlay = go.AddComponent<RewardedAdOverlay>();
            overlay.BuildUi();
            go.SetActive(false);
            return overlay;
        }

        void BuildUi()
        {
            var rt = gameObject.AddComponent<RectTransform>();
            UIFactory.Fill(rt);
            _group = gameObject.AddComponent<CanvasGroup>();

            var bg = UIFactory.Image(transform, "Bg", new Color(0.05f, 0.05f, 0.08f, 0.98f));
            UIFactory.Fill(bg.rectTransform);

            var title = UIFactory.Text(transform, "Title", "REWARDED AD", 64, UIFactory.TextMain);
            UIFactory.SetPoint(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 240f), new Vector2(800f, 80f));

            var sub = UIFactory.Text(transform, "Sub", "PLACEHOLDER — ADS SDK COMES LATER", 30, UIFactory.TextDim);
            UIFactory.SetPoint(sub.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 160f), new Vector2(800f, 40f));

            _countdown = UIFactory.Text(transform, "Countdown", "3", 120, UIFactory.Accent);
            UIFactory.SetPoint(_countdown.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(300f, 140f));

            _claim = UIFactory.Button(transform, "Claim", "CLAIM REWARD", 44, UIFactory.Accent, () =>
            {
                var cb = _onReward;
                Hide();
                cb?.Invoke();
            });
            UIFactory.SetPoint(_claim.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0f, -180f), new Vector2(560f, 110f));

            _close = UIFactory.Button(transform, "Close", "X", 40, new Color(1f, 1f, 1f, 0.12f), Hide);
            UIFactory.SetPoint(_close.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(-30f, -30f), new Vector2(88f, 88f));
        }

        public void Show(Action onReward)
        {
            _onReward = onReward;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            StopAllCoroutines();
            StartCoroutine(Run());
        }

        void Hide()
        {
            _onReward = null;
            StopAllCoroutines();
            gameObject.SetActive(false);
        }

        IEnumerator Run()
        {
            _claim.gameObject.SetActive(false);
            _close.gameObject.SetActive(false);
            float left = AdSeconds;
            while (left > 0f)
            {
                _countdown.text = Mathf.CeilToInt(left).ToString();
                left -= Time.unscaledDeltaTime;
                yield return null;
            }
            _countdown.text = "OK";
            _claim.gameObject.SetActive(true);
            _close.gameObject.SetActive(true);
        }
    }
}
