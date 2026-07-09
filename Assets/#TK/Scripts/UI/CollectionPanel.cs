using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TK.Data;
using TMPro;

namespace TK.UI
{
    [System.Serializable]
    public struct PageButtonData
    {
        public Button button;
        public Sprite activeSprite;
        public Sprite inactiveSprite;
    }

    public enum PageLayoutType
    {
        Grid6x,
        Poster
    }

    [System.Serializable]
    public struct CollectionPageConfig
    {
        public PageLayoutType layoutType;
        public List<RecipeData> pageItems; 
        public Sprite posterBackground;
        [Tooltip("If true, the background image will automatically resize its RectTransform to match the sprite's exact pixel size.")]
        public bool useNativeBackgroundSize;
        [Tooltip("Optional: A prefab containing unique decorations (like coins or flags) for this specific poster.")]
        public GameObject posterDecorationsPrefab;
    }

    public class CollectionPanel : MainMenuPanel
    {
        [Header("Collection UI References")]
        [SerializeField] private RectTransform itemGridContainer;
        [SerializeField] private CollectionCardUI itemSlotPrefab;
        [SerializeField] private Button closeButton;

        [Header("Poster UI References")]
        [SerializeField] private GameObject posterContainer;
        [SerializeField] private Transform posterDecorationContainer;
        [SerializeField] private Image posterBackgroundImage;
        [SerializeField] private Image posterFoodImage;
        [SerializeField] private TextMeshProUGUI posterTitle;
        [SerializeField] private TextMeshProUGUI posterDesc;
        [Header("Poster Locked State UI")]
        [SerializeField] private GameObject posterLockedTitleScribble;
        [SerializeField] private GameObject posterLockedDescScribble;
        [SerializeField] private Color unlockedColor = Color.white;
        [SerializeField] private Color lockedShadowColor = Color.black;

        [Header("Pagination & Data")]
        [SerializeField] private PageButtonData[] pageButtons;
        [SerializeField] private List<CollectionPageConfig> pages;

        [Header("Settings")]
        [SerializeField] private int cardsPerPage = 6;

        private List<CollectionCardUI> activeCards = new List<CollectionCardUI>();
        private int currentPageIndex = 0;
        private GameObject currentDecorationsInstance;

        protected override void Awake()
        {
            base.Awake();

            if (itemGridContainer != null)
            {
                foreach (Transform child in itemGridContainer)
                {
                    child.gameObject.SetActive(false);
                }
            }

            if (closeButton != null)
                closeButton.onClick.AddListener(() => Hide());

            for (int i = 0; i < pageButtons.Length; i++)
            {
                int index = i;
                if (pageButtons[i].button != null)
                {
                    pageButtons[i].button.onClick.AddListener(() => 
                    {
                        if (TK.Audio.AudioManager.Instance != null)
                        {
                            TK.Audio.AudioManager.Instance.PlayPageTurn();
                        }
                        ShowPage(index);
                    });
                }
            }
        }

        public override void Show(bool immediate = false)
        {
            base.Show(immediate);

            InitializeGrid();
            currentPageIndex = -1;
            ShowPage(0);
        }

        private void InitializeGrid()
        {
            if (itemGridContainer == null || itemSlotPrefab == null) return;

            while (activeCards.Count < cardsPerPage)
            {
                CollectionCardUI newCard = Instantiate(itemSlotPrefab, itemGridContainer);
                activeCards.Add(newCard);
            }
        }

        public void ShowPage(int pageIndex)
        {
            if (pages == null || pageIndex < 0 || pageIndex >= pages.Count) return;
            
            currentPageIndex = pageIndex;
            CollectionPageConfig config = pages[pageIndex];

            if (config.layoutType == PageLayoutType.Grid6x)
            {
                if (itemGridContainer != null) itemGridContainer.gameObject.SetActive(true);
                if (posterContainer != null) posterContainer.SetActive(false);
                
                SetupGrid(config);
            }
            else if (config.layoutType == PageLayoutType.Poster)
            {
                if (itemGridContainer != null) itemGridContainer.gameObject.SetActive(false);
                if (posterContainer != null) posterContainer.SetActive(true);
                
                SetupPoster(config);
            }

            UpdateButtonVisuals(pageIndex);
        }

