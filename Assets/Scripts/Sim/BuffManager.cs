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
            public Transform Tr;
        }

        readonly List<Pickup> _pickups = new List<Pickup>();
        float _timer;
        PlayerSim _sim;

        public void Build(PlayerSim sim)
        {
            _sim = sim;
            _timer = UnityEngine.Random.Range(GameConfig.BuffSpawnMinInterval, GameConfig.BuffSpawnMaxInterval) * 0.5f;
        }

        public void Tick(float dt)
        {
            double playerDist = GameState.Data.totalDistance;

            _timer -= dt;
            if (_timer <= 0f && _pickups.Count < 2)
            {
                Spawn(playerDist + UnityEngine.Random.Range(GameConfig.BuffSpawnAheadMin, GameConfig.BuffSpawnAheadMax));
                _timer = UnityEngine.Random.Range(GameConfig.BuffSpawnMinInterval, GameConfig.BuffSpawnMaxInterval);
            }

            for (int i = _pickups.Count - 1; i >= 0; i--)
            {
                var p = _pickups[i];
                float rel = (float)(p.Dist - playerDist);
                if (rel <= 0.5f)
                {
                    // collected by riding through it
                    _sim.PickUpBuff();
                    BuffCollected?.Invoke();
                    Destroy(p.Tr.gameObject);
                    _pickups.RemoveAt(i);
                    continue;
                }
                p.Tr.localPosition = new Vector3(rel, 0.15f + Mathf.Sin(Time.time * 4f + i) * 0.12f, 0f);
            }
        }

        void Spawn(double atDist)
        {
            var go = new GameObject("SpeedBuff");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PixelSprites.BuffBolt();
            sr.sortingOrder = 6;
            _pickups.Add(new Pickup { Dist = atDist, Tr = go.transform });
        }

        public void Clear()
        {
            foreach (var p in _pickups) Destroy(p.Tr.gameObject);
            _pickups.Clear();
        }
    }
}
