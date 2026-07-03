using UnityEngine;
using System.Collections.Generic;

namespace TK.Gameplay
{
    // Store the collected items 
    public class PlayerInventory : MonoBehaviour, IItemCollector
    {
        [Header("Hold Point")]
        [SerializeField] private Transform _holdPoint;

        [Header("Pick Up Settings")]
        [SerializeField] private int _maxPickUpItems = 5;

        private List<CollectibleController> _heldItems = new List<CollectibleController>();

        // ── IItemCollector ─────────────────────────────────────────────────────
        public bool CanCollect   => !IsFull;
        public Transform HoldPoint => _holdPoint;

        // ── Public accessors ───────────────────────────────────────────────────
        public bool IsFull    => _heldItems.Count >= _maxPickUpItems;
        public int  HeldCount => _heldItems.Count;
        public IReadOnlyList<CollectibleController> HeldItems => _heldItems;

        // ── Events ─────────────────────────────────────────────────────────────
        public event System.Action OnItemAdded;
        public event System.Action OnInventoryFull;

        // ── IItemCollector: AddItem ────────────────────────────────────────────
        public void AddItem(CollectibleController _collectible)
        {
            if (IsFull) return;

            _heldItems.Add(_collectible);

            OnItemAdded?.Invoke();

            if (IsFull)
                OnInventoryFull?.Invoke();
        }

        // ── Upgrade ────────────────────────────────────────────────────────────
        public void AddMaxPickUpItems(int amount)
        {
            _maxPickUpItems += amount;
        }
    }
}
