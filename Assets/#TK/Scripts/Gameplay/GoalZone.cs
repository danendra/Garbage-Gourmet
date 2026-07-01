using UnityEngine;

namespace TK.Gameplay
{
    public class GoalZone : MonoBehaviour
    {
        [SerializeField] LevelManager gameManager;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            PlayerMovement player = other.GetComponent<PlayerMovement>();

            if (player == null)
            {
                return;
            }

            if (player.hasCollected)
            {
                GameSession.CollectedItemType = player.heldItemType;
                GameSession.CollectedRarity = player.heldRarity;

                if (player.heldItemType == ItemType.Food)
                    gameManager.WinGame(player);
                else if (player.heldItemType == ItemType.Trash)
                    gameManager.LoseGame(player);
            }
        }
    }
}