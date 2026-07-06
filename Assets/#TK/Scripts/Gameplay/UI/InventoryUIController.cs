using UnityEngine;
using UnityEngine.UI;

namespace TK.UI
{
    using System.Collections;
    using Gameplay;

    public class InventoryUIController : MonoBehaviour
    {
        [SerializeField] protected Image[] _arrImageIcons;

        protected PlayerInventory _playerInventory;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            _playerInventory = LevelManager.Instance.GetPlayerInventory;
            _playerInventory.OnItemAdded += OnItemAdded;

            Initialize();
        }

        void OnDestroy()
        {
            _playerInventory.OnItemAdded -= OnItemAdded;
        }

        public void Initialize()
        {
            for (int i = 0; i < _arrImageIcons.Length; i++)
            {
                if(i < _playerInventory.MaxPickUpItems)
                {
                    _arrImageIcons[i].transform.parent.gameObject.SetActive(true);
                }
                else
                {
                    _arrImageIcons[i].transform.parent.gameObject.SetActive(false);
                }
                
                _arrImageIcons[i].gameObject.SetActive(false);
            }
        }

        public void OnItemAdded(CollectibleController _collectible)
        {
            _arrImageIcons[_playerInventory.HeldItems.Count - 1].sprite = _collectible.GetSprite;
            _arrImageIcons[_playerInventory.HeldItems.Count - 1].gameObject.SetActive(true);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}