using UnityEngine;

namespace TK.Data
{
    public static class FTUESaveSystem
    {
        private const string KEY_FTUE_FIRST_LAUNCH_COMPLETED = "ftue_first_launch_completed";
        private const string KEY_FTUE_GAMEPLAY_COMPLETED = "ftue_gameplay_completed";
        private const string KEY_FTUE_RELEASE_COMPLETED = "ftue_release_completed";
        private const string KEY_FTUE_COLLECTION_COMPLETED = "ftue_collection_completed";
        private const string KEY_FTUE_UPGRADE_COMPLETED = "ftue_upgrade_completed";
        private const string KEY_FTUE_INVENTORY_UPGRADE_COMPLETED = "ftue_inventory_upgrade_completed";

        public static void SaveFTUEFirstLaunchCompleted(bool completed)
        {
            PlayerPrefs.SetInt(KEY_FTUE_FIRST_LAUNCH_COMPLETED, completed ? 1 : 0);
            PlayerPrefs.Save();
        }
        public static void SaveFTUEGameplayCompleted(bool completed)
        {
            PlayerPrefs.SetInt(KEY_FTUE_GAMEPLAY_COMPLETED, completed ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void SaveFTUEReleaseCompleted(bool completed)
        {
            PlayerPrefs.SetInt(KEY_FTUE_RELEASE_COMPLETED, completed ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void SaveFTUECollectionCompleted(bool completed)
        {
            PlayerPrefs.SetInt(KEY_FTUE_COLLECTION_COMPLETED, completed ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void SaveFTUEUpgradeCompleted(bool completed)
        {
            PlayerPrefs.SetInt(KEY_FTUE_UPGRADE_COMPLETED, completed ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void SaveFTUEInventoryUpgradeCompleted(bool completed)
        {
            PlayerPrefs.SetInt(KEY_FTUE_INVENTORY_UPGRADE_COMPLETED, completed ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static bool LoadFTUEFirstLaunchCompleted()
        {
            return PlayerPrefs.GetInt(KEY_FTUE_FIRST_LAUNCH_COMPLETED, 0) == 1;
        }

        public static bool LoadFTUEGameplayCompleted()
        {
            return PlayerPrefs.GetInt(KEY_FTUE_GAMEPLAY_COMPLETED, 0) == 1;
        }

        public static bool LoadFTUEReleaseCompleted()
        {
            return PlayerPrefs.GetInt(KEY_FTUE_RELEASE_COMPLETED, 0) == 1;
        }

        public static bool LoadFTUECollectionCompleted()
        {
            return PlayerPrefs.GetInt(KEY_FTUE_COLLECTION_COMPLETED, 0) == 1;
        }

        public static bool LoadFTUEUpgradeCompleted()
        {
            return PlayerPrefs.GetInt(KEY_FTUE_UPGRADE_COMPLETED, 0) == 1;
        }

        public static bool LoadFTUEInventoryUpgradeCompleted()
        {
            return PlayerPrefs.GetInt(KEY_FTUE_INVENTORY_UPGRADE_COMPLETED, 0) == 1;
        }

        // Ini buat testing ya kakak
        public static void ResetAll()
        {
            PlayerPrefs.DeleteKey(KEY_FTUE_FIRST_LAUNCH_COMPLETED);
            PlayerPrefs.DeleteKey(KEY_FTUE_GAMEPLAY_COMPLETED);
            PlayerPrefs.DeleteKey(KEY_FTUE_RELEASE_COMPLETED);
            PlayerPrefs.DeleteKey(KEY_FTUE_COLLECTION_COMPLETED);
            PlayerPrefs.DeleteKey(KEY_FTUE_UPGRADE_COMPLETED);
            PlayerPrefs.DeleteKey(KEY_FTUE_INVENTORY_UPGRADE_COMPLETED);
            PlayerPrefs.Save();
        }
    }
}