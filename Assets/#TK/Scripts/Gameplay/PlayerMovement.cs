using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace TK.Gameplay
{

    public class PlayerMovement : MonoBehaviour, IItemCollector
    {
        // Component references
        private Camera _mainCamera;
        private Rigidbody2D rb;

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

        // Item collection

        [Header("Held Item")]
        [SerializeField] private Transform _holdPoint;
        private List<CollectedItemData> _heldItems = new List<CollectedItemData>();


        public Transform HoldPoint => _holdPoint;        
        public IReadOnlyList<CollectedItemData> HeldItems => _heldItems;
        public int HeldCount => _heldItems.Count;
        public bool hasCollected => _heldItems.Count >= _maxPickUpItems;
        public bool CanCollect => !hasCollected;

        [Header("Hand Visual")]
        [SerializeField] private SpriteRenderer _handRenderer;
        [SerializeField] private Sprite _openHandSprite;
        [SerializeField] private Sprite _grabHandSprite;

        [Header("Pick Up Settings")]
        [SerializeField] private int _maxPickUpItems = 5;

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
        private Vector3 _handDefaultScale;

        public bool IsDragging => _isDragging;
        public Vector3 HandPosition => transform.position;

        // Unity methods
        void Start()
        {
            _mainCamera = Camera.main;
            rb = GetComponent<Rigidbody2D>();

            if (_handRenderer != null)
            {
                _handDefaultScale = _handRenderer.transform.localScale;
            }

            if (_handRenderer != null && _openHandSprite != null)
            {
                _handRenderer.sprite = _openHandSprite;
            }

            _targetX = transform.position.x;
            _currentY = transform.position.y;
            _previousX = transform.position.x;

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

        // Movement logic

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

                _currentY += _returnSpeed * Time.fixedDeltaTime; // return upward
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

            rb.MovePosition(Vector3.Lerp(
                transform.position,
                finalPosition,
                Time.fixedDeltaTime * _dragSpeed
            ));
        }

        // Touch input

        private void HandleTouch()
        {
            var touch = Touchscreen.current.primaryTouch;

            bool isTouching = touch.press.isPressed;

            // START
            if (isTouching && !_wasTouching)
            {
                TryStartDrag(touch.position.ReadValue());
            }

            // CONTINUE
            if (_isDragging && isTouching)
            {
                DragTo(touch.position.ReadValue());
            }

            // STOP
            if (!isTouching && _wasTouching)
            {
                _isDragging = false;
            }

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

        // Public methods
        public float GetDepth()
        {
            return _distanceTravelled;
        }

        /// Adds the item to the held inventory. 
        public void AddItem(CollectedItemData data)
        {
            if (hasCollected) return; 

            _heldItems.Add(data);
            _isDragging = false;

            if (_handRenderer != null && _grabHandSprite != null)
            {
                _handRenderer.sprite = _grabHandSprite;
                _handRenderer.transform.localScale = new Vector3(
                    _handDefaultScale.x * 1.12f,
                    _handDefaultScale.y * 0.88f,
                    _handDefaultScale.z
                );

                StartCoroutine(HandScaleBack());
            }
        }
        private System.Collections.IEnumerator HandScaleBack()
        {
            Transform hand = _handRenderer.transform;

            Vector3 startScale = hand.localScale;

            float time = 0f;
            float duration = 0.12f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;

                hand.localScale = Vector3.Lerp(startScale, _handDefaultScale, t);

                yield return null;
            }

            hand.localScale = _handDefaultScale;
        }

        public float GetDeltaX()
        {
            return _velocityX;
        }
        public float GetReturnSpeed()
        {
            return _returnSpeed;
        }

    }
}