using System.Collections;
using UnityEngine;
using DG.Tweening;

namespace TK.Gameplay
{

    public enum ITEM_TYPE
    {
        Top_Bun,
        Cheese,
        Patty,
        Vegetable,
        Trash,
        Bottom_Bun        
    }

    public enum RARITY
    {
        Common,
        Rare,
        Bad,
        Legendary
    }

    public class CollectibleController : MonoBehaviour
    {
        [Header("Item Data")]
        [SerializeField] private ITEM_TYPE _itemType;
        [SerializeField] private RARITY _rarity;
        [SerializeField] private int _intScore = 1;
        [SerializeField] private float _fltMultiplier = 1;
        [SerializeField] private GameObject _goFoodServed;

        public new ITEM_TYPE GetType => _itemType;
        public RARITY GetRarity => _rarity;
        public Sprite GetSprite => _spriteRenderer.sprite;
        public int GetScore => _intScore;
        public float GetMultiplier => _fltMultiplier;

        public GameObject GetFoodServed => _goFoodServed;

        private Collider2D _collider;
        private SpriteRenderer _spriteRenderer;

        void Start()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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

            if (collector == null || !collector.CanCollect || LevelManager.Instance.GetHandMovement.State == HAND_STATE.Ascent)
                return;

            _collider.enabled = false;

            PlayPickupAnimation(collector);
        }

        private void PlayPickupAnimation(IItemCollector _collector)
        {
            // TODO: fix draw order
            Transform target = _collector.HoldPoint;
            Vector3 startScale = transform.localScale;

            // Preserve sign in case scale.x/y is used for sprite flipping
            Vector3 scaleSign = new Vector3(
                Mathf.Sign(startScale.x),
                Mathf.Sign(startScale.y),
                1f
            );

            _collector.AddItem(this);
            transform.SetParent(target, true); // worldPositionStays = true, keeps current visual pose

            // ====================================
            // VARIABILITY SETTINGS
            // ====================================
            float snapDuration = Random.Range(0.10f, 0.15f);
            float settleDuration = Random.Range(0.10f, 0.15f);
            float popScaleMult = Random.Range(1.08f, 1.22f);
            float wobbleAngle = Random.Range(-20f, 20f);

            // 2D jitter — X/Y only, Z locked to 0 to avoid messing with sorting
            Vector3 jitterOffset = new Vector3(
                Random.Range(-.3f, .3f),
                Random.Range(-.3f, .3f),
                0f
            );

            float jitterZRot = Random.Range(-45f, 45f);

            Vector3 popScale = new Vector3(
                Mathf.Abs(startScale.x) * popScaleMult * scaleSign.x,
                Mathf.Abs(startScale.y) * popScaleMult * scaleSign.y,
                startScale.z
            );

            Sequence seq = DOTween.Sequence();

            // ====================================
            // SNAP TO HAND (local space — tracks hand automatically)
            // ====================================
            seq.Append(transform.DOLocalMove(jitterOffset, snapDuration).SetEase(Ease.OutQuad));
            seq.Join(transform.DOLocalRotate(Vector3.zero, snapDuration, RotateMode.Fast).SetEase(Ease.OutQuad));
            seq.Join(transform.DOScale(popScale, snapDuration).SetEase(Ease.OutBack));

            seq.AppendCallback(() =>
            {
                transform.localRotation = Quaternion.Euler(0f, 0f, wobbleAngle);
            });

            // ====================================
            // SETTLE BACK
            // ====================================
            seq.Append(transform.DOScale(startScale, settleDuration).SetEase(Ease.OutQuad));
            seq.Join(transform.DOLocalRotate(new Vector3(0f, 0f, jitterZRot), settleDuration, RotateMode.Fast).SetEase(Ease.OutQuad));

            seq.OnComplete(() =>
            {
                transform.localScale = startScale;
                transform.localRotation = Quaternion.Euler(0f, 0f, jitterZRot);
            });

            seq.SetLink(gameObject);
        }
    }
}
