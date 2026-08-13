using System;
using UnityEngine;

namespace IdleBike
{
    /// <summary>Player speed / sprint / drag simulation. Ticked by GameManager.</summary>
    public class PlayerSim
    {
        public event Action SprintStarted;
        public event Action SprintEmptied;

        public float CruiseSpeed => BikeDefs.CruiseSpeed(GameState.Data.bikeLevel);

        public void Tick(float dt)
        {
            var b = Tuning.Balance;
            float cruise = BikeDefs.CruiseSpeed(GameState.Data.bikeLevel);

            // Sprint energy
            bool wantSprint = GameState.SprintHeld;
            bool sprinting = GameState.IsSprinting;
            if (sprinting)
            {
                GameState.SprintEnergy -= b.sprintDrainPerSec * dt;
                if (GameState.SprintEnergy <= 0f || !wantSprint)
                {
                    bool emptied = GameState.SprintEnergy <= 0f;
                    GameState.SprintEnergy = Mathf.Max(0f, GameState.SprintEnergy);
                    sprinting = false;
                    if (emptied) SprintEmptied?.Invoke();
                }
            }
            else
            {
                float regen = b.sprintRegenPerSec + (GameState.IsDrafting ? b.sprintRegenDraftBonus : 0f);
                GameState.SprintEnergy = Mathf.Min(b.sprintMax, GameState.SprintEnergy + regen * dt);
                if (wantSprint && GameState.SprintEnergy >= b.sprintMinToStart)
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
            if (!GameState.IsDrafting) target *= 1f - b.dragPenalty;
            if (sprinting) target *= b.sprintMultiplier;
            if (GameState.BuffTimeLeft > 0f) target *= b.buffMultiplier;

            // Smooth toward target
            float cur = GameState.CurrentSpeed;
            float rate = target > cur ? b.acceleration : b.deceleration;
            // scale accel with cruise so high-tier bikes don't take forever to reach speed
            rate *= Mathf.Max(1f, cruise * 0.15f);
            GameState.CurrentSpeed = Mathf.MoveTowards(cur, target, rate * dt);
        }

        public void PickUpBuff()
        {
            GameState.BuffTimeLeft = Tuning.Balance.buffDuration;
        }
    }
}
