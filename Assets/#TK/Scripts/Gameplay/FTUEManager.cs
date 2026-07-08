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
        [SerializeField] private GameObject _collectionFTUE;
        [SerializeField] private GameObject _upgradeFTUE;

        [Header("Player References")]
        [SerializeField] private HandMovement _handMovement;
        [SerializeField] private PlayerInventory _inventory;

        [Header("Settings")]
        [SerializeField] private float _dragThreshold = 50f; 

        private enum FTUEGameplayStep { None, WaitForDrag, WaitForTap }
        private FTUEGameplayStep _currentStep = FTUEGameplayStep.None;

        private bool _inventoryFTUECanDismiss = false;
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

        private void ShowInventoryFTUE()
        {
            Time.timeScale = 0f;

            _inventoryFTUE.SetActive(true);
            _inventoryFTUECanDismiss = false;
            _currentStep = FTUEGameplayStep.WaitForTap;

            DOVirtual.DelayedCall(5f, () =>
            {
                _inventoryFTUECanDismiss = true;
            }, ignoreTimeScale: true).SetId(this);
        }

        private void CheckTap()
        {
            if (!_inventoryFTUECanDismiss) return;

            if (Touch.activeTouches.Count > 0 &&
                Touch.activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                OnInventoryFTUECompleted();
            }
        }

        private void OnInventoryFTUECompleted()
        {
            _currentStep = FTUEGameplayStep.None;

            _inventoryFTUE.SetActive(false);
            Time.timeScale = 1f;

            FTUESaveSystem.SaveFTUEGameplayCompleted(true);
        }
    }
}
