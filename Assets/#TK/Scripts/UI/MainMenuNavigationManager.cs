using UnityEngine;

namespace TK.UI
{
    public class MainMenuNavigationManager : MonoBehaviour
    {
        public static MainMenuNavigationManager Instance { get; private set; }

        [Header("Menu Panels")]
        [SerializeField] private SettingsPanel settingsPanel;
        [SerializeField] private UpgradePanel upgradePanel;
        [SerializeField] private CollectionPanel collectionPanel;

        [Header("Navigation Buttons")]
        [SerializeField] private UnityEngine.UI.Button openSettingsButton;
        [SerializeField] private UnityEngine.UI.Button openUpgradeButton;
        [SerializeField] private UnityEngine.UI.Button openCollectionButton;

        private MainMenuPanel activePanel;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Bind navigation button listeners
            if (openSettingsButton != null)
                openSettingsButton.onClick.AddListener(OpenSettings);
            if (openUpgradeButton != null)
                openUpgradeButton.onClick.AddListener(OpenUpgrade);
            if (openCollectionButton != null)
                openCollectionButton.onClick.AddListener(OpenCollection);

            // Initially hide all panels immediately
            CloseAllImmediately();
        }

        public void OpenSettings() => OpenPanel(settingsPanel);
        public void OpenUpgrade() => OpenPanel(upgradePanel);
        public void OpenCollection() => OpenPanel(collectionPanel);

        public void OpenPanel(MainMenuPanel targetPanel)
        {
            if (targetPanel == null) return;

            // Toggle behavior: if the clicked panel is already open, close it
            if (activePanel == targetPanel && targetPanel.IsShown)
            {
                CloseActivePanel();
                return;
            }

            // If there's an active panel, hide it
            if (activePanel != null && activePanel.IsShown)
            {
                // Pass immediate:true to prevent playing click sound twice
                activePanel.Hide(true);
            }

            // Show the new target panel
            activePanel = targetPanel;
            activePanel.Show();
        }

        public void CloseActivePanel()
        {
            if (activePanel != null)
            {
                activePanel.Hide();
                activePanel = null;
            }
        }

        public void CloseAllImmediately()
        {
            if (settingsPanel != null) settingsPanel.Hide(true);
            if (upgradePanel != null) upgradePanel.Hide(true);
            if (collectionPanel != null) collectionPanel.Hide(true);
            activePanel = null;
        }
    }
}
