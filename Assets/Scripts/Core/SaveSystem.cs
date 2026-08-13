using System;
using System.IO;
using UnityEngine;

namespace IdleBike
{
    public static class SaveSystem
    {
        static string Path => System.IO.Path.Combine(Application.persistentDataPath, "idlebike_save.json");

        public static SaveData LoadOrCreate()
        {
            try
            {
                if (File.Exists(Path))
                {
                    var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(Path));
                    if (data != null) return Migrate(data);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[IdleBike] Failed to load save, starting fresh: {e.Message}");
            }
            var fresh = new SaveData { lastSaveUnix = NowUnix() };
            if (Tuning.Audio != null)
            {
                fresh.musicVolume = Tuning.Audio.defaultMusicVolume;
                fresh.sfxVolume = Tuning.Audio.defaultSfxVolume;
            }
            fresh.ownedCosmetics.Add("jersey_red");
            fresh.ownedCosmetics.Add("helmet_white");
            fresh.ownedCosmetics.Add("trail_none");
            return fresh;
        }

        static SaveData Migrate(SaveData data)
        {
            if (data.version < 2)
            {
                if (string.IsNullOrEmpty(data.equippedTrail)) data.equippedTrail = "trail_none";
                if (data.skillRanks == null) data.skillRanks = new System.Collections.Generic.List<SkillRankEntry>();
                if (!data.ownedCosmetics.Contains("trail_none")) data.ownedCosmetics.Add("trail_none");
                data.version = 2;
            }
            if (data.version < 3)
            {
                if (data.teamName == null) data.teamName = "";
                if (data.teamTag == null) data.teamTag = "";
                if (data.giftInbox == null) data.giftInbox = new System.Collections.Generic.List<GiftEntry>();
                data.version = 3;
            }
            return data;
        }

        public static void Save()
        {
            if (GameState.Data == null) return;
            GameState.Data.lastSaveUnix = NowUnix();
            try
            {
                File.WriteAllText(Path, JsonUtility.ToJson(GameState.Data));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[IdleBike] Failed to save: {e.Message}");
            }
        }

        public static void ResetProgress()
        {
            var old = GameState.Data;
            var fresh = new SaveData
            {
                lastSaveUnix = NowUnix(),
                musicVolume = old != null ? old.musicVolume : 0.6f,
                sfxVolume = old != null ? old.sfxVolume : 0.8f,
                vibration = old == null || old.vibration,
            };
            fresh.ownedCosmetics.Add("jersey_red");
            fresh.ownedCosmetics.Add("helmet_white");
            fresh.ownedCosmetics.Add("trail_none");
            GameState.Data = fresh;
            GameState.SprintEnergy = Tuning.Balance.sprintMax;
            GameState.BuffTimeLeft = 0f;
            GameState.CurrentSpeed = 0f;
            Save();
            GameState.NotifyProgressReset();
        }

        public static long NowUnix() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
