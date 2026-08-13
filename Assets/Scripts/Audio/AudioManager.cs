using UnityEngine;

namespace IdleBike
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager I { get; private set; }

        AudioSource _music;
        AudioSource _sfx;

        public void Build()
        {
            I = this;
            _music = gameObject.AddComponent<AudioSource>();
            _music.loop = true;
            _music.clip = ProceduralSfx.MusicLoop();
            _music.volume = GameState.Data.musicVolume;
            _music.Play();

            _sfx = gameObject.AddComponent<AudioSource>();
            _sfx.loop = false;
            _sfx.volume = GameState.Data.sfxVolume;
        }

        public float MusicVolume
        {
            get => GameState.Data.musicVolume;
            set
            {
                GameState.Data.musicVolume = Mathf.Clamp01(value);
                if (_music != null) _music.volume = GameState.Data.musicVolume;
            }
        }

        public float SfxVolume
        {
            get => GameState.Data.sfxVolume;
            set
            {
                GameState.Data.sfxVolume = Mathf.Clamp01(value);
                if (_sfx != null) _sfx.volume = GameState.Data.sfxVolume;
            }
        }

        public void Play(AudioClip clip)
        {
            if (_sfx != null && clip != null) _sfx.PlayOneShot(clip);
        }

        public void PlayClick() => Play(ProceduralSfx.Click());
        public void PlayCoin() => Play(ProceduralSfx.Coin());
        public void PlayUpgrade() => Play(ProceduralSfx.Upgrade());
        public void PlayWhoosh() => Play(ProceduralSfx.Whoosh());
        public void PlayBuff() => Play(ProceduralSfx.Buff());
        public void PlayError() => Play(ProceduralSfx.Error());
    }
}
