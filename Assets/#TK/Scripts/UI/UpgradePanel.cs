using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using TK.Data;

namespace TK.UI
{
    public class UpgradePanel : MainMenuPanel
    {
        [Header("Container & Prefab")]
        [SerializeField] private RectTransform cardContainer;
        [SerializeField] private UpgradeCard cardPrefab;

        [SerializeField] private Button closeButton;

        [Header("Upgrade Definitions Config")]
        [SerializeField] private UpgradeData globalUpgradeData;
        [SerializeField] private List<UpgradeItemData> upgradesList = new List<UpgradeItemData>();

        private List<UpgradeCard> spawnedCards = new List<UpgradeCard>();

        protected override void Awake()
        {
            base.Awake();

            if (cardContainer != null)
            {
                var existingLayout = cardContainer.GetComponent<UnityEngine.UI.LayoutGroup>();
                if (existingLayout == null)
                {
                    var layout = cardContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
                    layout.childAlignment = TextAnchor.MiddleCenter;
                    layout.spacing = 40f;
                    layout.childControlWidth = true;
                    layout.childControlHeight = true;
                    layout.childForceExpandWidth = false;
                    layout.childForceExpandHeight = false;
                }

                foreach (Transform child in cardContainer)
                {
                    child.gameObject.SetActive(false);
                }
            }

            if (closeButton != null)
                closeButton.onClick.AddListener(() => Hide());
        }

        public override void Show(bool immediate = false)
        {
            base.Show(immediate);
            
            if (TK.MainMenu.MainMenuManager.Instance != null)
            {
                TK.MainMenu.MainMenuManager.Instance.BringCoinUIToFront();
            }
            
            UpdateUpgradeUI();
        }

        public void UpdateUpgradeUI()
        {
            if (cardContainer == null || cardPrefab == null) return;

            int currentCoins = UpgradeSaveSystem.LoadCoins();

            int totalUpgrades = upgradesList.Count;

            if (cardContainer != null)
            {
                for (int i = cardContainer.childCount - 1; i >= 0; i--)
                {
                    Transform child = cardContainer.GetChild(i);
                    UpgradeCard uc = child.GetComponent<UpgradeCard>();
                    if (uc == null || !spawnedCards.Contains(uc))
                    {
                        child.gameObject.SetActive(false);
                    }
                }
            }

            foreach (var card in spawnedCards)
            {
                card.gameObject.SetActive(false);
            }

            int cardIndex = 0;

            for (int i = 0; i < totalUpgrades; i++)
            {
                UpgradeItemData data = upgradesList[i];
                if (data == null) continue;

                int currentLevel = LoadUpgradeLevel(data.upgradeKey);
                
                int maxLevel = 0;
                int nextCost = 0;

                if (globalUpgradeData != null)
                {
                    if (data.upgradeKey == "upgrade_arm_level")
                    {
                        maxLevel = globalUpgradeData.MaxArmLevel;
                        if (currentLevel < maxLevel)
                            nextCost = globalUpgradeData.ArmLengthLevels[currentLevel].Cost;
                    }
                    else if (data.upgradeKey == "upgrade_pickup_level")
                    {
                        maxLevel = globalUpgradeData.MaxPickUpLevel;
                        if (currentLevel < maxLevel)
                            nextCost = globalUpgradeData.MaxPickUpLevels[currentLevel].Cost;
                    }
                    else if (data.upgradeKey == "upgrade_release_level")
                    {
                        maxLevel = globalUpgradeData.MaxReleaseLevel;
                        if (currentLevel < maxLevel)
                            nextCost = globalUpgradeData.MaxReleaseLevels[currentLevel].Cost;
                    }
                }
                else
                {
                    Debug.LogWarning("[UpgradePanel] globalUpgradeData is missing! Please assign it in the inspector.");
                }

                bool isMaxLevel = currentLevel >= maxLevel;
                bool canAfford = currentCoins >= nextCost;

                UpgradeCard cardInstance;
                if (cardIndex < spawnedCards.Count)
                {
                    cardInstance = spawnedCards[cardIndex];
                }
                else
                {
                    cardInstance = Instantiate(cardPrefab, cardContainer);
                    spawnedCards.Add(cardInstance);
                }

                cardInstance.gameObject.SetActive(true);

                cardInstance.Setup(data, currentLevel, isMaxLevel, canAfford, nextCost, () => TryUpgrade(data, nextCost, currentLevel));
                cardIndex++;
            }
        }

        private int LoadUpgradeLevel(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning("[UpgradePanel] Upgrade key is empty! Make sure your UpgradeItemData ScriptableObjects have an 'upgradeKey' assigned.");
                return 0;
            }

            if (key == "upgrade_arm_level") return UpgradeSaveSystem.LoadArmLevel();
            if (key == "upgrade_pickup_level") return UpgradeSaveSystem.LoadPickUpLevel();
            if (key == "upgrade_release_level") return UpgradeSaveSystem.LoadReleaseLevel();

            return PlayerPrefs.GetInt(key, 0);
        }

        private void SaveUpgradeLevel(string key, int newLevel)
        {
            if (string.IsNullOrEmpty(key)) return;

            if (key == "upgrade_arm_level") UpgradeSaveSystem.SaveArmLevel(newLevel);
            else if (key == "upgrade_pickup_level") UpgradeSaveSystem.SavePickUpLevel(newLevel);
            else if (key == "upgrade_release_level") UpgradeSaveSystem.SaveReleaseLevel(newLevel);
            else
            {
                PlayerPrefs.SetInt(key, newLevel);
                PlayerPrefs.Save();
            }
        }

        private void TryUpgrade(UpgradeItemData data, int cost, int currentLevel)
        {
            int currentCoins = UpgradeSaveSystem.LoadCoins();
            if (currentCoins < cost) return;

            UpgradeSaveSystem.SaveCoins(currentCoins - cost);

            SaveUpgradeLevel(data.upgradeKey, currentLevel + 1);

            if (TK.Gameplay.GameManager.Instance != null)
            {
                TK.Gameplay.GameManager.Instance.ReloadUpgradeLevels();
            }

            if (TK.Audio.AudioManager.Instance != null)
            {
                TK.Audio.AudioManager.Instance.PlayButtonClick();
            }

            UpdateUpgradeUI();
        }
    }
}
