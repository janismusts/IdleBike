using UnityEngine;

namespace IdleBike
{
    /// <summary>
    /// Animated rider (bike + jersey tint + helmet overlay + optional trail).
    /// Uses generated art from Resources/Art when present, procedural pixel
    /// placeholders otherwise. Used for the player and NPCs.
    /// </summary>
    public class RiderVisual : MonoBehaviour
    {
        SpriteRenderer _sr;
        SpriteRenderer _helmetSr;
        SpriteRenderer _trailSr;

        Sprite[] _artFrames;     // null => procedural fallback
        Sprite[] _helmetFrames;
        Sprite[] _trailFrames;

        int _tierIndex;
        Color32 _jersey = new Color32(210, 60, 60, 255);
        Color32 _helmetTint = new Color32(235, 235, 235, 255);

        float _pedalPhase;
        float _bobPhase;
        int _lastFrame = -1;
        EmoteBubble _emote;
        int _sortingOrder;

        /// <summary>Speed used to drive the pedaling animation (m/s).</summary>
        public float AnimSpeed;

        /// <summary>Lane y on the road; the bob animation oscillates around this.</summary>
        public float BaseY;

        public void Init(int sortingOrder)
        {
            _sortingOrder = sortingOrder;
            transform.localScale = Vector3.one * Tuning.Visual.riderScale;
            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sortingOrder = sortingOrder;

            var v = Tuning.Visual;
            var helmetGo = new GameObject("Helmet");
            helmetGo.transform.SetParent(transform, false);
            helmetGo.transform.localPosition = new Vector3(v.helmetOffset.x, v.helmetOffset.y, 0f);
            _helmetSr = helmetGo.AddComponent<SpriteRenderer>();
            _helmetSr.sortingOrder = sortingOrder + 1;

            var trailGo = new GameObject("Trail");
            trailGo.transform.SetParent(transform, false);
            // trail pivot is bottom-right: anchor it a bit behind the bike's rear
            trailGo.transform.localPosition = new Vector3(v.trailOffset.x, v.trailOffset.y, 0f);
            trailGo.transform.localScale = Vector3.one * v.trailScale;
            _trailSr = trailGo.AddComponent<SpriteRenderer>();
            _trailSr.sortingOrder = sortingOrder - 1;

            _bobPhase = Random.value * 10f;
        }

        /// <summary>Show an emote speech bubble above this rider's head.</summary>
        public void ShowEmote(int emoteIndex)
        {
            if (_emote == null) _emote = EmoteBubble.Attach(transform, 200); // always above riders
            _emote.Show(emoteIndex);
        }

        /// <summary>Re-apply VisualTuning to this rider (dev live-tuning).</summary>
        public void ApplyTuning()
        {
            var v = Tuning.Visual;
            transform.localScale = Vector3.one * v.riderScale;
            if (_trailSr != null)
            {
                _trailSr.transform.localPosition = new Vector3(v.trailOffset.x, v.trailOffset.y, 0f);
                _trailSr.transform.localScale = Vector3.one * v.trailScale;
            }
            if (_emote != null)
                _emote.transform.localPosition = new Vector3(v.emoteBubbleOffset.x, v.emoteBubbleOffset.y, 0f);
            _lastFrame = -1; // force Refresh so the helmet re-places with new offset/scale
        }

        /// <summary>
        /// Set the helmet frame, scaled around the helmet's own center so helmetScale
        /// doesn't drag it away from the head (its pivot is the sheet-space ground point).
        /// </summary>
        void PlaceHelmet(Sprite s)
        {
            var v = Tuning.Visual;
            _helmetSr.sprite = s;
            float k = v.helmetScale;
            Vector2 centerLocal = (new Vector2(s.rect.width, s.rect.height) * 0.5f - s.pivot) / s.pixelsPerUnit;
            _helmetSr.transform.localPosition = new Vector3(
                v.helmetOffset.x + (1f - k) * centerLocal.x,
                v.helmetOffset.y + (1f - k) * centerLocal.y, 0f);
            _helmetSr.transform.localScale = Vector3.one * k;
        }

        public void ApplyLook(int tierIndex, Color32 jersey, CosmeticDef helmet, CosmeticDef trail)
        {
            _tierIndex = Mathf.Clamp(tierIndex, 0, BikeDefs.Tiers.Length - 1);
            _jersey = jersey;
            _helmetTint = helmet != null ? helmet.Color : new Color32(235, 235, 235, 255);

            _artFrames = ArtLibrary.RiderFrames8(_tierIndex, jersey);
            _helmetFrames = _artFrames != null && helmet != null && !string.IsNullOrEmpty(helmet.Style)
                ? ArtLibrary.HelmetFrames8(helmet.Style)
                : null;
            _trailFrames = trail != null && !string.IsNullOrEmpty(trail.Style)
                ? ArtLibrary.TrailFrames8(trail.Style)
                : null;

            _helmetSr.enabled = _helmetFrames != null;
            _helmetSr.color = (Color)_helmetTint;
            _trailSr.enabled = _trailFrames != null;

            _lastFrame = -1;
            Refresh();
        }

        void Update()
        {
            if (_sr == null) return;
            // tuning + dev toggles for cosmetic overlays
            if (_helmetSr != null)
                _helmetSr.enabled = _helmetFrames != null && Tuning.Visual.showHelmets && !DebugFlags.HideHelmets;
            if (_trailSr != null) _trailSr.enabled = _trailFrames != null && !DebugFlags.HideTrails;
            var a = Tuning.Anim;
            float spd = Mathf.Max(0f, AnimSpeed);
            _pedalPhase += spd * a.pedalRate * Time.deltaTime;
            _bobPhase += Time.deltaTime * (a.bobBaseFrequency + spd * a.bobFrequencyPerSpeed);
            var lp = transform.localPosition;
            transform.localPosition = new Vector3(lp.x, BaseY + Mathf.Sin(_bobPhase) * a.bobAmplitude, lp.z);

            // lower on the road renders in front
            int order = Lanes.SortOrder(BaseY, _sortingOrder);
            _sr.sortingOrder = order;
            if (_helmetSr != null) _helmetSr.sortingOrder = order + 1;
            if (_trailSr != null) _trailSr.sortingOrder = order - 1;

            Refresh();
        }

        void Refresh()
        {
            if (_sr == null) return;
            int frameCount = _artFrames != null ? _artFrames.Length : 4;
            int frame = Mathf.FloorToInt(_pedalPhase) % frameCount;
            if (frame < 0) frame += frameCount;
            if (frame == _lastFrame) return;
            _lastFrame = frame;

            if (_artFrames != null)
            {
                _sr.sprite = _artFrames[frame];
                if (_helmetFrames != null) PlaceHelmet(_helmetFrames[frame]);
                if (_trailFrames != null) _trailSr.sprite = _trailFrames[frame];
            }
            else
            {
                var tier = BikeDefs.Tiers[_tierIndex];
                _sr.sprite = PixelSprites.Rider(tier.Silhouette, tier.FrameColor, _jersey, _helmetTint, frame & 3);
                if (_trailFrames != null) _trailSr.sprite = _trailFrames[frame & 3];
            }
        }
    }
}
