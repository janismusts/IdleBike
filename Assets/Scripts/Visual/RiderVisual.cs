using UnityEngine;

namespace IdleBike
{
    /// <summary>Animated rider sprite (pedaling frames + bob). Used for the player and NPCs.</summary>
    public class RiderVisual : MonoBehaviour
    {
        public BikeSilhouette Silhouette = BikeSilhouette.Trike;
        public Color32 FrameColor = new Color32(140, 90, 60, 255);
        public Color32 Jersey = new Color32(210, 60, 60, 255);
        public Color32 Helmet = new Color32(235, 235, 235, 255);

        SpriteRenderer _sr;
        float _pedalPhase;
        float _bobPhase;
        int _lastFrame = -1;

        /// <summary>Speed used to drive the pedaling animation (m/s).</summary>
        public float AnimSpeed;

        public void Init(int sortingOrder)
        {
            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sortingOrder = sortingOrder;
            _bobPhase = Random.value * 10f;
            Refresh();
        }

        public void ApplyLook(BikeSilhouette sil, Color32 frame, Color32 jersey, Color32 helmet)
        {
            Silhouette = sil; FrameColor = frame; Jersey = jersey; Helmet = helmet;
            _lastFrame = -1;
            Refresh();
        }

        void Update()
        {
            if (_sr == null) return;
            var a = Tuning.Anim;
            float spd = Mathf.Max(0f, AnimSpeed);
            _pedalPhase += spd * a.pedalRate * Time.deltaTime;
            _bobPhase += Time.deltaTime * (a.bobBaseFrequency + spd * a.bobFrequencyPerSpeed);
            var lp = transform.localPosition;
            transform.localPosition = new Vector3(lp.x, Mathf.Sin(_bobPhase) * a.bobAmplitude, lp.z);
            Refresh();
        }

        void Refresh()
        {
            if (_sr == null) return;
            int frame = Mathf.FloorToInt(_pedalPhase) & 3;
            if (frame == _lastFrame) return;
            _lastFrame = frame;
            _sr.sprite = PixelSprites.Rider(Silhouette, FrameColor, Jersey, Helmet, frame);
        }
    }
}
