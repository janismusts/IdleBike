using System.Collections.Generic;
using UnityEngine;

namespace IdleBike
{
    /// <summary>Tiny pixel canvas for building placeholder pixel-art sprites in code.</summary>
    public class PixelCanvas
    {
        public readonly int W, H;
        readonly Color32[] _px;

        public PixelCanvas(int w, int h)
        {
            W = w; H = h;
            _px = new Color32[w * h];
            var clear = new Color32(0, 0, 0, 0);
            for (int i = 0; i < _px.Length; i++) _px[i] = clear;
        }

        public void Set(int x, int y, Color32 c)
        {
            if (x < 0 || y < 0 || x >= W || y >= H) return;
            _px[y * W + x] = c;
        }

        public void Rect(int x, int y, int w, int h, Color32 c)
        {
            for (int i = x; i < x + w; i++)
                for (int j = y; j < y + h; j++)
                    Set(i, j, c);
        }

        public void Disc(int cx, int cy, int r, Color32 c)
        {
            for (int x = cx - r; x <= cx + r; x++)
                for (int y = cy - r; y <= cy + r; y++)
                    if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r)
                        Set(x, y, c);
        }

        public void Circle(int cx, int cy, int r, Color32 c)
        {
            for (int x = cx - r; x <= cx + r; x++)
                for (int y = cy - r; y <= cy + r; y++)
                {
                    int d = (x - cx) * (x - cx) + (y - cy) * (y - cy);
                    if (d <= r * r && d > (r - 1) * (r - 1))
                        Set(x, y, c);
                }
        }

        public void Line(int x0, int y0, int x1, int y1, Color32 c)
        {
            int dx = Mathf.Abs(x1 - x0), dy = -Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;
            while (true)
            {
                Set(x0, y0, c);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }

        public Sprite ToSprite(float ppu, Vector2 pivot)
        {
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
            tex.SetPixels32(_px);
            tex.Apply();
            return Sprite.Create(tex, new UnityEngine.Rect(0, 0, W, H), pivot, ppu);
        }
    }

    /// <summary>
    /// Procedural placeholder pixel-art. Real assets come from Codex prompts in
    /// docs/ART_PROMPTS.md and will replace these generators.
    /// </summary>
    public static class PixelSprites
    {
        public const float PPU = 12f;

        static readonly Color32 Tire = new Color32(28, 28, 34, 255);
        static readonly Color32 Hub = new Color32(150, 150, 160, 255);
        static readonly Color32 Skin = new Color32(235, 188, 150, 255);
        static readonly Color32 Pants = new Color32(48, 48, 68, 255);
        static readonly Color32 Shoe = new Color32(20, 20, 24, 255);
        static readonly Color32 Dark = new Color32(35, 32, 38, 255);

        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        // ---------- Rider ----------

        public static Sprite Rider(BikeSilhouette sil, Color32 frame, Color32 jersey, Color32 helmet, int animFrame)
        {
            string key = $"rider_{sil}_{frame.r}_{frame.g}_{frame.b}_{jersey.r}_{jersey.g}_{jersey.b}_{helmet.r}_{helmet.g}_{helmet.b}_{animFrame}";
            if (Cache.TryGetValue(key, out var s)) return s;

            var c = new PixelCanvas(28, 22);
            DrawRider(c, sil, frame, jersey, helmet, animFrame);
            s = c.ToSprite(PPU, new Vector2(0.5f, 0f));
            Cache[key] = s;
            return s;
        }

