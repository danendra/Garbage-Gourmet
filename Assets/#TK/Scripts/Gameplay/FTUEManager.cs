using UnityEngine;
using DG.Tweening;
using TK.Data;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace TK.Gameplay
{
    public class FTUEManager : MonoBehaviour
    {
        public static FTUEManager Instance { get; private set; }

        [Header("FTUE UI")]
        [SerializeField] private GameObject _movementFTUE;
        [SerializeField] private GameObject _inventoryFTUE;
        [SerializeField] private GameObject _releaseFTUE;
        [SerializeField] private GameObject _inventoryUpgradeFTUE;

        [Header("Player References")]
        [SerializeField] private HandMovement _handMovement;
        [SerializeField] private PlayerInventory _inventory;

        [Header("Settings")]
        [SerializeField] private float _dragThreshold = 50f; 

        [Header("Timing")]
        [SerializeField] private float _InventoryHoldTime = 1f;

        private enum FTUEGameplayStep { None, WaitForDrag, WaitForTap, WaitForDoubleTap }
        private FTUEGameplayStep _currentStep = FTUEGameplayStep.None;

        private bool _inventoryFTUECanDismiss = false;
        private bool _releaseFTUECanDismiss = false;
        private bool _inventoryUpgradeFTUECanDismiss = false;
        private float _cumulativeDragX = 0f;

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
                case FTUEGameplayStep.WaitForDrag:
                    CheckDrag();
                    break;

                case FTUEGameplayStep.WaitForTap:
                    CheckTap();
                    break;
                
                case FTUEGameplayStep.WaitForDoubleTap:
                    CheckDoubleTap();
                    break;
            }
        }

        public void StartGameplayFTUE()
        {
            DOVirtual.DelayedCall(1f, ShowMovementFTUE).SetId(this);
        }

        private void ShowMovementFTUE()
        {
            _handMovement.ChangeStateToIdle(); 

            _movementFTUE.SetActive(true);

            _cumulativeDragX = 0f;
            _currentStep = FTUEGameplayStep.WaitForDrag;
        }

        private void CheckDrag()
        {
            _cumulativeDragX += _handMovement.RawDragDeltaX;

            if (_cumulativeDragX >= _dragThreshold)
                OnMovementFTUECompleted();
        }

        private void OnMovementFTUECompleted()
        {
            _currentStep = FTUEGameplayStep.None;

            _movementFTUE.SetActive(false);
            _handMovement.ChangeStateToDescent(); 

            _inventory.OnItemAdded += OnFirstItemAdded;
        }

        private void OnFirstItemAdded(CollectibleController _collectible)
        {
            _inventory.OnItemAdded -= OnFirstItemAdded;
            ShowInventoryFTUE();
        }

        public void StartReleaseFTUE()
        {
            _inventory.OnItemAdded += OnFirstItemAddedForRelease;
        }

        private void OnFirstItemAddedForRelease(CollectibleController _collectible)
        {
            _inventory.OnItemAdded -= OnFirstItemAddedForRelease;
            ShowReleaseFTUE();
        }

        private void ShowInventoryFTUE()
        {
            Time.timeScale = 0f;

            _inventoryFTUE.SetActive(true);
            _inventoryFTUECanDismiss = false;
            _currentStep = FTUEGameplayStep.WaitForTap;

            DOVirtual.DelayedCall(_InventoryHoldTime, () =>
            {
                _inventoryFTUECanDismiss = true;
            }, ignoreTimeScale: true).SetId(this);
        }

        private void CheckTap()
        {
            if(!_inventoryFTUECanDismiss && !_inventoryUpgradeFTUECanDismiss) return;
            
            if (Touch.activeTouches.Count > 0 &&
                Touch.activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                if( _inventoryFTUE.activeSelf)
                {
                    OnInventoryFTUECompleted();
                }
                else if (_inventoryUpgradeFTUE.activeSelf)
                {
                    OnInventoryUpgradeFTUECompleted();
                }
            }
        }

        private void OnInventoryFTUECompleted()
        {
            _currentStep = FTUEGameplayStep.None;

            _inventoryFTUE.SetActive(false);
            Time.timeScale = 1f;

            FTUESaveSystem.SaveFTUEGameplayCompleted(true);
        }

        private void ShowReleaseFTUE()
        {
            Time.timeScale = 0f;

            _releaseFTUE.SetActive(true);
            _releaseFTUECanDismiss = false;
            _currentStep = FTUEGameplayStep.WaitForDoubleTap;

            DOVirtual.DelayedCall(_InventoryHoldTime, () =>
            {
                _releaseFTUECanDismiss = true;
            }, ignoreTimeScale: true).SetId(this);
        }

        private void CheckDoubleTap()
        {
            if(!_releaseFTUECanDismiss) return;

            if (Touch.activeTouches.Count > 0 &&
                Touch.activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                if (_releaseFTUE.activeSelf)
                {
                    OnReleaseFTUECompleted();
                }
            }
        }

        private void OnReleaseFTUECompleted()
        {
            _currentStep = FTUEGameplayStep.None;

            _releaseFTUE.SetActive(false);
            Time.timeScale = 1f;

            FTUESaveSystem.SaveFTUEReleaseCompleted(true);
        }

        public void OnFirstSessionCompleted()
        {
            FTUESaveSystem.SaveFTUEFirstLaunchCompleted(true);
        }

        public void StartInventoryUpgradeFTUE()
        {
            DOVirtual.DelayedCall(1f, ShowInventoryUpgradeFTUE).SetId(this);
        }

        private void ShowInventoryUpgradeFTUE()
        {
            Time.timeScale = 0f;

            _inventoryUpgradeFTUE.SetActive(true);
            _inventoryUpgradeFTUECanDismiss = false;
            _currentStep = FTUEGameplayStep.WaitForTap;

            DOVirtual.DelayedCall(_InventoryHoldTime, () =>
            {
                _inventoryUpgradeFTUECanDismiss = true;
            }, ignoreTimeScale: true).SetId(this);
        }

        private void OnInventoryUpgradeFTUECompleted()
        {
            _currentStep = FTUEGameplayStep.None;

            _inventoryUpgradeFTUE.SetActive(false);
            Time.timeScale = 1f;
            FTUESaveSystem.SaveFTUEInventoryUpgradeCompleted(true);
        }
    }
}
