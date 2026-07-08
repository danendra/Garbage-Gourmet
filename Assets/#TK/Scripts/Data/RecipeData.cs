using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TK.Data
{
    using Gameplay;

    [CreateAssetMenu(fileName = "RecipeData", menuName = "TK/RecipeData")]
    public class RecipeData : ScriptableObject
    {
        [Header("Collection Display Data")]
        public string CollectionName;
        public Sprite CollectionImage;
        [TextArea(3, 5)]
        public string CollectionDescription;

        [Header("Recipe Logic Data")]
        [Tooltip("Ingredient arrangement from bottom to top")]
        public CollectibleController[] CollectibleIngredients;
        public bool IsRarityFixed;
        public bool IsSame;
        public float FltMultiplier;

        public bool IsIngredientCorrect(IEnumerable<CollectibleController> _ieCollectible)
        {
            if (CollectibleIngredients.Length == 1 && _ieCollectible.Count() == 1)
            {
                return true;
            }
            else if (IsSame && _ieCollectible.Count() > 2)
            {
                ITEM_TYPE _type = CollectibleIngredients[0].GetType;
                RARITY _rarity = CollectibleIngredients[0].GetRarity;

                if (IsRarityFixed)
                {
                    return _ieCollectible.All(_collectible => _collectible.GetType == _type && _collectible.GetRarity == _rarity);
                }
                else
                {
                    return _ieCollectible.All(_collectible => _collectible.GetType == _type);
                }
            }
            else if (IsRarityFixed)
            {
                var _nameCollect = _ieCollectible.Select(_item => _item.name);
                var _nameRecipe = CollectibleIngredients.Select(_item => _item.name);

                return _nameRecipe.SequenceEqual(_nameCollect);
            }
            else
            {
                var _typeCollect = _ieCollectible.Select(_item => _item.GetType);
                var _typeRecipe = CollectibleIngredients.Select(_item => _item.GetType);

                return _typeRecipe.SequenceEqual(_typeCollect);
            }
        }

        public void AddBurger()
        {            
            int _intTotalMake = PlayerPrefs.GetInt("BURGER_" + name, 0);
            _intTotalMake++;

            PlayerPrefs.SetInt("BURGER_" + name, _intTotalMake);
        }

        public bool IsNew()
        {
            return PlayerPrefs.HasKey("BURGER_" + name);            
        }
    }
}