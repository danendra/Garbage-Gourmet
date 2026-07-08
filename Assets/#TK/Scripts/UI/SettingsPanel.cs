using UnityEngine;
using UnityEngine.UI;
using TK.Audio;
using System.Reflection;

namespace TK.UI
{
    public class SettingsPanel : MainMenuPanel
    {
        private const string PREFS_MASTER_VOL = "MasterVolume";
        private const string PREFS_MUSIC_VOL = "MusicVolume";
        private const string PREFS_SFX_VOL = "SFXVolume";

        private const string METHOD_GET_MASTER_VOL = "GetMasterVolume";
        private const string METHOD_GET_MUSIC_VOL = "GetMusicVolume";
        private const string METHOD_GET_SFX_VOL = "GetSFXVolume";

        private const string METHOD_SET_MASTER_VOL = "SetMasterVolume";
        private const string METHOD_SET_MUSIC_VOL = "SetMusicVolume";
        private const string METHOD_SET_SFX_VOL = "SetSFXVolume";

        [Header("Settings UI Components")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Button closeButton;

        protected override void Awake()
        {
            base.Awake();
            
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);

            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

            if (closeButton != null)
                closeButton.onClick.AddListener(() => Hide());
        }

        protected override void OnShown()
        {
            base.OnShown();
            LoadCurrentVolumes();
        }

        private void LoadCurrentVolumes()
        {
            if (AudioManager.Instance == null) return;

            float master = GetVolumeFromManager(METHOD_GET_MASTER_VOL, PREFS_MASTER_VOL, 1f);
            float music = GetVolumeFromManager(METHOD_GET_MUSIC_VOL, PREFS_MUSIC_VOL, 1f);
            float sfx = GetVolumeFromManager(METHOD_GET_SFX_VOL, PREFS_SFX_VOL, 1f);

            if (masterVolumeSlider != null) masterVolumeSlider.value = master;
            if (musicVolumeSlider != null) musicVolumeSlider.value = music;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfx;
        }

        private void SetMasterVolume(float val)
        {
            SetVolumeInManager(METHOD_SET_MASTER_VOL, PREFS_MASTER_VOL, val);

            AudioListener.volume = val;
        }

        private void SetMusicVolume(float val)
        {
            SetVolumeInManager(METHOD_SET_MUSIC_VOL, PREFS_MUSIC_VOL, val);
        }

        private void SetSFXVolume(float val)
        {
            SetVolumeInManager(METHOD_SET_SFX_VOL, PREFS_SFX_VOL, val);
        }

        private float GetVolumeFromManager(string methodName, string prefsKey, float defaultVal)
        {
            MethodInfo method = typeof(AudioManager).GetMethod(methodName);
            if (method != null && AudioManager.Instance != null)
            {
                return (float)method.Invoke(AudioManager.Instance, null);
            }
            return PlayerPrefs.GetFloat(prefsKey, defaultVal);
        }

        private void SetVolumeInManager(string methodName, string prefsKey, float val)
        {
            PlayerPrefs.SetFloat(prefsKey, val);
            MethodInfo method = typeof(AudioManager).GetMethod(methodName, new[] { typeof(float) });
            if (method != null && AudioManager.Instance != null)
            {
                method.Invoke(AudioManager.Instance, new object[] { val });
            }
        }
    }
}
