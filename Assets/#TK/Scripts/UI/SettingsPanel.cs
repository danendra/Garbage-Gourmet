using UnityEngine;
using UnityEngine.UI;
using TK.Audio;
using System.Reflection;

namespace TK.UI
{
    public class SettingsPanel : MainMenuPanel
    {
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

            float master = GetVolumeFromManager("GetMasterVolume", "MasterVolume", 1f);
            float music = GetVolumeFromManager("GetMusicVolume", "MusicVolume", 1f);
            float sfx = GetVolumeFromManager("GetSFXVolume", "SFXVolume", 1f);

            if (masterVolumeSlider != null) masterVolumeSlider.value = master;
            if (musicVolumeSlider != null) musicVolumeSlider.value = music;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfx;
        }

        private void SetMasterVolume(float val)
        {
            SetVolumeInManager("SetMasterVolume", "MasterVolume", val);
            // Fallback for simple master volume control if mixer is not implemented
            AudioListener.volume = val;
        }

        private void SetMusicVolume(float val)
        {
            SetVolumeInManager("SetMusicVolume", "MusicVolume", val);
        }

        private void SetSFXVolume(float val)
        {
            SetVolumeInManager("SetSFXVolume", "SFXVolume", val);
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
