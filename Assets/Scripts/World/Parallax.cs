using System.Collections.Generic;
using UnityEngine;

namespace IdleBike
{
    /// <summary>One repeating parallax layer of scattered props (clouds, hills, trees...).</summary>
    public class ParallaxLayer : MonoBehaviour
    {
        public float Factor = 0.5f;   // fraction of world scroll speed
        public float SpanWidth = 300f;

        readonly List<Transform> _props = new List<Transform>();
        readonly List<float> _baseX = new List<float>();

        public void AddProp(Sprite sprite, float x, float y, int sortingOrder, float scale = 1f, Color? tint = null)
        {
            var go = new GameObject("Prop");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            if (tint.HasValue) sr.color = tint.Value;
            go.transform.localScale = Vector3.one * scale;
            go.transform.localPosition = new Vector3(x, y, 0f);
            _props.Add(go.transform);
            _baseX.Add(x);
        }

        public void Tick()
        {
            float scroll = (float)(GameState.Data.totalDistance * Factor % SpanWidth);
            for (int i = 0; i < _props.Count; i++)
            {
                float x = _baseX[i] - scroll;
                // wrap into [-SpanWidth/2, SpanWidth/2)
                x = Mathf.Repeat(x + SpanWidth * 0.5f, SpanWidth) - SpanWidth * 0.5f;
                var lp = _props[i].localPosition;
                _props[i].localPosition = new Vector3(x, lp.y, lp.z);
            }
        }
    }

    /// <summary>Builds and ticks all background/foreground parallax layers.</summary>
    public class ParallaxBackground : MonoBehaviour
    {
        readonly List<ParallaxLayer> _layers = new List<ParallaxLayer>();

        public void Build()
        {
            // Far mountains
            var mountains = NewLayer("Mountains", 0.08f, 320f);
            for (int i = 0; i < 5; i++)
                mountains.AddProp(PixelSprites.Mountain(), Rand(i, 0) * 320f - 160f, 0.4f, -40,
                    Mathf.Lerp(Tuning.Visual.mountainScaleMin, Tuning.Visual.mountainScaleMax, Rand(i, 1)),
                    new Color(1f, 1f, 1f, 0.9f));

            // Clouds
            var clouds = NewLayer("Clouds", 0.05f, 300f);
            for (int i = 0; i < 7; i++)
                clouds.AddProp(PixelSprites.Cloud(i % 3), Rand(i, 2) * 300f - 150f, 5f + Rand(i, 3) * 6f, -50,
                    1f + Rand(i, 4) * 1.4f);

            // Hills
            var hills = NewLayer("Hills", 0.25f, 260f);
            for (int i = 0; i < 6; i++)
                hills.AddProp(PixelSprites.Hill(), Rand(i, 5) * 260f - 130f, 0.15f, -30, 1.2f + Rand(i, 6) * 1.4f);

            // Trees + bushes just behind the road
            var trees = NewLayer("Trees", 1f, 200f);
            for (int i = 0; i < 14; i++)
            {
                if (i % 3 == 2)
                    trees.AddProp(PixelSprites.Bush(), Rand(i, 7) * 200f - 100f, 0.05f, -8);
                else
                    trees.AddProp(PixelSprites.Tree(i % 3), Rand(i, 8) * 200f - 100f, 0.05f, -9, 0.9f + Rand(i, 9) * 0.6f);
            }
        }

        ParallaxLayer NewLayer(string name, float factor, float span)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var layer = go.AddComponent<ParallaxLayer>();
            layer.Factor = factor;
            layer.SpanWidth = span;
            _layers.Add(layer);
            return layer;
        }

        // deterministic pseudo-random in [0,1) so layout is stable between runs
        static float Rand(int i, int salt)
        {
            float v = Mathf.Sin(i * 127.1f + salt * 311.7f) * 43758.5453f;
            return v - Mathf.Floor(v);
        }

        public void Tick()
        {
            foreach (var l in _layers) l.Tick();
        }
    }
}
