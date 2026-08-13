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
        const int RiderFrameW = 128, RiderFrameH = 96, RiderFrames = 8;
        const int TrailFrameW = 96, TrailFrameH = 48;
        const int UiIconSize = 32, BikeIconSize = 64;

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

        // ---------- riders ----------

        /// <summary>8 riding frames for a bike tier with the jersey tinted; null if art missing.</summary>
        public static Sprite[] RiderFrames8(int tierIndex, Color32 jersey)
        {
            if (tierIndex < 0 || tierIndex >= BikeDefs.Tiers.Length) return null;
            string sheet = BikeDefs.Tiers[tierIndex].SheetName;
            string key = $"rider_{sheet}_{jersey.r}_{jersey.g}_{jersey.b}";
            if (FrameCache.TryGetValue(key, out var cached)) return cached;

            var tex = Tex("Art/riding/" + sheet);
            if (tex == null) { FrameCache[key] = null; return null; }

            var tinted = TintWhite(tex, jersey);
            var frames = SliceRow(tinted, RiderFrameW, RiderFrameH, RiderFrames,
                new Vector2(0.5f, 0f), Tuning.Visual.riderArtPixelsPerUnit);
            FrameCache[key] = frames;
            return frames;
        }

        /// <summary>8 helmet overlay frames (white base — tint via SpriteRenderer.color).</summary>
        public static Sprite[] HelmetFrames8(string style)
        {
            string key = "helmet_" + style;
            if (FrameCache.TryGetValue(key, out var cached)) return cached;
            var tex = Tex("Art/cosmetics/helmet-" + style);
            var frames = tex == null ? null : SliceRow(tex, RiderFrameW, RiderFrameH, RiderFrames,
                new Vector2(0.5f, 0f), Tuning.Visual.riderArtPixelsPerUnit);
            FrameCache[key] = frames;
            return frames;
        }

        /// <summary>8 trail effect frames (pivot at bottom-right so it hangs behind the rear wheel).</summary>
        public static Sprite[] TrailFrames8(string trail)
        {
            string key = "trail_" + trail;
            if (FrameCache.TryGetValue(key, out var cached)) return cached;
            var tex = Tex("Art/cosmetics/trail-" + trail);
            var frames = tex == null ? null : SliceRow(tex, TrailFrameW, TrailFrameH, RiderFrames,
                new Vector2(1f, 0f), Tuning.Visual.riderArtPixelsPerUnit);
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
        public static Sprite EnvRoad() => Atlas("road", 11, new Vector2(0.5f, 1f));
        public static Sprite EnvFlowers() => Atlas("flowers", 7, new Vector2(0.5f, 0f));

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

        /// <summary>32x32 white UI icon (tint via Image.color); null if art missing.</summary>
        public static Sprite Icon(UiIcon icon)
        {
            string key = "icon_" + (int)icon;
            if (SpriteCache.TryGetValue(key, out var cached)) return cached;
            var tex = Tex("Art/ui/ui-icons");
            Sprite s = null;
            if (tex != null && tex.width >= ((int)icon + 1) * UiIconSize)
                s = Sprite.Create(tex, new Rect((int)icon * UiIconSize, 0, UiIconSize, UiIconSize),
                    new Vector2(0.5f, 0.5f), UiIconSize);
            SpriteCache[key] = s;
            return s;
        }

        /// <summary>32x32 emote icon (index matches Emotes.All); null if art missing.</summary>
        public static Sprite Emote(int index)
        {
            string key = "emote_" + index;
            if (SpriteCache.TryGetValue(key, out var cached)) return cached;
            var tex = Tex("Art/social/emotes");
            Sprite s = null;
            if (tex != null && tex.width >= (index + 1) * UiIconSize)
                s = Sprite.Create(tex, new Rect(index * UiIconSize, 0, UiIconSize, UiIconSize),
                    new Vector2(0.5f, 0.5f), UiIconSize);
            SpriteCache[key] = s;
            return s;
        }

        public enum SocialIcon { Team = 0, Gift = 1, Smiley = 2, Send = 3 }

        /// <summary>32x32 white social icon (tint via Image.color); null if art missing.</summary>
        public static Sprite Social(SocialIcon icon)
        {
            string key = "social_" + (int)icon;
            if (SpriteCache.TryGetValue(key, out var cached)) return cached;
            var tex = Tex("Art/social/social-icons");
            Sprite s = null;
            if (tex != null && tex.width >= ((int)icon + 1) * UiIconSize)
                s = Sprite.Create(tex, new Rect((int)icon * UiIconSize, 0, UiIconSize, UiIconSize),
                    new Vector2(0.5f, 0.5f), UiIconSize);
            SpriteCache[key] = s;
            return s;
        }

        /// <summary>64x64 bike icon for a tier (garage/upgrade UI); null if art missing.</summary>
        public static Sprite BikeIcon(int tierIndex)
        {
            string key = "bikeicon_" + tierIndex;
            if (SpriteCache.TryGetValue(key, out var cached)) return cached;
            var tex = Tex("Art/ui/bike-upgrade-icons");
            Sprite s = null;
            if (tex != null && tex.width >= (tierIndex + 1) * BikeIconSize)
                s = Sprite.Create(tex, new Rect(tierIndex * BikeIconSize, 0, BikeIconSize, BikeIconSize),
                    new Vector2(0.5f, 0.5f), BikeIconSize);
            SpriteCache[key] = s;
            return s;
        }

        // ---------- helpers ----------

        static Sprite[] SliceRow(Texture2D tex, int frameW, int frameH, int count, Vector2 pivot, float ppu)
        {
            int available = Mathf.Min(count, tex.width / frameW);
            if (available <= 0) return null;
            var frames = new Sprite[count];
            for (int i = 0; i < count; i++)
            {
                int idx = Mathf.Min(i, available - 1);
                frames[i] = Sprite.Create(tex, new Rect(idx * frameW, 0, frameW, frameH), pivot, ppu);
            }
            return frames;
        }

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
