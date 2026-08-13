using UnityEngine;

namespace IdleBike
{
    /// <summary>
    /// Synthesized placeholder audio. Real assets come from prompts in docs/SOUND_PROMPTS.md.
    /// </summary>
    public static class ProceduralSfx
    {
        const int Rate = 44100;

        public static AudioClip Click() => Cached(ref _click, "click", () =>
            Tone(0.06f, t => Square(1200f, t) * Env(t, 0.06f, 8f) * 0.4f));
        static AudioClip _click;

        public static AudioClip Coin() => Cached(ref _coin, "coin", () =>
            Tone(0.18f, t =>
            {
                float f = t < 0.07f ? 988f : 1319f; // B5 -> E6
                return Square(f, t) * Env(t, 0.18f, 6f) * 0.30f;
            }));
        static AudioClip _coin;

        public static AudioClip Upgrade() => Cached(ref _upgrade, "upgrade", () =>
            Tone(0.5f, t =>
            {
                // rising arpeggio C E G C
                float[] fs = { 523f, 659f, 784f, 1047f };
                int idx = Mathf.Min(3, (int)(t / 0.11f));
                return (Square(fs[idx], t) * 0.6f + Sine(fs[idx] * 0.5f, t) * 0.4f) * Env(t % 0.11f, 0.11f, 4f) * 0.35f;
            }));
        static AudioClip _upgrade;

        public static AudioClip Whoosh() => Cached(ref _whoosh, "whoosh", () =>
            Tone(0.35f, t =>
            {
                float n = (Mathf.PerlinNoise(t * 90f, 0.37f) - 0.5f) * 2f;
                float sweep = Mathf.Sin(Mathf.PI * t / 0.35f);
                return n * sweep * 0.5f;
            }));
        static AudioClip _whoosh;

        public static AudioClip Buff() => Cached(ref _buff, "buff", () =>
            Tone(0.4f, t =>
            {
                float f = 660f + 900f * (t / 0.4f);
                return (Sine(f, t) + Sine(f * 1.5f, t) * 0.5f) * Env(t, 0.4f, 5f) * 0.3f;
            }));
        static AudioClip _buff;

        public static AudioClip Error() => Cached(ref _error, "error", () =>
            Tone(0.2f, t => Square(160f, t) * Env(t, 0.2f, 6f) * 0.3f));
        static AudioClip _error;

        /// <summary>Gentle 8-bar chiptune-ish pad loop, seamless.</summary>
        public static AudioClip MusicLoop() => Cached(ref _music, "music", () =>
        {
            const float bpm = 88f;
            const float beat = 60f / bpm;
            float len = beat * 32f; // 8 bars of 4
            int samples = Mathf.RoundToInt(len * Rate);

            // chord roots (Hz): C  Am  F  G  (x2)
            float[][] chords =
            {
                new[] { 261.6f, 329.6f, 392.0f },
                new[] { 220.0f, 261.6f, 329.6f },
                new[] { 174.6f, 220.0f, 261.6f },
                new[] { 196.0f, 246.9f, 293.7f },
            };

            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / Rate;
                int bar = (int)(t / (beat * 4f)) % 8;
                float[] chord = chords[bar % 4];

                // soft pad
                float pad = 0f;
                for (int v = 0; v < chord.Length; v++)
                    pad += Mathf.Sin(2f * Mathf.PI * chord[v] * 0.5f * t) / chord.Length;

                // sparse arp on top
                float arpStep = beat * 0.5f;
                int step = (int)(t / arpStep);
                float noteT = t - step * arpStep;
                float arpF = chord[step % chord.Length] * 2f;
                float arp = Mathf.Sin(2f * Mathf.PI * arpF * t) * Mathf.Exp(-noteT * 7f) * 0.35f;

                data[i] = (pad * 0.35f + arp) * 0.5f;
            }

            // short crossfade at the loop point to avoid a click
            int fade = Rate / 20;
            for (int i = 0; i < fade; i++)
            {
                float w = (float)i / fade;
                data[i] = data[i] * w + data[samples - fade + i] * (1f - w);
            }

            var clip = AudioClip.Create("music", samples, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        });
        static AudioClip _music;

        // ---------- helpers ----------

        static AudioClip Cached(ref AudioClip slot, string name, System.Func<AudioClip> make)
        {
            if (slot == null) slot = make();
            return slot;
        }

        static AudioClip Tone(float dur, System.Func<float, float> gen)
        {
            int samples = Mathf.RoundToInt(dur * Rate);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
                data[i] = Mathf.Clamp(gen((float)i / Rate), -1f, 1f);
            var clip = AudioClip.Create("sfx", samples, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static float Sine(float f, float t) => Mathf.Sin(2f * Mathf.PI * f * t);
        static float Square(float f, float t) => Mathf.Sign(Mathf.Sin(2f * Mathf.PI * f * t)) * 0.7f;
        static float Env(float t, float dur, float k) => Mathf.Exp(-t * k) * Mathf.Clamp01((dur - t) / dur * 10f);
    }
}
