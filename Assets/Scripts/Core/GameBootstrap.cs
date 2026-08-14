using UnityEngine;

namespace IdleBike
{
    /// <summary>
    /// Builds the whole game from code at startup — no scene setup required.
    /// Works in any scene, including an empty one.
    /// </summary>
    public static class GameBootstrap
    {
        static bool _booted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (_booted) return;
            _booted = true;

            Tuning.Load();
            GameState.Data = SaveSystem.LoadOrCreate();
            GameState.SprintEnergy = Tuning.Balance.sprintMax;

            Application.targetFrameRate = 60;
#if UNITY_ANDROID || UNITY_IOS
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Screen.orientation = ScreenOrientation.Portrait;
#endif

            // Take over from whatever the open scene contains.
            foreach (var cam in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
                cam.gameObject.SetActive(false);
            foreach (var listener in Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
                listener.enabled = false;

            var root = new GameObject("IdleBike");
            Object.DontDestroyOnLoad(root);

            // Manager + sim
            var manager = root.AddComponent<GameManager>();
            manager.Init();
            manager.ComputeOffline();

            // Camera
            var camGo = new GameObject("CameraRig");
            camGo.transform.SetParent(root.transform, false);
            var rig = camGo.AddComponent<CameraRig>();
            rig.Build();

            // World: sky layers stay level; the Tilt node rotates with the road grade
            var worldGo = new GameObject("World");
            worldGo.transform.SetParent(root.transform, false);

            var tiltGo = new GameObject("Tilt");
            tiltGo.transform.SetParent(worldGo.transform, false);
            manager.WorldTilt = tiltGo.transform;

            var parallax = new GameObject("Parallax").AddComponent<ParallaxBackground>();
            parallax.transform.SetParent(worldGo.transform, false);
            parallax.Build(tiltGo.transform);
            manager.Parallax = parallax;

            var road = new GameObject("Road").AddComponent<RoadScroller>();
            road.transform.SetParent(tiltGo.transform, false);
            road.Build();
            manager.Road = road;

            // Player (starts mid-road; drag up/down steers the lane)
            var playerGo = new GameObject("Player");
            playerGo.transform.SetParent(tiltGo.transform, false);
            playerGo.transform.localPosition = Vector3.zero;
            var playerVis = playerGo.AddComponent<RiderVisual>();
            playerVis.Init(5);
            GameState.PlayerLaneY = Lanes.MidY;
            GameState.PlayerLaneTarget = Lanes.MidY;
            playerVis.BaseY = GameState.PlayerLaneY;
            manager.PlayerVisual = playerVis;
            manager.ApplyPlayerLook();

            // NPCs + buffs
            var npcs = new GameObject("Npcs").AddComponent<NpcManager>();
            npcs.transform.SetParent(tiltGo.transform, false);
            npcs.Terrain = manager.Terrain;
            npcs.Build();
            manager.Npcs = npcs;

            // Teammates
            var teamRiders = new GameObject("TeamRiders").AddComponent<TeamRiderManager>();
            teamRiders.transform.SetParent(tiltGo.transform, false);
            teamRiders.Terrain = manager.Terrain;
            teamRiders.Build();
            manager.TeamRiders = teamRiders;
            TeamService.GenerateGifts();

            var buffs = new GameObject("Buffs").AddComponent<BuffManager>();
            buffs.transform.SetParent(tiltGo.transform, false);
            buffs.Build(manager.Sim);
            manager.Buffs = buffs;
            buffs.BuffCollected += () => { AudioManager.I.PlayBuff(); Haptics.Medium(); };

            // Audio
            var audio = new GameObject("Audio").AddComponent<AudioManager>();
            audio.transform.SetParent(root.transform, false);
            audio.Build();

            // UI
            var ui = new GameObject("UI").AddComponent<UIRoot>();
            ui.transform.SetParent(root.transform, false);
            ui.Build(manager);

            // Dev tools (editor / development builds only)
            if (DebugTools.Enabled)
            {
                var debug = root.AddComponent<DebugTools>();
                debug.Init(manager);
                ui.AttachDebugButton(debug);
            }
        }
    }
}
