using System;
using UnityEngine;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace TK.Gameplay
{
    public enum HAND_STATE
    {
        Hidden,
        Idle,
        Descent,
        Holding,
        Ascent
    }

    public class HandMovement : MonoBehaviour
    {
        [SerializeField] private float _descentSpeed = 1f;
        [SerializeField] private Transform _ascentTarget;
        [SerializeField] private float _ascentDurationPerUnit = 1f;
        

        [Header("Horizontal Movement")]
        [SerializeField] private float _dragSensitivity = 0.02f; // how much movement per pixel dragged
        [SerializeField] private float _minX = -4f; // left boundary
        [SerializeField] private float _maxX = 4f; // right boundary
        [SerializeField] private float _smoothTime = 0.15f; // lower = snappier, higher = floatier

        [Header("Components")]
        [SerializeField] private HandVisual _handVisual;
        [SerializeField] private ArmLineRenderer _armLineRenderer;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private PlayerInventory _inventory;

        [Header("Arm Reach")]
        [SerializeField] private float _maximumArmReach = 10f;

        private Rigidbody2D _rb;
        private CinemachineImpulseSource _impulseSource;
        private Sequence _ascentSequence;
        private SpriteRenderer _handVisualSprite;

        // horizontal drag-to-target movement
        private float _targetX;
        private float _xVelocity; // used internally by SmoothDamp
        
        public float RawDragDeltaX { get; private set; }

        // touch drag tracking
        private Vector2 _lastTouchPos;
        private bool _dragging;

        // state
        private HAND_STATE _state = HAND_STATE.Idle; // change to idle later
        public HAND_STATE State
        {
            get => _state;
            set
            {
                if (_state == value) return;
                _state = value;
                HandleStateChange();
            }
        }

        private bool _inventoryFull = false;

        public float GetDepth() => Mathf.Abs(_playerTransform.position.y - transform.position.y);

        public void ChangeStateToHidden() => State = HAND_STATE.Hidden;
        public void ChangeStateToIdle() => State = HAND_STATE.Idle;
        public void ChangeStateToDescent() => State = HAND_STATE.Descent;
        public void ChangeStateToHolding() => State = HAND_STATE.Holding;
        public void ChangeStateToAscent() => State = HAND_STATE.Ascent;


        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.linearDamping = 0f;

            _inventory = GetComponent<PlayerInventory>();

            _impulseSource = GetComponent<CinemachineImpulseSource>();

            _handVisualSprite = _handVisual.GetComponent<SpriteRenderer>();

            _targetX = _rb.position.x;
        }

        private void Start()
        {
            HandleStateChange();
        }

        private void OnEnable()
        {
            _armLineRenderer.OnSnapToLineStarted += HandleSnapToLineStarted;
            _armLineRenderer.OnSnapToLineEnded += HandleSnapToLineEnded;
            _inventory.OnInventoryFull += HandleInventoryFull;

            EnhancedTouchSupport.Enable();
        }

        private void OnDisable()
        {
            _armLineRenderer.OnSnapToLineStarted -= HandleSnapToLineStarted;
            _armLineRenderer.OnSnapToLineEnded -= HandleSnapToLineEnded;
            _inventory.OnInventoryFull -= HandleInventoryFull;

            EnhancedTouchSupport.Disable();

            if (_ascentSequence != null && _ascentSequence.IsActive())
            {
                _ascentSequence.Kill();
            }
        }

        private void HandleSnapToLineStarted()
        {

        }

        private void HandleSnapToLineEnded()
        {
            State = HAND_STATE.Ascent;
        }

        private void HandleInventoryFull()
        {
            _inventoryFull = true;
        }

        private void Update()
        {
            
            if (_state == HAND_STATE.Descent)
            {
                CheckTimeToAscent();
                HandleTouchInput();
            }
            else if (_state == HAND_STATE.Idle)
            {
                // Buat FTUE, biar player bisa drag tangan sebelum turun
                HandleTouchInput();
            }
        }

        private void HandleTouchInput()
        {
            RawDragDeltaX = 0f;

            if (Touch.activeTouches.Count == 0)
            {
                _dragging = false;
                return;
            }

            Touch touch = Touch.activeTouches[0];

            switch (touch.phase)
            {
                case UnityEngine.InputSystem.TouchPhase.Began:
                    _dragging = true;
                    _lastTouchPos = touch.screenPosition;
                    break;

                case UnityEngine.InputSystem.TouchPhase.Moved:
                case UnityEngine.InputSystem.TouchPhase.Stationary:
                    if (_dragging)
                    {
                        float deltaX = touch.screenPosition.x - _lastTouchPos.x;
                        RawDragDeltaX = Mathf.Abs(deltaX);
                        _targetX = Mathf.Clamp(_targetX + deltaX * _dragSensitivity, _minX, _maxX);
                        _lastTouchPos = touch.screenPosition;
                    }
                    break;

                case UnityEngine.InputSystem.TouchPhase.Ended:
                case UnityEngine.InputSystem.TouchPhase.Canceled:
                    _dragging = false;
                    break;
            }
        }

        public event Action OnAscended;

        private void HandleStateChange()
        {
            switch (_state)
            {
                case HAND_STATE.Hidden:
                    if (_handVisualSprite.enabled)
                        _handVisualSprite.enabled = false;
                    _armLineRenderer.ChangeStateToInactive();
                    
                    break;

                case HAND_STATE.Idle:
                    if (!_handVisualSprite.enabled)
                        _handVisualSprite.enabled = true;

                    _rb.linearVelocity = Vector2.zero;
                    _targetX = _rb.position.x; // stop horizontal movement
                    _armLineRenderer.ChangeStateToGrowing();
                    _handVisual.ChangeStateToIdle();
                    break;
                case HAND_STATE.Descent:
                    if (!_handVisualSprite.enabled)
                        _handVisualSprite.enabled = true;

                    _rb.bodyType = RigidbodyType2D.Dynamic; // physics-driven again
                    _armLineRenderer.ChangeStateToGrowing();
                    _handVisual.ChangeStateToIdle();
                    break;
                case HAND_STATE.Holding:
                    if (!_handVisualSprite.enabled)
                        _handVisualSprite.enabled = true;

                    _rb.linearVelocity = Vector2.zero;
                    _targetX = _rb.position.x;
                    CameraShakeManager.Instance.CameraShake(_impulseSource, 1f);
                    _armLineRenderer.ChangeStateToSnap();
                    _handVisual.ChangeStateToGrab();
                    break;
                case HAND_STATE.Ascent:
                    if (!_handVisualSprite.enabled)
                        _handVisualSprite.enabled = true;

                    _rb.linearVelocity = Vector2.zero;
                    _rb.bodyType = RigidbodyType2D.Kinematic; // hand off to DOTween

                    float distance = Vector2.Distance(_rb.position, _ascentTarget.position);
                    float duration = distance * _ascentDurationPerUnit;

                    _ascentSequence = DOTween.Sequence();
                    _ascentSequence.Append(
                        transform.DOMove(_ascentTarget.position, duration)
                            .SetEase(Ease.InBack, 0.5f)
                    );
                    _ascentSequence.AppendCallback(() => 
                    {
                        State = HAND_STATE.Idle;
                        OnAscended?.Invoke();
                    });
                    break;
            }
        }

        private void FixedUpdate()
        {
            UpdateTilt();

            switch (_state)
            {
                case HAND_STATE.Descent:
                    MoveHorizontalSmoothed();
                    _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, -_descentSpeed);
                    break;
                case HAND_STATE.Holding:
                case HAND_STATE.Idle:
                    _rb.linearVelocity = Vector2.zero;
                    break;
                case HAND_STATE.Ascent:
                default:
                    break;
            }
        }

        private void MoveHorizontalSmoothed()
        {
            float smoothedX = Mathf.SmoothDamp(_rb.position.x, _targetX, ref _xVelocity, _smoothTime);
            _rb.linearVelocity = new Vector2((smoothedX - _rb.position.x) / Time.fixedDeltaTime, _rb.linearVelocity.y);
        }

        private void UpdateTilt()
        {
            Vector2 dir = _armLineRenderer.GetHandSegmentDirection();
            float tiltAngle = Vector2.SignedAngle(Vector2.up, dir);

            _handVisual.transform.rotation = Quaternion.Euler(0f, 0f, tiltAngle);
        }

        private void CheckTimeToAscent()
        {
            if (_inventoryFull || GetDepth() >= _maximumArmReach)
            {
                ChangeStateToHolding();
            }
        }

        public void SetArmLength(float length)
        {
            _maximumArmReach = length;
        }
    }
}