using UnityEngine;

namespace TK.Data
{
    [CreateAssetMenu(fileName = "UpgradeData", menuName = "TK/UpgradeData")]
    public class UpgradeData : ScriptableObject
    {
        [Header("Arm Length Upgrades")]
        [Tooltip("Each index is a level (0 = base). Value is the max arm reach distance.")]
        public int[] ArmLengthLevels = { 10, 15, 20, 25 };

        [Header("Max Pick Up Upgrades")]
        [Tooltip("Each index is a level (0 = base). Value is the max number of items the player can hold.")]
        public int[] MaxPickUpLevels = { 5, 7, 9, 12 };

        // ── Helpers ─────────────────────────────────────────────────────────────
        public int MaxArmLevel    => ArmLengthLevels.Length - 1;
        public int MaxPickUpLevel => MaxPickUpLevels.Length - 1;

        public int GetArmLength(int level)
        {
            level = Mathf.Clamp(level, 0, MaxArmLevel);
            return ArmLengthLevels[level];
        }

        public int GetMaxPickUp(int level)
        {
            level = Mathf.Clamp(level, 0, MaxPickUpLevel);
            return MaxPickUpLevels[level];
        }
    }
}
