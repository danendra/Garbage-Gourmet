using UnityEngine;
using DG.Tweening;
using TK.Data;
using TK.Gameplay;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace TK.MainMenu
{
    public class FTUEMenuManager : MonoBehaviour
    {
        public static FTUEMenuManager Instance { get; private set; }

        [SerializeField] private GameObject _upgradeFTUE;
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

        [Header("Threshold")]
        [SerializeField] private int _upgradePointThreshold = 40000;

        private enum FTUEMenuStep { None, WaitToEnterUpgrade, WaitForTap, WaitForUpgrade }
        private FTUEMenuStep _currentStep = FTUEMenuStep.None;

        private bool _upgradePanel1 = false;
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
            if (FTUESaveSystem.LoadFTUEUpgradeCompleted()) return;

            int cumulativePoints = GameManager.Instance != null
                ? GameManager.Instance.intCummulativePoint
                : 0;

            if (cumulativePoints < _upgradePointThreshold) return;

            StartUpgradeFTUE();
        }

        public void StartUpgradeFTUE()
        {
            DOVirtual.DelayedCall(1f, ShowUpgradeFTUE).SetId(this);
        }

        private void ShowUpgradeFTUE()
        {
            _upgradeFTUE.SetActive(true);
            _currentStep = FTUEMenuStep.WaitToEnterUpgrade;

            // Wait for the specific upgrade button to be clicked, not any screen tap
            if (_upgradeButton != null)
                _upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        }

        private void OnUpgradeButtonClicked()
        {
            if (_upgradeButton != null)
                _upgradeButton.onClick.RemoveListener(OnUpgradeButtonClicked);

            if (_upgradeMenu != null) _upgradeMenu.SetActive(false);
            if (_upgradePanel != null) _upgradePanel.SetActive(true);

            _canTap = false;
            DOVirtual.DelayedCall(_touchDelay, () => { _canTap = true; }).SetId(this);

            SwitchCurrentStep(FTUEMenuStep.WaitForTap);
        }

        private void CheckTap()
        {
            if (!_canTap) return;
            if (Touch.activeTouches.Count == 0) return;
            if (Touch.activeTouches[0].phase != UnityEngine.InputSystem.TouchPhase.Began) return;

            if (_upgradeFTUE.activeSelf)
            {
                if (!_upgradePanel1)
                {
                    // Swap to second panel visuals
                    _text1.SetActive(false);
                    _text2.SetActive(true);

                    _panelBackground1.SetActive(false);
                    _panelBackground2.SetActive(true);

                    _racoonVisual1.SetActive(false);
                    _racoonVisual2.SetActive(true);

                    _upgradePanel1 = true;
                }

                SwitchCurrentStep(FTUEMenuStep.WaitForUpgrade);
            }
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

            FTUESaveSystem.SaveFTUEUpgradeCompleted(true);
        }

        private void SwitchCurrentStep(FTUEMenuStep newStep)
        {
            _currentStep = newStep;
        }
    }
}
