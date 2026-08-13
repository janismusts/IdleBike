using System.Collections.Generic;
using UnityEngine;

namespace IdleBike
{
    /// <summary>
    /// NPC riders on the road. They give draft cover. Later, real players at a similar
    /// total distance will appear through the same visual path (server-driven).
    /// NPC speeds are relative to the player's drag-reduced cruise speed so that slower
    /// ones drift back into draft range and behind-spawns overtake the player.
    /// While drafted, an NPC paces the player for a while so the draft state is stable.
    /// </summary>
    public class NpcManager : MonoBehaviour
    {
        class Npc
        {
            public double Dist;       // absolute track distance, meters
            public float Speed;       // m/s
            public float PaceLeft;    // seconds of "match player speed" left while drafted
            public RiderVisual Visual;
        }

        readonly List<Npc> _npcs = new List<Npc>();
        double _nextSpawnAt;

        static readonly string[] JerseyPool = { "jersey_red", "jersey_blue", "jersey_green", "jersey_gold", "jersey_night" };
        static readonly string[] HelmetPool = { "helmet_white", "helmet_red", "helmet_aero" };

        static float EffectiveCruise()
        {
            return BikeDefs.CruiseSpeed(GameState.Data.bikeLevel) * (1f - Tuning.Balance.dragPenalty);
        }

        public void Build()
        {
            var b = Tuning.Balance;
            _nextSpawnAt = GameState.Data.totalDistance + Random.Range(b.npcSpawnMinGap, b.npcSpawnMaxGap);
        }

        public void Tick(float dt)
        {
            var b = Tuning.Balance;
            double playerDist = GameState.Data.totalDistance;

            // spawn
            if (_npcs.Count < b.npcMaxAlive && playerDist >= _nextSpawnAt)
            {
                Spawn(playerDist);
                _nextSpawnAt = playerDist + Random.Range(b.npcSpawnMinGap, b.npcSpawnMaxGap);
            }

            // pick the draft target: nearest NPC ahead within the window
            float draftWindow = b.draftWindowBase + GameState.CurrentSpeed * b.draftWindowPerSpeed;
            int draftIdx = -1;
            float bestRel = float.MaxValue;
            for (int i = 0; i < _npcs.Count; i++)
            {
                float rel = (float)(_npcs[i].Dist - playerDist);
                if (rel > b.draftMinGap && rel <= draftWindow && rel < bestRel)
                {
                    bestRel = rel;
                    draftIdx = i;
                }
            }
            GameState.IsDrafting = draftIdx >= 0;

            // move + despawn
            for (int i = _npcs.Count - 1; i >= 0; i--)
            {
                var n = _npcs[i];
                float speed = n.Speed;
                if (i == draftIdx && n.PaceLeft > 0f)
                {
                    // pace the player so the draft isn't a one-frame flicker
                    speed = GameState.CurrentSpeed;
                    n.PaceLeft -= dt;
                }
                n.Dist += speed * dt;

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
                n.Visual.AnimSpeed = speed;
            }
        }

        void Spawn(double playerDist)
        {
            var b = Tuning.Balance;
            bool behind = Random.value < b.npcSpawnBehindChance;
            double atDist;
            float factor;
            if (behind)
            {
                atDist = playerDist - Random.Range(8f, 25f);
                factor = Random.Range(b.npcBehindSpeedMinFactor, b.npcBehindSpeedMaxFactor);
            }
            else
            {
                atDist = playerDist + Random.Range(b.npcSpawnMinGap, b.npcSpawnMaxGap);
                factor = Random.Range(b.npcSpeedMinFactor, b.npcSpeedMaxFactor);
            }

            var go = new GameObject("NpcRider");
            go.transform.SetParent(transform, false);
            var vis = go.AddComponent<RiderVisual>();
            vis.Init(4); // behind the player (player sorts at 5)

            var tier = BikeDefs.Tiers[Random.Range(0, BikeDefs.Tiers.Length)];
            var jersey = Cosmetics.Get(JerseyPool[Random.Range(0, JerseyPool.Length)]);
            var helmet = Cosmetics.Get(HelmetPool[Random.Range(0, HelmetPool.Length)]);
            vis.ApplyLook(tier.Silhouette, tier.FrameColor, jersey.Color, helmet.Color);

            _npcs.Add(new Npc
            {
                Dist = atDist,
                Speed = EffectiveCruise() * factor,
                PaceLeft = b.draftPaceSeconds,
                Visual = vis,
            });
        }

        /// <summary>Refresh NPC speeds after a bike upgrade so they stay relevant.</summary>
        public void OnPlayerLevelChanged()
        {
            var b = Tuning.Balance;
            float cruise = EffectiveCruise();
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