        static void DrawRider(PixelCanvas c, BikeSilhouette sil, Color32 frame, Color32 jersey, Color32 helmet, int animFrame)
        {
            int wheelR;
            int rearX, frontX, wheelY;
            int seatX, seatY;      // saddle
            int barX, barY;        // handlebar grip
            int headX, headY;      // head center
            bool leanForward = false;

            switch (sil)
            {
                case BikeSilhouette.Trike:
                    wheelR = 2; rearX = 8; frontX = 18; wheelY = 2;
                    seatX = 10; seatY = 7; barX = 17; barY = 8;
                    headX = 11; headY = 14;
                    break;
                case BikeSilhouette.SmallBike:
                    wheelR = 3; rearX = 7; frontX = 19; wheelY = 3;
                    seatX = 10; seatY = 9; barX = 18; barY = 10;
                    headX = 12; headY = 16;
                    break;
                case BikeSilhouette.RoadBike:
                    wheelR = 4; rearX = 6; frontX = 20; wheelY = 4;
                    seatX = 10; seatY = 11; barX = 19; barY = 10;
                    headX = 15; headY = 16; leanForward = true;
                    break;
                default: // CityBike
                    wheelR = 4; rearX = 6; frontX = 20; wheelY = 4;
                    seatX = 10; seatY = 11; barX = 19; barY = 12;
                    headX = 12; headY = 18;
                    break;
            }

            int crankX = (rearX + frontX) / 2, crankY = wheelY;

            // Wheels
            c.Circle(rearX, wheelY, wheelR, Tire);
            c.Circle(frontX, wheelY, wheelR, Tire);
            c.Set(rearX, wheelY, Hub);
            c.Set(frontX, wheelY, Hub);
            if (sil == BikeSilhouette.Trike)
            {
                // second rear wheel hint (slightly offset, darker)
                c.Circle(rearX - 1, wheelY, wheelR, new Color32(60, 60, 70, 255));
            }

            // Frame
            c.Line(rearX, wheelY, seatX, seatY, frame);           // seat stay
            c.Line(rearX, wheelY, crankX, crankY + 1, frame);     // chain stay
            c.Line(crankX, crankY + 1, seatX, seatY, frame);      // seat tube
            c.Line(crankX, crankY + 1, barX, barY - 1, frame);    // down tube
            c.Line(seatX, seatY, barX - 1, barY - 1, frame);      // top tube
            c.Line(frontX, wheelY, barX, barY, frame);            // fork
            c.Rect(seatX - 1, seatY + 1, 3, 1, Dark);             // saddle
            c.Rect(barX, barY, 2, 1, Dark);                       // handlebar

            // Pedals: crank rotates with animFrame (4 frames)
            float ang = animFrame * Mathf.PI * 0.5f;
            int pedalR = 2;
            int fx = crankX + Mathf.RoundToInt(Mathf.Cos(ang) * pedalR);
            int fy = crankY + 1 + Mathf.RoundToInt(Mathf.Sin(ang) * pedalR);
            int bx = crankX - Mathf.RoundToInt(Mathf.Cos(ang) * pedalR);
            int by = crankY + 1 - Mathf.RoundToInt(Mathf.Sin(ang) * pedalR);

            // Legs (hip at saddle): thigh to knee, shin to pedal
            int hipX = seatX, hipY = seatY;
            DrawLeg(c, hipX, hipY, bx, by, false); // far leg first (behind)
            DrawLeg(c, hipX, hipY, fx, fy, true);  // near leg

            // Torso
            int shoulderX = leanForward ? headX - 1 : headX - 1;
            int shoulderY = headY - 2;
            c.Line(hipX, hipY + 1, shoulderX, shoulderY, jersey);
            c.Line(hipX + 1, hipY + 1, shoulderX + 1, shoulderY, jersey);
            // Arm to handlebar
            c.Line(shoulderX + 1, shoulderY, barX, barY + 1, jersey);
            c.Set(barX, barY + 1, Skin); // hand

            // Head + helmet
            c.Disc(headX, headY, 2, Skin);
            c.Rect(headX - 2, headY + 1, 5, 2, helmet);
            c.Set(headX + 2, headY + 2, helmet); // visor tip

            void DrawLeg(PixelCanvas cv, int hx, int hy, int footX, int footY, bool near)
            {
                var col = near ? Pants : new Color32((byte)(Pants.r / 2), (byte)(Pants.g / 2), (byte)(Pants.b / 2), 255);
                int kneeX = (hx + footX) / 2 + 1;
                int kneeY = (hy + footY) / 2 + 1;
                cv.Line(hx, hy, kneeX, kneeY, col);
                cv.Line(kneeX, kneeY, footX, footY, col);
                cv.Set(footX, footY, Shoe);
            }
        }

        // ---------- Environment ----------

