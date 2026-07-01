using System.Collections;
using UnityEngine;

namespace TK.Gameplay
{

    public enum ITEM_TYPE
    {
        Food,
        Trash
    }

    public enum RARITY
    {
        Common,
        Uncommon,
        Rare,
        Bad,
        Legendary
    }

    public class Collectible : MonoBehaviour
    {
        [Header("Item Data")]
        [SerializeField] private ITEM_TYPE _itemType;
        [SerializeField] private RARITY _rarity;
        [SerializeField] private int _intReward = 1;

        private SpriteRenderer _spriteRenderer;
        private Collider2D _collider;

        void Awake()
        {
            
        }

        void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();
        }

        public void Initialize(Vector3 _position, Quaternion _rotation)
        {
            transform.position = _position;
            transform.rotation = _rotation;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            PlayerMovement player = other.GetComponent<PlayerMovement>();

            if (player == null || player.hasCollected)
                return;

            player.CollectItem(_itemType, _rarity);

            GameSession.CollectedSprite = _spriteRenderer.sprite;
            GameSession.CollectedItemName = name;

            _collider.enabled = false;

            StartCoroutine(IEPickupAnimation(player));
        }

        private IEnumerator IEPickupAnimation(PlayerMovement player)
        {
            Transform target = player.HoldPoint;

            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;
            Vector3 startScale = transform.localScale;

            // ====================================
            // RARITY SETTINGS
            // ====================================

            float snapDuration = 0.12f;
            float popScale = 1.15f;
            float wobbleAngle = 0f;

            switch (_rarity)
            {
                case RARITY.Uncommon:
                    popScale = 1.20f;
                    break;

                case RARITY.Rare:
                    popScale = 1.28f;
                    wobbleAngle = 12f;
                    break;

                case RARITY.Bad:
                    popScale = 1.38f;
                    wobbleAngle = 18f;
                    snapDuration = 0.10f;
                    break;

                case RARITY.Legendary:
                    popScale = 1.55f;
                    wobbleAngle = 25f;
                    snapDuration = 0.08f;
                    break;
            }

            // ====================================
            // SNAP TO HAND
            // ====================================

            float time = 0f;

            while (time < snapDuration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / snapDuration);

                transform.position = Vector3.Lerp(startPos, target.position, t);
                transform.rotation = Quaternion.Lerp(startRot, target.rotation, t);

                transform.localScale =
                    Vector3.Lerp(startScale, startScale * popScale, t);

                yield return null;
            }

            // ====================================
            // PARENT TO HAND
            // ====================================

            transform.SetParent(target, true);
            transform.localPosition = Vector3.zero;

            // wobble start
            transform.localRotation =
                Quaternion.Euler(0, 0, wobbleAngle);

            // ====================================
            // SETTLE BACK
            // ====================================

            time = 0f;
            float settleDuration = 0.12f;

            while (time < settleDuration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / settleDuration);

                transform.localScale =
                    Vector3.Lerp(startScale * popScale, startScale, t);

                transform.localRotation =
                    Quaternion.Lerp(
                        Quaternion.Euler(0, 0, wobbleAngle),
                        Quaternion.identity,
                        t
                    );

                yield return null;
            }

            transform.localScale = startScale;
            transform.localRotation = Quaternion.identity;
        }
    }
}