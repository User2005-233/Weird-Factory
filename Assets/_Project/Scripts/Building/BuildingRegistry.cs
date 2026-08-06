using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BuildingRegistry", menuName = "Factory/BuildingRegistry")]
public class BuildingRegistry : ScriptableObject
{
    public BuildingDefinition[] buildingDefinitions;
    public Recipe[] recipes;
    public ItemType[] itemTypes;

    private Dictionary<string, BuildingDefinition> _byId;
    private Dictionary<string, Recipe> _recipeById;
    private Dictionary<string, ItemType> _itemById;

    public void Init()  // BuildingManager 启动时调一次
    {
        _byId = new();
        foreach (var def in buildingDefinitions)
            _byId[def.buildingId] = def;
        _recipeById = new();
        foreach (var recipe in recipes)
            _recipeById[recipe.recipeId] = recipe;
        _itemById = new();
        foreach (var item in itemTypes)
            _itemById[item.itemId] = item;
    }

    public BuildingDefinition GetBuilding(string id) => _byId.GetValueOrDefault(id);
    public Recipe GetRecipe(string id) => _recipeById.GetValueOrDefault(id);
    public ItemType GetItem(string id) => _itemById.GetValueOrDefault(id);
}