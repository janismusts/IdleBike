using UnityEngine;

namespace IdleBike
{
    [CreateAssetMenu(fileName = "GameBalance", menuName = "IdleBike/Game Balance")]
    public class GameBalance : ScriptableObject
    {
        [Header("Speed model (m/s)")]
        public float baseSpeed = 2.0f;
        [Tooltip("+X% of base per bike level (before tier multiplier)")]
        public float speedPerLevel = 0.07f;
        [Tooltip("Speed fraction lost to headwind when not drafting")]
        [Range(0f, 0.9f)] public float dragPenalty = 0.28f;
        public float sprintMultiplier = 1.6f;
        public float acceleration = 3.5f;
        public float deceleration = 5.0f;

        [Header("Sprint energy")]
        public float sprintMax = 100f;
        public float sprintDrainPerSec = 22f;
        [Tooltip("Slow base refill — drafting is the fast way back")]
        public float sprintRegenPerSec = 4f;
        [Tooltip("Extra regen while drafting")]
        public float sprintRegenDraftBonus = 12f;
        [Tooltip("Can't start sprinting below this")]
        public float sprintMinToStart = 10f;

        [Header("Drafting")]
        public float draftWindowBase = 4f;
        [Tooltip("Draft window grows with speed (m per m/s)")]
        public float draftWindowPerSpeed = 0.25f;
        public float draftMinGap = 0.4f;

        [Header("Lanes (vertical position on the road)")]
        [Tooltip("Keep riders this far from the road edges")]
        public float laneMargin = 0.3f;
        [Tooltip("Max vertical distance to count as the same lane (draft, pickups)")]
        public float laneTolerance = 0.45f;
        [Tooltip("How fast the player moves vertically (units/s)")]
        public float laneMoveSpeed = 3f;
        [Tooltip("Drag steering sensitivity multiplier")]
        public float laneDragSensitivity = 1.3f;

        [Header("NPC riders")]
        public int npcMaxAlive = 6;
        public float npcSpawnMinGap = 10f;
        public float npcSpawnMaxGap = 60f;
        public float npcDespawnBehind = 70f;
        public float npcDespawnAhead = 160f;
        [Tooltip("NPC speed relative to the player's drag-reduced cruise speed (ahead spawns)")]
        public float npcSpeedMinFactor = 0.82f;
        public float npcSpeedMaxFactor = 1.12f;
        [Tooltip("Chance an NPC spawns behind the player and overtakes (gives draft cover)")]
        [Range(0f, 1f)] public float npcSpawnBehindChance = 0.35f;
        [Tooltip("Behind-spawn speed factors (must be >1 so they actually overtake)")]
        public float npcBehindSpeedMinFactor = 1.05f;
        public float npcBehindSpeedMaxFactor = 1.30f;
        [Tooltip("Seconds an NPC matches the player's speed while being drafted")]
        public float draftPaceSeconds = 8f;

        [Header("Speed buffs")]
        public float buffSpawnMinInterval = 18f;
        public float buffSpawnMaxInterval = 45f;
        public float buffSpawnAheadMin = 10f;
        public float buffSpawnAheadMax = 30f;
        public float buffMultiplier = 1.5f;
        public float buffDuration = 6f;

        [Header("Terrain (hills)")]
        [Tooltip("Length of one grade segment in meters")]
        public float gradeSegmentLength = 250f;
        [Range(0f, 1f)] public float uphillChance = 0.28f;
        [Range(0f, 1f)] public float downhillChance = 0.16f;
        [Tooltip("Max uphill grade (0.09 = 9%)")]
        public float gradeUphillMax = 0.09f;
        public float gradeDownhillMax = 0.06f;
        [Tooltip("How fast the visible grade blends between segments (grade units per second)")]
        public float gradeChangePerSec = 0.04f;
        [Tooltip("Speed penalty per grade unit uphill (grade 0.09 * 4 = 36% slower)")]
        public float uphillSlowFactor = 4f;
        public float uphillMinSpeedMult = 0.45f;
        [Tooltip("Speed bonus per grade unit downhill")]
        public float downhillBoostFactor = 2.5f;
        public float downhillMaxSpeedMult = 1.35f;

        [Header("Team / social")]
        [Tooltip("A teammate within this many meters counts as riding together")]
        public float teamTogetherRange = 10f;
        [Tooltip("Drag penalty reduction while the team rides together (0.5 = half drag)")]
        [Range(0f, 1f)] public float teamDragReduction = 0.5f;
        public int teamSize = 4;
        [Tooltip("How often a teammate sends a gift (hours, simulated locally for now)")]
        public float giftIntervalHours = 6f;
        [Tooltip("Coins gift is worth this many minutes of cruise income")]
        public float giftCoinsMinutes = 15f;
        public float giftBuffSeconds = 30f;
        public float sendGiftCooldownHours = 8f;
        public int giftInboxCap = 6;

        [Header("Emotes")]
        public float emoteDuration = 2.5f;
        public float npcEmoteMinInterval = 15f;
        public float npcEmoteMaxInterval = 45f;

        [Header("Economy")]
        public double coinsPerMeter = 0.4;
        [Tooltip("+X% coins per bike level")]
        public double coinsPerMeterPerLevel = 0.05;
        public double upgradeBaseCost = 15.0;
        public double upgradeCostGrowth = 1.32;

        [Header("Offline earnings")]
        [Tooltip("Fraction of cruise income earned while away")]
        public double offlineRateFactor = 0.5;
        public float offlineMaxHours = 8f;
        [Tooltip("Don't show the popup below this many seconds away")]
        public float offlineMinSeconds = 60f;
    }
}
