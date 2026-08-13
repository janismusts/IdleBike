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

        public void Build()
        {
            _nextSpawnAt = GameState.Data.totalDistance + Random.Range(GameConfig.NpcSpawnMinGap, GameConfig.NpcSpawnMaxGap);
        }

        public void Tick(float dt)
        {
            double playerDist = GameState.Data.totalDistance;
            float cruise = BikeDefs.CruiseSpeed(GameState.Data.bikeLevel);

            // spawn ahead
            if (_npcs.Count < GameConfig.NpcMaxAlive && playerDist >= _nextSpawnAt)
            {
                Spawn(playerDist + Random.Range(GameConfig.NpcSpawnMinGap, GameConfig.NpcSpawnMaxGap), cruise);
                _nextSpawnAt = playerDist + Random.Range(GameConfig.NpcSpawnMinGap, GameConfig.NpcSpawnMaxGap);
            }

            // move + despawn
            bool drafting = false;
            float draftWindow = GameConfig.DraftWindowBase + GameState.CurrentSpeed * GameConfig.DraftWindowPerSpeed;
            for (int i = _npcs.Count - 1; i >= 0; i--)
            {
                var n = _npcs[i];
                n.Dist += n.Speed * dt;
                float rel = (float)(n.Dist - playerDist);
                if (rel < -GameConfig.NpcDespawnBehind || rel > GameConfig.NpcDespawnAhead)
                {
                    Destroy(n.Visual.gameObject);
                    _npcs.RemoveAt(i);
                    continue;
                }
                n.Visual.transform.localPosition = new Vector3(rel, 0f, 0f);
                n.Visual.AnimSpeed = n.Speed;
                if (rel > GameConfig.DraftMinGap && rel <= draftWindow) drafting = true;
            }
            GameState.IsDrafting = drafting;
        }

        void Spawn(double atDist, float cruise)
        {
            var go = new GameObject("NpcRider");
            go.transform.SetParent(transform, false);
            var vis = go.AddComponent<RiderVisual>();
            vis.Init(4); // behind the player (player sorts at 5)

            int tierIdx = Random.Range(0, BikeDefs.Tiers.Length);
            var tier = BikeDefs.Tiers[tierIdx];
            var jersey = Cosmetics.Get(JerseyPool[Random.Range(0, JerseyPool.Length)]);
            var helmets = new[] { "helmet_white", "helmet_red", "helmet_aero" };
            var helmet = Cosmetics.Get(helmets[Random.Range(0, helmets.Length)]);
            vis.ApplyLook(tier.Silhouette, tier.FrameColor, jersey.Color, helmet.Color);

            _npcs.Add(new Npc
            {
                Dist = atDist,
                Speed = cruise * Random.Range(GameConfig.NpcSpeedMinFactor, GameConfig.NpcSpeedMaxFactor),
                Visual = vis,
            });
        }

        /// <summary>Refresh NPC speeds after a bike upgrade so they stay relevant.</summary>
        public void OnPlayerLevelChanged()
        {
            float cruise = BikeDefs.CruiseSpeed(GameState.Data.bikeLevel);
            foreach (var n in _npcs)
                n.Speed = cruise * Random.Range(GameConfig.NpcSpeedMinFactor, GameConfig.NpcSpeedMaxFactor);
        }

        public void Clear()
        {
            foreach (var n in _npcs) Destroy(n.Visual.gameObject);
            _npcs.Clear();
            _nextSpawnAt = GameState.Data.totalDistance + Random.Range(GameConfig.NpcSpawnMinGap, GameConfig.NpcSpawnMaxGap);
        }
    }
}
