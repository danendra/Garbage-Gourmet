using UnityEngine;

namespace TK.Data
{
    /// <summary>
    /// ScriptableObject storing configurable player data, currencies, and settings.
    /// Follows best practices to prevent hardcoding gameplay values.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerData", menuName = "TK/Data/PlayerData")]
    public class PlayerData : ScriptableObject
    {
        [Header("Starting Configuration")]
        [SerializeField] private int startingCoins = 100;
        [SerializeField] private int startingLevel = 1;

        [Header("Upgrade Settings")]
        [SerializeField] private float upgradeCostMultiplier = 1.5f;
        [SerializeField] private int maxUpgradeTier = 5;

        [Header("Player Settings (Volume/State)")]
        [SerializeField] private bool musicEnabled = true;
        [SerializeField] private bool sfxEnabled = true;

        // Public properties to access data
        public int StartingCoins => startingCoins;
        public int StartingLevel => startingLevel;
        public float UpgradeCostMultiplier => upgradeCostMultiplier;
        public int MaxUpgradeTier => maxUpgradeTier;

        public bool MusicEnabled
        {
            get => musicEnabled;
            set => musicEnabled = value;
        }

        public bool SfxEnabled
        {
            get => sfxEnabled;
            set => sfxEnabled = value;
        }

        /// <summary>
        /// Resets data to default values.
        /// </summary>
        public void ResetData()
        {
            musicEnabled = true;
            sfxEnabled = true;
        }
    }
}
