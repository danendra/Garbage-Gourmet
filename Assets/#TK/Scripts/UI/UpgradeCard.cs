using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TK.Data;

namespace TK.UI
{
    public class UpgradeCard : MonoBehaviour
    {
        [Header("UI Component References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button upgradeButton;

        public void Setup(UpgradeItemData data, int currentLevel, bool isMaxLevel, bool canAfford, int nextCost, System.Action onUpgradeClick)
        {
            if (iconImage != null) iconImage.sprite = data.icon;
            if (titleText != null) titleText.text = data.title;
            if (descriptionText != null) descriptionText.text = data.description;

            if (levelText != null)
            {
                levelText.text = $"Level {currentLevel + 1}";
            }

            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveAllListeners();

                if (isMaxLevel)
                {
                    if (costText != null) costText.text = "Maxed";
                    upgradeButton.interactable = false;
                }
                else
                {
                    if (costText != null) costText.text = $"Upgrade | <sprite=0> {nextCost.ToString("N0")}";

                    upgradeButton.interactable = canAfford;
                    upgradeButton.onClick.AddListener(() => onUpgradeClick?.Invoke());
                }
            }
        }
    }
}
