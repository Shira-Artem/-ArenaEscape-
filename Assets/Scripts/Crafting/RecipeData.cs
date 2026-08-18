using UnityEngine;

[CreateAssetMenu(fileName = "RecipeData", menuName = "Crafting/Recipe Data")]
public class RecipeData : ScriptableObject
{
    [System.Serializable]
    public struct Ingredient
    {
        public string itemName;
        public int    amount;
    }

    public Ingredient[] ingredients;
    public string       resultName;
    public int          resultAmount = 1;
}
