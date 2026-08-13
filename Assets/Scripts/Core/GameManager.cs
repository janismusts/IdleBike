using UnityEngine;

namespace IdleBike
{
    /// <summary>Owns the simulation loop and ties world pieces together.</summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }

        public PlayerSim Sim { get; private set; }
        public TerrainSystem Terrain { get; private set; }
        public NpcManager Npcs;
        public BuffManager Buffs;
        public RoadScroller Road;
        public ParallaxBackground Parallax;
        public RiderVisual PlayerVisual;
        public UIRoot UI;
        /// <summary>World node that rotates with the road grade (road, riders, pickups).</summary>
        public Transform WorldTilt;

        // Offline earnings computed on boot, granted when the popup is collected.
        public double OfflineSeconds { get; private set; }
        public double OfflineMeters { get; private set; }
        public double OfflineCoins { get; private set; }

        float _saveTimer;

        public void Init()
        {
            I = this;
            SkillSystem.Rebuild();
            Terrain = new TerrainSystem();
            Terrain.Reset();
            Sim = new PlayerSim(Terrain);
            Sim.SprintStarted += () => { AudioManager.I.PlayWhoosh(); Haptics.Light(); };
            Sim.SprintEmptied += () => Haptics.Medium();
            GameState.BikeLevelChanged += OnBikeLevelChanged;
            GameState.CosmeticsChanged += ApplyPlayerLook;
            GameState.ProgressReset += OnProgressReset;
        }

        /// <summary>
        /// Consume the time-away window since lastSaveUnix and add it to the pending
        /// offline reward. Called on boot and on resume from background; accumulates
        /// so an uncollected popup keeps its value across pauses.
        /// </summary>
        public void ComputeOffline()
        {
            var b = Tuning.Balance;
            long now = SaveSystem.NowUnix();
            double away = now - GameState.Data.lastSaveUnix;
            if (away < b.offlineMinSeconds) return;
            away = System.Math.Min(away, b.offlineMaxHours * 3600.0);
            float cruise = BikeDefs.CruiseSpeed(GameState.Data.bikeLevel) * (1f - SkillEffects.EffectiveDragPenalty);
            double meters = cruise * away * b.offlineRateFactor;
            OfflineSeconds += away;
            OfflineMeters += meters;
            OfflineCoins += meters * BikeDefs.CoinsPerMeter(GameState.Data.bikeLevel);
            SaveSystem.Save(); // stamp lastSaveUnix: the window is consumed now
        }

        public void CollectOffline()
        {
            if (OfflineCoins <= 0) return;
            GameState.Data.totalDistance += OfflineMeters;
            GameState.AddCoins(OfflineCoins);
            OfflineSeconds = OfflineMeters = OfflineCoins = 0;
            SaveSystem.Save();
        }

        void Update()
        {
            if (GameState.Data == null || Sim == null) return;
            float dt = Mathf.Min(Time.deltaTime, 0.1f);

            Terrain.Tick(dt);
            Sim.Tick(dt);
            double meters = GameState.CurrentSpeed * dt;
            GameState.Data.totalDistance += meters;
            double coinRate = BikeDefs.CoinsPerMeter(GameState.Data.bikeLevel);
            if (Terrain.IsFlat) coinRate *= 1.0 + SkillEffects.FlatCoinBonus;
            GameState.Data.coins += meters * coinRate;

            if (WorldTilt != null)
                WorldTilt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan(Terrain.CurrentGrade) * Mathf.Rad2Deg);
            if (PlayerVisual != null) PlayerVisual.AnimSpeed = GameState.CurrentSpeed;
            if (Npcs != null) Npcs.Tick(dt);
            if (Buffs != null) Buffs.Tick(dt);
            if (Road != null) Road.Tick();
            if (Parallax != null) Parallax.Tick();

            _saveTimer += dt;
            if (_saveTimer >= GameConfig.AutosaveInterval)
            {
                _saveTimer = 0f;
                SaveSystem.Save();
            }
        }

        public void ApplyPlayerLook()
        {
            if (PlayerVisual == null) return;
            PlayerVisual.ApplyLook(
                BikeDefs.TierIndexForLevel(GameState.Data.bikeLevel),
                Cosmetics.EquippedColor(CosmeticSlot.Jersey),
                Cosmetics.Equipped(CosmeticSlot.Helmet),
                Cosmetics.Equipped(CosmeticSlot.Trail));
        }

        void OnBikeLevelChanged()
        {
            ApplyPlayerLook();
            if (Npcs != null) Npcs.OnPlayerLevelChanged();
        }

        void OnProgressReset()
        {
            SkillSystem.Rebuild();
            Terrain.Reset();
            ApplyPlayerLook();
            if (Npcs != null) Npcs.Clear();
            if (Buffs != null) Buffs.Clear();
        }

        void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                SaveSystem.Save();
                return;
            }
            // resumed from background — mobile apps rarely cold-boot
            if (GameState.Data == null) return;
            _saveTimer = 0f;
            ComputeOffline();
            if (OfflineCoins >= 1.0 && UI != null) UI.ShowOfflinePopup();
        }

        void OnApplicationQuit()
        {
            SaveSystem.Save();
        }

        void OnDestroy()
        {
            GameState.BikeLevelChanged -= OnBikeLevelChanged;
            GameState.CosmeticsChanged -= ApplyPlayerLook;
            GameState.ProgressReset -= OnProgressReset;
        }
    }
}
