using UnityEngine;

namespace IdleBike
{
    /// <summary>Runtime debug switches. Only honored in the editor / development builds.</summary>
    public static class DebugFlags
    {
        public static bool HideHelmets;
        public static bool HideTrails;
        public static bool LiveTuning = true;
    }

    /// <summary>
    /// Dev-only helper: watches the tuning ScriptableObjects while playing and re-applies
    /// build-time visual values (rider attachments, parallax layout, road, volumes) so
    /// inspector edits show up live. Created by GameBootstrap in editor/dev builds only.
    /// </summary>
    public class DebugTools : MonoBehaviour
    {
        public static bool Enabled => Application.isEditor || Debug.isDebugBuild;

        GameManager _manager;
        float _timer;
        string _visualSnapshot;
        string _audioSnapshot;

        /// <summary>Set by GameBootstrap; visibility follows VisualTuning.showDevButton.</summary>
        public GameObject DevButton;

        public void Init(GameManager manager)
        {
            _manager = manager;
            _visualSnapshot = Snapshot(Tuning.Visual);
            _audioSnapshot = Snapshot(Tuning.Audio);
        }

        void Update()
        {
            if (DevButton != null && DevButton.activeSelf != Tuning.Visual.showDevButton)
                DevButton.SetActive(Tuning.Visual.showDevButton);

            if (!DebugFlags.LiveTuning || _manager == null) return;
            _timer += Time.unscaledDeltaTime;
            if (_timer < 0.5f) return;
            _timer = 0f;

            string visual = Snapshot(Tuning.Visual);
            if (visual != _visualSnapshot)
            {
                _visualSnapshot = visual;
                ApplyVisuals();
            }

            string audio = Snapshot(Tuning.Audio);
            if (audio != _audioSnapshot)
            {
                _audioSnapshot = audio;
                // setters re-apply gains
                AudioManager.I.MusicVolume = AudioManager.I.MusicVolume;
                AudioManager.I.SfxVolume = AudioManager.I.SfxVolume;
            }
        }

        static string Snapshot(ScriptableObject so) => so != null ? JsonUtility.ToJson(so) : "";

        /// <summary>Re-apply visual tuning to everything already built.</summary>
        public void ApplyVisuals()
        {
            foreach (var rider in Object.FindObjectsByType<RiderVisual>(FindObjectsSortMode.None))
                rider.ApplyTuning();
            if (_manager.Parallax != null) _manager.Parallax.Rebuild();
            if (_manager.Road != null) _manager.Road.Rebuild();
            if (_manager.TeamRiders != null) _manager.TeamRiders.Rebuild();
        }
    }
}
