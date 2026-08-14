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

        void PlayClip(string name, System.Func<AudioClip> fallback, float perSoundVolume)
        {
            if (_sfx == null) return;
            var clip = Clip(name, fallback);
            if (clip != null) _sfx.PlayOneShot(clip, Mathf.Clamp(perSoundVolume, 0f, 2f));
        }

        // per-sound volumes read live from AudioTuning on every play
        public void PlayClick() => PlayClip("ui_click", ProceduralSfx.Click, Tuning.Audio.uiClick);
        public void PlayCoin() => PlayClip("coin_pickup", ProceduralSfx.Coin, Tuning.Audio.coinPickup);
        public void PlayUpgrade() => PlayClip("upgrade_buy", ProceduralSfx.Upgrade, Tuning.Audio.upgradeBuy);
        public void PlayFanfare() => PlayClip("new_bike_fanfare", ProceduralSfx.Upgrade, Tuning.Audio.newBikeFanfare);
        public void PlayWhoosh() => PlayClip("sprint_start", ProceduralSfx.Whoosh, Tuning.Audio.sprintStart);
        public void PlaySprintEmpty() => PlayClip("sprint_empty", ProceduralSfx.Error, Tuning.Audio.sprintEmpty);
        public void PlayBuff() => PlayClip("buff_pickup", ProceduralSfx.Buff, Tuning.Audio.buffPickup);
        public void PlayBuffEnd() => PlayClip("buff_end", ProceduralSfx.Click, Tuning.Audio.buffEnd);
        public void PlayDraftEnter() => PlayClip("draft_enter", ProceduralSfx.Whoosh, Tuning.Audio.draftEnter);
        public void PlayOfflineCollect() => PlayClip("offline_collect", ProceduralSfx.Coin, Tuning.Audio.offlineCollect);
        public void PlayError() => PlayClip("error_denied", ProceduralSfx.Error, Tuning.Audio.errorDenied);
        public void PlayEmotePop() => PlayClip("emote_pop", ProceduralSfx.Click, Tuning.Audio.emotePop);
        public void PlayGiftReceive() => PlayClip("gift_receive", ProceduralSfx.Coin, Tuning.Audio.giftReceive);
        public void PlayGiftSend() => PlayClip("gift_send", ProceduralSfx.Click, Tuning.Audio.giftSend);
        public void PlayTeamJoin() => PlayClip("team_join", ProceduralSfx.Upgrade, Tuning.Audio.teamJoin);
    }
}
