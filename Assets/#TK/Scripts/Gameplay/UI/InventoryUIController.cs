using UnityEngine;
using UnityEngine.UI;

namespace TK.UI
{
    using System.Collections;
    using Gameplay;

    public class InventoryUIController : MonoBehaviour
    {
        [SerializeField] protected InventoryBoxController[] _arrInventoryBox;

        protected PlayerInventory _inventory;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            _inventory = LevelManager.Instance.GetPlayerInventory;
            _inventory.OnItemAdded += OnItemAdded;

            Initialize();
        }

        void OnDestroy()
        {
            _inventory.OnItemAdded -= OnItemAdded;
        }

        public void Initialize()
        {
            for (int i = 0; i < _arrInventoryBox.Length; i++)
            {
                if(i < _inventory.MaxPickUpItems)
                {
                    _arrInventoryBox[i].gameObject.SetActive(true);
                }
                else
                {
                    _arrInventoryBox[i].gameObject.SetActive(false);
                }                
            }
        }

        public void OnItemAdded(CollectibleController _collectible)
        {
            _arrInventoryBox[_inventory.HeldItems.Count - 1].AddItem(_collectible.GetSprite);            
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}