using UnityEngine;

namespace TK.Gameplay
{
    public static class GameSession
    {
        public static int FinalScore;
        public static bool PlayerWon;

        public static ITEM_TYPE CollectedItemType;
        public static RARITY CollectedRarity;

        public static Sprite CollectedSprite;
        public static string CollectedItemName;
    }
}