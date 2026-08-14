using System.Collections.Generic;
using UnityEngine;

namespace IdleBike
{
    /// <summary>
    /// Teammates riding with the player. They rubber-band around their preferred offset
    /// so the team stays loosely together. Riding close to one grants the team drag
    /// bonus, and one directly ahead gives normal draft cover. Server-driven later.
    /// </summary>
    public class TeamRiderManager : MonoBehaviour
    {
        class Rider
        {
            public Teammate Mate;
            public double Dist;
            public float LaneY;
            public RiderVisual Visual;
            public TextMesh Label;
            public float EmoteTimer;
        }

        readonly List<Rider> _riders = new List<Rider>();
        public TerrainSystem Terrain;

        public void Build()
        {
            TeamService.TeamChanged += Rebuild;
            Rebuild();
        }

        void OnDestroy()
        {
            TeamService.TeamChanged -= Rebuild;
        }

        public void Rebuild()
        {
            foreach (var r in _riders) if (r.Visual != null) Destroy(r.Visual.gameObject);
            _riders.Clear();
            if (!TeamService.InTeam) return;

            var b = Tuning.Balance;
            foreach (var mate in TeamService.Teammates)
            {
                var go = new GameObject("Teammate_" + mate.Name);
                go.transform.SetParent(transform, false);
                var vis = go.AddComponent<RiderVisual>();
                vis.Init(3); // behind NPCs and the player

                int playerTier = BikeDefs.TierIndexForLevel(GameState.Data.bikeLevel);
                int tierIdx = mate.TierIndex >= 0 ? mate.TierIndex
                    : Mathf.Clamp(playerTier + Random.Range(-1, 2), 0, BikeDefs.Tiers.Length - 1);
                mate.TierIndex = tierIdx;
                var helmet = Cosmetics.Get("helmet_white");
                vis.ApplyLook(tierIdx, mate.Jersey, helmet, null);

                // name tag
                var labelGo = new GameObject("Name");
                labelGo.transform.SetParent(go.transform, false);
                labelGo.transform.localPosition = new Vector3(0f, Tuning.Visual.nameTagHeight, 0f);
                labelGo.transform.localScale = Vector3.one * 0.12f;
                var label = labelGo.AddComponent<TextMesh>();
                label.font = UIFactory.DefaultFont;
                label.text = $"[{TeamService.TeamTag}] {mate.Name}";
                label.fontSize = 32;
                label.anchor = TextAnchor.LowerCenter;
                label.alignment = TextAlignment.Center;
                label.color = new Color(1f, 1f, 1f, 0.9f);
                var mr = labelGo.GetComponent<MeshRenderer>();
                mr.material = UIFactory.DefaultFont.material;
                mr.sortingOrder = 45;

                _riders.Add(new Rider
                {
                    Mate = mate,
                    Dist = GameState.Data.totalDistance + mate.PreferredOffset,
                    LaneY = Lanes.RandomLane(),
                    Visual = vis,
                    Label = label,
                    EmoteTimer = Random.Range(b.npcEmoteMinInterval, b.npcEmoteMaxInterval),
                });
            }
        }

        public void Tick(float dt)
        {
            if (_riders.Count == 0)
            {
                GameState.TeamNearby = false;
                return;
            }

            var b = Tuning.Balance;
            double playerDist = GameState.Data.totalDistance;
            float playerSpeed = GameState.CurrentSpeed;
            float terrainMult = Terrain != null ? Terrain.SpeedMultiplier(false) : 1f;

            bool together = false;
            bool teamDraft = false;
            float draftWindow = (b.draftWindowBase + playerSpeed * b.draftWindowPerSpeed)
                                * SkillEffects.DraftWindowMult;

            foreach (var r in _riders)
            {
                float rel = (float)(r.Dist - playerDist);
                // rubber-band toward the preferred offset around the player
                float correction = (r.Mate.PreferredOffset - rel) * 0.15f;
                float speed = Mathf.Max(0f, playerSpeed + correction) * terrainMult;
                r.Dist += speed * dt;

                rel = (float)(r.Dist - playerDist);
                var lp = r.Visual.transform.localPosition;
                r.Visual.transform.localPosition = new Vector3(rel, lp.y, 0f);
                r.Visual.BaseY = r.LaneY;
                r.Visual.AnimSpeed = speed;

                if (Mathf.Abs(rel) <= b.teamTogetherRange) together = true;
                if (rel > b.draftMinGap && rel <= draftWindow
                    && Lanes.SameLane(GameState.PlayerLaneY, r.LaneY)) teamDraft = true;

                // teammates cheer once in a while
                r.EmoteTimer -= dt;
                if (r.EmoteTimer <= 0f)
                {
                    r.EmoteTimer = Random.Range(b.npcEmoteMinInterval, b.npcEmoteMaxInterval) * 1.5f;
                    int[] friendly = { 0, 1, 2, 8, 10, 11 }; // wave, thumbs, heart, muscle, trophy, fire
                    r.Visual.ShowEmote(friendly[Random.Range(0, friendly.Length)]);
                }
            }

            GameState.TeamNearby = together;
            if (teamDraft) GameState.IsDrafting = true; // teammates give draft cover too
        }

        public void Clear()
        {
            Rebuild();
        }
    }
}
