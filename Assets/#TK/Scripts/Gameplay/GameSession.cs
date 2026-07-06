using UnityEngine;
using System.Collections.Generic;

namespace TK.Gameplay
{
    public static class GameSession
    {
        public static int FinalScore;
        public static bool PlayerWon;

        // Full list of every item the player collected this run.
        public static List<CollectibleController> CollectedItems = new List<CollectibleController>();

        // The best item selected by GoalZone (Ini buat test, bisa dihapus kalau ga kepake lagi)
        public static CollectibleController BestItem;
        public static ITEM_TYPE CollectedItemType;
        public static RARITY CollectedRarity;
        public static Sprite CollectedSprite;
        public static string CollectedItemName;
    }
}