        public static Sprite RoadTile()
        {
            return Cached("road", () =>
            {
                var c = new PixelCanvas(32, 12);
                var asphalt = new Color32(70, 70, 78, 255);
                var asphalt2 = new Color32(64, 64, 72, 255);
                var edge = new Color32(120, 120, 128, 255);
                for (int x = 0; x < 32; x++)
                    for (int y = 0; y < 12; y++)
                        c.Set(x, y, ((x * 7 + y * 13) % 11 == 0) ? asphalt2 : asphalt);
                c.Rect(0, 11, 32, 1, edge);
                // lane dash
                for (int x = 0; x < 32; x += 8) c.Rect(x, 6, 4, 1, new Color32(200, 200, 190, 255));
                return c.ToSprite(PPU, new Vector2(0.5f, 1f)); // pivot at top (road surface)
            });
        }

        public static Sprite Ground()
        {
            return Cached("ground", () =>
            {
                var c = new PixelCanvas(4, 4);
                c.Rect(0, 0, 4, 4, new Color32(52, 46, 44, 255));
                return c.ToSprite(4f, new Vector2(0.5f, 1f));
            });
        }

        public static Sprite Tree(int variant)
        {
            return Cached("tree" + variant, () =>
            {
                var c = new PixelCanvas(14, 18);
                var trunk = new Color32(96, 66, 44, 255);
                var leaf = variant == 0 ? new Color32(58, 132, 66, 255)
                         : variant == 1 ? new Color32(46, 110, 58, 255)
                                        : new Color32(80, 150, 70, 255);
                var leafD = new Color32((byte)(leaf.r - 18), (byte)(leaf.g - 20), (byte)(leaf.b - 12), 255);
                c.Rect(6, 0, 2, 6, trunk);
                c.Disc(7, 10, 5, leafD);
                c.Disc(5, 12, 4, leaf);
                c.Disc(9, 12, 4, leaf);
                c.Disc(7, 14, 3, leaf);
                return c.ToSprite(PPU, new Vector2(0.5f, 0f));
            });
        }

        public static Sprite Bush()
        {
            return Cached("bush", () =>
            {
                var c = new PixelCanvas(10, 6);
                var g = new Color32(66, 140, 72, 255);
                c.Disc(3, 2, 2, g);
                c.Disc(6, 2, 2, g);
                c.Disc(5, 3, 2, new Color32(80, 158, 84, 255));
                return c.ToSprite(PPU, new Vector2(0.5f, 0f));
            });
        }

        public static Sprite Hill()
        {
            return Cached("hill", () =>
            {
                var c = new PixelCanvas(64, 26);
                var g = new Color32(96, 148, 96, 255);
                for (int x = 0; x < 64; x++)
                {
                    float t = (x - 32f) / 32f;
                    int h = Mathf.RoundToInt(24f * (1f - t * t));
                    for (int y = 0; y < h; y++) c.Set(x, y, g);
                }
                return c.ToSprite(PPU, new Vector2(0.5f, 0f));
            });
        }

        public static Sprite Mountain()
        {
            return Cached("mountain", () =>
            {
                var c = new PixelCanvas(80, 36);
                var m = new Color32(130, 140, 165, 255);
                var snow = new Color32(230, 235, 245, 255);
                for (int x = 0; x < 80; x++)
                {
                    int h = 34 - Mathf.Abs(x - 40) * 34 / 42;
                    if (h < 0) h = 0;
                    for (int y = 0; y < h; y++)
                        c.Set(x, y, y > 26 ? snow : m);
                }
                return c.ToSprite(PPU, new Vector2(0.5f, 0f));
            });
        }

        public static Sprite Cloud(int variant)
        {
            return Cached("cloud" + variant, () =>
            {
                var c = new PixelCanvas(18, 8);
                var w = new Color32(250, 250, 252, 235);
                c.Disc(5, 3, 3, w);
                c.Disc(9, 4, 3 + variant % 2, w);
                c.Disc(13, 3, 2, w);
                c.Rect(3, 1, 12, 3, w);
                return c.ToSprite(PPU, new Vector2(0.5f, 0.5f));
            });
        }

        public static Sprite Coin()
        {
            return Cached("coin", () =>
            {
                var c = new PixelCanvas(10, 10);
                var gold = new Color32(240, 195, 60, 255);
                var rim = new Color32(190, 140, 35, 255);
                c.Disc(4, 5, 4, rim);
                c.Disc(4, 5, 3, gold);
                c.Set(3, 6, new Color32(255, 235, 160, 255));
                return c.ToSprite(PPU, new Vector2(0.5f, 0.5f));
            });
        }

