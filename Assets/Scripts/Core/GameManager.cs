using UnityEngine;

namespace IdleBike
{
    /// <summary>Owns the simulation loop and ties world pieces together.</summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }

        public PlayerSim Sim { get; private set; }
        public NpcManager Npcs;
        public BuffManager Buffs;
        public RoadScroller Road;
        public ParallaxBackground Parallax;
        public RiderVisual PlayerVisual;

        // Offline earnings computed on boot, granted when the popup is collected.
        public double OfflineSeconds { get; private set; }
        public double OfflineMeters { get; private set; }
        public double OfflineCoins { get; private set; }

        float _saveTimer;

        public void Init()
        {
            I = this;
            Sim = new PlayerSim();
            Sim.SprintStarted += () => { AudioManager.I.PlayWhoosh(); Haptics.Light(); };
            Sim.SprintEmptied += () => Haptics.Medium();
            GameState.BikeLevelChanged += OnBikeLevelChanged;
            GameState.CosmeticsChanged += ApplyPlayerLook;
            GameState.ProgressReset += OnProgressReset;
        }

        public void ComputeOffline()
        {
            var b = Tuning.Balance;
            long now = SaveSystem.NowUnix();
            double away = now - GameState.Data.lastSaveUnix;
            if (away < b.offlineMinSeconds) return;
            away = System.Math.Min(away, b.offlineMaxHours * 3600.0);
            float cruise = BikeDefs.CruiseSpeed(GameState.Data.bikeLevel) * (1f - b.dragPenalty);
            OfflineSeconds = away;
            OfflineMeters = cruise * away * b.offlineRateFactor;
            OfflineCoins = OfflineMeters * BikeDefs.CoinsPerMeter(GameState.Data.bikeLevel);
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

            Sim.Tick(dt);
            double meters = GameState.CurrentSpeed * dt;
            GameState.Data.totalDistance += meters;
            GameState.Data.coins += meters * BikeDefs.CoinsPerMeter(GameState.Data.bikeLevel);

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
            var tier = BikeDefs.TierForLevel(GameState.Data.bikeLevel);
            PlayerVisual.ApplyLook(tier.Silhouette, tier.FrameColor,
                Cosmetics.EquippedColor(CosmeticSlot.Jersey),
                Cosmetics.EquippedColor(CosmeticSlot.Helmet));
        }

        void OnBikeLevelChanged()
        {
            ApplyPlayerLook();
            if (Npcs != null) Npcs.OnPlayerLevelChanged();
        }

        void OnProgressReset()
        {
            ApplyPlayerLook();
            if (Npcs != null) Npcs.Clear();
            if (Buffs != null) Buffs.Clear();
        }

        void OnApplicationPause(bool paused)
        {
            if (paused) SaveSystem.Save();
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
