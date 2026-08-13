using System.Collections.Generic;
using UnityEngine;

namespace IdleBike
{
    public enum SkillTree { Climbing, Flats, Endurance }

    public class SkillDef
    {
        public string Id;
        public SkillTree Tree;
        public string Name;
        public string Desc;          // uses {0} for the per-rank percentage
        public int MaxRank;
        public double CostBase;
        public double CostGrowth;
        public float PerRank;        // effect magnitude per rank (fraction)

        public SkillDef(string id, SkillTree tree, string name, string desc, int maxRank,
            double costBase, double costGrowth, float perRank)
        {
            Id = id; Tree = tree; Name = name; Desc = desc; MaxRank = maxRank;
            CostBase = costBase; CostGrowth = costGrowth; PerRank = perRank;
        }

        public string DescFor(int nextRank) =>
            string.Format(Desc, Mathf.RoundToInt(PerRank * 100f * Mathf.Max(1, nextRank)));
    }

    public static class SkillDefs
    {
        public static readonly SkillDef[] All =
        {
            // CLIMBING — better on hills
            new SkillDef("hill_legs",       SkillTree.Climbing, "Hill Legs",       "-{0}% uphill slowdown",        5, 200,   2.4, 0.08f),
            new SkillDef("climb_power",     SkillTree.Climbing, "Climbing Power",  "+{0}% speed on climbs",        5, 350,   2.4, 0.03f),
            new SkillDef("descender",       SkillTree.Climbing, "Descender",       "+{0}% downhill boost",         5, 300,   2.4, 0.06f),

            // FLATS — better on even ground
            new SkillDef("aero_tuck",       SkillTree.Flats,    "Aero Tuck",       "+{0}% speed on flat roads",    5, 250,   2.4, 0.03f),
            new SkillDef("smooth_roller",   SkillTree.Flats,    "Smooth Roller",   "+{0}% coins on flat roads",    5, 400,   2.4, 0.05f),
            new SkillDef("charged_wheels",  SkillTree.Flats,    "Charged Wheels",  "+{0}% speed buff duration",    5, 500,   2.4, 0.10f),

            // ENDURANCE — general abilities
            new SkillDef("big_lungs",       SkillTree.Endurance,"Big Lungs",       "+{0}% sprint bar size",        5, 300,   2.4, 0.10f),
            new SkillDef("fast_recovery",   SkillTree.Endurance,"Fast Recovery",   "+{0}% sprint recovery",        5, 300,   2.4, 0.12f),
            new SkillDef("wind_cutter",     SkillTree.Endurance,"Wind Cutter",     "-{0}% headwind drag",          5, 600,   2.6, 0.06f),
            new SkillDef("slipstream",      SkillTree.Endurance,"Slipstream",      "+{0}% draft window",           5, 450,   2.4, 0.10f),
        };

        public static SkillDef Get(string id)
        {
            foreach (var s in All) if (s.Id == id) return s;
            return null;
        }

        public static string TreeName(SkillTree tree)
        {
            switch (tree)
            {
                case SkillTree.Climbing: return "CLIMBING";
                case SkillTree.Flats: return "FLATS";
                default: return "ENDURANCE";
            }
        }
    }

    /// <summary>Skill ranks stored in the save; buy with coins.</summary>
    public static class SkillSystem
    {
        static Dictionary<string, int> _ranks;

        public static event System.Action SkillBought;

        static Dictionary<string, int> Ranks
        {
            get
            {
                if (_ranks == null) Rebuild();
                return _ranks;
            }
        }

        public static void Rebuild()
        {
            _ranks = new Dictionary<string, int>();
            var list = GameState.Data != null ? GameState.Data.skillRanks : null;
            if (list == null) return;
            foreach (var e in list)
                if (!string.IsNullOrEmpty(e.id)) _ranks[e.id] = e.rank;
        }

        public static int Rank(string id) => Ranks.TryGetValue(id, out var r) ? r : 0;

        public static double CostFor(SkillDef def)
        {
            int rank = Rank(def.Id);
            return System.Math.Round(def.CostBase * System.Math.Pow(def.CostGrowth, rank));
        }

        public static bool IsMaxed(SkillDef def) => Rank(def.Id) >= def.MaxRank;

        public static bool Buy(SkillDef def)
        {
            if (def == null || IsMaxed(def)) return false;
            if (!GameState.SpendCoins(CostFor(def))) return false;

            int newRank = Rank(def.Id) + 1;
            Ranks[def.Id] = newRank;
            var list = GameState.Data.skillRanks;
            var entry = list.Find(e => e.id == def.Id);
            if (entry == null) list.Add(new SkillRankEntry { id = def.Id, rank = newRank });
            else entry.rank = newRank;

            SaveSystem.Save();
            SkillBought?.Invoke();
            return true;
        }
    }

    /// <summary>Aggregated skill effects consumed by the simulation.</summary>
    public static class SkillEffects
    {
        static float Eff(string id) => SkillSystem.Rank(id) * (SkillDefs.Get(id)?.PerRank ?? 0f);

        public static float UphillPenaltyReduction => Eff("hill_legs");
        public static float UphillPowerBonus => Eff("climb_power");
        public static float DownhillBonus => Eff("descender");
        public static float FlatSpeedBonus => Eff("aero_tuck");
        public static double FlatCoinBonus => Eff("smooth_roller");
        public static float BuffDurationMult => 1f + Eff("charged_wheels");
        public static float SprintMaxMult => 1f + Eff("big_lungs");
        public static float SprintRegenMult => 1f + Eff("fast_recovery");
        public static float DragPenaltyMult => 1f - Eff("wind_cutter");
        public static float DraftWindowMult => 1f + Eff("slipstream");

        /// <summary>Player's drag penalty after skills (fraction of speed lost when not drafting).</summary>
        public static float EffectiveDragPenalty => Tuning.Balance.dragPenalty * DragPenaltyMult;

        /// <summary>Sprint bar capacity after skills.</summary>
        public static float EffectiveSprintMax => Tuning.Balance.sprintMax * SprintMaxMult;
    }
}
