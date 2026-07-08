using UnityEngine;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections;
using System.Linq;

namespace TK.Gameplay
{
    // Store the collected items 
    public class PlayerInventory : MonoBehaviour, IItemCollector
    {
        [Header("Hold Point")]
        [SerializeField] private Transform _holdPoint;

        [Header("Pick Up Settings")]
        [SerializeField] private int _maxPickUpItems = 5;

        private Stack<CollectibleController> _heldItems = new Stack<CollectibleController>();

        // ── IItemCollector ─────────────────────────────────────────────────────
        public bool CanCollect   => !IsFull;
        public Transform HoldPoint => _holdPoint;

        // ── Public accessors ───────────────────────────────────────────────────
        public bool IsFull    => _heldItems.Count >= _maxPickUpItems;
        public int  HeldCount => _heldItems.Count;
        public int MaxPickUpItems => _maxPickUpItems;
        public int IntReleaseChance {get; protected set;}
        public IReadOnlyList<CollectibleController> HeldItems => _heldItems.ToList<CollectibleController>();

        // ── Events ─────────────────────────────────────────────────────────────
        public event System.Action<CollectibleController> OnItemAdded;
        public event System.Action<int> OnItemRemoved;
        public event System.Action OnInventoryFull;       

        private bool _isFirstTouch;
        private float _fltDelayDoubleTouch = 0.2f;
        private float _fltCountdown; 

        // ── IItemCollector: AddItem ────────────────────────────────────────────
        public void AddItem(CollectibleController _collectible)
        {
            if (IsFull) return;

            _heldItems.Push(_collectible);

            OnItemAdded?.Invoke(_collectible);

            if (IsFull)
                OnInventoryFull?.Invoke();
        }

        // ── Upgrade ────────────────────────────────────────────────────────────
        public void SetMaxPickUpItems(int amount)
        {
            _maxPickUpItems = amount;
        }

        public void SetReleaseChance(int _intAmount)
        {
            IntReleaseChance = _intAmount;
        }

        private void ReleaseItem()
        {
            if (_heldItems.Count > 0)
            {
                CollectibleController _collectible = _heldItems.Pop();

                _collectible.transform.parent = null;

                OnItemRemoved.Invoke(_heldItems.Count);

                IntReleaseChance--;
            }
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0) && IntReleaseChance > 0)
            {
                if (_isFirstTouch)
                {
                    ReleaseItem();

                    _isFirstTouch = false;
                }
                else
                {
                    _isFirstTouch = true;
                    _fltCountdown = _fltDelayDoubleTouch;
                }
            }

            if (_isFirstTouch)
            {
                if (_fltCountdown > 0)
                {
                    _fltCountdown -= Time.deltaTime;

                    if (_fltCountdown < 0)
                    {
                        _isFirstTouch = false;
                    }
                }
            }

        }
    }
}
