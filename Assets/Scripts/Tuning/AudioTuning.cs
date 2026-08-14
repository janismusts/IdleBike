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

        [Header("Per-sound volumes (on top of SFX volume and gain)")]
        [Range(0f, 2f)] public float uiClick = 1f;
        [Range(0f, 2f)] public float coinPickup = 1f;
        [Range(0f, 2f)] public float upgradeBuy = 1f;
        [Range(0f, 2f)] public float newBikeFanfare = 1f;
        [Range(0f, 2f)] public float sprintStart = 1f;
        [Range(0f, 2f)] public float sprintEmpty = 1f;
        [Range(0f, 2f)] public float buffPickup = 1f;
        [Range(0f, 2f)] public float buffEnd = 1f;
        [Range(0f, 2f)] public float draftEnter = 1f;
        [Range(0f, 2f)] public float offlineCollect = 1f;
        [Range(0f, 2f)] public float errorDenied = 1f;
        [Range(0f, 2f)] public float emotePop = 1f;
        [Range(0f, 2f)] public float giftReceive = 1f;
        [Range(0f, 2f)] public float giftSend = 1f;
        [Range(0f, 2f)] public float teamJoin = 1f;
    }
}
