using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Anoa.Module;
using DG.Tweening;
using TMPro;

namespace TK.Gameplay
{
    using Anoa;
    using Data;
    using TK.Audio;
    using UI;

    public class ResultController : MonoBehaviour
    {
        [SerializeField] private PlayerInventory _inventory;
        [SerializeField] private Transform _transSpawn;
        [SerializeField] private GameObject _objMask;
        [SerializeField] private PoolerContainer poolScore;
        [SerializeField] private PoolerContainer poolMultiplier;
        [SerializeField] private RaccoonVisual _raccoonVisual;
        [SerializeField] private DOTweenAnimation _dgAnimation;
        [SerializeField] private GameObject _objVomit;

        public void PlayResult()
        {
            StartCoroutine(IEPlayResult());
        }

        private IEnumerator IEPlayResult()
        {
            _raccoonVisual.ChangeStateToIdle();
            yield return new WaitForSeconds(0.5f);
            _raccoonVisual.ChangeStateToEat();

            int _intScore = 0;

            IReadOnlyList<CollectibleController> _listCollectible = _inventory.HeldItems.OrderByDescending(_collectible => _collectible.GetType).ThenBy(_collectible => _collectible.GetRarity).ToList();
            List<Rigidbody2D> _listRB = new List<Rigidbody2D>();

            _objMask.SetActive(false);
            UIManager.Instance.HideGameplay();

            GameObject _object;
            Rigidbody2D _rb;

            int _index = 6;

            foreach (CollectibleController _collectible in _listCollectible)
            {
                _object = Instantiate(_collectible.GetFoodServed, _collectible.transform.position + Vector3.up * 10.0f + Vector3.right * Random.Range(-2.0f, 2.0f), Quaternion.identity, _transSpawn);
                _rb = _object.GetComponent<Rigidbody2D>();
                _listRB.Add(_rb);

                _rb.gravityScale = 0;
                _object.transform.localScale = Vector3.one * 2;
                _object.GetComponentInChildren<SpriteRenderer>().sortingOrder = _index;

                _object.transform.DOMove(_transSpawn.position + Vector3.up * 10, 1.0f).SetEase(Ease.OutBack);

                AudioManager.Instance.PlaySFX(SFXId.Rise);

                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));

                _index++;
            }

            yield return new WaitForSeconds(0.5f);

            _index = 0;
            _objMask.SetActive(true);

            bool _isFinalBurger = false;

            AudioManager.Instance.PlaySFX(SFXId.Fall);

            foreach (CollectibleController _collectible in _listCollectible)
            {
                if (_collectible.GetRarity == RARITY.Legendary)
                {
                    _isFinalBurger = true;
                }

                _rb = _listRB[_index];
                _object = _rb.gameObject;

                _rb.freezeRotation = true;
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0;
                _object.transform.position = _transSpawn.position;
                _object.transform.rotation = Quaternion.identity;
                _rb.gravityScale = 1;

                _intScore += Mathf.RoundToInt(_collectible.GetScore * _collectible.GetMultiplier);

                StartCoroutine(IEDelayShowScore(_collectible, _object));

                yield return new WaitForSeconds(0.4f);

                _index++;
            }

            RecipeData _recipe = null;
            bool _isTrash = false;

            if (_listCollectible.Count(_item => _item.GetType == ITEM_TYPE.Trash) > 0)
            {
                _isTrash = true;
            }
            else
            {
                if (LevelManager.Instance.FindRecipe(_listCollectible, out _recipe))
                {
                    Debug.Log(_recipe.name);
                }
                else
                {
                    Debug.Log("Any Burger");
                }
            }

            yield return new WaitForSeconds(1.0f);

            _dgAnimation.RecreateTweenAndPlay();

            yield return new WaitForSeconds(0.5f);

            if (_isTrash)
            {
                AudioManager.Instance.PlayEatTrash();
                TK.Audio.HapticManager.PlayImpact(TK.Audio.HapticImpactType.Heavy);
                _objVomit.SetActive(true);
            }
            else
            {
                AudioManager.Instance.PlayEatFood();
                TK.Audio.HapticManager.PlayImpact(TK.Audio.HapticImpactType.Light);
            }

            yield return new WaitForSeconds(0.5f);

            if (!_isTrash)
            {
                AudioManager.Instance.PlayBurp();
                TK.Audio.HapticManager.PlayImpact(TK.Audio.HapticImpactType.Light);
            }

            // deactivate rb game object
            foreach (Rigidbody2D _rbItem in _listRB)
            {
                _rbItem.gameObject.SetActive(false);
            }

            // if final burger, change to explode, else idle
            if (_isFinalBurger)
            {
                _raccoonVisual.ChangeStateToIdle();
                yield return new WaitForSeconds(1f);   
                _raccoonVisual.ChangeStateToExplode();
            } 
            else
            {
                if (!_isTrash)
                {
                    _raccoonVisual.ChangeStateToIdle();
                }
            }        

            if (_isFinalBurger)
            {
                GameManager.Instance.ResetCummulative();
                yield return new WaitForSeconds(8f);
                GameManager.Instance.LoadScene("MainMenu");
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
                UIManager.Instance.GetUIResult.Initialize(_listCollectible.ToArray(), _recipe, _intScore, _isTrash);
            }
        }

        public IEnumerator IEDelayShowScore(CollectibleController _collectible, GameObject _object)
        {
            yield return new WaitForSeconds(1.0f);

            TMP_Text txtScore = poolScore.Pop<TMP_Text>();
            TMP_Text txtMultiplier = poolMultiplier.Pop<TMP_Text>();
            int _intTarget = Mathf.RoundToInt(_collectible.GetScore * _collectible.GetMultiplier);
            float _fltTime = 0;

            txtMultiplier.text = "X" + _collectible.GetMultiplier;
            txtScore.text = _collectible.GetScore.ToString();

            txtMultiplier.transform.position = _object.transform.position + Vector3.right * 0.2f;
            txtScore.transform.position = _object.transform.position + Vector3.right * -0.2f;

            txtMultiplier.gameObject.SetActive(true);
            txtScore.gameObject.SetActive(true);

            txtScore.gameObject.GetComponent<DOTweenAnimation>().RecreateTweenAndPlay();
            txtMultiplier.gameObject.GetComponent<DOTweenAnimation>().RecreateTweenAndPlay();

            AudioManager.Instance.PlaySFX(SFXId.Land);
            TK.Audio.HapticManager.PlayImpact(TK.Audio.HapticImpactType.Light);

            while (_fltTime < 0.3f)
            {
                txtScore.text = AnoaModule.ConvertThousand(Mathf.Lerp(_collectible.GetScore, _intTarget, _fltTime / 0.3f));

                _fltTime += Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }

            txtScore.text = AnoaModule.ConvertThousand(_intTarget);

            yield return new WaitForSeconds(0.35f);

            txtScore.gameObject.SetActive(false);
            txtMultiplier.gameObject.SetActive(false);
        }
    }
}