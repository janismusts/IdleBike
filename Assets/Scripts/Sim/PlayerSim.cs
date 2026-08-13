using System;
using UnityEngine;

namespace IdleBike
{
    /// <summary>Player speed / sprint / drag simulation. Ticked by GameManager.</summary>
    public class PlayerSim
    {
        public event Action SprintStarted;
        public event Action SprintEmptied;

        bool _wasSprinting;

        public float CruiseSpeed => BikeDefs.CruiseSpeed(GameState.Data.bikeLevel);

        public void Tick(float dt)
        {
            var s = GameState.Data;
            float cruise = BikeDefs.CruiseSpeed(s.bikeLevel);

            // Sprint energy
            bool wantSprint = GameState.SprintHeld;
            bool sprinting = GameState.IsSprinting;
            if (sprinting)
            {
                GameState.SprintEnergy -= GameConfig.SprintDrainPerSec * dt;
                if (GameState.SprintEnergy <= 0f || !wantSprint)
                {
                    GameState.SprintEnergy = Mathf.Max(0f, GameState.SprintEnergy);
                    sprinting = false;
                    if (GameState.SprintEnergy <= 0f) SprintEmptied?.Invoke();
                }
            }
            else
            {
                float regen = GameConfig.SprintRegenPerSec + (GameState.IsDrafting ? GameConfig.SprintRegenDraftBonus : 0f);
                GameState.SprintEnergy = Mathf.Min(GameConfig.SprintMax, GameState.SprintEnergy + regen * dt);
                if (wantSprint && GameState.SprintEnergy >= GameConfig.SprintMinToStart)
                {
                    sprinting = true;
                    SprintStarted?.Invoke();
                }
            }
            GameState.IsSprinting = sprinting;
            _wasSprinting = sprinting;

            // Buff timer
            if (GameState.BuffTimeLeft > 0f)
                GameState.BuffTimeLeft = Mathf.Max(0f, GameState.BuffTimeLeft - dt);

            // Target speed
            float target = cruise;
            if (!GameState.IsDrafting) target *= 1f - GameConfig.DragPenalty;
            if (sprinting) target *= GameConfig.SprintMultiplier;
            if (GameState.BuffTimeLeft > 0f) target *= GameConfig.BuffMultiplier;

            // Smooth toward target
            float cur = GameState.CurrentSpeed;
            float rate = target > cur ? GameConfig.Acceleration : GameConfig.Deceleration;
            // scale accel with cruise so high-tier bikes don't take forever to reach speed
            rate *= Mathf.Max(1f, cruise * 0.15f);
            GameState.CurrentSpeed = Mathf.MoveTowards(cur, target, rate * dt);
        }

        public void PickUpBuff()
        {
            GameState.BuffTimeLeft = GameConfig.BuffDuration;
        }
    }
}
