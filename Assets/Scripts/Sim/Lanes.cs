using UnityEngine;

namespace IdleBike
{
    /// <summary>
    /// Vertical positioning on the wide road. y=0 is the road's top edge; riders live
    /// between the margins. Lower on screen renders in front (bigger sorting order).
    /// </summary>
    public static class Lanes
    {
        public static float RoadHeight
        {
            get
            {
                var road = ArtLibrary.EnvRoad();
                float h = road != null ? road.bounds.size.y : 1f;
                return h * Mathf.Max(0.1f, Tuning.Visual.roadScale);
            }
        }

        public static float MinY => -RoadHeight + Tuning.Balance.laneMargin;
        public static float MaxY => -Tuning.Balance.laneMargin;
        public static float MidY => (MinY + MaxY) * 0.5f;

        public static float Clamp(float y) => Mathf.Clamp(y, MinY, MaxY);

        public static float RandomLane() => Random.Range(MinY, MaxY);

        /// <summary>Are two riders close enough vertically to interact (draft, pickups)?</summary>
        public static bool SameLane(float a, float b) =>
            Mathf.Abs(a - b) <= Tuning.Balance.laneTolerance;

        /// <summary>Sorting order from lane y — lower on the road draws in front.</summary>
        public static int SortOrder(float laneY, int offset = 0) =>
            10 + Mathf.RoundToInt(-laneY * 20f) + offset;
    }
}
