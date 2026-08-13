using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace IdleBike
{
    /// <summary>Full-screen fade used for app start and hard transitions (e.g. progress reset).</summary>
    public class ScreenFader : MonoBehaviour
    {
        Image _img;

        public static ScreenFader Create(Transform parent)
        {
            var canvas = UIFactory.CreateCanvas("FaderCanvas", 100, parent);
            var img = UIFactory.Image(canvas.transform, "Fade", Color.black);
            UIFactory.Fill(img.rectTransform);
            var fader = canvas.gameObject.AddComponent<ScreenFader>();
            fader._img = img;
            return fader;
        }

        public void FadeIn(float duration)
        {
            StopAllCoroutines();
            StartCoroutine(FadeRoutine(1f, 0f, duration, null));
        }

        /// <summary>Fade to black, run the action, fade back.</summary>
        public void Flash(Action midAction, float duration = 0.35f)
        {
            StopAllCoroutines();
            StartCoroutine(FlashRoutine(midAction, duration));
        }

        IEnumerator FlashRoutine(Action midAction, float duration)
        {
            yield return FadeRoutine(0f, 1f, duration, null);
            midAction?.Invoke();
            yield return FadeRoutine(1f, 0f, duration, null);
        }

        IEnumerator FadeRoutine(float from, float to, float duration, Action done)
        {
            _img.raycastTarget = true;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                SetAlpha(Mathf.Lerp(from, to, Mathf.Clamp01(t / duration)));
                yield return null;
            }
            SetAlpha(to);
            _img.raycastTarget = to > 0.01f;
            done?.Invoke();
        }

        void SetAlpha(float a)
        {
            var c = _img.color;
            _img.color = new Color(c.r, c.g, c.b, a);
        }
    }
}
