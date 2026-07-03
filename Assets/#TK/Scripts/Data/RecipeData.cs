using UnityEngine;

namespace TK.Data
{
    using Gameplay;

    [CreateAssetMenu(fileName = "RecipeData", menuName = "TK/RecipeData")]
    public class RecipeData : ScriptableObject
    {
        [Tooltip("Ingredient arrangement from bottom to top")]
        public CollectibleController[] CollectibleIngredients;
        public float FltMultiplier;
    }
}