using System.Collections.Generic;
using UnityEngine;

namespace IdleBike
{
    /// <summary>
    /// NPC riders on the road. They give draft cover. Later, real players at a similar
    /// total distance will appear through the same visual path (server-driven).
    /// </summary>
    public class NpcManager : MonoBehaviour
    {
        class Npc
        {
            public double Dist;      // absolute track distance, meters
            public float Speed;      // m/s
            public RiderVisual Visual;
        }

        readonly List<Npc> _npcs = new List<Npc>();
        double _nextSpawnAt;

        static readonly string[] JerseyPool = { "jersey_red", "jersey_blue", "jersey_green", "jersey_gold", "jersey_night" };
        static readonly string[] HelmetPool = { "helmet_white", "helmet_red", "helmet_aero" };

        public void Build()
        {
            var b = Tuning.Balance;
            _nextSpawnAt = GameState.Data.totalDistance + Random.Range(b.npcSpawnMinGap, b.npcSpawnMaxGap);
        }

        public void Tick(float dt)
        {
            var b = Tuning.Balance;
            double playerDist = GameState.Data.totalDistance;
            float cruise = BikeDefs.CruiseSpeed(GameState.Data.bikeLevel);

            // spawn ahead
            if (_npcs.Count < b.npcMaxAlive && playerDist >= _nextSpawnAt)
            {
                Spawn(playerDist + Random.Range(b.npcSpawnMinGap, b.npcSpawnMaxGap), cruise);
                _nextSpawnAt = playerDist + Random.Range(b.npcSpawnMinGap, b.npcSpawnMaxGap);
            }

            // move + despawn
            bool drafting = false;
            float draftWindow = b.draftWindowBase + GameState.CurrentSpeed * b.draftWindowPerSpeed;
            for (int i = _npcs.Count - 1; i >= 0; i--)
            {
                var n = _npcs[i];
                n.Dist += n.Speed * dt;
                float rel = (float)(n.Dist - playerDist);
                if (rel < -b.npcDespawnBehind || rel > b.npcDespawnAhead)
                {
                    Destroy(n.Visual.gameObject);
                    _npcs.RemoveAt(i);
                    continue;
                }
                // keep y — RiderVisual's bob animation owns it
                var lp = n.Visual.transform.localPosition;
                n.Visual.transform.localPosition = new Vector3(rel, lp.y, 0f);
                n.Visual.AnimSpeed = n.Speed;
                if (rel > b.draftMinGap && rel <= draftWindow) drafting = true;
            }
            GameState.IsDrafting = drafting;
        }

        void Spawn(double atDist, float cruise)
        {
            var b = Tuning.Balance;
            var go = new GameObject("NpcRider");
            go.transform.SetParent(transform, false);
            var vis = go.AddComponent<RiderVisual>();
            vis.Init(4); // behind the player (player sorts at 5)

            int tierIdx = Random.Range(0, BikeDefs.Tiers.Length);
            var tier = BikeDefs.Tiers[tierIdx];
            var jersey = Cosmetics.Get(JerseyPool[Random.Range(0, JerseyPool.Length)]);
            var helmet = Cosmetics.Get(HelmetPool[Random.Range(0, HelmetPool.Length)]);
            vis.ApplyLook(tier.Silhouette, tier.FrameColor, jersey.Color, helmet.Color);

            _npcs.Add(new Npc
            {
                Dist = atDist,
                Speed = cruise * Random.Range(b.npcSpeedMinFactor, b.npcSpeedMaxFactor),
                Visual = vis,
            });
        }

        /// <summary>Refresh NPC speeds after a bike upgrade so they stay relevant.</summary>
        public void OnPlayerLevelChanged()
        {
            var b = Tuning.Balance;
            float cruise = BikeDefs.CruiseSpeed(GameState.Data.bikeLevel);
            foreach (var n in _npcs)
                n.Speed = cruise * Random.Range(b.npcSpeedMinFactor, b.npcSpeedMaxFactor);
        }

        public void Clear()
        {
            var b = Tuning.Balance;
            foreach (var n in _npcs) Destroy(n.Visual.gameObject);
            _npcs.Clear();
            _nextSpawnAt = GameState.Data.totalDistance + Random.Range(b.npcSpawnMinGap, b.npcSpawnMaxGap);
        }
    }
}
