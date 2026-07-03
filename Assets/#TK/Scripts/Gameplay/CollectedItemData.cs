using UnityEngine;

namespace TK.Gameplay
{
    /// Plain data class representing a single item the player has collected during a run.
    /// Passed from Collectible → PlayerMovement → GoalZone → GameSession.
    public class CollectedItemData
    {
        public ITEM_TYPE Type;
        public RARITY   ItemRarity;
        public Sprite   ItemSprite;
        public string   DisplayName;
    }
}
