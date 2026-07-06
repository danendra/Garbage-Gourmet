using UnityEngine;

namespace TK.Gameplay
{
    public class GoalZone : MonoBehaviour
    {
        [SerializeField] LevelManager gameManager;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            PlayerMovement  player    = other.GetComponent<PlayerMovement>();
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();

            if (player == null || inventory == null)
                return;

            // Only trigger once the player is ascending.
            if (!player.HasCollected)
                return;

            // Player hit the maximum arm reach without picking anything up.
            if (inventory.HeldCount == 0)
            {
                gameManager.LoseGame(player);
                return;
            }

            CollectibleController best = SelectBestItem(inventory);

            // Store the full list for any future use or remove based on design changes.
            GameSession.CollectedItems.Clear();
            foreach (var item in inventory.HeldItems)
                GameSession.CollectedItems.Add(item);

            // Store the winning item.
            GameSession.BestItem          = best;
            GameSession.CollectedItemType = best.GetType;
            GameSession.CollectedRarity   = best.GetRarity;
            GameSession.CollectedSprite   = best.GetSprite;
            GameSession.CollectedItemName = best.name;

            if (best.GetType == ITEM_TYPE.Trash)
                gameManager.LoseGame(player);
            else
                gameManager.WinGame(player);
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