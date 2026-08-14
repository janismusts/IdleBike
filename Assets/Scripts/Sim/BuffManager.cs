using System;
using System.Collections.Generic;
using UnityEngine;

namespace IdleBike
{
    /// <summary>Speed buff pickups that appear on the road ahead of the player.</summary>
    public class BuffManager : MonoBehaviour
    {
        public event Action BuffCollected;

        class Pickup
        {
            public double Dist;
            public float LaneY;
            public Transform Tr;
        }

        readonly List<Pickup> _pickups = new List<Pickup>();
        float _timer;
        PlayerSim _sim;

        public void Build(PlayerSim sim)
        {
            _sim = sim;
            _timer = UnityEngine.Random.Range(Tuning.Balance.buffSpawnMinInterval, Tuning.Balance.buffSpawnMaxInterval) * 0.5f;
        }

        public void Tick(float dt)
        {
            var b = Tuning.Balance;
            double playerDist = GameState.Data.totalDistance;

            _timer -= dt;
            if (_timer <= 0f && _pickups.Count < 2)
            {
                Spawn(playerDist + UnityEngine.Random.Range(b.buffSpawnAheadMin, b.buffSpawnAheadMax));
                _timer = UnityEngine.Random.Range(b.buffSpawnMinInterval, b.buffSpawnMaxInterval);
            }

            for (int i = _pickups.Count - 1; i >= 0; i--)
            {
                var p = _pickups[i];
                float rel = (float)(p.Dist - playerDist);
                if (rel <= -2f)
                {
                    // far behind without touching it (e.g. offline distance jump) — just remove
                    Destroy(p.Tr.gameObject);
                    _pickups.RemoveAt(i);
                    continue;
                }
                if (rel <= 0.5f && Lanes.SameLane(GameState.PlayerLaneY, p.LaneY))
                {
                    // collected by riding through it in the same lane
                    _sim.PickUpBuff();
                    BuffCollected?.Invoke();
                    Destroy(p.Tr.gameObject);
                    _pickups.RemoveAt(i);
                    continue;
                }
                p.Tr.localPosition = new Vector3(rel,
                    p.LaneY + 0.1f + Mathf.Sin(Time.time * Tuning.Anim.buffBobFrequency + i) * Tuning.Anim.buffBobAmplitude, 0f);
            }
        }

        void Spawn(double atDist)
        {
            float laneY = Lanes.RandomLane();
            var go = new GameObject("SpeedBuff");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PixelSprites.BuffBolt();
            sr.sortingOrder = Lanes.SortOrder(laneY, -2); // just behind riders in the same lane
            _pickups.Add(new Pickup { Dist = atDist, LaneY = laneY, Tr = go.transform });
        }

        public void Clear()
        {
            foreach (var p in _pickups) Destroy(p.Tr.gameObject);
            _pickups.Clear();
        }
    }
}
