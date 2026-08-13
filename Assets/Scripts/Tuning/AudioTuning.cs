using UnityEngine;

namespace IdleBike
{
    [CreateAssetMenu(fileName = "AudioTuning", menuName = "IdleBike/Audio Tuning")]
    public class AudioTuning : ScriptableObject
    {
        [Header("Defaults for fresh saves")]
        [Range(0f, 1f)] public float defaultMusicVolume = 0.6f;
        [Range(0f, 1f)] public float defaultSfxVolume = 0.8f;

        [Header("Master gains (multiplied with user volume)")]
        [Range(0f, 2f)] public float musicGain = 1f;
        [Range(0f, 2f)] public float sfxGain = 1f;
    }
}
