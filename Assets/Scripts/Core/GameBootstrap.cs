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

            GameState.Data = SaveSystem.LoadOrCreate();

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

            // World
            var worldGo = new GameObject("World");
            worldGo.transform.SetParent(root.transform, false);

            var parallax = new GameObject("Parallax").AddComponent<ParallaxBackground>();
            parallax.transform.SetParent(worldGo.transform, false);
            parallax.Build();
            manager.Parallax = parallax;

            var road = new GameObject("Road").AddComponent<RoadScroller>();
            road.transform.SetParent(worldGo.transform, false);
            road.Build();
            manager.Road = road;

            // Player
            var playerGo = new GameObject("Player");
            playerGo.transform.SetParent(worldGo.transform, false);
            playerGo.transform.localPosition = Vector3.zero;
            var playerVis = playerGo.AddComponent<RiderVisual>();
            playerVis.Init(5);
            manager.PlayerVisual = playerVis;
            manager.ApplyPlayerLook();

            // NPCs + buffs
            var npcs = new GameObject("Npcs").AddComponent<NpcManager>();
            npcs.transform.SetParent(worldGo.transform, false);
            npcs.Build();
            manager.Npcs = npcs;

            var buffs = new GameObject("Buffs").AddComponent<BuffManager>();
            buffs.transform.SetParent(worldGo.transform, false);
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
        }
    }
}