        public static Sprite BuffBolt()
        {
            return Cached("bolt", () =>
            {
                var c = new PixelCanvas(10, 14);
                var y = new Color32(255, 220, 60, 255);
                var o = new Color32(255, 160, 40, 255);
                c.Line(6, 13, 3, 7, y);
                c.Line(7, 13, 4, 7, y);
                c.Line(3, 7, 6, 7, o);
                c.Line(6, 7, 2, 0, y);
                c.Line(7, 7, 3, 0, y);
                return c.ToSprite(PPU, new Vector2(0.5f, 0f));
            });
        }

        public static Sprite KmPost()
        {
            return Cached("kmpost", () =>
            {
                var c = new PixelCanvas(4, 10);
                c.Rect(1, 0, 2, 8, new Color32(200, 200, 200, 255));
                c.Rect(0, 7, 4, 3, new Color32(230, 80, 80, 255));
                return c.ToSprite(PPU, new Vector2(0.5f, 0f));
            });
        }

        public static Sprite White()
        {
            return Cached("white", () =>
            {
                var c = new PixelCanvas(4, 4);
                c.Rect(0, 0, 4, 4, new Color32(255, 255, 255, 255));
                return c.ToSprite(4f, new Vector2(0.5f, 0.5f));
            });
        }

        // ---------- UI icons (white, tint via Image.color) ----------

        public static Sprite IconBike()
        {
            return Cached("icon_bike", () =>
            {
                var c = new PixelCanvas(16, 16);
                var w = new Color32(255, 255, 255, 255);
                c.Circle(4, 5, 3, w);
                c.Circle(12, 5, 3, w);
                c.Line(4, 5, 8, 10, w);
                c.Line(8, 10, 12, 5, w);
                c.Line(8, 10, 8, 5, w);
                c.Line(4, 5, 8, 5, w);
                c.Rect(7, 11, 3, 1, w);
                return c.ToSprite(16f, new Vector2(0.5f, 0.5f));
            });
        }

        public static Sprite IconSkills()
        {
            return Cached("icon_skills", () =>
            {
                var c = new PixelCanvas(16, 16);
                var w = new Color32(255, 255, 255, 255);
                c.Line(8, 1, 8, 6, w);
                c.Line(8, 6, 3, 10, w);
                c.Line(8, 6, 13, 10, w);
                c.Line(8, 6, 8, 11, w);
                c.Disc(3, 12, 2, w);
                c.Disc(8, 13, 2, w);
                c.Disc(13, 12, 2, w);
                return c.ToSprite(16f, new Vector2(0.5f, 0.5f));
            });
        }

        public static Sprite IconShop()
        {
            return Cached("icon_shop", () =>
            {
                var c = new PixelCanvas(16, 16);
                var w = new Color32(255, 255, 255, 255);
                c.Line(2, 12, 4, 12, w);
                c.Line(4, 12, 6, 5, w);
                c.Line(6, 5, 14, 5, w);
                c.Line(14, 5, 12, 10, w);
                c.Line(12, 10, 5, 10, w);
                c.Disc(6, 2, 1, w);
                c.Disc(12, 2, 1, w);
                return c.ToSprite(16f, new Vector2(0.5f, 0.5f));
            });
        }

        public static Sprite IconGear()
        {
            return Cached("icon_gear", () =>
            {
                var c = new PixelCanvas(16, 16);
                var w = new Color32(255, 255, 255, 255);
                c.Disc(8, 8, 5, w);
                c.Disc(8, 8, 2, new Color32(0, 0, 0, 0));
                c.Rect(7, 0, 3, 3, w);
                c.Rect(7, 13, 3, 3, w);
                c.Rect(0, 7, 3, 3, w);
                c.Rect(13, 7, 3, 3, w);
                c.Rect(2, 2, 2, 2, w);
                c.Rect(12, 2, 2, 2, w);
                c.Rect(2, 12, 2, 2, w);
                c.Rect(12, 12, 2, 2, w);
                return c.ToSprite(16f, new Vector2(0.5f, 0.5f));
            });
        }

        // ---------- Social: speech bubble + emote placeholders ----------

