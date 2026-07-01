using UnityEngine;

namespace TK.Gameplay
{

    public enum ItemType
    {
        Food,
        Trash
    }

    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    [System.Serializable]
    public class ItemVisual
    {
        public Sprite ItemSprite;
        public string DisplayName;
        public Vector3 DisplayScale = Vector3.one;
    }

    public class Collectible : MonoBehaviour
    {
        [Header("Item Data")]
        [SerializeField] private ItemType _itemType;
        [SerializeField] private Rarity _rarity;

        [Header("Visual Variants")]
        [SerializeField] private ItemVisual[] _variants;

        private SpriteRenderer _sr;
        private PolygonCollider2D _poly;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _poly = GetComponent<PolygonCollider2D>();
        }

        void Start()
        {
            SetRandomVisual();
        }

        public void Initialize(Vector3 _position, Quaternion _rotation)
        {
            transform.position = _position;
            transform.rotation = _rotation;
        }

        private ItemVisual _currentVisual;

        private void SetRandomVisual()
        {
            if (_variants == null || _variants.Length == 0)
            {
                Debug.LogWarning(gameObject.name + " has no variants assigned.");
                return;
            }

            int randomIndex = Random.Range(0, _variants.Length);
            _currentVisual = _variants[randomIndex];

            _sr.sprite = _currentVisual.ItemSprite;
            transform.localScale = _currentVisual.DisplayScale;

            RefreshCollider();
        }

        private void RefreshCollider()
        {
            if (_poly == null)
                return;

            // Rebuild collider using sprite's Physics Shape
            _poly.pathCount = 0;

            Sprite sprite = _sr.sprite;

            if (sprite == null)
                return;

            int shapeCount = sprite.GetPhysicsShapeCount();

            for (int i = 0; i < shapeCount; i++)
            {
                var points = new System.Collections.Generic.List<Vector2>();
                sprite.GetPhysicsShape(i, points);

                _poly.pathCount = i + 1;
                _poly.SetPath(i, points);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            IItemCollector collector = other.GetComponent<IItemCollector>();

            if (collector == null || !collector.CanCollect)
                return;

            // Build item data and hand it to the collector's inventory.
            var data = new CollectedItemData
            {
                Type    = _itemType,
                ItemRarity      = _rarity,
                ItemSprite      = _currentVisual.ItemSprite,
                DisplayName = _currentVisual.DisplayName
            };

            collector.AddItem(data);

            _poly.enabled = false;

            StartCoroutine(PickupAnimation(collector));
        }

        private System.Collections.IEnumerator PickupAnimation(IItemCollector collector)
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
                case Rarity.Uncommon:
                    popScale = 1.20f;
                    break;

                case Rarity.Rare:
                    popScale = 1.28f;
                    wobbleAngle = 12f;
                    break;

                case Rarity.Epic:
                    popScale = 1.38f;
                    wobbleAngle = 18f;
                    snapDuration = 0.10f;
                    break;

                case Rarity.Legendary:
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