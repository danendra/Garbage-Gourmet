using UnityEngine;

namespace TK.Gameplay
{
    public class GoalZone : MonoBehaviour
    {
        [SerializeField] LevelManager gameManager;

        private void OnEnable()
        {
            gameManager.Hand.OnAscended += OnAscended;
        }

        private void OnDisable()
        {
            gameManager.Hand.OnAscended += OnAscended;
        }

        private void OnAscended()
        {
            HandMovement hand = gameManager.Hand;
            PlayerInventory inventory = gameManager.GetPlayerInventory;

            if (hand == null || inventory == null)
                return;

            // Player hit the maximum arm reach without picking anything up.
            if (inventory.HeldCount == 0)
            {
                gameManager.LoseGame(hand);
                return;
            }

            CollectibleController best = SelectBestItem(inventory);

            if (best.GetType == ITEM_TYPE.Trash)
                gameManager.LoseGame(hand);
            else
                gameManager.WinGame(hand);
        }

        /// Selects the highest-value item from the inventory (Ini nanti diubah buat validasi list).
        private CollectibleController SelectBestItem(PlayerInventory _inventory)
        {
            CollectibleController _best = _inventory.HeldItems[0];

            for (int i = 1; i < _inventory.HeldCount; i++)
            {
                CollectibleController candidate = _inventory.HeldItems[i];

                // Food always beats Trash
                if (candidate.GetType != ITEM_TYPE.Trash && _best.GetType == ITEM_TYPE.Trash)
                {
                    _best = candidate;
                    continue;
                }

                // Same type — higher rarity wins
                if (candidate.GetType == _best.GetType && (int)candidate.GetRarity > (int)_best.GetRarity)
                {
                    _best = candidate;
                }
            }

            return _best;
        }
    }
}