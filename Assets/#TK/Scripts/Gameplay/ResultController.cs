using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

using Anoa.Module;
using DG.Tweening;
using TMPro;

namespace TK.Gameplay
{
    using Data;

    public class ResultController : MonoBehaviour
    {
        [SerializeField] private PlayerInventory _inventory;
        [SerializeField] private Transform _transSpawn;
        [SerializeField] private PoolerContainer poolScore;
        [SerializeField] private GameObject _objRaccoon;
        [SerializeField] private DOTweenAnimation _dgAnimation;

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
            int _intScore = 0;

            IReadOnlyList<CollectibleController> _listCollectible = _inventory.HeldItems.OrderByDescending(_collectible => _collectible.GetType).ToList();
            _objRaccoon.SetActive(true);

            GameObject _object;

            int _index = 6;

            foreach (CollectibleController _collectible in _listCollectible)
            {
                _object = Instantiate(_collectible.GetFoodServed, _transSpawn.position, quaternion.identity, _transSpawn);
                _object.transform.localScale = Vector3.one;
                _object.GetComponent<SpriteRenderer>().sortingOrder = _index;
                _object.SetActive(true);

                _intScore += _collectible.GetScore;

                StartCoroutine(IEDelayShowScore(_collectible.GetScore, _object));

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

            yield return new WaitForSeconds(1.0f);

            _dgAnimation.RecreateTweenAndPlay();

            do
            {
                yield return null;
            }
            while (!Input.GetMouseButtonUp(0));

            GameManager.Instance.LoadScene(0);
        }

        public IEnumerator IEDelayShowScore(int _intScore, GameObject _object)
        {
            yield return new WaitForSeconds(1.0f);

            TMP_Text txtScore = poolScore.Pop<TMP_Text>();
            txtScore.text = _intScore.ToString();
            txtScore.transform.position = _object.transform.position + Vector3.right * 0.2f;
            txtScore.gameObject.SetActive(true);
            txtScore.gameObject.GetComponent<DOTweenAnimation>().RecreateTweenAndPlay();

            yield return new WaitForSeconds(0.5f);

            txtScore.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}