        public static Sprite SpeechBubble()
        {
            return Cached("bubble", () =>
            {
                var c = new PixelCanvas(30, 26);
                var fill = new Color32(250, 250, 252, 255);
                var line = new Color32(40, 40, 50, 255);
                c.Rect(2, 8, 26, 14, fill);
                c.Rect(3, 7, 24, 16, fill);
                c.Rect(4, 6, 22, 18, fill);
                // outline
                c.Line(3, 22, 26, 22, line); c.Line(3, 7, 26, 7, line);
                c.Line(2, 8, 2, 21, line); c.Line(27, 8, 27, 21, line);
                c.Set(3, 21, line); c.Set(26, 21, line); c.Set(3, 8, line); c.Set(26, 8, line);
                // tail (bottom-left, pointing down at the rider)
                c.Line(8, 7, 8, 2, line);
                c.Line(12, 7, 9, 2, line);
                c.Rect(9, 4, 2, 3, fill);
                c.Set(9, 3, fill);
                return c.ToSprite(24f, new Vector2(0.3f, 0f));
            });
        }

        /// <summary>Placeholder pixel emote (index matches Emotes.All). Real art via prompts.</summary>
        public static Sprite Emote(int index)
        {
            return Cached("emote" + index, () =>
            {
                var c = new PixelCanvas(14, 14);
                var skin = new Color32(235, 188, 150, 255);
                var yellow = new Color32(250, 210, 60, 255);
                var dark = new Color32(45, 40, 45, 255);
                switch (((index % 12) + 12) % 12)
                {
                    case 0: // wave — hand
                        c.Rect(4, 2, 6, 7, skin);
                        for (int f = 0; f < 4; f++) c.Rect(3 + f * 2, 9, 1, 3 + (f % 2), skin);
                        c.Rect(9, 8, 2, 3, skin);
                        break;
                    case 1: // thumbs up
                        c.Rect(4, 2, 7, 6, skin);
                        c.Rect(5, 8, 2, 4, skin);
                        c.Rect(7, 10, 2, 2, skin);
                        break;
                    case 2: // heart
                        c.Disc(4, 9, 2, new Color32(220, 60, 70, 255));
                        c.Disc(9, 9, 2, new Color32(220, 60, 70, 255));
                        for (int y = 8; y >= 2; y--) c.Rect(2 + (8 - y), y, 10 - 2 * (8 - y), 1, new Color32(220, 60, 70, 255));
                        break;
                    case 3: // laugh
                        c.Disc(7, 7, 6, yellow);
                        c.Line(4, 9, 5, 10, dark); c.Line(9, 10, 10, 9, dark);
                        c.Rect(4, 4, 6, 2, dark);
                        c.Rect(5, 3, 4, 1, new Color32(240, 120, 120, 255));
                        break;
                    case 4: // angry
                        c.Disc(7, 7, 6, new Color32(230, 90, 70, 255));
                        c.Line(3, 10, 6, 8, dark); c.Line(8, 8, 11, 10, dark);
                        c.Rect(5, 3, 5, 1, dark);
                        break;
                    case 5: // sweat
                        c.Disc(7, 6, 5, yellow);
                        c.Set(4, 7, dark); c.Set(9, 7, dark);
                        c.Rect(5, 4, 4, 1, dark);
                        c.Disc(12, 10, 1, new Color32(90, 160, 240, 255));
                        c.Set(12, 12, new Color32(90, 160, 240, 255));
                        break;
                    case 6: // turtle
                        c.Disc(7, 6, 4, new Color32(70, 140, 70, 255));   // shell
                        c.Disc(7, 7, 3, new Color32(50, 110, 55, 255));
                        c.Rect(11, 5, 2, 2, new Color32(120, 190, 100, 255)); // head
                        c.Rect(3, 3, 2, 1, new Color32(120, 190, 100, 255));  // tail
                        c.Rect(5, 2, 1, 1, new Color32(120, 190, 100, 255));
                        c.Rect(9, 2, 1, 1, new Color32(120, 190, 100, 255));
                        break;
                    case 7: // rocket
                        c.Rect(6, 4, 3, 7, new Color32(200, 205, 215, 255));
                        c.Set(7, 12, new Color32(220, 70, 60, 255));
                        c.Rect(6, 11, 3, 1, new Color32(220, 70, 60, 255));
                        c.Rect(5, 4, 1, 2, new Color32(220, 70, 60, 255));
                        c.Rect(9, 4, 1, 2, new Color32(220, 70, 60, 255));
                        c.Rect(6, 1, 3, 2, new Color32(255, 170, 60, 255));
                        c.Set(7, 0, new Color32(255, 220, 90, 255));
                        break;
                    case 8: // muscle — flexed arm
                        c.Rect(2, 3, 5, 3, skin);
                        c.Disc(8, 7, 3, skin);
                        c.Rect(8, 9, 3, 3, skin);
                        break;
                    case 9: // zzz
                        DrawZ(c, 1, 1, 4, new Color32(120, 160, 240, 255));
                        DrawZ(c, 5, 5, 4, new Color32(140, 175, 245, 255));
                        DrawZ(c, 9, 9, 4, new Color32(165, 195, 250, 255));
                        break;
                    case 10: // trophy
                        var gold = new Color32(240, 195, 60, 255);
                        c.Rect(4, 7, 7, 5, gold);
                        c.Rect(3, 10, 9, 2, gold);
                        c.Set(2, 10, gold); c.Set(12, 10, gold);
                        c.Rect(6, 4, 3, 3, gold);
                        c.Rect(4, 2, 7, 2, new Color32(190, 140, 35, 255));
                        break;
                    default: // fire
                        c.Disc(7, 4, 4, new Color32(255, 140, 40, 255));
                        c.Disc(7, 4, 2, new Color32(255, 220, 90, 255));
                        c.Line(7, 8, 5, 12, new Color32(255, 140, 40, 255));
                        c.Line(8, 8, 9, 11, new Color32(255, 170, 60, 255));
                        break;
                }
                return c.ToSprite(14f, new Vector2(0.5f, 0.5f));
            });

            void DrawZ(PixelCanvas cv, int x, int y, int size, Color32 col)
            {
                cv.Rect(x, y + size - 1, size, 1, col);
                cv.Rect(x, y, size, 1, col);
                cv.Line(x + size - 1, y + size - 1, x, y, col);
            }
        }

