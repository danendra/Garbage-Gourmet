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

            PlayerMovement player = other.GetComponent<PlayerMovement>();

            if (player == null)
                return;

            if (!player.hasCollected || player.HeldCount == 0)
                return;

            // ── Pick the best item ────────────────────────────────────────────
            CollectedItemData best = SelectBestItem(player);

            // ── Write to GameSession ──────────────────────────────────────────
            // Store the full list for any future use (e.g. showing all items on result screen).
            GameSession.CollectedItems.Clear();
            foreach (var item in player.HeldItems)
                GameSession.CollectedItems.Add(item);

            // Store the winning item.
            GameSession.BestItem = best;
            GameSession.CollectedItemType = best.Type;
            GameSession.CollectedRarity   = best.ItemRarity;
            GameSession.CollectedSprite   = best.ItemSprite;
            GameSession.CollectedItemName = best.DisplayName;

            // ── Win / Lose ────────────────────────────────────────────────────
            if (best.Type == ItemType.Food)
                gameManager.WinGame(player);
            else
                gameManager.LoseGame(player);
        }

        private CollectedItemData SelectBestItem(PlayerMovement player)
        {
            CollectedItemData best = player.HeldItems[0];

            for (int i = 1; i < player.HeldCount; i++)
            {
                CollectedItemData candidate = player.HeldItems[i];

                if (candidate.Type == ItemType.Food && best.Type == ItemType.Trash)
                {
                    best = candidate;
                    continue;
                }

                if (candidate.Type == best.Type && (int)candidate.ItemRarity > (int)best.ItemRarity)
                {
                    best = candidate;
                }
            }

            return best;
        }
    }
}