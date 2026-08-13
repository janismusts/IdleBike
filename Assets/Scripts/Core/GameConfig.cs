namespace IdleBike
{
    /// <summary>Central tuning values. One unit == one meter.</summary>
    public static class GameConfig
    {
        public const string Version = "0.1.0";

        // Speed model
        public const float BaseSpeed = 2.0f;            // m/s at level 0 (rusty trike)
        public const float SpeedPerLevel = 0.07f;       // +7% of base per level (before tier mult)
        public const float DragPenalty = 0.28f;         // speed fraction lost to headwind when not drafting
        public const float SprintMultiplier = 1.6f;
        public const float Acceleration = 3.5f;         // m/s^2 toward target speed
        public const float Deceleration = 5.0f;

        // Sprint energy
        public const float SprintMax = 100f;
        public const float SprintDrainPerSec = 22f;
        public const float SprintRegenPerSec = 8f;
        public const float SprintRegenDraftBonus = 10f; // extra regen while drafting
        public const float SprintMinToStart = 10f;      // can't start sprinting below this

        // Drafting
        public const float DraftWindowBase = 4f;        // meters behind an NPC
        public const float DraftWindowPerSpeed = 0.25f; // window grows with speed
        public const float DraftMinGap = 0.4f;

        // NPCs
        public const int NpcMaxAlive = 6;
        public const float NpcSpawnMinGap = 25f;        // meters ahead of player
        public const float NpcSpawnMaxGap = 90f;
        public const float NpcDespawnBehind = 70f;
        public const float NpcDespawnAhead = 160f;
        public const float NpcSpeedMinFactor = 0.82f;   // relative to player cruise speed
        public const float NpcSpeedMaxFactor = 1.12f;

        // Speed buffs
        public const float BuffSpawnMinInterval = 18f;
        public const float BuffSpawnMaxInterval = 45f;
        public const float BuffSpawnAheadMin = 20f;
        public const float BuffSpawnAheadMax = 45f;
        public const float BuffMultiplier = 1.5f;
        public const float BuffDuration = 6f;

        // Economy
        public const double CoinsPerMeter = 0.4;
        public const double CoinsPerMeterPerLevel = 0.05; // +5% per bike level
        public const double UpgradeBaseCost = 15.0;
        public const double UpgradeCostGrowth = 1.32;

        // Offline earnings
        public const double OfflineRateFactor = 0.5;    // fraction of cruise income while away
        public const double OfflineMaxSeconds = 8 * 3600;
        public const double OfflineMinSeconds = 60;     // don't show popup below this

        // Camera
        public const float CamMinSize = 3.6f;
        public const float CamMaxSize = 12f;
        public const float CamSizePerSpeed = 0.11f;
        public const float CamSmoothTime = 0.6f;

        // Misc
        public const float AutosaveInterval = 5f;
        public const float MetersPerKm = 1000f;
    }
}
