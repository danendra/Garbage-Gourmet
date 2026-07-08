using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TK.Data;

namespace TK.UI
{
    [System.Serializable]
    public struct PageButtonData
    {
        public Button button;
        public Sprite activeSprite;
        public Sprite inactiveSprite;
    }

    public class CollectionPanel : MainMenuPanel
    {
        [Header("Collection UI References")]
        [SerializeField] private RectTransform itemGridContainer;
        [SerializeField] private CollectionCardUI itemSlotPrefab;
        [SerializeField] private Button closeButton;

        [Header("Pagination")]
        [SerializeField] private PageButtonData[] pageButtons;
        [SerializeField] private List<RecipeData> allCollections;

        [Header("Settings")]
        [SerializeField] private int cardsPerPage = 6;

        private List<CollectionCardUI> activeCards = new List<CollectionCardUI>();
        private int currentPageIndex = 0;

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
                    pageButtons[i].button.onClick.AddListener(() => ShowPage(index));
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
            if (activeCards.Count == 0) return;
            if (pageIndex == currentPageIndex && activeCards[0].gameObject.activeSelf) return;
            
            currentPageIndex = pageIndex;
            int startIndex = pageIndex * cardsPerPage;

            for (int i = 0; i < cardsPerPage; i++)
            {
                int dataIndex = startIndex + i;
                CollectionCardUI card = activeCards[i];

                if (dataIndex < allCollections.Count)
                {
                    RecipeData collectionData = allCollections[dataIndex];
                    
                    // Connect to SaveData system to check if 'collectionData' is unlocked by the player
                    bool isUnlocked = true; 
                    
                    card.Setup(collectionData, isUnlocked);
                }
                else
                {
                    card.Clear();
                }
            }
            
            UpdateButtonVisuals(pageIndex);
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
