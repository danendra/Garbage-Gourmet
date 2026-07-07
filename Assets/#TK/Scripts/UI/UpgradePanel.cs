using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TK.UI
{
    public class UpgradePanel : MainMenuPanel
    {
        [Header("Currency UI")]
        [SerializeField] private TextMeshProUGUI coinsText;

        [Header("Upgrade Buttons")]
        [SerializeField] private Button upgradeArmLengthButton;
        [SerializeField] private Button upgradeArmSpeedButton;
        [SerializeField] private Button upgradeCapacityButton;
        [SerializeField] private Button closeButton;

        [Header("Upgrade Cost & Level Labels")]
        [SerializeField] private TextMeshProUGUI armLengthLabel;
        [SerializeField] private TextMeshProUGUI armSpeedLabel;
        [SerializeField] private TextMeshProUGUI capacityLabel;

        protected override void Awake()
        {
            base.Awake();

            if (closeButton != null)
                closeButton.onClick.AddListener(() => Hide());

            if (upgradeArmLengthButton != null)
                upgradeArmLengthButton.onClick.AddListener(OnUpgradeArmLengthClicked);

            if (upgradeArmSpeedButton != null)
                upgradeArmSpeedButton.onClick.AddListener(OnUpgradeArmSpeedClicked);

            if (upgradeCapacityButton != null)
                upgradeCapacityButton.onClick.AddListener(OnUpgradeCapacityClicked);
        }

        protected override void OnShown()
        {
            base.OnShown();
            UpdateUpgradeUI();
        }

        public void UpdateUpgradeUI()
        {
            // Placeholder method: update coin text, cost texts, button interactability, etc.
            if (coinsText != null)
                coinsText.text = "100"; // Fallback placeholder
        }

        private void OnUpgradeArmLengthClicked()
        {
            Debug.Log("[UpgradePanel] Upgrade Arm Length Clicked!");
            // Implement logic here when ready
            UpdateUpgradeUI();
        }

        private void OnUpgradeArmSpeedClicked()
        {
            Debug.Log("[UpgradePanel] Upgrade Arm Speed Clicked!");
            // Implement logic here when ready
            UpdateUpgradeUI();
        }

        private void OnUpgradeCapacityClicked()
        {
            Debug.Log("[UpgradePanel] Upgrade Capacity Clicked!");
            // Implement logic here when ready
            UpdateUpgradeUI();
        }
    }
}
