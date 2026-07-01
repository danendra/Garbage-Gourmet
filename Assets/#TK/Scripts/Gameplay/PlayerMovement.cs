using UnityEngine;
using UnityEngine.InputSystem;

namespace TK.Gameplay
{

    public class PlayerMovement : MonoBehaviour
    {
        // Component references
        private Camera mainCamera;
        private Rigidbody2D rb;

        // Input/drag settings

        [Header("Drag Controls")]
        [SerializeField] private Collider2D dragAreaCollider;
        [SerializeField] private float leftWall;
        [SerializeField] private float rightWall;
        [SerializeField] private float dragSpeed = 10f;

        // Movement settings

        [Header("Vertical Movement")]
        [SerializeField] private float speed = 5f;
        [SerializeField] private float acceleration = 0.5f;
        [SerializeField] private float returnAcceleration = 2f;
        [SerializeField] private float maxReturnSpeed = 20f;
        private float returnSpeed = 0f;

        //Item collection

        [Header("Held Item")]
        public ITEM_TYPE heldItemType;
        public RARITY heldRarity;
        [SerializeField] private Transform holdPoint;
        public Transform HoldPoint => holdPoint;

        [Header("Hand Visual")]
        [SerializeField] private SpriteRenderer handRenderer;
        [SerializeField] private Sprite openHandSprite;
        [SerializeField] private Sprite grabHandSprite;

        // Game state

        [Header("Runtime State")]
        public bool hasCollected = false;
        private bool isDragging = false;
        public bool canMove = false;
        private bool wasTouching;
        private float targetX;
        private float currentY;
        private float distanceTravelled = 0f;
        private Vector3 offset;
        private float velocityX;
        private float previousX;
        private Vector3 handDefaultScale;
        public bool IsDragging => isDragging;
        public Vector3 HandPosition => transform.position;

        // Unity methods
        void Start()
        {
            mainCamera = Camera.main;
            rb = GetComponent<Rigidbody2D>();

            if (handRenderer != null)
            {
                handDefaultScale = handRenderer.transform.localScale;
            }

            if (handRenderer != null && openHandSprite != null)
            {
                handRenderer.sprite = openHandSprite;
            }

            targetX = transform.position.x;
            currentY = transform.position.y;
            previousX = transform.position.x;

        }

        void Update()
        {
            if (!canMove)
                return;

            if (Touchscreen.current != null && !hasCollected)
                HandleTouch();
        }

        void FixedUpdate()
        {
            if (!canMove)
                return;

            UpdateSpeed();
            UpdateVerticalMovement();
            MovePlayer();

            velocityX = (transform.position.x - previousX) / Time.fixedDeltaTime;
            previousX = transform.position.x;
        }

        // Movement logic

        private void UpdateSpeed()
        {
            speed += acceleration * Time.fixedDeltaTime;
        }

        private void UpdateVerticalMovement()
        {
            if (hasCollected)
            {
                returnSpeed += returnAcceleration * Time.fixedDeltaTime;
                returnSpeed = Mathf.Lerp(returnSpeed, maxReturnSpeed, Time.fixedDeltaTime * returnAcceleration);

                currentY += returnSpeed * Time.fixedDeltaTime; // return upward
            }
            else
            {
                returnSpeed = 0f;
                float moveAmount = speed * Time.fixedDeltaTime;
                currentY -= moveAmount;
                distanceTravelled += moveAmount; // descend
            }
        }

        private void MovePlayer()
        {
            Vector3 finalPosition = new Vector3(targetX, currentY, 0f);

            rb.MovePosition(Vector3.Lerp(
                transform.position,
                finalPosition,
                Time.fixedDeltaTime * dragSpeed
            ));
        }

        // Touch input

        private void HandleTouch()
        {
            var touch = Touchscreen.current.primaryTouch;

            bool isTouching = touch.press.isPressed;

            // START
            if (isTouching && !wasTouching)
            {
                TryStartDrag(touch.position.ReadValue());
            }

            // CONTINUE
            if (isDragging && isTouching)
            {
                DragTo(touch.position.ReadValue());
            }

            // STOP
            if (!isTouching && wasTouching)
            {
                isDragging = false;
            }

            wasTouching = isTouching;
        }

        private void TryStartDrag(Vector2 screenPosition)
        {
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
            worldPosition.z = 0f;

            if (dragAreaCollider.OverlapPoint(worldPosition))
            {
                isDragging = true;
                offset = transform.position - worldPosition;
            }
        }

        private void DragTo(Vector2 screenPosition)
        {
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
            worldPosition.z = 0f;

            float rawTargetX = worldPosition.x + offset.x;
            targetX = Mathf.Clamp(rawTargetX, leftWall, rightWall);
        }

        // Public methods
        public float GetDepth()
        {
            return distanceTravelled;
        }

        public void CollectItem(ITEM_TYPE type, RARITY itemRarity)
        {
            hasCollected = true;
            isDragging = false;

            heldItemType = type;
            heldRarity = itemRarity;

            if (handRenderer != null && grabHandSprite != null)
            {
                handRenderer.sprite = grabHandSprite;
                handRenderer.transform.localScale =
            new Vector3(
                handDefaultScale.x * 1.12f,
                handDefaultScale.y * 0.88f,
                handDefaultScale.z
            );

                StartCoroutine(HandScaleBack());
            }

        }
        private System.Collections.IEnumerator HandScaleBack()
        {
            Transform hand = handRenderer.transform;

            Vector3 startScale = hand.localScale;

            float time = 0f;
            float duration = 0.12f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;

                hand.localScale = Vector3.Lerp(startScale, handDefaultScale, t);

                yield return null;
            }

            hand.localScale = handDefaultScale;
        }

        public float GetDeltaX()
        {
            return velocityX;
        }
        public float GetReturnSpeed()
        {
            return returnSpeed;
        }

    }
}