        private void SetupGrid(CollectionPageConfig config)
        {
            if (activeCards.Count == 0) return;

            for (int i = 0; i < cardsPerPage; i++)
            {
                CollectionCardUI card = activeCards[i];

                if (i < config.pageItems.Count)
                {
                    RecipeData collectionData = config.pageItems[i];
                    
                    // TODO: Connect to SaveData system to check if 'collectionData' is unlocked by the player
                    bool isUnlocked = true; 
                    
                    card.Setup(collectionData, isUnlocked);
                    
                    // Ensure the card's game object is active in case it was disabled
                    card.gameObject.SetActive(true);
                }
                else
                {
                    card.Clear();
                    // Optional: You can choose to set it inactive if you don't want empty grid slots to show
                    // card.gameObject.SetActive(false); 
                }
            }
        }

        private void SetupPoster(CollectionPageConfig config)
        {
            if (config.pageItems == null || config.pageItems.Count == 0) return;

            RecipeData data = config.pageItems[0]; 

            // TODO: Connect to SaveData system to check if 'data' is unlocked by the player
            bool isUnlocked = true; 

            if (isUnlocked)
            {
                if (posterTitle != null) { posterTitle.gameObject.SetActive(true); posterTitle.text = data.CollectionName; }
                if (posterDesc != null) { posterDesc.gameObject.SetActive(true); posterDesc.text = data.CollectionDescription; }
                
                if (posterLockedTitleScribble != null) posterLockedTitleScribble.SetActive(false);
                if (posterLockedDescScribble != null) posterLockedDescScribble.SetActive(false);

                if (posterFoodImage != null) 
                {
                    posterFoodImage.sprite = data.CollectionImage;
                    posterFoodImage.color = unlockedColor;
                }
                
                if (posterBackgroundImage != null)
                {
                    posterBackgroundImage.sprite = config.posterBackground;
                    posterBackgroundImage.color = unlockedColor;
                    if (config.useNativeBackgroundSize) posterBackgroundImage.SetNativeSize();
                }

                // Spawn decorations
                if (currentDecorationsInstance != null) Destroy(currentDecorationsInstance);
                if (config.posterDecorationsPrefab != null && posterDecorationContainer != null)
                {
                    currentDecorationsInstance = Instantiate(config.posterDecorationsPrefab, posterDecorationContainer);
                }
            }
            else
            {
                if (posterTitle != null) posterTitle.gameObject.SetActive(false);
                if (posterDesc != null) posterDesc.gameObject.SetActive(false);
                
                if (posterLockedTitleScribble != null) posterLockedTitleScribble.SetActive(true);
                if (posterLockedDescScribble != null) posterLockedDescScribble.SetActive(true);

                if (posterFoodImage != null) 
                {
                    posterFoodImage.sprite = data.CollectionImage;
                    posterFoodImage.color = lockedShadowColor;
                }
                
                if (posterBackgroundImage != null)
                {
                    posterBackgroundImage.sprite = config.posterBackground;
                    posterBackgroundImage.color = lockedShadowColor;
                    if (config.useNativeBackgroundSize) posterBackgroundImage.SetNativeSize();
                }

                // Do not spawn decorations when locked
                if (currentDecorationsInstance != null) Destroy(currentDecorationsInstance);
            }
        }

        private void UpdateButtonVisuals(int activeIndex)
        {
            for (int i = 0; i < pageButtons.Length; i++)
            {
                if (pageButtons[i].button != null)
                {
                    Image btnImage = pageButtons[i].button.GetComponent<Image>();
                    if (btnImage != null)
                    {
                        if (i == activeIndex && pageButtons[i].activeSprite != null)
                        {
                            btnImage.sprite = pageButtons[i].activeSprite;
                        }
                        else if (i != activeIndex && pageButtons[i].inactiveSprite != null)
                        {
                            btnImage.sprite = pageButtons[i].inactiveSprite;
                        }
                    }
                }
            }
        }
    }
}
