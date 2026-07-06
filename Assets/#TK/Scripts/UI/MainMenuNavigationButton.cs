using UnityEngine;
using UnityEngine.UI;
using TK.Managers;
using TK.Data;

namespace TK.UI
{
    /// <summary>
    /// Attached to each bottom navigation tab button in the Main Menu.
    /// Delegates page switching to PageManager and applies visual selected/unselected state.
    /// </summary>
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(RectTransform))]
    public class MainMenuNavigationButton : MonoBehaviour
    {
        public enum PageType { Stall, Upgrade, Settings }

        [Header("Navigation")]
        [SerializeField] private PageType targetPage;

        [Header("Visual References")]
        [SerializeField] private GameObject selectedOverlay;
        [SerializeField] private RectTransform iconRect;
        [SerializeField] private RectTransform textRect;

        // Cached components
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            GetComponent<Button>().onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            if (PageManager.Instance == null)
            {
                Debug.LogWarning("[MainMenuNavigationButton] PageManager instance not found!");
                return;
            }

            switch (targetPage)
            {
                case PageType.Stall:    PageManager.Instance.ShowPage(PageType.Stall);    break;
                case PageType.Upgrade:  PageManager.Instance.ShowPage(PageType.Upgrade);  break;
                case PageType.Settings: PageManager.Instance.ShowPage(PageType.Settings); break;
            }
        }

        /// <summary>
        /// Applies the selected or unselected visual state to this tab button.
        /// </summary>
        public void SetSelected(bool isSelected, MainMenuUIConfig config)
        {
            if (config == null) return;

            if (selectedOverlay != null)
                selectedOverlay.SetActive(isSelected);

            if (textRect != null)
            {
                textRect.gameObject.SetActive(isSelected);
                if (isSelected)
                {
                    textRect.offsetMin = new Vector2(textRect.offsetMin.x, config.SelectedTextBottomOffset);
                    textRect.offsetMax = new Vector2(textRect.offsetMax.x, config.SelectedTextTopOffset);
                }
            }

            if (iconRect != null)
            {
                float scale = isSelected ? config.SelectedIconScale : config.UnselectedIconScale;
                iconRect.localScale = new Vector3(scale, scale, scale);

                float parentHeight = rectTransform.rect.height;
                float anchorY = iconRect.anchorMin.y;
                float targetY = isSelected
                    ? parentHeight * (1f - anchorY)
                    : parentHeight * (0.5f - anchorY);

                iconRect.anchoredPosition = new Vector2(iconRect.anchoredPosition.x, targetY);
            }
        }
    }
}
