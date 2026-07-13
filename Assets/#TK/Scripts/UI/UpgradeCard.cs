using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TK.Data;

namespace TK.UI
{
    using Anoa;
    public class UpgradeCard : MonoBehaviour
    {
        [Header("UI Component References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button upgradeButton;

        [Header("Button State Sprites")]
        [SerializeField] private Sprite affordableSprite;
        [SerializeField] private Sprite unaffordableSprite;
        [SerializeField] private Sprite maxedSprite;

        [Header("Text Sprite Assets")]
        [SerializeField] private TMP_SpriteAsset affordableStarAsset;
        [SerializeField] private TMP_SpriteAsset unaffordableStarAsset;

        public void Setup(UpgradeItemData data, int currentLevel, bool isMaxLevel, bool canAfford, int nextCost, System.Action onUpgradeClick)
        {
            if (iconImage != null) iconImage.sprite = data.icon;
            if (titleText != null) titleText.text = data.title;
            if (descriptionText != null) descriptionText.text = data.description;

            if (levelText != null)
            {
                levelText.text = $"Level {currentLevel}";
            }

            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveAllListeners();

                if (isMaxLevel)
                {
                    if (costText != null) costText.text = "Maxed";
                    
                    if (upgradeButton.image != null)
                    {
                        if (maxedSprite != null)
                        {
                            upgradeButton.image.sprite = maxedSprite;
                        }
                        else if (unaffordableSprite != null)
                        {
                            upgradeButton.image.sprite = unaffordableSprite;
                        }
                    }

                    upgradeButton.interactable = false;
                }
                else
                {
                    if (costText != null) 
                    {
                        if (canAfford)
                        {
                            if (affordableStarAsset != null) costText.spriteAsset = affordableStarAsset;
                        }
                        else
                        {
                            if (unaffordableStarAsset != null) costText.spriteAsset = unaffordableStarAsset;
                        }
                        
                        costText.text = $"Upgrade | <sprite=0> {AnoaModule.ConvertCurency(nextCost)}";
                    }

                    if (upgradeButton.image != null)
                    {
                        if (canAfford && affordableSprite != null)
                        {
                            upgradeButton.image.sprite = affordableSprite;
                        }
                        else if (!canAfford && unaffordableSprite != null)
                        {
                            upgradeButton.image.sprite = unaffordableSprite;
                        }
                    }

                    upgradeButton.interactable = canAfford;
                    upgradeButton.onClick.AddListener(() => onUpgradeClick?.Invoke());
                }
            }
        }
    }
}
