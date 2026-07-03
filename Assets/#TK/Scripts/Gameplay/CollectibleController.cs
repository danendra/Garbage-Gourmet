using System.Collections;
using UnityEngine;

namespace TK.Gameplay
{

    public enum ITEM_TYPE
    {
        Top_Bun,
        Bottom_Bun,
        Patty,
        Cheese,
        Vegetable,
        Trash
    }

    public enum RARITY
    {
        Common,        
        Rare,
        Bad,
        Legendary
    }

    [System.Serializable]
    public class ItemVisual
    {
        public Sprite ItemSprite;
        public string DisplayName;
        public Vector3 DisplayScale = Vector3.one;
    }

    public class CollectibleController : MonoBehaviour
    {
        [Header("Item Data")]
        [SerializeField] private ITEM_TYPE _itemType;
        [SerializeField] private RARITY _rarity;
        [SerializeField] private int _intScore = 1;
        [SerializeField] private GameObject _goFoodServed;

        public GameObject GetFoodServed => _goFoodServed;

        private Collider2D _collider;
        private SpriteRenderer _spriteRenderer;

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

            IItemCollector collector = other.GetComponent<IItemCollector>();

            if (collector == null || !collector.CanCollect)
                return;

            GameSession.CollectedSprite = _spriteRenderer.sprite;
            GameSession.CollectedItemName = name;

            _collider.enabled = false;

            StartCoroutine(IEPickupAnimation(collector));
        }

        private IEnumerator IEPickupAnimation(IItemCollector collector)
        {
            Transform target = collector.HoldPoint;

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