        public static Sprite IconTeam()
        {
            return Cached("icon_team", () =>
            {
                var c = new PixelCanvas(16, 16);
                var w = new Color32(255, 255, 255, 255);
                c.Disc(5, 10, 2, w);
                c.Disc(11, 10, 2, w);
                c.Rect(2, 3, 6, 5, w);
                c.Rect(8, 3, 6, 5, w);
                c.Rect(7, 2, 2, 4, new Color32(0, 0, 0, 0));
                return c.ToSprite(16f, new Vector2(0.5f, 0.5f));
            });
        }

        public static Sprite IconGift()
        {
            return Cached("icon_gift", () =>
            {
                var c = new PixelCanvas(16, 16);
                var w = new Color32(255, 255, 255, 255);
                c.Rect(3, 2, 10, 7, w);
                c.Rect(2, 9, 12, 3, w);
                c.Rect(7, 2, 2, 10, new Color32(0, 0, 0, 0));
                c.Rect(7, 2, 2, 10, new Color32(255, 255, 255, 90));
                c.Disc(5, 13, 2, w);
                c.Disc(11, 13, 2, w);
                c.Disc(5, 13, 1, new Color32(0, 0, 0, 0));
                c.Disc(11, 13, 1, new Color32(0, 0, 0, 0));
                return c.ToSprite(16f, new Vector2(0.5f, 0.5f));
            });
        }

        public static Sprite IconSmiley()
        {
            return Cached("icon_smiley", () =>
            {
                var c = new PixelCanvas(16, 16);
                var w = new Color32(255, 255, 255, 255);
                c.Circle(8, 8, 6, w);
                c.Set(6, 10, w); c.Set(10, 10, w);
                c.Line(5, 6, 7, 5, w);
                c.Line(7, 5, 9, 5, w);
                c.Line(9, 5, 11, 6, w);
                return c.ToSprite(16f, new Vector2(0.5f, 0.5f));
            });
        }

        static Sprite Cached(string key, System.Func<Sprite> make)
        {
            if (Cache.TryGetValue(key, out var s)) return s;
            s = make();
            Cache[key] = s;
            return s;
        }
    }
}
