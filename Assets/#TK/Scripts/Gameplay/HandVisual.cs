using UnityEngine;
using System.Collections;

namespace TK.Gameplay
{
    // Responsible for all hand visuals: tilt based on horizontal velocity and sprite-swap + squeeze animation on item pickup.
    public class HandVisual : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerMovement _player;
        [SerializeField] private PlayerInventory _inventory;

        [Header("Tilt Settings")]
        [SerializeField] private float _smoothTime = 0.1f;
        [SerializeField] private float _maxAngle = 15f;

        [Header("Hand Sprites")]
        [SerializeField] private SpriteRenderer _handRenderer;
        [SerializeField] private Sprite _openHandSprite;
        [SerializeField] private Sprite _grabHandSprite;

        private float _currentAngle;
        private float _angleVelocity;
        private Vector3 _handDefaultScale;

        // ── Unity lifecycle ────────────────────────────────────────────────────

        void Start()
        {
            if (_handRenderer != null)
            {
                _handDefaultScale = _handRenderer.transform.localScale;

                if (_openHandSprite != null)
                    _handRenderer.sprite = _openHandSprite;
            }

            _inventory.OnItemAdded += OnItemPickedUp;
        }

        void OnDestroy()
        {
            _inventory.OnItemAdded -= OnItemPickedUp;
        }

        // ── Update: tilt ───────────────────────────────────────────────────────

        void Update()
        {
            if (_player == null) return;

            float targetAngle = Mathf.Clamp(_player.GetDeltaX() * 6f, -25f, 25f);

            _currentAngle = Mathf.SmoothDampAngle(
                _currentAngle,
                targetAngle,
                ref _angleVelocity,
                0.08f
            );

            transform.localRotation = Quaternion.Euler(0f, 0f, _currentAngle);
        }

        // ── Pickup visual ──────────────────────────────────────────────────────

        private void OnItemPickedUp()
        {
            if (_handRenderer == null || _grabHandSprite == null) return;

            _handRenderer.sprite = _grabHandSprite;
            _handRenderer.transform.localScale = new Vector3(
                _handDefaultScale.x * 1.12f,
                _handDefaultScale.y * 0.88f,
                _handDefaultScale.z
            );

            StartCoroutine(HandScaleBack());
        }

        private IEnumerator HandScaleBack()
        {
            Transform hand = _handRenderer.transform;
            Vector3 startScale = hand.localScale;

            float time = 0f;
            float duration = 0.12f;

            while (time < duration)
            {
                time += Time.deltaTime;
                hand.localScale = Vector3.Lerp(startScale, _handDefaultScale, time / duration);
                yield return null;
            }

            hand.localScale = _handDefaultScale;
        }
    }
}