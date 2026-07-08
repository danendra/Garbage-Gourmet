using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace TK.UI
{
    using Gameplay;
    using Data;

    public class UIResultController : MonoBehaviour
    {
        [SerializeField] private Image[] _arrImgBurgers;
        [SerializeField] private TMP_Text txtBurgerName;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        public void Initialize(CollectibleController[] _arrCollectibles, RecipeData _recipe)
        {
            for (int i = 0; i < _arrCollectibles.Length; i++)
            {
                _arrImgBurgers[i].sprite = _arrCollectibles[i].GetSprite;
                _arrImgBurgers[i].gameObject.SetActive(true);
            }

            if (_recipe)
            {
                txtBurgerName.text = _recipe.name;
            }
            else
            {
                txtBurgerName.text = "Any Burger";
            }

            gameObject.SetActive(true);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}