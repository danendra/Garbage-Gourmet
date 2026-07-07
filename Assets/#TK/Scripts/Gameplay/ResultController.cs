using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Anoa;
using TK.Data;

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
            IReadOnlyList<CollectibleController> _listCollectible = _inventory.HeldItems.OrderByDescending(_collectible => _collectible.GetType).ToList();            
            GameObject _object;

            int _index = 6;

            foreach (CollectibleController _collectible in _listCollectible)
            {
                _object = Instantiate(_collectible.GetFoodServed, _transSpawn.position, quaternion.identity, _transSpawn);
                _object.transform.localScale = Vector3.one * 3;
                _object.GetComponent<SpriteRenderer>().sortingOrder = _index;
                _object.SetActive(true);

                yield return new WaitForSeconds(0.3f);

                _index++;
            }

            RecipeData _recipe = null;

            if (LevelManager.Instance.FindRecipe(_listCollectible, out _recipe))
            {
                Debug.Log(_recipe.name);
            }
            else
            {
                Debug.Log("Any Burger");
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}