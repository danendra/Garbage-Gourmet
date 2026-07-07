using UnityEngine;

namespace TK.Data
{
    public static class UpgradeSaveSystem
    {
        private const string KEY_ARM_LEVEL    = "upgrade_arm_level";
        private const string KEY_PICKUP_LEVEL = "upgrade_pickup_level";
        private const string KEY_RELEASE_LEVEL = "upgrade_release_level";

        // ── Arm Length ───────────────────────────────────────────────────────────
        public static void SaveArmLevel(int level)
        {
            PlayerPrefs.SetInt(KEY_ARM_LEVEL, level);
            PlayerPrefs.Save();
        }

        public static int LoadArmLevel()
        {
            return PlayerPrefs.GetInt(KEY_ARM_LEVEL, 1);
        }

        // ── Max Pick Up ──────────────────────────────────────────────────────────
        public static void SavePickUpLevel(int level)
        {
            PlayerPrefs.SetInt(KEY_PICKUP_LEVEL, level);
            PlayerPrefs.Save();
        }

        public static int LoadPickUpLevel()
        {
            return PlayerPrefs.GetInt(KEY_PICKUP_LEVEL, 0);
        }

        public static void SaveReleaseLevel(int level)
        {
            PlayerPrefs.SetInt(KEY_RELEASE_LEVEL, level);
            PlayerPrefs.Save();
        }

        public static int LoadReleaseLevel()
        {
            return PlayerPrefs.GetInt(KEY_RELEASE_LEVEL, 0);
        }

        public static void ResetAll()
        {
            PlayerPrefs.DeleteKey(KEY_ARM_LEVEL);
            PlayerPrefs.DeleteKey(KEY_PICKUP_LEVEL);
            PlayerPrefs.DeleteKey(KEY_RELEASE_LEVEL);
            PlayerPrefs.Save();
        }
    }
}
