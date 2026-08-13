using UnityEngine;

namespace IdleBike
{
    [CreateAssetMenu(fileName = "AnimationTuning", menuName = "IdleBike/Animation Tuning")]
    public class AnimationTuning : ScriptableObject
    {
        [Header("Rider")]
        [Tooltip("Pedal animation frames advanced per meter ridden")]
        public float pedalRate = 0.9f;
        public float bobAmplitude = 0.03f;
        public float bobBaseFrequency = 2f;
        public float bobFrequencyPerSpeed = 0.25f;

        [Header("Buff pickup")]
        public float buffBobFrequency = 4f;
        public float buffBobAmplitude = 0.12f;

        [Header("UI transitions")]
        public float panelOpenDuration = 0.16f;
        public float panelCloseDuration = 0.12f;
        public float panelStartScale = 0.94f;
        public float startFadeDuration = 0.6f;
        public float flashFadeDuration = 0.35f;
    }
}
