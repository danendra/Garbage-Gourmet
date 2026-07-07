using UnityEngine;
using UnityEngine.UI;

namespace TK.UI
{
    public class CollectionPanel : MainMenuPanel
    {
        [Header("Collection UI References")]
        [SerializeField] private RectTransform itemGridContainer;
        [SerializeField] private GameObject itemSlotPrefab;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Button closeButton;

        protected override void Awake()
        {
            base.Awake();

            if (closeButton != null)
                closeButton.onClick.AddListener(() => Hide());
        }

        protected override void OnShown()
        {
            base.OnShown();
            PopulateCollectionGrid();
        }

        public void PopulateCollectionGrid()
        {
            // Placeholder method: clear current grid items and instantiate item slot prefabs
            if (itemGridContainer == null || itemSlotPrefab == null) return;

            // Clear old children (except template if it's placed inside the grid)
            foreach (Transform child in itemGridContainer)
            {
                if (child.gameObject != itemSlotPrefab)
                {
                    Destroy(child.gameObject);
                }
            }

            // In the future, loop through player's collection data and instantiate:
            // GameObject slot = Instantiate(itemSlotPrefab, itemGridContainer);
            // slot.SetActive(true);
        }
    }
}
