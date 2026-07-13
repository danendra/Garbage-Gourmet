using UnityEngine;
using TK.MainMenu;

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

        public MainMenuPanel ActivePanel => activePanel;

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

            if (openSettingsButton != null)
                openSettingsButton.onClick.AddListener(OpenSettings);
            if (openUpgradeButton != null)
                openUpgradeButton.onClick.AddListener(OpenUpgrade);
            if (openCollectionButton != null)
                openCollectionButton.onClick.AddListener(OpenCollection);

            CloseAllImmediately();
        }

        public void OpenSettings() => OpenPanel(settingsPanel);
        public void OpenUpgrade() => OpenPanel(upgradePanel);
        public void OpenCollection() => OpenPanel(collectionPanel);

        public void OpenPanel(MainMenuPanel targetPanel)
        {
            if (targetPanel == null) return;

            if (activePanel == targetPanel && targetPanel.IsShown)
            {
                CloseActivePanel();
                return;
            }

            if (activePanel != null && activePanel.IsShown)
            {
                activePanel.Hide(true);
            }

            activePanel = targetPanel;
            activePanel.transform.SetAsLastSibling();
            activePanel.Show();

            if (MainMenuManager.Instance != null)
            {
                MainMenuManager.Instance.SetMenuButtonsState(false);
                if (targetPanel == upgradePanel)
                {
                    MainMenuManager.Instance.BringCoinUIToFront();
                }
            }
        }

        public void CloseActivePanel()
        {
            if (activePanel != null)
            {
                activePanel.Hide();
                activePanel = null;
            }

            if (MainMenuManager.Instance != null)
            {
                MainMenuManager.Instance.SetMenuButtonsState(true);
            }
        }

        public void NotifyPanelClosed(MainMenuPanel panel)
        {
            if (activePanel == panel)
            {
                activePanel = null;
                if (MainMenuManager.Instance != null)
                {
                    MainMenuManager.Instance.SetMenuButtonsState(true);
                }
            }
        }

        public void CloseAllImmediately()
        {
            if (settingsPanel != null) settingsPanel.Hide(true);
            if (upgradePanel != null) upgradePanel.Hide(true);
            if (collectionPanel != null) collectionPanel.Hide(true);
            activePanel = null;
        }

        public void SetUpgradeFTUEButtonsState(bool state)
        {
            if (openSettingsButton != null)
                openSettingsButton.interactable = state;
            if (openCollectionButton != null)
                openCollectionButton.interactable = state;
        }

        public void SetCollectionFTUEButtonsState(bool state)
        {
            if (openSettingsButton != null)
                openSettingsButton.interactable = state;
            if (openUpgradeButton != null)
                openUpgradeButton.interactable = state;
        }
    }
}
