using UnityEngine;

namespace TK.Data
{
    [CreateAssetMenu(fileName = "UpgradeData", menuName = "TK/UpgradeData")]
    public class UpgradeData : ScriptableObject
    {
        [System.Serializable]
        public class UpgradeLevel
        {
            public int Value;
            public int Cost;
        }

        [Header("Arm Length Upgrades")]
        [Tooltip("Each index is a level (0 = base). Value is the max arm reach distance.")]
        public UpgradeLevel[] ArmLengthLevels = new UpgradeLevel[]
        {
            new UpgradeLevel { Value = 15, Cost = 100 },
            new UpgradeLevel { Value = 25, Cost = 200 },
            new UpgradeLevel { Value = 50, Cost = 300 },
            new UpgradeLevel { Value = 90, Cost = 400 }
        };

        [Header("Max Pick Up Upgrades")]
        [Tooltip("Each index is a level (0 = base). Value is the max number of items the player can hold.")]
        public UpgradeLevel[] MaxPickUpLevels = new UpgradeLevel[]
        {
            new UpgradeLevel { Value = 5, Cost = 100 },
            new UpgradeLevel { Value = 7, Cost = 200 },
            new UpgradeLevel { Value = 9, Cost = 300 },
            new UpgradeLevel { Value = 12, Cost = 400 }
        };

        [Header("Max Release Upgrades")]
        [Tooltip("Each index is a level (0 = base). Value is the max number of items the player can release.")]
        public UpgradeLevel[] MaxReleaseLevels = new UpgradeLevel[]
        {
            new UpgradeLevel { Value = 2, Cost = 100 },
            new UpgradeLevel { Value = 3, Cost = 200 },
            new UpgradeLevel { Value = 4, Cost = 300 },
            new UpgradeLevel { Value = 5, Cost = 400 }
        };

        // ── Helpers ─────────────────────────────────────────────────────────────
        public int MaxArmLevel    => ArmLengthLevels.Length - 1;
        public int MaxPickUpLevel => MaxPickUpLevels.Length - 1;
        public int MaxReleaseLevel => MaxReleaseLevels.Length - 1;

        public int GetArmLength(int level)
        {
            level = Mathf.Clamp(level, 1, MaxArmLevel);
            return ArmLengthLevels[level].Value;
        }

        public int GetMaxPickUp(int level)
        {
            level = Mathf.Clamp(level, 0, MaxPickUpLevel);
            return MaxPickUpLevels[level].Value;
        }

        public int GetMaxRelease(int level)
        {
            level = Mathf.Clamp(level, 0, MaxReleaseLevel);
            return MaxReleaseLevels[level].Value;
        }
    }
}
