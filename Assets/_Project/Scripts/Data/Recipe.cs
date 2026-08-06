using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Factory/Recipe")]
public class Recipe : ScriptableObject
{
    public string recipeId;
    public ItemType[] inputTypes;
    public int[] inputAmounts;
    public ItemType[] outputTypes;
    public int[] outputAmounts;
    public float processTime;
    public string requiredBuildingType;
}
