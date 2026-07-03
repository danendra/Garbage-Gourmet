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

            CollectedItemData best = SelectBestItem(inventory);

            // Store the full list for any future use or remove based on design changes.
            GameSession.CollectedItems.Clear();
            foreach (var item in inventory.HeldItems)
                GameSession.CollectedItems.Add(item);

            // Store the winning item.
            GameSession.BestItem          = best;
            GameSession.CollectedItemType = best.Type;
            GameSession.CollectedRarity   = best.ItemRarity;
            GameSession.CollectedSprite   = best.ItemSprite;
            GameSession.CollectedItemName = best.DisplayName;

            if (best.Type == ITEM_TYPE.Food)
                gameManager.WinGame(player);
            else
                gameManager.LoseGame(player);
        }

        /// Selects the highest-value item from the inventory (Ini nanti diubah buat validasi list).
        private CollectedItemData SelectBestItem(PlayerInventory inventory)
        {
            CollectedItemData best = inventory.HeldItems[0];

            for (int i = 1; i < inventory.HeldCount; i++)
            {
                CollectedItemData candidate = inventory.HeldItems[i];

                // Food always beats Trash
                if (candidate.Type == ITEM_TYPE.Food && best.Type == ITEM_TYPE.Trash)
                {
                    best = candidate;
                    continue;
                }

                // Same type — higher rarity wins
                if (candidate.Type == best.Type && (int)candidate.ItemRarity > (int)best.ItemRarity)
                {
                    best = candidate;
                }
            }

            return best;
        }
    }
}