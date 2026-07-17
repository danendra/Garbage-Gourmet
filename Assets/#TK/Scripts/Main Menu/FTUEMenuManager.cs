using UnityEngine;
using DG.Tweening;
using TK.Data;
using TK.Gameplay;
using TK.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TK.Audio;

namespace TK.MainMenu
{
    public class FTUEMenuManager : MonoBehaviour
    {
        public static FTUEMenuManager Instance { get; private set; }

        [SerializeField] private GameObject _upgradeFTUE;
        [SerializeField] private GameObject _collectionFTUE;
        [SerializeField] private float _touchDelay = 2f;

        [Header("Upgrade")]
        [SerializeField] private GameObject _upgradeMenu;
        [SerializeField] private GameObject _upgradePanel;
        [SerializeField] private GameObject _panelBackground1;
        [SerializeField] private GameObject _panelBackground2;
        [SerializeField] private GameObject _racoonVisual1;
        [SerializeField] private GameObject _racoonVisual2;
        [SerializeField] private GameObject _text1;
        [SerializeField] private GameObject _text2;
        [SerializeField] private UnityEngine.UI.Button _upgradeButton;

        [Header("Collection")]
        [SerializeField] private GameObject _collectionMenu;
        [SerializeField] private GameObject _collectionPanel;
        [SerializeField] private GameObject _collectionRacoon1;
        [SerializeField] private GameObject _collectionRacoon2;
        [SerializeField] private GameObject _collectionText1;
        [SerializeField] private GameObject _collectionText2;
        [SerializeField] private GameObject _background;
        [SerializeField] private UnityEngine.UI.Button _collectionButton;

        [Header("Threshold")]
        [SerializeField] private int _upgradePointThreshold = 40000;


        [Header("Button References")]
        [SerializeField] private UnityEngine.UI.Button _closeUpgradeButton;
        [SerializeField] private UnityEngine.UI.Button _closeCollectionButton;

        private enum FTUEMenuStep { None, WaitToEnterUpgrade, WaitForTap, WaitForUpgrade, WaitToEnterCollection }
        private FTUEMenuStep _currentStep = FTUEMenuStep.None;

        private bool _upgradePanel1 = false;
        private bool _collectionPanel1 = false;
        private bool _canTap = false;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        private void OnDisable()
        {
            DOTween.Kill(this);
            EnhancedTouchSupport.Disable();
        }

        private void Update()
        {
            switch (_currentStep)
            {
                case FTUEMenuStep.WaitForTap:
                    CheckTap();
                    break;
                case FTUEMenuStep.WaitForUpgrade:
                    CheckUpgradeCompleted();
                    break;
            }
        }

        public void TryStartUpgradeFTUE()
        {
            if (FTUESaveSystem.LoadFTUEUpgradeCompleted())
            {
                TryStartCollectionFTUE();
                return;
            }

            int coins = GameManager.Instance != null ? GameManager.Instance.PlayerCoins : 0;
            int threshold = _upgradePointThreshold > 0 ? _upgradePointThreshold : 40000;

            if (coins >= threshold)
            {
                StartUpgradeFTUE();
            }
            else
            {
                TryStartCollectionFTUE();
            }
        }

        public void StartUpgradeFTUE()
        {
            ShowUpgradeFTUE();
        }

        private void ShowUpgradeFTUE()
        {
            _upgradeFTUE.SetActive(true);
            _currentStep = FTUEMenuStep.WaitToEnterUpgrade;

            // Disable play, settings, and collection buttons on the menu
            if (MainMenuManager.Instance != null && MainMenuNavigationManager.Instance != null)
                MainMenuManager.Instance.SetPlayButtonState(false);
                MainMenuNavigationManager.Instance.SetUpgradeFTUEButtonsState(false);
            
            if(_background != null) _background.SetActive(true);

            // Wait for the specific upgrade button to be clicked, not any screen tap
            if (_upgradeButton != null)
                _upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        }

        private void OnUpgradeButtonClicked()
        {
            if (_upgradeButton != null)
                _upgradeButton.onClick.RemoveListener(OnUpgradeButtonClicked);
            
            if(_closeUpgradeButton != null)
                _closeUpgradeButton.interactable = false;

            if (_upgradeMenu != null) _upgradeMenu.SetActive(false);
            if (_upgradePanel != null) _upgradePanel.SetActive(true);

            AudioManager.Instance.PlaySFX(SFXId.TextTalk);
            _canTap = false;
            DOVirtual.DelayedCall(_touchDelay, () => { _canTap = true; }).SetId(this);

            SwitchCurrentStep(FTUEMenuStep.WaitForTap);
        }

