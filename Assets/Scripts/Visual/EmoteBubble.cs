using System.Collections;
using UnityEngine;

namespace IdleBike
{
    /// <summary>Speech bubble with an emote icon above a rider's head. Pops in, holds, fades.</summary>
    public class EmoteBubble : MonoBehaviour
    {
        SpriteRenderer _bubble;
        SpriteRenderer _icon;
        Coroutine _routine;

        public static EmoteBubble Attach(Transform rider, int sortingOrder)
        {
            var go = new GameObject("EmoteBubble");
            go.transform.SetParent(rider, false);
            go.transform.localPosition = new Vector3(0.35f, 2.05f, 0f);
            var b = go.AddComponent<EmoteBubble>();

            b._bubble = go.AddComponent<SpriteRenderer>();
            b._bubble.sprite = PixelSprites.SpeechBubble();
            b._bubble.sortingOrder = sortingOrder;

            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(go.transform, false);
            iconGo.transform.localPosition = new Vector3(0.25f, 0.62f, 0f);
            b._icon = iconGo.AddComponent<SpriteRenderer>();
            b._icon.sortingOrder = sortingOrder + 1;

            go.SetActive(false);
            return b;
        }

        public void Show(int emoteIndex)
        {
            var art = ArtLibrary.Emote(emoteIndex);
            _icon.sprite = art != null ? art : PixelSprites.Emote(emoteIndex);
            // both art (32px@32ppu) and placeholder (14px@14ppu) are 1 unit — fit the bubble body
            _icon.transform.localScale = Vector3.one * 0.5f;

            gameObject.SetActive(true);
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(Play());
        }

        IEnumerator Play()
        {
            float hold = Tuning.Balance.emoteDuration;
            SetAlpha(1f);

            // pop in with a small overshoot
            float t = 0f;
            const float popDur = 0.14f;
            while (t < popDur)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / popDur);
                transform.localScale = Vector3.one * Mathf.Lerp(0.2f, 1.08f, k);
                yield return null;
            }
            transform.localScale = Vector3.one;

            yield return new WaitForSeconds(hold);

            // fade out
            t = 0f;
            const float fadeDur = 0.2f;
            while (t < fadeDur)
            {
                t += Time.deltaTime;
                SetAlpha(1f - Mathf.Clamp01(t / fadeDur));
                yield return null;
            }
            gameObject.SetActive(false);
        }

        void SetAlpha(float a)
        {
            var c = _bubble.color; _bubble.color = new Color(c.r, c.g, c.b, a);
            var i = _icon.color; _icon.color = new Color(i.r, i.g, i.b, a);
        }
    }
}
