using System.Collections.Generic;
using UnityEngine;

namespace IdleBike
{
    /// <summary>
    /// Loads the generated pixel-art from Resources/Art and slices it by known grids
    /// (auto-slicing in the import metas is island-based and unreliable). Every getter
    /// returns null when the asset is missing so callers can fall back to PixelSprites.
    /// Rider sheets: 8 frames of 128x96. Trails: 8 frames of 96x48. Jerseys/helmets are
    /// white in the art and get tinted (jersey via pixel processing, helmet via renderer color).
    /// </summary>
    public static class ArtLibrary
    {
        const int RiderFrames = 8;

        static readonly Dictionary<string, Texture2D> TexCache = new Dictionary<string, Texture2D>();
        static readonly Dictionary<string, Sprite[]> FrameCache = new Dictionary<string, Sprite[]>();
        static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();

        // ---------- textures ----------

        static Texture2D Tex(string path)
        {
            if (TexCache.TryGetValue(path, out var t)) return t;
            t = Resources.Load<Texture2D>(path);
            TexCache[path] = t; // cache misses too
            return t;
        }

        // ---------- authored slices ----------
        // Sheets are sliced in the Sprite Editor; those slices are the authority.
        // For animation frames we keep sheet-space alignment (pivot at a fixed point of
        // the frame's cell) so helmet overlays land exactly where the art has them.

        static readonly Dictionary<string, Sprite[]> RawSlices = new Dictionary<string, Sprite[]>();

        static Sprite[] SlicesOf(string path)
        {
            if (RawSlices.TryGetValue(path, out var s)) return s;
            var all = Resources.LoadAll<Sprite>(path);
            if (all != null && all.Length > 0)
                System.Array.Sort(all, (a, b) => a.rect.x.CompareTo(b.rect.x));
            else
                all = null;
            RawSlices[path] = all;
            return all;
        }

        /// <summary>Union rect of authored slices per frame cell; null cell => full cell rect.</summary>
        static Sprite[] SheetFrames(string path, int frameCount, float cellPivotX, float ppu, Texture2D texOverride)
        {
            var slices = SlicesOf(path);
            if (slices == null) return null;
            var tex = texOverride != null ? texOverride : slices[0].texture;
            float cellW = (float)tex.width / frameCount;

            var cells = new Rect?[frameCount];
            foreach (var s in slices)
            {
                int ci = Mathf.Clamp((int)(s.rect.center.x / cellW), 0, frameCount - 1);
                cells[ci] = cells[ci].HasValue ? Union(cells[ci].Value, s.rect) : s.rect;
            }

            var frames = new Sprite[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                var rect = cells[i] ?? new Rect(i * cellW, 0f, cellW, tex.height);
                // pivot at a fixed sheet-space point of this cell (ground line, cellPivotX across)
                float px = i * cellW + cellW * cellPivotX;
                var pivot = new Vector2((px - rect.x) / rect.width, (0f - rect.y) / rect.height);
                frames[i] = Sprite.Create(tex, rect, pivot, ppu);
            }
            return frames;
        }

        static Rect Union(Rect a, Rect b) =>
            Rect.MinMaxRect(Mathf.Min(a.xMin, b.xMin), Mathf.Min(a.yMin, b.yMin),
                Mathf.Max(a.xMax, b.xMax), Mathf.Max(a.yMax, b.yMax));

        /// <summary>Cluster authored slices into icons: slices whose x-ranges (nearly) touch merge.</summary>
        static Rect[] IconRects(string path, float mergeGap = 3f)
        {
            var slices = SlicesOf(path);
            if (slices == null) return null;
            var result = new List<Rect>();
            Rect cur = slices[0].rect;
            for (int i = 1; i < slices.Length; i++)
            {
                var r = slices[i].rect;
                if (r.xMin <= cur.xMax + mergeGap) cur = Union(cur, r);
                else { result.Add(cur); cur = r; }
            }
            result.Add(cur);
            return result.ToArray();
        }