        private void CheckTap()
        {
            if (!_canTap) return;
            if (!WasPrimaryPointerPressedThisFrame()) return;

            if (_upgradeFTUE.activeSelf)
            {
                if (!_upgradePanel1)
                {
                    // Swap to second panel visuals
                    _text1.SetActive(false);
                    _text2.SetActive(true);
                    AudioManager.Instance.PlaySFX(SFXId.TextScream);

                    _panelBackground1.SetActive(false);
                    _panelBackground2.SetActive(true);

                    _racoonVisual1.SetActive(false);
                    _racoonVisual2.SetActive(true);

                    _upgradePanel1 = true;
                }

                SwitchCurrentStep(FTUEMenuStep.WaitForUpgrade);
            }
            else if (_collectionFTUE.activeSelf)
            {
                if (!_collectionPanel1)
                {
                    _collectionText1.SetActive(false);
                    _collectionText2.SetActive(true);
                    AudioManager.Instance.PlaySFX(SFXId.TextScream);

                    _collectionRacoon1.SetActive(false);
                    _collectionRacoon2.SetActive(true);

                    _collectionPanel1 = true;

                    _canTap = false;
                    DOVirtual.DelayedCall(_touchDelay, () => { _canTap = true; }).SetId(this);

                    SwitchCurrentStep(FTUEMenuStep.WaitForTap);
                }
                else
                {
                    OnCollectionFTUECompleted();
                }
            }
        }

        private bool WasPrimaryPointerPressedThisFrame()
        {
            if (Touch.activeTouches.Count > 0 &&
                Touch.activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                return true;
            }

            Mouse mouse = Mouse.current;
            return mouse != null && mouse.leftButton.wasPressedThisFrame;
        }

        private void CheckUpgradeCompleted()
        {
            if (GameManager.Instance == null) return;

            bool hasUpgraded = GameManager.Instance.ArmLevel > 0
                            || GameManager.Instance.PickUpLevel > 0
                            || GameManager.Instance.ReleaseLevel > 0;

            if (hasUpgraded)
                OnUpgradeFTUECompleted();
        }

        private void OnUpgradeFTUECompleted()
        {
            _currentStep = FTUEMenuStep.None;

            _upgradeFTUE.SetActive(false);
            if(_background != null) _background.SetActive(false);

            FTUESaveSystem.SaveFTUEUpgradeCompleted(true);

            if (MainMenuManager.Instance != null)
                MainMenuManager.Instance.SetPlayButtonState(true);

            if (MainMenuNavigationManager.Instance != null)
                MainMenuNavigationManager.Instance.SetUpgradeFTUEButtonsState(true);

            if (_closeUpgradeButton != null)
            {
                _closeUpgradeButton.interactable = true;
                _closeUpgradeButton.onClick.AddListener(OnUpgradePanelClosed);
            }
            else
            {
                TryStartCollectionFTUE();
            }
        }

        private void OnUpgradePanelClosed()
        {
            if (_closeUpgradeButton != null)
                _closeUpgradeButton.onClick.RemoveListener(OnUpgradePanelClosed);

            TryStartCollectionFTUE();
        }

        public void TryStartCollectionFTUE()
        {
            if (FTUESaveSystem.LoadFTUECollectionCompleted()) return;

            if (_currentStep != FTUEMenuStep.None) return;

            StartCollectionFTUE();
        }

        public void StartCollectionFTUE()
        {
            _collectionFTUE.SetActive(true);
            _currentStep = FTUEMenuStep.WaitToEnterCollection;

            if (MainMenuManager.Instance != null && MainMenuNavigationManager.Instance != null)
                MainMenuManager.Instance.SetPlayButtonState(false);
                MainMenuNavigationManager.Instance.SetCollectionFTUEButtonsState(false);
            
            if(_closeCollectionButton != null)
                _closeCollectionButton.interactable = false;

            if (_collectionText1 != null) _collectionText1.SetActive(true);
            if (_collectionText2 != null) _collectionText2.SetActive(false);
            if (_collectionRacoon1 != null) _collectionRacoon1.SetActive(true);
            if (_collectionRacoon2 != null) _collectionRacoon2.SetActive(false);
            if(_background != null) _background.SetActive(true);
            _collectionPanel1 = false;
            _canTap = false;

            if (_collectionButton != null)
                _collectionButton.onClick.AddListener(OnCollectionButtonClicked);
        }

        private void OnCollectionButtonClicked()
        {
            if (_collectionButton != null)
                _collectionButton.onClick.RemoveListener(OnCollectionButtonClicked);

            if (_collectionMenu != null) _collectionMenu.SetActive(false);
            if (_collectionPanel != null) _collectionPanel.SetActive(true);

            AudioManager.Instance.PlaySFX(SFXId.TextTalk);
            _canTap = false;
            DOVirtual.DelayedCall(_touchDelay, () => { _canTap = true; }).SetId(this);

            SwitchCurrentStep(FTUEMenuStep.WaitForTap);
        }

        private void OnCollectionFTUECompleted()
        {
            if (MainMenuManager.Instance != null && MainMenuNavigationManager.Instance != null)
                MainMenuManager.Instance.SetPlayButtonState(true);
                MainMenuNavigationManager.Instance.SetCollectionFTUEButtonsState(true);
            
            if(_closeCollectionButton != null)
                _closeCollectionButton.interactable = true;

            _currentStep = FTUEMenuStep.None;

            if(_background != null) _background.SetActive(false);
            _collectionFTUE.SetActive(false);

            FTUESaveSystem.SaveFTUECollectionCompleted(true);
        }

        private void SwitchCurrentStep(FTUEMenuStep newStep)
        {
            _currentStep = newStep;
        }
    }
}
