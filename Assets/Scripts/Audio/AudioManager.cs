using System.Collections.Generic;
using UnityEngine;

namespace IdleBike
{
    /// <summary>
    /// Plays generated audio from Resources/Art/Sound when present, synthesized
    /// placeholders (ProceduralSfx) otherwise.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager I { get; private set; }

        const string FxPath = "Art/Sound/FX/";
        const string MusicPath = "Art/Sound/Music/";

        AudioSource _music;
        AudioSource _sfx;
        readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();

        public void Build()
        {
            I = this;
            _music = gameObject.AddComponent<AudioSource>();
            _music.loop = true;
            var musicClip = Resources.Load<AudioClip>(MusicPath + "Main Ride Loop");
            _music.clip = musicClip != null ? musicClip : ProceduralSfx.MusicLoop();
            _music.volume = GameState.Data.musicVolume * Tuning.Audio.musicGain;
            _music.Play();

            _sfx = gameObject.AddComponent<AudioSource>();
            _sfx.loop = false;
            _sfx.volume = GameState.Data.sfxVolume * Tuning.Audio.sfxGain;
        }

        public float MusicVolume
        {
            get => GameState.Data.musicVolume;
            set
            {
                GameState.Data.musicVolume = Mathf.Clamp01(value);
                if (_music != null) _music.volume = GameState.Data.musicVolume * Tuning.Audio.musicGain;
            }
        }

        public float SfxVolume
        {
            get => GameState.Data.sfxVolume;
            set
            {
                GameState.Data.sfxVolume = Mathf.Clamp01(value);
                if (_sfx != null) _sfx.volume = GameState.Data.sfxVolume * Tuning.Audio.sfxGain;
            }
        }

        AudioClip Clip(string name, System.Func<AudioClip> fallback)
        {
            if (_clips.TryGetValue(name, out var c)) return c;
            c = Resources.Load<AudioClip>(FxPath + name);
            if (c == null) c = fallback();
            _clips[name] = c;
            return c;
        }

        void PlayClip(string name, System.Func<AudioClip> fallback)
        {
            if (_sfx == null) return;
            var clip = Clip(name, fallback);
            if (clip != null) _sfx.PlayOneShot(clip);
        }

        public void PlayClick() => PlayClip("ui_click", ProceduralSfx.Click);
        public void PlayCoin() => PlayClip("coin_pickup", ProceduralSfx.Coin);
        public void PlayUpgrade() => PlayClip("upgrade_buy", ProceduralSfx.Upgrade);
        public void PlayFanfare() => PlayClip("new_bike_fanfare", ProceduralSfx.Upgrade);
        public void PlayWhoosh() => PlayClip("sprint_start", ProceduralSfx.Whoosh);
        public void PlaySprintEmpty() => PlayClip("sprint_empty", ProceduralSfx.Error);
        public void PlayBuff() => PlayClip("buff_pickup", ProceduralSfx.Buff);
        public void PlayBuffEnd() => PlayClip("buff_end", ProceduralSfx.Click);
        public void PlayDraftEnter() => PlayClip("draft_enter", ProceduralSfx.Whoosh);
        public void PlayOfflineCollect() => PlayClip("offline_collect", ProceduralSfx.Coin);
        public void PlayError() => PlayClip("error_denied", ProceduralSfx.Error);
        public void PlayEmotePop() => PlayClip("emote_pop", ProceduralSfx.Click);
        public void PlayGiftReceive() => PlayClip("gift_receive", ProceduralSfx.Coin);
        public void PlayGiftSend() => PlayClip("gift_send", ProceduralSfx.Click);
        public void PlayTeamJoin() => PlayClip("team_join", ProceduralSfx.Upgrade);
    }
}
