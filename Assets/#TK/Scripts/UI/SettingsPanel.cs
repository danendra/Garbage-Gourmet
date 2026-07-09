using UnityEngine;
using UnityEngine.UI;
using TK.Audio;
using System.Reflection;

namespace TK.UI
{
    public class SettingsPanel : MainMenuPanel
    {
        private const string PREFS_MUSIC_VOL = "MusicVolume";
        private const string PREFS_SFX_VOL = "SFXVolume";
        private const string PREFS_HAPTICS = "Setting_Haptics";
        private const string PREFS_SCREENSHAKE = "Setting_ScreenShake";

        private const string METHOD_GET_MUSIC_VOL = "GetMusicVolume";
        private const string METHOD_GET_SFX_VOL = "GetSFXVolume";

        private const string METHOD_SET_MUSIC_VOL = "SetMusicVolume";
        private const string METHOD_SET_SFX_VOL = "SetSFXVolume";

        [Header("Settings UI Components (Toggles)")]
        [SerializeField] private UIToggleButton sfxToggle;
        [SerializeField] private UIToggleButton musicToggle;
        [SerializeField] private UIToggleButton hapticsToggle;
        [SerializeField] private UIToggleButton screenShakeToggle;
        [SerializeField] private Button closeButton;

        [Header("Reset Game UI")]
        [SerializeField] private Button openResetPopupButton;
        [SerializeField] private GameObject resetPopupPanel;
        [SerializeField] private Button confirmResetButton;
        [SerializeField] private Button cancelResetButton;

        protected override void Awake()
        {
            base.Awake();
            
            if (sfxToggle != null)
                sfxToggle.onToggleChanged.AddListener(SetSFX);

            if (musicToggle != null)
                musicToggle.onToggleChanged.AddListener(SetMusic);

            if (hapticsToggle != null)
                hapticsToggle.onToggleChanged.AddListener(SetHaptics);
                
            if (screenShakeToggle != null)
                screenShakeToggle.onToggleChanged.AddListener(SetScreenShake);

            if (closeButton != null)
                closeButton.onClick.AddListener(() => Hide());

            if (openResetPopupButton != null)
                openResetPopupButton.onClick.AddListener(ShowResetPopup);

            if (confirmResetButton != null)
                confirmResetButton.onClick.AddListener(ConfirmReset);

            if (cancelResetButton != null)
                cancelResetButton.onClick.AddListener(HideResetPopup);
        }

        protected override void OnShown()
        {
            base.OnShown();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            if (AudioManager.Instance == null) return;

            float music = GetVolumeFromManager(METHOD_GET_MUSIC_VOL, PREFS_MUSIC_VOL, 1f);
            float sfx = GetVolumeFromManager(METHOD_GET_SFX_VOL, PREFS_SFX_VOL, 1f);

            bool isMusicOn = music > 0f;
            bool isSfxOn = sfx > 0f;

            bool isHapticsOn = PlayerPrefs.GetInt(PREFS_HAPTICS, 1) == 1;
            bool isScreenShakeOn = PlayerPrefs.GetInt(PREFS_SCREENSHAKE, 1) == 1;

            if (musicToggle != null) musicToggle.InitializeState(isMusicOn);
            if (sfxToggle != null) sfxToggle.InitializeState(isSfxOn);
            if (hapticsToggle != null) hapticsToggle.InitializeState(isHapticsOn);
            if (screenShakeToggle != null) screenShakeToggle.InitializeState(isScreenShakeOn);
        }

        private void SetSFX(bool isOn)
        {
            float val = isOn ? 1f : 0f;
            SetVolumeInManager(METHOD_SET_SFX_VOL, PREFS_SFX_VOL, val);
        }

        private void SetMusic(bool isOn)
        {
            float val = isOn ? 1f : 0f;
            SetVolumeInManager(METHOD_SET_MUSIC_VOL, PREFS_MUSIC_VOL, val);
        }

        private void SetHaptics(bool isOn)
        {
            PlayerPrefs.SetInt(PREFS_HAPTICS, isOn ? 1 : 0);
            PlayerPrefs.Save();
            
            // TODO: Implement Haptics logic here when it's ready
            // if (isOn) EnableHaptics(); else DisableHaptics();
        }

        private void SetScreenShake(bool isOn)
        {
            PlayerPrefs.SetInt(PREFS_SCREENSHAKE, isOn ? 1 : 0);
            PlayerPrefs.Save();
            
            // TODO: Implement Screen Shake logic here when it's ready
            // if (isOn) EnableScreenShake(); else DisableScreenShake();
        }

        private void ShowResetPopup()
        {
            if (resetPopupPanel != null) resetPopupPanel.SetActive(true);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        }

        private void HideResetPopup()
        {
            if (resetPopupPanel != null) resetPopupPanel.SetActive(false);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        }

        private void ConfirmReset()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();

            if (Gameplay.GameManager.Instance != null)
            {
                Gameplay.GameManager.Instance.ResetUpgrades();
            }
            else
            {
                Data.UpgradeSaveSystem.ResetAll();
            }

            PlayerPrefs.DeleteKey("CURRENT_POINT");
            PlayerPrefs.DeleteKey("CUMMULATIVE_POINT");

            PlayerPrefs.DeleteKey("ftue_first_launch_completed");
            PlayerPrefs.Save();

            HideResetPopup();
            Hide();

            if (Gameplay.GameManager.Instance != null)
            {
                Gameplay.GameManager.Instance.Restart();
            }
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
