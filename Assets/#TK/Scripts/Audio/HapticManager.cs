using UnityEngine;
using System.Runtime.InteropServices;

namespace TK.Audio
{
    public enum HapticImpactType
    {
        Light = 0,
        Medium = 1,
        Heavy = 2
    }

    public enum HapticNotificationType
    {
        Success = 0,
        Warning = 1,
        Error = 2
    }

    public static class HapticManager
    {
        private const string PREFS_HAPTICS = "Setting_Haptics";

        #if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void _PlayImpactHaptic(int style);

        [DllImport("__Internal")]
        private static extern void _PlayNotificationHaptic(int type);
        #endif

        private static bool IsHapticsEnabled()
        {
            return PlayerPrefs.GetInt(PREFS_HAPTICS, 1) == 1;
        }

        public static void PlayImpact(HapticImpactType style)
        {
            if (!IsHapticsEnabled()) return;

            #if UNITY_IOS && !UNITY_EDITOR
            _PlayImpactHaptic((int)style);
            #elif UNITY_ANDROID && !UNITY_EDITOR
            PlayAndroidHaptic((int)style);
            #else
            Debug.Log($"[HapticManager] Editor Simulated Impact: {style}");
            #endif
        }

        public static void PlayNotification(HapticNotificationType type)
        {
            if (!IsHapticsEnabled()) return;

            #if UNITY_IOS && !UNITY_EDITOR
            _PlayNotificationHaptic((int)type);
            #elif UNITY_ANDROID && !UNITY_EDITOR
            PlayAndroidNotification((int)type);
            #else
            Debug.Log($"[HapticManager] Editor Simulated Notification: {type}");
            #endif
        }

        #if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaObject _vibrator;
        private static AndroidJavaClass _vibrationEffectClass;

        private static void InitializeAndroid()
        {
            if (_vibrator != null) return;

            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    _vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                    _vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[HapticManager] Failed to initialize Android vibrator: {ex.Message}");
            }
        }

        private static void PlayAndroidHaptic(int style)
        {
            InitializeAndroid();
            if (_vibrator == null) return;

            long duration = 15; // default light
            int amplitude = 128; // mid level intensity

            switch (style)
            {
                case 0: // Light
                    duration = 15;
                    amplitude = 80;
                    break;
                case 1: // Medium
                    duration = 30;
                    amplitude = 150;
                    break;
                case 2: // Heavy
                    duration = 60;
                    amplitude = 255;
                    break;
            }

            try
            {
                // Requires Android 26 (Oreo) or higher for amplitude controls
                if (GetAndroidSDKVersion() >= 26 && _vibrationEffectClass != null)
                {
                    using (AndroidJavaObject effect = _vibrationEffectClass.CallStatic<AndroidJavaObject>("createOneShot", duration, amplitude))
                    {
                        _vibrator.Call("vibrate", effect);
                    }
                }
                else
                {
                    // Fallback for older devices
                    _vibrator.Call("vibrate", duration);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[HapticManager] Android vibration failed: {ex.Message}");
            }
        }

        private static void PlayAndroidNotification(int type)
        {
            InitializeAndroid();
            if (_vibrator == null) return;

            long[] pattern = new long[] { 0, 50, 50, 50 }; // default
            int[] amplitudes = new int[] { 0, 100, 0, 100 };

            switch (type)
            {
                case 0: // Success: Quick double beat
                    pattern = new long[] { 0, 20, 80, 40 };
                    amplitudes = new int[] { 0, 100, 0, 180 };
                    break;
                case 1: // Warning: Medium single beat
                    pattern = new long[] { 0, 60 };
                    amplitudes = new int[] { 0, 150 };
                    break;
                case 2: // Error: Heavy triple beat
                    pattern = new long[] { 0, 40, 60, 40, 60, 80 };
                    amplitudes = new int[] { 0, 200, 0, 200, 0, 255 };
                    break;
            }

            try
            {
                if (GetAndroidSDKVersion() >= 26 && _vibrationEffectClass != null)
                {
                    using (AndroidJavaObject effect = _vibrationEffectClass.CallStatic<AndroidJavaObject>("createWaveform", pattern, amplitudes, -1))
                    {
                        _vibrator.Call("vibrate", effect);
                    }
                }
                else
                {
                    _vibrator.Call("vibrate", pattern, -1);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[HapticManager] Android pattern vibration failed: {ex.Message}");
            }
        }

        private static int GetAndroidSDKVersion()
        {
            using (AndroidJavaClass buildVersion = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                return buildVersion.GetStatic<int>("SDK_INT");
            }
        }
        #endif
    }
}
