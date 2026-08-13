using UnityEngine;

namespace IdleBike
{
    /// <summary>
    /// Road gradient. The route is an endless sequence of fixed-length segments whose
    /// grade is derived deterministically from the segment index, so the same distance
    /// always has the same hill regardless of session. Positive grade = uphill.
    /// </summary>
    public class TerrainSystem
    {
        float _smoothedGrade;

        /// <summary>Signed grade, e.g. 0.06 = 6% uphill. Smoothed over segment borders.</summary>
        public float CurrentGrade => _smoothedGrade;

        public bool IsUphill => _smoothedGrade > 0.005f;
        public bool IsDownhill => _smoothedGrade < -0.005f;
        public bool IsFlat => !IsUphill && !IsDownhill;

        public void Reset()
        {
            _smoothedGrade = GradeAt(GameState.Data.totalDistance);
        }

        public void Tick(float dt)
        {
            float target = GradeAt(GameState.Data.totalDistance);
            _smoothedGrade = Mathf.MoveTowards(_smoothedGrade, target, Tuning.Balance.gradeChangePerSec * dt);
        }

        public static float GradeAt(double distance)
        {
            var b = Tuning.Balance;
            long seg = (long)(distance / Mathf.Max(1f, b.gradeSegmentLength));
            if (seg < 2) return 0f; // gentle start
            float roll = Hash01(seg, 0);
            if (roll < b.uphillChance)
                return Mathf.Lerp(0.02f, b.gradeUphillMax, Hash01(seg, 1));
            if (roll < b.uphillChance + b.downhillChance)
                return -Mathf.Lerp(0.02f, b.gradeDownhillMax, Hash01(seg, 2));
            return 0f;
        }

        /// <summary>Speed multiplier from the current grade. Skills soften climbs / boost descents.</summary>
        public float SpeedMultiplier(bool withSkills)
        {
            var b = Tuning.Balance;
            float g = _smoothedGrade;
            if (g > 0.001f)
            {
                float penalty = g * b.uphillSlowFactor;
                if (withSkills) penalty *= 1f - SkillEffects.UphillPenaltyReduction;
                float mult = Mathf.Max(b.uphillMinSpeedMult, 1f - penalty);
                if (withSkills) mult *= 1f + SkillEffects.UphillPowerBonus;
                return mult;
            }
            if (g < -0.001f)
            {
                float boost = -g * b.downhillBoostFactor;
                if (withSkills) boost *= 1f + SkillEffects.DownhillBonus;
                return Mathf.Min(b.downhillMaxSpeedMult, 1f + boost);
            }
            return 1f;
        }

        static float Hash01(long seg, int salt)
        {
            unchecked
            {
                ulong x = (ulong)seg * 2654435761UL + (ulong)salt * 40503UL + 0x9E3779B97F4A7C15UL;
                x ^= x >> 30; x *= 0xBF58476D1CE4E5B9UL;
                x ^= x >> 27; x *= 0x94D049BB133111EBUL;
                x ^= x >> 31;
                return (x & 0xFFFFFF) / (float)0x1000000;
            }
        }
    }
}
