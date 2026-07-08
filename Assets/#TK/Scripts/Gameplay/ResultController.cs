using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            ///
            /// IVAN disini flow animasi makannya ya
            /// tutup mulut dulu baru delay buat buka mulutnya
            /// 

            int _intScore = 0;

            IReadOnlyList<CollectibleController> _listCollectible = _inventory.HeldItems.OrderByDescending(_collectible => _collectible.GetType).ToList();
            List<Rigidbody2D> _listRB = new List<Rigidbody2D>();

            _objRaccoon.SetActive(true);

            GameObject _object;
            Rigidbody2D _rb;

            int _index = 6;

            foreach (CollectibleController _collectible in _listCollectible)
            {
                _object = Instantiate(_collectible.GetFoodServed, _collectible.transform.position + Vector3.up * 10.0f + Vector3.right * Random.Range(-2.0f, 2.0f), Quaternion.identity, _transSpawn);
                _rb = _object.GetComponent<Rigidbody2D>();
                _listRB.Add(_rb);

                _object.transform.localScale = Vector3.one;
                _object.GetComponent<SpriteRenderer>().sortingOrder = _index;

                _object.transform.DOMove(_transSpawn.position + Vector3.up * 10, 1.0f).SetEase(Ease.OutBack);                

                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));

                _index++;
            }

            yield return new WaitForSeconds(0.5f);

            _index = 0;

            foreach (CollectibleController _collectible in _listCollectible)
            {
                _rb = _listRB[_index];
                _object = _rb.gameObject;                

                _rb.WakeUp();
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0;
                _object.transform.position = _transSpawn.position;
                _object.transform.rotation = Quaternion.identity;

                _intScore += _collectible.GetScore;

                StartCoroutine(IEDelayShowScore(_collectible.GetScore, _object));

                yield return new WaitForSeconds(0.4f);

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

            ///
            /// IVAN disini kasih delay tutup mulut dan makan
            /// 

            GameManager.Instance.AddPoint(_intScore);

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

            yield return new WaitForSeconds(0.45f);

            txtScore.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}