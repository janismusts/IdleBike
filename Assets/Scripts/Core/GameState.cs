using System;
using System.Collections.Generic;

namespace IdleBike
{
    [Serializable]
    public class SkillRankEntry
    {
        public string id;
        public int rank;
    }

    [Serializable]
    public class SaveData
    {
        public int version = 2;
        public double coins;
        public int bikeLevel;
        public double totalDistance;           // meters
        public float musicVolume = 0.6f;
        public float sfxVolume = 0.8f;
        public bool vibration = true;
        public List<string> ownedCosmetics = new List<string>();
        public string equippedJersey = "jersey_red";
        public string equippedHelmet = "helmet_white";
        public string equippedTrail = "trail_none";
        public List<SkillRankEntry> skillRanks = new List<SkillRankEntry>();
        public long lastSaveUnix;
    }

    /// <summary>Global runtime state. Created by GameBootstrap, saved via SaveSystem.</summary>
    public static class GameState
    {
        public static SaveData Data;

        // Transient (not saved)
        public static float CurrentSpeed;
        public static float SprintEnergy = 100f; // set from Tuning.Balance.sprintMax on boot
        public static bool SprintHeld;
        public static bool IsSprinting;
        public static bool IsDrafting;
        public static float BuffTimeLeft;

        public static event Action CoinsChanged;
        public static event Action BikeLevelChanged;
        public static event Action CosmeticsChanged;
        public static event Action ProgressReset;

        public static void AddCoins(double amount)
        {
            Data.coins += amount;
            CoinsChanged?.Invoke();
        }

        public static bool SpendCoins(double amount)
        {
            if (Data.coins < amount) return false;
            Data.coins -= amount;
            CoinsChanged?.Invoke();
            return true;
        }

        public static void NotifyBikeLevelChanged() => BikeLevelChanged?.Invoke();
        public static void NotifyCosmeticsChanged() => CosmeticsChanged?.Invoke();
        public static void NotifyProgressReset() => ProgressReset?.Invoke();
    }
}
