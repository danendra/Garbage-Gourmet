using UnityEngine;
using TK.UI;
using TK.Data;

namespace TK.Managers
{
    /// <summary>
    /// Singleton manager responsible for switching between Main Menu pages
    /// and keeping navigation tab visuals in sync.
    /// </summary>
    public class PageManager : MonoBehaviour
    {
        public static PageManager Instance { get; private set; }

        [Header("Pages")]
        [SerializeField] private GameObject stallPage;
        [SerializeField] private GameObject upgradePage;
        [SerializeField] private GameObject settingsPage;

        [Header("UI Styling Config")]
        [SerializeField] private MainMenuUIConfig uiConfig;

        [Header("Navigation Tab Buttons")]
        [SerializeField] private MainMenuNavigationButton stallButton;
        [SerializeField] private MainMenuNavigationButton upgradeButton;
        [SerializeField] private MainMenuNavigationButton settingsButton;

        private GameObject[] pages;
        private MainMenuNavigationButton[] buttons;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning($"[PageManager] Duplicate instance detected on '{gameObject.name}'. Destroying.");
                Destroy(gameObject);
                return;
            }

            pages   = new[] { stallPage,   upgradePage,   settingsPage };
            buttons = new[] { stallButton, upgradeButton, settingsButton };
        }

        private void Start()
        {
            ShowPage(MainMenuNavigationButton.PageType.Stall);
        }

        public void ShowStall()   => ShowPage(MainMenuNavigationButton.PageType.Stall);
        public void ShowUpgrade() => ShowPage(MainMenuNavigationButton.PageType.Upgrade);
        public void ShowSettings()=> ShowPage(MainMenuNavigationButton.PageType.Settings);

        /// <summary>
        /// Activates the requested page and deactivates all others.
        /// Updates every tab button's visual state to match.
        /// </summary>
        public void ShowPage(MainMenuNavigationButton.PageType activePage)
        {
            for (int i = 0; i < pages.Length; i++)
            {
                if (pages[i] != null)
                    pages[i].SetActive(i == (int)activePage);

                if (buttons[i] != null)
                    buttons[i].SetSelected(i == (int)activePage, uiConfig);
            }
        }
    }
}
