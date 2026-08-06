using UnityEngine;

[CreateAssetMenu(fileName = "NewBuilding", menuName = "Factory/BuildingDefinition")]
public class BuildingDefinition : ScriptableObject
{
    public string buildingId;
    public string displayName;
    public Vector2Int footprint;
    public GridDirection[] inputDirections;
    public GridDirection[] outputDirections;
    public GameObject prefab;
    public Material previewMaterial;
    public string[] validRecipeIds;
}