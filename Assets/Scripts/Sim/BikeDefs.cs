using UnityEngine;

namespace IdleBike
{
    public enum BikeSilhouette { Trike, SmallBike, CityBike, RoadBike }

    public class BikeTier
    {
        public string Name;
        public int UnlockLevel;
        public float SpeedMult;
        public BikeSilhouette Silhouette;
        public Color32 FrameColor;
        /// <summary>Riding sheet name under Resources/Art/riding (8 frames, 128x96).</summary>
        public string SheetName;

        public BikeTier(string name, int unlockLevel, float speedMult, BikeSilhouette s, Color32 frame, string sheet)
        {
            Name = name; UnlockLevel = unlockLevel; SpeedMult = speedMult; Silhouette = s; FrameColor = frame; SheetName = sheet;
        }
    }

    public static class BikeDefs
    {
        public static readonly BikeTier[] Tiers =
        {
            new BikeTier("Rusty Trike",     0,  1.0f,  BikeSilhouette.Trike,    new Color32(140,  90,  60, 255), "rusty-trike-ride"),
            new BikeTier("Kid's Bike",      5,  1.35f, BikeSilhouette.SmallBike,new Color32(200,  70,  70, 255), "kids-bike-ride"),
            new BikeTier("Old Clunker",    10,  1.8f,  BikeSilhouette.CityBike, new Color32(110, 110, 120, 255), "old-clunker-ride"),
            new BikeTier("BMX",            15,  2.35f, BikeSilhouette.SmallBike,new Color32( 60, 170,  75, 255), "bmx-ride"),
            new BikeTier("Mountain Bike",  20,  3.0f,  BikeSilhouette.CityBike, new Color32( 50, 100, 200, 255), "mountain-bike-ride"),
            new BikeTier("City Cruiser",   25,  3.8f,  BikeSilhouette.CityBike, new Color32(230, 180,  60, 255), "city-cruiser-ride"),
            new BikeTier("Road Bike",      30,  4.9f,  BikeSilhouette.RoadBike, new Color32(220,  60, 140, 255), "road-bike-ride"),
            new BikeTier("Gravel Racer",   35,  6.3f,  BikeSilhouette.RoadBike, new Color32( 90, 200, 210, 255), "gravel-racer-ride"),
            new BikeTier("Track Bike",     40,  8.0f,  BikeSilhouette.RoadBike, new Color32(240, 240, 240, 255), "track-bike-ride"),
            new BikeTier("Aero Superbike", 45, 10.2f,  BikeSilhouette.RoadBike, new Color32( 30,  30,  40, 255), "aero-superbike-ride"),
        };

        public static int TierIndexForLevel(int level)
        {
            int best = 0;
            for (int i = 0; i < Tiers.Length; i++)
                if (level >= Tiers[i].UnlockLevel) best = i;
            return best;
        }

        public static BikeTier TierForLevel(int level) => Tiers[TierIndexForLevel(level)];

        public static BikeTier NextTier(int level)
        {
            for (int i = 0; i < Tiers.Length; i++)
                if (Tiers[i].UnlockLevel > level) return Tiers[i];
            return null;
        }

        /// <summary>Cruise speed (m/s) at a bike level, before drag/sprint/buffs/terrain.</summary>
        public static float CruiseSpeed(int level)
        {
            var b = Tuning.Balance;
            return b.baseSpeed * (1f + b.speedPerLevel * level) * TierForLevel(level).SpeedMult;
        }

        public static double UpgradeCost(int level)
        {
            var b = Tuning.Balance;
            return System.Math.Round(b.upgradeBaseCost * System.Math.Pow(b.upgradeCostGrowth, level));
        }

        public static double CoinsPerMeter(int level)
        {
            var b = Tuning.Balance;
            return b.coinsPerMeter * (1.0 + b.coinsPerMeterPerLevel * level);
        }
    }
}
