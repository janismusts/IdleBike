using System;
using UnityEngine;

namespace IdleBike
{
    /// <summary>Player speed / sprint / drag / terrain simulation. Ticked by GameManager.</summary>
    public class PlayerSim
    {
        public event Action SprintStarted;
        public event Action SprintEmptied;

        readonly TerrainSystem _terrain;

        // After the bar empties, require a release + re-press before sprinting again
        // (otherwise a continuous hold restarts the sprint every ~1.7s, spamming SFX/haptics).
        bool _lockedUntilRelease;

        public PlayerSim(TerrainSystem terrain)
        {
            _terrain = terrain;
        }

        public float CruiseSpeed => BikeDefs.CruiseSpeed(GameState.Data.bikeLevel);

        public void Tick(float dt)
        {
            var b = Tuning.Balance;
            float cruise = BikeDefs.CruiseSpeed(GameState.Data.bikeLevel);
            float sprintMax = SkillEffects.EffectiveSprintMax;

            // Sprint energy
            bool wantSprint = GameState.SprintHeld;
            if (!wantSprint) _lockedUntilRelease = false;
            bool sprinting = GameState.IsSprinting;
            if (sprinting)
            {
                GameState.SprintEnergy -= b.sprintDrainPerSec * dt;
                if (GameState.SprintEnergy <= 0f || !wantSprint)
                {
                    bool emptied = GameState.SprintEnergy <= 0f;
                    GameState.SprintEnergy = Mathf.Max(0f, GameState.SprintEnergy);
                    sprinting = false;
                    if (emptied)
                    {
                        _lockedUntilRelease = true;
                        SprintEmptied?.Invoke();
                    }
                }
            }
            else
            {
                float regen = (b.sprintRegenPerSec + (GameState.IsDrafting ? b.sprintRegenDraftBonus : 0f))
                              * SkillEffects.SprintRegenMult;
                GameState.SprintEnergy = Mathf.Min(sprintMax, GameState.SprintEnergy + regen * dt);
                // sprint only starts on a FULL bar
                if (wantSprint && !_lockedUntilRelease && GameState.SprintEnergy >= sprintMax - 0.01f)
                {
                    sprinting = true;
                    SprintStarted?.Invoke();
                }
            }
            GameState.IsSprinting = sprinting;

            // Buff timer
            if (GameState.BuffTimeLeft > 0f)
                GameState.BuffTimeLeft = Mathf.Max(0f, GameState.BuffTimeLeft - dt);

            // Target speed
            float target = cruise;
            if (!GameState.IsDrafting)
            {
                float drag = SkillEffects.EffectiveDragPenalty;
                // riding together with the team shelters everyone from the wind
                if (GameState.TeamNearby) drag *= 1f - b.teamDragReduction;
                target *= 1f - drag;
            }
            if (sprinting) target *= b.sprintMultiplier;
            if (GameState.BuffTimeLeft > 0f) target *= b.buffMultiplier;
            if (_terrain != null)
            {
                target *= _terrain.SpeedMultiplier(true);
                if (_terrain.IsFlat) target *= 1f + SkillEffects.FlatSpeedBonus;
            }

            // Smooth toward target
            float cur = GameState.CurrentSpeed;
            float rate = target > cur ? b.acceleration : b.deceleration;
            // scale accel with cruise so high-tier bikes don't take forever to reach speed
            rate *= Mathf.Max(1f, cruise * 0.15f);
            GameState.CurrentSpeed = Mathf.MoveTowards(cur, target, rate * dt);
        }

        public void PickUpBuff()
        {
            GameState.BuffTimeLeft = Tuning.Balance.buffDuration * SkillEffects.BuffDurationMult;
        }
    }
}
