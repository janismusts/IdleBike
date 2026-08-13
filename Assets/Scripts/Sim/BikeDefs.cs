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

        public BikeTier(string name, int unlockLevel, float speedMult, BikeSilhouette s, Color32 frame)
        {
            Name = name; UnlockLevel = unlockLevel; SpeedMult = speedMult; Silhouette = s; FrameColor = frame;
        }
    }

    public static class BikeDefs
    {
        public static readonly BikeTier[] Tiers =
        {
            new BikeTier("Rusty Trike",     0,  1.0f,  BikeSilhouette.Trike,    new Color32(140,  90,  60, 255)),
            new BikeTier("Kid's Bike",      5,  1.35f, BikeSilhouette.SmallBike,new Color32(200,  70,  70, 255)),
            new BikeTier("Old Clunker",    10,  1.8f,  BikeSilhouette.CityBike, new Color32(110, 110, 120, 255)),
            new BikeTier("BMX",            15,  2.35f, BikeSilhouette.SmallBike,new Color32( 60, 170,  75, 255)),
            new BikeTier("Mountain Bike",  20,  3.0f,  BikeSilhouette.CityBike, new Color32( 50, 100, 200, 255)),
            new BikeTier("City Cruiser",   25,  3.8f,  BikeSilhouette.CityBike, new Color32(230, 180,  60, 255)),
            new BikeTier("Road Bike",      30,  4.9f,  BikeSilhouette.RoadBike, new Color32(220,  60, 140, 255)),
            new BikeTier("Gravel Racer",   35,  6.3f,  BikeSilhouette.RoadBike, new Color32( 90, 200, 210, 255)),
            new BikeTier("Track Bike",     40,  8.0f,  BikeSilhouette.RoadBike, new Color32(240, 240, 240, 255)),
            new BikeTier("Aero Superbike", 45, 10.2f,  BikeSilhouette.RoadBike, new Color32( 30,  30,  40, 255)),
        };

        public static BikeTier TierForLevel(int level)
        {
            BikeTier best = Tiers[0];
            for (int i = 0; i < Tiers.Length; i++)
                if (level >= Tiers[i].UnlockLevel) best = Tiers[i];
            return best;
        }

        public static BikeTier NextTier(int level)
        {
            for (int i = 0; i < Tiers.Length; i++)
                if (Tiers[i].UnlockLevel > level) return Tiers[i];
            return null;
        }

        /// <summary>Cruise speed (m/s) at a bike level, before drag/sprint/buffs.</summary>
        public static float CruiseSpeed(int level)
        {
            return GameConfig.BaseSpeed * (1f + GameConfig.SpeedPerLevel * level) * TierForLevel(level).SpeedMult;
        }

        public static double UpgradeCost(int level)
        {
            return System.Math.Round(GameConfig.UpgradeBaseCost * System.Math.Pow(GameConfig.UpgradeCostGrowth, level));
        }

        public static double CoinsPerMeter(int level)
        {
            return GameConfig.CoinsPerMeter * (1.0 + GameConfig.CoinsPerMeterPerLevel * level);
        }
    }
}
