using System.Collections.Generic;
using UnityEngine;

namespace IdleBike
{
    /// <summary>
    /// Infinite side-view road. The player stays near x=0 and the world scrolls by
    /// total distance. Road surface is at y=0 (sprites hang below it).
    /// </summary>
    public class RoadScroller : MonoBehaviour
    {
        const int TileCount = 40;
        float _tileWidth;
        readonly List<Transform> _tiles = new List<Transform>();
        readonly List<Transform> _posts = new List<Transform>();
        Transform _ground;

        public void Build()
        {
            var tileSprite = PixelSprites.RoadTile();
            _tileWidth = tileSprite.bounds.size.x;
            for (int i = 0; i < TileCount; i++)
            {
                var go = new GameObject("RoadTile");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = tileSprite;
                sr.sortingOrder = -10;
                _tiles.Add(go.transform);
            }

            // solid ground fill below the road
            var g = new GameObject("Ground");
            g.transform.SetParent(transform, false);
            var gsr = g.AddComponent<SpriteRenderer>();
            gsr.sprite = PixelSprites.Ground();
            gsr.sortingOrder = -11;
            g.transform.localScale = new Vector3(400f, 40f, 1f);
            g.transform.localPosition = new Vector3(0f, -0.9f, 0f);
            _ground = g.transform;

            // km posts every 100 m
            for (int i = 0; i < 4; i++)
            {
                var p = new GameObject("KmPost");
                p.transform.SetParent(transform, false);
                var psr = p.AddComponent<SpriteRenderer>();
                psr.sprite = PixelSprites.KmPost();
                psr.sortingOrder = -5;
                _posts.Add(p.transform);
            }
        }

        public void Tick()
        {
            double dist = GameState.Data.totalDistance;
            float offset = (float)(dist % _tileWidth);
            float startX = -_tileWidth * (TileCount / 2);
            for (int i = 0; i < _tiles.Count; i++)
                _tiles[i].localPosition = new Vector3(startX + i * _tileWidth - offset, 0f, 0f);

            // posts at multiples of 100 m near the player
            double basePost = System.Math.Floor(dist / 100.0) * 100.0;
            for (int i = 0; i < _posts.Count; i++)
            {
                double postDist = basePost + (i - 1) * 100.0;
                _posts[i].localPosition = new Vector3((float)(postDist - dist), 0f, 0f);
            }
        }
    }
}
