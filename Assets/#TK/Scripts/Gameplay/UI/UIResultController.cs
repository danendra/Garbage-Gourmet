using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace TK.UI
{
    using Gameplay;
    using Data;
    using Audio;
    using Unity.Mathematics;

    public class UIResultController : MonoBehaviour
    {
        [SerializeField] private Image[] _arrImgBurgers;
        [SerializeField] private TMP_Text _txtBurgerName;
        [SerializeField] private TMP_Text _txtScore;
        [SerializeField] private TMP_Text _txtMultiplier;
        [SerializeField] private GameObject _objNew;
        [SerializeField] private GameObject _objTag;
        [SerializeField] private Button _btnResult;

        private float _fltCurrentScore;
        private int _intTargetScore;
        private int _intSpeedScore;
        private bool _isShowScore;
        private RecipeData _recipeData;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        public void Initialize(CollectibleController[] _arrCollectibles, RecipeData _recipe, int _intCalculateScore, bool _isTrash)
        {
            AudioManager.Instance.PlayResultMusic();
            AudioManager.Instance.PlaySFX(SFXId.Horn);

            RectTransform _rectTransform;
            Vector2 _size;

            _recipeData = _recipe;

            for (int i = 0; i < _arrCollectibles.Length; i++)
            {
                _arrImgBurgers[i].sprite = _arrCollectibles[i].GetSprite;
                _arrImgBurgers[i].SetNativeSize();

                _rectTransform = _arrImgBurgers[i].rectTransform;
                _size = _rectTransform.sizeDelta;
                _size.y *= 3;
                _rectTransform.sizeDelta = _size;

                _arrImgBurgers[i].gameObject.SetActive(true);
            }

            if (_arrCollectibles.Length == 0)
            {
                _txtBurgerName.text = "No Burger";

                _btnResult.interactable = true;
            }
            else if (_isTrash)
            {
                _txtBurgerName.text = "Trash Burger";

                Invoke("TrashScore", 1.5f);
            }
            else if (_recipe)
            {
                _txtBurgerName.text = _recipe.name;
                _objNew.SetActive(_recipe.IsNew());
                _recipe.AddBurger();

                Invoke("FinalizeScore", 1.5f);
            }
            else
            {
                _txtBurgerName.text = "Any Burger";
                _objNew.SetActive(false);

                GameManager.Instance.AddPoint(_intCalculateScore);

                _btnResult.interactable = true;
            }

            _fltCurrentScore = 0;
            _intTargetScore = _intCalculateScore;
            _intSpeedScore = _intCalculateScore;

            _isShowScore = true;

            gameObject.SetActive(true);            
        }

        public void FinalizeScore()
        {
            _intTargetScore = Mathf.RoundToInt(_intTargetScore * _recipeData.FltMultiplier);
            _intSpeedScore = Mathf.CeilToInt(_intTargetScore - _fltCurrentScore);

            _txtMultiplier.text = "X" + _recipeData.FltMultiplier;
            _objTag.SetActive(true);

            _isShowScore = true;

            GameManager.Instance.AddPoint(_intTargetScore);

            _btnResult.interactable = true;
        }

        public void TrashScore()
        {
            _intTargetScore = 0;
            _intSpeedScore = Mathf.CeilToInt(_fltCurrentScore);
            _txtMultiplier.text = "X0";
            _objTag.SetActive(true);

            _isShowScore = true;

            _btnResult.interactable = true;
        }

        public void BackToMainMenu()
        {
            GameManager.Instance.LoadScene(0);
        }

        // Update is called once per frame
        void Update()
        {
            if (_isShowScore)
            {
                _fltCurrentScore = Mathf.MoveTowards(_fltCurrentScore, _intTargetScore, Time.deltaTime * _intSpeedScore);

                if (Mathf.Abs(_fltCurrentScore - _intTargetScore) < 1)
                {
                    _fltCurrentScore = _intTargetScore;
                    _isShowScore = false;
                }

                _txtScore.text = Mathf.FloorToInt(_fltCurrentScore).ToString();
            }
        }
    }
}