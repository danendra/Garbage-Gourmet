using UnityEngine;
using UnityEngine.InputSystem;

namespace TK.Gameplay
{
    public class PlayerMovement : MonoBehaviour
    {
        // Component references
        private Camera _mainCamera;
        private Rigidbody2D _rb;

        // Input/drag settings

        [Header("Drag Controls")]
        [SerializeField] private Collider2D _dragAreaCollider;
        [SerializeField] private float _leftWall;
        [SerializeField] private float _rightWall;
        [SerializeField] private float _dragSpeed = 10f;

        // Movement settings

        [Header("Vertical Movement")]
        [SerializeField] private float _speed = 5f;
        [SerializeField] private float _acceleration = 0.5f;
        [SerializeField] private float _returnAcceleration = 2f;
        [SerializeField] private float _maxReturnSpeed = 20f;
        private float _returnSpeed = 0f;

        [Header("Arm Reach")]
        [SerializeField] private float _maximumArmReach = 10f;
        public float MaximumArmReach => _maximumArmReach;

        [Header("Inventory")]
        [SerializeField] private PlayerInventory _inventory;

        // Game state

        [Header("Runtime State")]
        private bool _isDragging = false;
        public bool _canMove = false;
        private bool _wasTouching;
        private float _targetX;
        private float _currentY;
        private float _distanceTravelled = 0f;
        private Vector3 _offset;
        private float _velocityX;
        private float _previousX;

        // Set to true by the OnInventoryFull event from PlayerInventory.
        private bool _inventoryFull = false;

        public bool hasCollected => _inventoryFull || _distanceTravelled >= _maximumArmReach;
        public bool IsDragging   => _isDragging;
        public Vector3 HandPosition => transform.position;

        // ── Unity lifecycle ────────────────────────────────────────────────────
        void Start()
        {
            _mainCamera = Camera.main;
            _rb = GetComponent<Rigidbody2D>();

            _targetX   = transform.position.x;
            _currentY  = transform.position.y;
            _previousX = transform.position.x;

            // Stop drag on any pickup so the item snap animation isn't fighting input.
            _inventory.OnItemAdded    += OnItemAdded;
            // Flag ascent when the bag is full.
            _inventory.OnInventoryFull += OnInventoryFull;
        }

        void OnDestroy()
        {
            _inventory.OnItemAdded     -= OnItemAdded;
            _inventory.OnInventoryFull -= OnInventoryFull;
        }

        private void OnItemAdded()
        {
            _isDragging = false;
        }

        private void OnInventoryFull()
        {
            _inventoryFull = true;
        }

        void Update()
        {
            if (!_canMove)
                return;

            if (Touchscreen.current != null && !hasCollected)
                HandleTouch();
        }

        void FixedUpdate()
        {
            if (!_canMove)
                return;

            UpdateSpeed();
            UpdateVerticalMovement();
            MovePlayer();

            _velocityX = (transform.position.x - _previousX) / Time.fixedDeltaTime;
            _previousX = transform.position.x;
        }

        // ── Movement logic ─────────────────────────────────────────────────────
        private void UpdateSpeed()
        {
            _speed += _acceleration * Time.fixedDeltaTime;
        }

        private void UpdateVerticalMovement()
        {
            if (hasCollected)
            {
                _returnSpeed += _returnAcceleration * Time.fixedDeltaTime;
                _returnSpeed = Mathf.Lerp(_returnSpeed, _maxReturnSpeed, Time.fixedDeltaTime * _returnAcceleration);

                _currentY += _returnSpeed * Time.fixedDeltaTime; // ascend
            }
            else
            {
                _returnSpeed = 0f;
                float moveAmount = _speed * Time.fixedDeltaTime;
                _currentY -= moveAmount;
                _distanceTravelled += moveAmount; // descend
            }
        }

        private void MovePlayer()
        {
            Vector3 finalPosition = new Vector3(_targetX, _currentY, 0f);

            _rb.MovePosition(Vector3.Lerp(
                transform.position,
                finalPosition,
                Time.fixedDeltaTime * _dragSpeed
            ));
        }

        // ── Touch input ────────────────────────────────────────────────────────
        private void HandleTouch()
        {
            var touch = Touchscreen.current.primaryTouch;

            bool isTouching = touch.press.isPressed;

            // START
            if (isTouching && !_wasTouching)
                TryStartDrag(touch.position.ReadValue());

            // CONTINUE
            if (_isDragging && isTouching)
                DragTo(touch.position.ReadValue());

            // STOP
            if (!isTouching && _wasTouching)
                _isDragging = false;

            _wasTouching = isTouching;
        }

        private void TryStartDrag(Vector2 screenPosition)
        {
            Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(screenPosition);
            worldPosition.z = 0f;

            if (_dragAreaCollider.OverlapPoint(worldPosition))
            {
                _isDragging = true;
                _offset = transform.position - worldPosition;
            }
        }

        private void DragTo(Vector2 screenPosition)
        {
            Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(screenPosition);
            worldPosition.z = 0f;

            float rawTargetX = worldPosition.x + _offset.x;
            _targetX = Mathf.Clamp(rawTargetX, _leftWall, _rightWall);
        }

        // ── Public methods ─────────────────────────────────────────────────────
        public float GetDepth()      => _distanceTravelled;
        public float GetDeltaX()     => _velocityX;
        public float GetReturnSpeed() => _returnSpeed;

        // ── Upgrades ───────────────────────────────────────────────────────────
        public void AddArmLength(int amount)
        {
            _maximumArmReach += amount;
        }
    }
}