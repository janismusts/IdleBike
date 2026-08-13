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

        /// <summary>Seamless horizontal strip: tiles laid edge to edge; sets SpanWidth to match.</summary>
        public void AddStrip(Sprite sprite, float y, int sortingOrder, float scale, int count)
        {
            float w = sprite.bounds.size.x * scale;
            SpanWidth = w * count;
            for (int i = 0; i < count; i++)
                AddProp(sprite, i * w - SpanWidth * 0.5f + w * 0.5f, y, sortingOrder, scale);
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

    /// <summary>
    /// Builds and ticks all parallax layers. Sky layers (mountains, clouds, hills) stay
    /// level on this object; the near tree line goes under the tilt node so it follows
    /// the road grade.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        readonly List<ParallaxLayer> _layers = new List<ParallaxLayer>();

        public void Build(Transform tiltParent)
        {
            var v = Tuning.Visual;

            // Far mountains
            var mountains = NewLayer("Mountains", v.mountainParallax, 320f, null);
            var mountainArt = ArtLibrary.EnvMountains();
            if (mountainArt != null)
            {
                mountains.AddStrip(mountainArt, v.mountainY, -40, v.mountainScale, Mathf.Max(2, v.mountainTiles));
            }
            else
            {
                for (int i = 0; i < 5; i++)
                    mountains.AddProp(PixelSprites.Mountain(), Rand(i, 0) * 320f - 160f, v.mountainY, -40,
                        Mathf.Lerp(v.mountainScaleMin, v.mountainScaleMax, Rand(i, 1)),
                        new Color(1f, 1f, 1f, 0.9f));
            }

            // Clouds
            var clouds = NewLayer("Clouds", v.cloudParallax, 300f, null);
            for (int i = 0; i < Mathf.Max(1, v.cloudCount); i++)
            {
                var cloudSprite = ArtLibrary.EnvCloud(i % 3);
                if (cloudSprite == null) cloudSprite = PixelSprites.Cloud(i % 3);
                clouds.AddProp(cloudSprite, Rand(i, 2) * 300f - 150f,
                    Mathf.Lerp(v.cloudMinY, v.cloudMaxY, Rand(i, 3)), -50,
                    Mathf.Lerp(v.cloudMinScale, v.cloudMaxScale, Rand(i, 4)));
            }

            // Rolling hills
            var hills = NewLayer("Hills", v.hillParallax, 260f, null);
            var hillArt = ArtLibrary.EnvHills();
            if (hillArt != null)
            {
                hills.AddStrip(hillArt, v.hillY, -30, v.hillScale, Mathf.Max(2, v.hillTiles));
            }
            else
            {
                for (int i = 0; i < 6; i++)
                    hills.AddProp(PixelSprites.Hill(), Rand(i, 5) * 260f - 130f, v.hillY, -30, 1.2f + Rand(i, 6) * 1.4f);
            }

            // Trees + bushes just behind the road — tilted with the grade
            var trees = NewLayer("Trees", v.treeParallax, 200f, tiltParent);
            for (int i = 0; i < Mathf.Max(1, v.treeCount); i++)
            {
                if (i % 4 == 3)
                {
                    var flowers = ArtLibrary.EnvFlowers();
                    if (flowers != null) { trees.AddProp(flowers, Rand(i, 10) * 200f - 100f, v.treeY, -8); continue; }
                }
                if (i % 3 == 2)
                {
                    var bush = ArtLibrary.EnvBush(i);
                    if (bush == null) bush = PixelSprites.Bush();
                    trees.AddProp(bush, Rand(i, 7) * 200f - 100f, v.treeY, -8);
                }
                else
                {
                    var tree = ArtLibrary.EnvTree(i % 3);
                    float scale = tree != null
                        ? Mathf.Lerp(v.treeMinScale, v.treeMaxScale, Rand(i, 9))
                        : 0.9f + Rand(i, 9) * 0.6f;
                    if (tree == null) tree = PixelSprites.Tree(i % 3);
                    trees.AddProp(tree, Rand(i, 8) * 200f - 100f, v.treeY, -9, scale);
                }
            }
        }

        ParallaxLayer NewLayer(string name, float factor, float span, Transform parentOverride)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parentOverride != null ? parentOverride : transform, false);
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
