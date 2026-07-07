using UnityEngine;

namespace TK.Data
{
    [CreateAssetMenu(fileName = "UpgradeData", menuName = "TK/UpgradeData")]
    public class UpgradeData : ScriptableObject
    {
        [System.Serializable]
        public class UpgradeLevel
        {
            public int value;
            public int cost;
        }

        [Header("Arm Length Upgrades")]
        [Tooltip("Each index is a level (0 = base). Value is the max arm reach distance.")]
        public UpgradeLevel[] ArmLengthLevels = new UpgradeLevel[]
        {
            new UpgradeLevel { value = 15, cost = 100 },
            new UpgradeLevel { value = 25, cost = 200 },
            new UpgradeLevel { value = 50, cost = 300 },
            new UpgradeLevel { value = 90, cost = 400 }
        };

        [Header("Max Pick Up Upgrades")]
        [Tooltip("Each index is a level (0 = base). Value is the max number of items the player can hold.")]
        public UpgradeLevel[] MaxPickUpLevels = new UpgradeLevel[]
        {
            new UpgradeLevel { value = 5, cost = 100 },
            new UpgradeLevel { value = 7, cost = 200 },
            new UpgradeLevel { value = 9, cost = 300 },
            new UpgradeLevel { value = 12, cost = 400 }
        };

        [Header("Max Release Upgrades")]
        [Tooltip("Each index is a level (0 = base). Value is the max number of items the player can release.")]
        public UpgradeLevel[] MaxReleaseLevels = new UpgradeLevel[]
        {
            new UpgradeLevel { value = 2, cost = 100 },
            new UpgradeLevel { value = 3, cost = 200 },
            new UpgradeLevel { value = 4, cost = 300 },
            new UpgradeLevel { value = 5, cost = 400 }
        };

        // ── Helpers ─────────────────────────────────────────────────────────────
        public int MaxArmLevel    => ArmLengthLevels.Length - 1;
        public int MaxPickUpLevel => MaxPickUpLevels.Length - 1;
        public int MaxReleaseLevel => MaxReleaseLevels.Length - 1;

        public int GetArmLength(int level)
        {
            level = Mathf.Clamp(level, 0, MaxArmLevel);
            return ArmLengthLevels[level].value;
        }

        public int GetMaxPickUp(int level)
        {
            level = Mathf.Clamp(level, 0, MaxPickUpLevel);
            return MaxPickUpLevels[level].value;
        }

        public int GetMaxRelease(int level)
        {
            level = Mathf.Clamp(level, 0, MaxReleaseLevel);
            return MaxReleaseLevels[level].value;
        }
    }
}