        static Sprite IconAt(string path, int index, string cacheKey)
        {
            if (SpriteCache.TryGetValue(cacheKey, out var cached)) return cached;
            Sprite s = null;
            var rects = IconRects(path);
            if (rects != null && index >= 0 && index < rects.Length)
            {
                var tex = SlicesOf(path)[0].texture;
                var rect = rects[index];
                // ppu = max dimension => icon is ~1 world unit; UI Images ignore ppu anyway
                s = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), Mathf.Max(rect.width, rect.height));
            }
            SpriteCache[cacheKey] = s;
            return s;
        }

        // ---------- riders ----------

        /// <summary>8 riding frames for a bike tier with the jersey tinted; null if art missing.</summary>
        public static Sprite[] RiderFrames8(int tierIndex, Color32 jersey)
        {
            if (tierIndex < 0 || tierIndex >= BikeDefs.Tiers.Length) return null;
            string sheet = BikeDefs.Tiers[tierIndex].SheetName;
            string key = $"rider_{sheet}_{jersey.r}_{jersey.g}_{jersey.b}";
            if (FrameCache.TryGetValue(key, out var cached)) return cached;

            string path = "Art/riding/" + sheet;
            var tex = Tex(path);
            Sprite[] frames = null;
            if (tex != null)
            {
                var tinted = TintWhite(tex, jersey);
                frames = SheetFrames(path, RiderFrames, 0.5f, Tuning.Visual.riderArtPixelsPerUnit, tinted);
            }
            FrameCache[key] = frames;
            return frames;
        }

        /// <summary>8 helmet overlay frames (white base — tint via SpriteRenderer.color).</summary>
        public static Sprite[] HelmetFrames8(string style)
        {
            string key = "helmet_" + style;
            if (FrameCache.TryGetValue(key, out var cached)) return cached;
            var frames = SheetFrames("Art/cosmetics/helmet-" + style, RiderFrames, 0.5f,
                Tuning.Visual.riderArtPixelsPerUnit, null);
            FrameCache[key] = frames;
            return frames;
        }

        /// <summary>8 trail effect frames (pivot at the cell's bottom-right so it hangs behind the rear wheel).</summary>
        public static Sprite[] TrailFrames8(string trail)
        {
            string key = "trail_" + trail;
            if (FrameCache.TryGetValue(key, out var cached)) return cached;
            var frames = SheetFrames("Art/cosmetics/trail-" + trail, RiderFrames, 1f,
                Tuning.Visual.riderArtPixelsPerUnit, null);
            FrameCache[key] = frames;
            return frames;
        }

        // ---------- environment ----------
        // The atlas is sliced in the Sprite Editor (environment-atlas_N). We take the
        // user-authored rects from those slices but re-create the sprites with our own
        // pivots and PPU so world placement stays consistent.

        static Dictionary<string, Sprite> _envSlices;

        static Sprite EnvSlice(int index)
        {
            if (_envSlices == null)
            {
                _envSlices = new Dictionary<string, Sprite>();
                foreach (var s in Resources.LoadAll<Sprite>("Art/environment/environment-atlas"))
                    _envSlices[s.name] = s;
            }
            return _envSlices.TryGetValue("environment-atlas_" + index, out var slice) ? slice : null;
        }

        static Sprite Atlas(string key, int sliceIndex, Vector2 pivot)
        {
            key = "env_" + key;
            if (SpriteCache.TryGetValue(key, out var cached)) return cached;
            var slice = EnvSlice(sliceIndex);
            Sprite s = null;
            if (slice != null)
                s = Sprite.Create(slice.texture, slice.rect, pivot, Tuning.Visual.envArtPixelsPerUnit);
            SpriteCache[key] = s;
            return s;
        }

        public static Sprite EnvHills() => Atlas("hills", 0, new Vector2(0.5f, 0f));
        public static Sprite EnvMountains() => Atlas("mountains", 1, new Vector2(0.5f, 0f));
        public static Sprite EnvFlowers() => Atlas("flowers", 7, new Vector2(0.5f, 0f));

        /// <summary>Wide multi-lane road file when present, else the atlas road slice.</summary>
        public static Sprite EnvRoad()
        {
            var wide = StandaloneSprite("Art/environment/environment-road-wide", "env_roadwide",
                new Vector2(0.5f, 1f));
            return wide != null ? wide : Atlas("road", 11, new Vector2(0.5f, 1f));
        }

        /// <summary>Tileable grass ground fill; null until the asset exists (see ART_PROMPTS §8).</summary>
        public static Sprite EnvGrassFill() =>
            StandaloneSprite("Art/environment/environment-grass-fill", "env_grassfill", new Vector2(0.5f, 1f));

        /// <summary>Grass surface strip with tufts; null until the asset exists.</summary>
        public static Sprite EnvGrassStrip() =>
            StandaloneSprite("Art/environment/environment-grass-strip", "env_grassstrip", new Vector2(0.5f, 0f));

        /// <summary>Whole standalone texture as one sprite (FullRect so it supports tiled draw mode).</summary>
        static Sprite StandaloneSprite(string path, string key, Vector2 pivot)
        {
            if (SpriteCache.TryGetValue(key, out var cached)) return cached;
            var tex = Tex(path);
            Sprite s = null;
            if (tex != null)
                s = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), pivot,
                    Tuning.Visual.envArtPixelsPerUnit, 0, SpriteMeshType.FullRect);
            SpriteCache[key] = s;
            return s;
        }

        public static Sprite EnvTree(int variant)
        {
            switch (((variant % 3) + 3) % 3)
            {
                case 0: return Atlas("oak", 2, new Vector2(0.5f, 0f));
                case 1: return Atlas("poplar", 3, new Vector2(0.5f, 0f));
                default: return Atlas("pine", 4, new Vector2(0.5f, 0f));
            }
        }

        public static Sprite EnvBush(int variant)
        {
            return variant % 2 == 0
                ? Atlas("bush0", 5, new Vector2(0.5f, 0f))
                : Atlas("bush1", 6, new Vector2(0.5f, 0f));
        }

        public static Sprite EnvCloud(int variant)
        {
            switch (((variant % 3) + 3) % 3)
            {
                case 0: return Atlas("cloud0", 8, new Vector2(0.5f, 0.5f));
                case 1: return Atlas("cloud1", 9, new Vector2(0.5f, 0.5f));
                default: return Atlas("cloud2", 10, new Vector2(0.5f, 0.5f));
            }
        }

        // ---------- UI icons ----------

        public enum UiIcon
        {
            Skills = 0, Shop = 1, Bike = 2, Gear = 3, Coin = 4, Bolt = 5,
            SpeakerOn = 6, SpeakerOff = 7, Vibration = 8, Play = 9, Close = 10,
        }

        /// <summary>White UI icon (tint via Image.color), from authored slices; null if art missing.</summary>
        public static Sprite Icon(UiIcon icon) =>
            IconAt("Art/ui/ui-icons", (int)icon, "icon_" + (int)icon);

        /// <summary>Emote icon (index matches Emotes.All), from authored slices; null if art missing.</summary>
        public static Sprite Emote(int index) =>
            IconAt("Art/social/emotes", index, "emote_" + index);

        public enum SocialIcon { Team = 0, Gift = 1, Smiley = 2, Send = 3 }

        /// <summary>White social icon (tint via Image.color), from authored slices; null if art missing.</summary>
        public static Sprite Social(SocialIcon icon) =>
            IconAt("Art/social/social-icons", (int)icon, "social_" + (int)icon);

        /// <summary>Bike icon for a tier (garage/upgrade UI), from authored slices; null if art missing.</summary>
        public static Sprite BikeIcon(int tierIndex) =>
            IconAt("Art/ui/bike-upgrade-icons", tierIndex, "bikeicon_" + tierIndex);

        // ---------- helpers ----------

        /// <summary>Copy the texture with near-white pixels multiplied by the tint (jersey coloring).</summary>
        static Texture2D TintWhite(Texture2D src, Color32 tint)
        {
            if (tint.r > 240 && tint.g > 240 && tint.b > 240) return src; // white jersey: keep original
            Color32[] px;
            try
            {
                px = src.GetPixels32();
            }
            catch (UnityException)
            {
                return src; // texture not readable — show untinted art rather than nothing
            }
            for (int i = 0; i < px.Length; i++)
            {
                var p = px[i];
                if (p.a == 0) continue;
                int min = Mathf.Min(p.r, Mathf.Min(p.g, p.b));
                int max = Mathf.Max(p.r, Mathf.Max(p.g, p.b));
                if (min >= 180 && max - min <= 40)
                {
                    px[i] = new Color32(
                        (byte)(p.r * tint.r / 255),
                        (byte)(p.g * tint.g / 255),
                        (byte)(p.b * tint.b / 255),
                        p.a);
                }
            }
            var outTex = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
            outTex.SetPixels32(px);
            outTex.Apply();
            return outTex;
        }
    }
}
