using UnityEngine;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace IdleBike
{
    /// <summary>
    /// Cross-platform haptics. Android: Vibrator/VibrationEffect via JNI.
    /// iOS: UIImpactFeedbackGenerator via Assets/Plugins/iOS/IdleBikeHaptics.mm.
    /// Editor/other: no-op. Respects the vibration setting.
    /// </summary>
    public static class Haptics
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void _ibHapticImpact(int style);
        [DllImport("__Internal")] static extern void _ibHapticSelection();
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        static AndroidJavaObject _vibrator;
        static int _sdkInt;
        static bool _initialized;

        static bool _warned;

        static void EnsureInit()
        {
            if (_initialized) return;
            _initialized = true;
            EnsureVibratePermission();
            try
            {
                using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                    _sdkInt = version.GetStatic<int>("SDK_INT");
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    _vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[IdleBike] Haptics init failed: {e.Message}");
                _vibrator = null;
            }
        }

        // Unity only injects android.permission.VIBRATE into the manifest when it sees a
        // Handheld.Vibrate() reference in compiled code. Our raw JNI Vibrator calls are
        // invisible to that scan, so keep a never-executed-on-device reference here.
        static void EnsureVibratePermission()
        {
            if (Application.isEditor) Handheld.Vibrate();
        }

        static void Vibrate(long ms, int amplitude)
        {
            EnsureInit();
            if (_vibrator == null) return;
            try
            {
                if (_sdkInt >= 26)
                {
                    using (var effectClass = new AndroidJavaClass("android.os.VibrationEffect"))
                    using (var effect = effectClass.CallStatic<AndroidJavaObject>("createOneShot", ms, amplitude))
                        _vibrator.Call("vibrate", effect);
                }
                else
                {
                    _vibrator.Call("vibrate", ms);
                }
            }
            catch (System.Exception e)
            {
                if (!_warned)
                {
                    _warned = true;
                    Debug.LogWarning($"[IdleBike] Vibrate failed: {e.Message}");
                }
            }
        }
#endif

        static bool Enabled => GameState.Data != null && GameState.Data.vibration;

        /// <summary>Small tick — buttons, selections.</summary>
        public static void Light()
        {
            if (!Enabled) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            Vibrate(15, 60);
#elif UNITY_IOS && !UNITY_EDITOR
            _ibHapticImpact(0); // UIImpactFeedbackStyleLight
#endif
        }

        /// <summary>Notable event — buff pickup, purchase.</summary>
        public static void Medium()
        {
            if (!Enabled) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            Vibrate(30, 140);
#elif UNITY_IOS && !UNITY_EDITOR
            _ibHapticImpact(1); // UIImpactFeedbackStyleMedium
#endif
        }

        /// <summary>Big event — new bike tier.</summary>
        public static void Heavy()
        {
            if (!Enabled) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            Vibrate(55, 255);
#elif UNITY_IOS && !UNITY_EDITOR
            _ibHapticImpact(2); // UIImpactFeedbackStyleHeavy
#endif
        }

        public static void Selection()
        {
            if (!Enabled) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            Vibrate(10, 40);
#elif UNITY_IOS && !UNITY_EDITOR
            _ibHapticSelection();
#endif
        }
    }
}
