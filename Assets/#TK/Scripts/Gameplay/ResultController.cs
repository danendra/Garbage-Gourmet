using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Anoa;
using System.Linq;

namespace TK.Gameplay
{
    public class ResultController : MonoBehaviour
    {
        [SerializeField] private PlayerInventory _inventory;
        [SerializeField] private Transform _transSpawn;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        public void PlayResult()
        {
            StartCoroutine(IEPlayResult());
        }

        private IEnumerator IEPlayResult()
        {
            List<CollectibleController> _listCollectible = AnoaModule.RandomList(_inventory.HeldItems.ToArray());
            CollectibleController _collectibleBun;
            GameObject _object;

            //Move Bottom Bun to first
            _collectibleBun = _listCollectible.Find(_item => _item.GetType == ITEM_TYPE.Bottom_Bun);

            if (_collectibleBun)
            {
                _listCollectible.Remove(_collectibleBun);
                _listCollectible.Insert(0, _collectibleBun);
            }

            //Move Top Bun to last
            _collectibleBun = _listCollectible.Find(_item => _item.GetType == ITEM_TYPE.Top_Bun);

            if (_collectibleBun)
            {
                _listCollectible.Remove(_collectibleBun);
                _listCollectible.Insert(_listCollectible.Count, _collectibleBun);
            }

            foreach (CollectibleController _collectible in _listCollectible)
            {
                _object = Instantiate(_collectible.GetFoodServed, _transSpawn.position, quaternion.identity, _transSpawn);
                _object.transform.localScale = Vector3.one * 3;
                _object.SetActive(true);

                yield return new WaitForSeconds(0.3f);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}