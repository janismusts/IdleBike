using UnityEngine;

namespace IdleBike
{
    /// <summary>
    /// Access point for tuning ScriptableObjects. Assets live in Assets/Resources/Tuning;
    /// missing assets fall back to code defaults so the game always runs.
    /// </summary>
    public static class Tuning
    {
        public static GameBalance Balance { get; private set; }
        public static VisualTuning Visual { get; private set; }
        public static AnimationTuning Anim { get; private set; }
        public static AudioTuning Audio { get; private set; }

        public static void Load()
        {
            Balance = LoadOrDefault<GameBalance>("Tuning/GameBalance");
            Visual = LoadOrDefault<VisualTuning>("Tuning/VisualTuning");
            Anim = LoadOrDefault<AnimationTuning>("Tuning/AnimationTuning");
            Audio = LoadOrDefault<AudioTuning>("Tuning/AudioTuning");
        }

        static T LoadOrDefault<T>(string path) where T : ScriptableObject
        {
            var so = Resources.Load<T>(path);
            if (so == null)
            {
                Debug.LogWarning($"[IdleBike] Tuning asset missing at Resources/{path} — using code defaults.");
                so = ScriptableObject.CreateInstance<T>();
            }
            return so;
        }
    }
}
